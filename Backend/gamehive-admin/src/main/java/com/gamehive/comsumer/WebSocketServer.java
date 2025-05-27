package com.gamehive.comsumer;

import com.alibaba.fastjson2.JSONObject;
import com.gamehive.comsumer.Game.Cell;
import com.gamehive.comsumer.Game.Game;
import com.gamehive.comsumer.constants.FeedBackEventTypeEnum;
import com.gamehive.comsumer.constants.ReceiveEventTypeEnum;
import com.gamehive.comsumer.constants.GameStatusEnum;
import com.gamehive.comsumer.message.FeedBackObj;
import com.gamehive.comsumer.message.ReceiveObj;
import com.gamehive.constants.GameTypeEnum;
import com.gamehive.constants.SpecialPlayerEnum;
import com.gamehive.mapper.GameTypeMapper;
import com.gamehive.mapper.PlayerMapper;
import com.gamehive.mapper.RecordMapper;
import com.gamehive.pojo.Player;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import org.springframework.util.LinkedMultiValueMap;
import org.springframework.util.MultiValueMap;
import org.springframework.web.client.RestTemplate;

import javax.websocket.*;
import javax.websocket.server.PathParam;
import javax.websocket.server.ServerEndpoint;
import java.io.IOException;
import java.util.Objects;
import java.util.concurrent.ConcurrentHashMap;

@Component
@ServerEndpoint("/websocket/{userId}")  // 修改为直接接收用户ID
public class WebSocketServer {

    public static ConcurrentHashMap<Long, WebSocketServer> users = new ConcurrentHashMap<>();
    //匹配池
    private Session session = null;
    private Player user;

    //非单例模式
    public static PlayerMapper playerMapper;
    public static RecordMapper recordMapper;
    public static GameTypeMapper gameTypeMapper;
    public static RestTemplate restTemplate;//spring boot间通信
    private final static String addPlayerUrl = "http://127.0.0.1:3001/player/add/";
    private final static String removePlayerUrl = "http://127.0.0.1:3001/player/remove/";

    public Game game = null;
    private boolean isMatching = false; // 标记用户是否正在匹配

    @Autowired
    public void setUserMapper(PlayerMapper playerMapper) {
        WebSocketServer.playerMapper = playerMapper;
    }

    @Autowired
    public void setRecordMapper(RecordMapper recordMapper) {
        WebSocketServer.recordMapper = recordMapper;
    }

    @Autowired
    public void serGameTypeMapper(GameTypeMapper gameTypeMapper) {
        WebSocketServer.gameTypeMapper = gameTypeMapper;
    }

    @Autowired
    public void setRestTemplate(RestTemplate restTemplate) {
        //在两个spring boot 间通信
        WebSocketServer.restTemplate = restTemplate;
    }

    @OnOpen
    public void onOpen(Session session, @PathParam("userId") String userIdStr) throws IOException {
        // 建立连接
        this.session = session;
        System.out.println("WebSocket连接建立，用户ID: " + userIdStr);
        try {
            Long userId = Long.parseLong(userIdStr);
            this.user = playerMapper.selectPlayerByUserId(userId);
            if (this.user != null) {
                System.out.println("用户验证成功，用户ID: " + userId + ", 用户名: " + this.user.getUserName());
                users.put(userId, this);
            } else {
                System.out.println("用户不存在于数据库中，关闭连接");
                this.session.close();
            }
        } catch (NumberFormatException e) {
            System.out.println("无效的用户ID格式，关闭连接");
            this.session.close();
        }
    }

    /**
     * 开启游戏 特判LMM方，LMM永远为玩家b
     */
    public static void startGame(Long aId, SpecialPlayerEnum playerAType, Long bId, SpecialPlayerEnum playerBType,
                                 GameTypeEnum gameTypeEnum, Boolean forLMM) {
        Player a = playerMapper.selectPlayerByUserId(aId), b;
        if (forLMM) {
            b = new Player();
            b.setUserId((long) SpecialPlayerEnum.LMM.getCode());
            b.setUserName(SpecialPlayerEnum.LMM.getChineseName());
        } else {
            b = playerMapper.selectPlayerByUserId(bId);
        }
        Game game = new Game(a, b, playerAType, playerBType, gameTypeEnum, forLMM);
        System.out.println("开启一场对局：");
        System.out.println("玩家A: ID=" + a.getUserId() + ", 名称=" + a.getUserName() + ", 类型=" + playerAType.getChineseName());
        System.out.println("玩家B: ID=" + b.getUserId() + ", 名称=" + b.getUserName() + ", 类型=" + playerBType.getChineseName());
        System.out.println("游戏类型: " + gameTypeEnum.getChineseName());
        System.out.println("是否为LMM对局: " + forLMM);
        System.out.println("先手玩家: " + (game.getFirst().equals(game.getPlayerA()) ? "玩家A" : "玩家B"));
        //一方断开链接则无视其操作
        //一方断开链接则无视其操作
        if (users.get(a.getUserId()) != null) {
            users.get(a.getUserId()).game = game;
            users.get(a.getUserId()).isMatching = false; // 游戏开始，不再是匹配状态
        }
        if (!forLMM && users.get(b.getUserId()) != null) {
            users.get(b.getUserId()).game = game;
            users.get(b.getUserId()).isMatching = false; // 游戏开始，不再是匹配状态
        }
        game.start();

        FeedBackObj forA = new FeedBackObj(), forB = new FeedBackObj();
        forA.setEvent(FeedBackEventTypeEnum.START.getType());
        forB.setEvent(FeedBackEventTypeEnum.START.getType());
        forA.setFirst(game.getFirst().equals(game.getPlayerA()));
        forB.setFirst(game.getFirst().equals(game.getPlayerB()));
        //设置对手信息
        forA.setOpponentId(b.getUserId());
        forA.setOpponentName(b.getUserName());
        forB.setOpponentId(a.getUserId());
        forB.setOpponentName(a.getUserName());

        //一方断开链接则无视其操作
        if (users.get(a.getUserId()) != null) {
            users.get(a.getUserId()).sendMessage(JSONObject.toJSONString(forA));
        }
        if (!forLMM && users.get(b.getUserId()) != null) {
            users.get(b.getUserId()).sendMessage(JSONObject.toJSONString(forB));
        }

    }

