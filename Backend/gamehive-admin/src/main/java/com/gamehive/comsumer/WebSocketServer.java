package com.gamehive.comsumer;

import com.alibaba.fastjson2.JSONObject;
import com.gamehive.comsumer.Game.Cell;
import com.gamehive.comsumer.Game.Game;
import com.gamehive.comsumer.constants.FeedBackEventTypeEnum;
import com.gamehive.comsumer.constants.ReceiveEventTypeEnum;
import com.gamehive.comsumer.message.FeedBackObj;
import com.gamehive.comsumer.message.ReceiveObj;
import com.gamehive.comsumer.utils.JwtAuthentication;
import com.gamehive.constants.GameTypeEnum;
import com.gamehive.constants.SpecialPlayerEnum;
import com.gamehive.mapper.GameTypeMapper;
import com.gamehive.mapper.PlayerMapper;
import com.gamehive.mapper.RecordMapper;
import com.gamehive.pojo.Player;
import java.io.IOException;
import java.util.Objects;
import java.util.concurrent.ConcurrentHashMap;
import javax.websocket.OnClose;
import javax.websocket.OnError;
import javax.websocket.OnMessage;
import javax.websocket.OnOpen;
import javax.websocket.Session;
import javax.websocket.server.PathParam;
import javax.websocket.server.ServerEndpoint;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import org.springframework.util.LinkedMultiValueMap;
import org.springframework.util.MultiValueMap;
import org.springframework.web.client.RestTemplate;

@Component
@ServerEndpoint("/websocket/{token}")  // 注意不要以'/'结尾
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
    public void onOpen(Session session, @PathParam("token") String token) throws IOException {
        // 建立连接
        this.session = session;
        System.out.println("connected");
        Long userId = JwtAuthentication.getUserId();
        this.user = playerMapper.selectById(userId);
        if (this.user != null) {
            users.put(userId, this);
        } else {
            this.session.close();
        }
    }

    public static void startGame(Long aId, SpecialPlayerEnum playerAType, Long bId, SpecialPlayerEnum playerBType,
            GameTypeEnum gameTypeEnum, Boolean forLMM) {
        Player a = playerMapper.selectById(aId), b;
        if (forLMM) {
            b = playerMapper.selectById(bId);
        } else {
            b = new Player();
            b.setUserId(SpecialPlayerEnum.LMM.getCode());
            b.setUserName(SpecialPlayerEnum.LMM.getChineseName());
        }
        Game game = new Game(a.getUserId(), playerAType,
                b.getUserId(), playerBType, gameTypeEnum, forLMM);

        //一方断开链接则无视其操作
        if (users.get(a.getUserId()) != null) {
            users.get(a.getUserId()).game = game;
        }
        if (!forLMM && users.get(b.getUserId()) != null) {
            users.get(b.getUserId()).game = game;
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
        System.out.println("closed");
        if (this.user != null) {
            users.remove(this.user.getUserId());
        }
    }

    //根据开始游戏请求进行匹配
    private void startMatching(ReceiveObj matchData) {
        System.out.println("startMatching");
        if (matchData.getPlayWithLMM()) {
            startGame(this.user.getUserId(), SpecialPlayerEnum.PLAYER, SpecialPlayerEnum.LMM.getCode(),
                    SpecialPlayerEnum.LMM, GameTypeEnum.fromChineseName(matchData.getGameType()), true);
            return;
        }
        //玩家选择了与其他玩家匹配
        MultiValueMap<String, String> data = new LinkedMultiValueMap<>();
        data.add("user_id", this.user.getUserId().toString());
        data.add("rating", this.user.getRaking().toString());
        //spring boot通信，接收方地址 数据 返回值class(反射机制)
        restTemplate.postForObject(addPlayerUrl, data, String.class);
    }

    private void stopMatching() {
//        System.out.println("stopMatching");
        MultiValueMap<String, String> data = new LinkedMultiValueMap<>();
        data.add("user_id", this.user.getUserId().toString());
        data.add("rating", this.user.getRaking().toString());
        restTemplate.postForObject(removePlayerUrl, data, String.class);

    }

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
        // 从Client接收消息
        System.out.println("接受的匹配请求");
        ReceiveObj data = JSONObject.parseObject(message, ReceiveObj.class);
        ReceiveEventTypeEnum event = ReceiveEventTypeEnum.fromType(data.getEvent());
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
        System.out.println("错误");
        error.printStackTrace();
    }


    public void sendMessage(String message) {//发送信息
        synchronized (this.session) {
            try {
                this.session.getBasicRemote().sendText(message);
            } catch (IOException e) {
                e.printStackTrace();
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