    @OnClose
    public void onClose() {
        // 关闭链接
        System.out.println("WebSocket连接关闭，用户ID: " + (this.user != null ? this.user.getUserId() : "未知"));
        if (this.user != null) {
            // 如果用户正在游戏中，需要结束游戏
            if (this.game != null) {
                System.out.println("用户断开连接，结束游戏");

                // 如果是联机对战（非LMM对战），判对手胜利
                if (!this.game.getForLMM()) {
                    System.out.println("联机对战中玩家断开连接，判对手胜利");
                    handlePlayerQuit();
                }

                this.game = null;
            }

            // 如果在匹配状态，从匹配池移除
            if (this.isMatching) {
                System.out.println("用户断开连接时正在匹配，从匹配池移除");
                this.isMatching = false;
                try {
                    MultiValueMap<String, String> data = new LinkedMultiValueMap<>();
                    data.add("user_id", this.user.getUserId().toString());
                    data.add("rating", this.user.getRaking().toString());
                    restTemplate.postForObject(removePlayerUrl, data, String.class);
                } catch (Exception e) {
                    System.out.println("从匹配池移除用户时发生错误: " + e.getMessage());
                }
            }

            users.remove(this.user.getUserId());
        }
    }


    /**
     * 玩家开始匹配
     */
    private void startMatching(ReceiveObj matchData) {
        System.out.println("startMatching");
        if (matchData.getPlayWithLMM()) {
            // 与大模型对战，直接开始游戏
            startGame(this.user.getUserId(), SpecialPlayerEnum.PLAYER, (long) SpecialPlayerEnum.LMM.getCode(),
                    SpecialPlayerEnum.LMM, GameTypeEnum.fromChineseName(matchData.getGameType()), true);
            return;
        }
        //玩家选择了与其他玩家匹配
        this.isMatching = true; // 标记为匹配状态
        MultiValueMap<String, String> data = new LinkedMultiValueMap<>();
        data.add("user_id", this.user.getUserId().toString());
        data.add("rating", this.user.getRaking().toString());
        data.add("game_type", matchData.getGameType());
        //spring boot通信，接收方地址 数据 返回值class(反射机制)
        restTemplate.postForObject(addPlayerUrl, data, String.class);
    }

    /**
     * 玩家停止匹配或终止游戏
     */
    private void stopMatching() {
        System.out.println("收到停止请求，用户ID: " + this.user.getUserId());

        // 如果用户正在游戏中，结束游戏
        if (this.game != null) {
            System.out.println("用户主动终止游戏");

            // 如果是联机对战（非LMM对战），判对手胜利
            if (!this.game.getForLMM()) {
                System.out.println("联机对战中玩家主动退出，判对手胜利");
                handlePlayerQuit();
            }

            this.game = null; // 清理游戏状态
            return; // 不需要从匹配池移除，因为用户不在匹配池中
        }

        // 如果在匹配状态，从匹配池移除
        if (this.isMatching) {
            System.out.println("用户停止匹配，从匹配池移除");
            this.isMatching = false;
            MultiValueMap<String, String> data = new LinkedMultiValueMap<>();
            data.add("user_id", this.user.getUserId().toString());
            data.add("rating", this.user.getRaking().toString());
            restTemplate.postForObject(removePlayerUrl, data, String.class);
        } else {
            System.out.println("用户不在匹配状态，无需操作");
        }
    }

    /**
     * 处理玩家退出游戏，判对手胜利
     */
    private void handlePlayerQuit() {
        if (this.game == null || this.game.getForLMM()) {
            return;
        }

        try {
            // 确定退出的玩家和对手
            Long quittingPlayerId = this.user.getUserId();
            Long opponentId;
            String winnerStatus;

            if (Objects.equals(this.game.getPlayerA().getUserId(), quittingPlayerId)) {
                // 玩家A退出，玩家B获胜
                opponentId = this.game.getPlayerB().getUserId();
                winnerStatus = GameStatusEnum.PLAYER_B_WIN.getName();
                System.out.println("玩家A " + this.user.getUserName() + " 退出，玩家B " + this.game.getPlayerB().getUserName() + " 获胜");
            } else {
                // 玩家B退出，玩家A获胜
                opponentId = this.game.getPlayerA().getUserId();
                winnerStatus = GameStatusEnum.PLAYER_A_WIN.getName();
                System.out.println("玩家B " + this.user.getUserName() + " 退出，玩家A " + this.game.getPlayerA().getUserName() + " 获胜");
            }

            // 发送游戏结果给对手
            WebSocketServer opponentConnection = users.get(opponentId);
            if (opponentConnection != null) {
                FeedBackObj resultMessage = new FeedBackObj();
                resultMessage.setEvent(FeedBackEventTypeEnum.RESULT.getType());
                resultMessage.setWinStatus(winnerStatus);
                opponentConnection.sendMessage(JSONObject.toJSONString(resultMessage));

                // 清理对手的游戏状态
                opponentConnection.game = null;
            }

            // 注意：这里不调用saveToDataBase，因为游戏是被强制结束的
            // 如果需要记录，可以考虑在Game类中添加public方法来处理强制结束的情况

        } catch (Exception e) {
            System.out.println("处理玩家退出时发生错误: " + e.getMessage());
            e.printStackTrace();
        }
    }

    /**
     * 接受人类玩家下棋，根据会话找是那个玩家操作
     */
    private void move(int x, int y) {
        if (Objects.equals(game.getPlayerA().getUserId(), user.getUserId())) {
            if (!game.getPlayerA().getPlayerType().equals(SpecialPlayerEnum.LMM)) {
                game.setNextStepA(new Cell(x, y));
            }
        } else if (Objects.equals(game.getPlayerB().getUserId(), user.getUserId())) {
            if (!game.getPlayerB().getPlayerType().equals(SpecialPlayerEnum.LMM)) {
                game.setNextStepB(new Cell(x, y));
            }
        }
    }

    @OnMessage
    public void onMessage(String message, Session session) {
        //从Client接收消息
        System.out.println("接受到消息" + message);
        ReceiveObj data = JSONObject.parseObject(message, ReceiveObj.class);
        ReceiveEventTypeEnum event = ReceiveEventTypeEnum.fromCode(Integer.parseInt(data.getEvent()));
        if (event != null) {
            switch (event) {
                case MOVE:
                    move(data.getX(), data.getY());
                    break;
                case STOP:
                    stopMatching();
                    break;
                case START:
                    startMatching(data);
                    break;
            }
        }
    }

    @OnError
    public void onError(Session session, Throwable error) {
        System.out.println("WebSocket发生错误，用户ID: " + (this.user != null ? this.user.getUserId() : "未知"));
        System.out.println("错误类型: " + error.getClass().getSimpleName() + ", 消息: " + error.getMessage());

        //对于EOFException是客户端正常断开连接
        if (!(error instanceof java.io.EOFException)) {
            error.printStackTrace();
        }

        // 清理连接
        if (this.user != null) {
            if (this.game != null) {
                System.out.println("因错误清理游戏状态");

                // 如果是联机对战（非LMM对战），判对手胜利
                if (!this.game.getForLMM()) {
                    System.out.println("联机对战中玩家因错误断开连接，判对手胜利");
                    handlePlayerQuit();
                }

                this.game = null;
            }

            // 如果在匹配状态，从匹配池移除
            if (this.isMatching) {
                System.out.println("用户因错误断开连接时正在匹配，从匹配池移除");
                this.isMatching = false;
                try {
                    MultiValueMap<String, String> data = new LinkedMultiValueMap<>();
                    data.add("user_id", this.user.getUserId().toString());
                    data.add("rating", this.user.getRaking().toString());
                    restTemplate.postForObject(removePlayerUrl, data, String.class);
                } catch (Exception e) {
                    System.out.println("从匹配池移除用户时发生错误: " + e.getMessage());
                }
            }

            users.remove(this.user.getUserId());
        }
    }


    public void sendMessage(String message) {//发送信息
        synchronized (this.session) {
            try {
                if (this.session != null && this.session.isOpen()) {
                    this.session.getBasicRemote().sendText(message);
                } else {
                    System.out.println("WebSocket会话已关闭，无法发送消息: " + message);
                }
            } catch (IOException e) {
                System.out.println("发送WebSocket消息失败: " + e.getMessage());
                // 清理已断开的连接
                if (this.user != null) {
                    users.remove(this.user.getUserId());
                }
            }
        }
    }

    public void setSession(String message) {
        synchronized (this.session) {
            try {
                this.session.getBasicRemote().sendText(message);
            } catch (IOException e) {
                e.printStackTrace();
            }
        }
    }
}
