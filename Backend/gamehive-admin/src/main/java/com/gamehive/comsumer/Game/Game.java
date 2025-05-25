package com.gamehive.comsumer.Game;

import com.alibaba.fastjson2.JSONObject;
import com.gamehive.comsumer.WebSocketServer;
import com.gamehive.comsumer.constants.CellRoleEnum;
import com.gamehive.comsumer.constants.FeedBackEventTypeEnum;
import com.gamehive.comsumer.constants.GameStatusEnum;
import com.gamehive.comsumer.message.FeedBackObj;
import com.gamehive.comsumer.stratey.*;
import com.gamehive.constants.GameTypeEnum;
import com.gamehive.constants.SpecialPlayerEnum;
import com.gamehive.pojo.GameType;
import com.gamehive.pojo.Player;
import com.gamehive.pojo.Record;
import lombok.Getter;
import org.springframework.util.LinkedMultiValueMap;
import org.springframework.util.MultiValueMap;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.locks.ReentrantLock;

@Getter
public class Game extends Thread {

    private final static String addBotUrl = "http://127.0.0.1:3002/LMMRunning/add/";
    private final Integer rows, cols;
    //是否为与大模型下棋
    private final Boolean forLMM;
    //游戏类型
    private final GameType gameType;
    //是否A先手
    private final GamePlayer first;
    //参与游戏玩家
    private final GamePlayer playerA, playerB;
    //当前棋盘状态
    private final List<List<CellRoleEnum>> map;
    private final ReentrantLock lock = new ReentrantLock();

    //落子次数
    private Integer round = 0;
    //玩家的下一步操作
    private Cell nextStepA = null, nextStepB = null;
    //当前游戏状态
    private GameStatusEnum status = GameStatusEnum.UNFINISHED;
    //抽象策略
    private GameStrategy gameStrategy;

    /**
     * 开启游戏
     */
    public Game(Player a, Player b, SpecialPlayerEnum playerAType, SpecialPlayerEnum playerBType,
                GameTypeEnum gameTypeEnum, Boolean forLMM) {
        this.forLMM = forLMM;
        this.gameType = WebSocketServer.gameTypeMapper.selectGameTypeByGameId((long) gameTypeEnum.getCode());
        //初始化判负策略
        switch (gameTypeEnum) {
            case GOBANG:
            case GOBANG_88:
                gameStrategy = new GoBangStrategy();
                break;
            case ANTI_GO:
                gameStrategy = new AntiGoStrategy();
                break;
            case TIC_TAC_TOE:
                gameStrategy = new TicTocToeStrategy();
                break;
            case MISERE_TIC_TAC_TOE:
                gameStrategy = new MisereTicTacToeStrategy();
                break;
        }
        //初始化地图
        rows = gameType.getBoardSize().intValue();
        cols = gameType.getBoardSize().intValue();
        map = new ArrayList<>(rows);
        for (int i = 0; i < rows; i++) {
            List<CellRoleEnum> row = new ArrayList<>(cols);
            for (int j = 0; j < cols; j++) {
                row.add(CellRoleEnum.EMPTY);
            }
            map.add(row);
        }

        //构建玩家信息
        playerA = new GamePlayer(a.getUserId(), a.getUserName(), playerAType, new ArrayList<>());
        playerB = new GamePlayer(b.getUserId(), b.getUserName(), playerBType, new ArrayList<>());
        //决定先后手
        boolean isAFirst = Math.random() < 0.5;
        if (isAFirst) {
            first = playerA;
        } else {
            first = playerB;
        }
        //回传先后手信息
        FeedBackObj forA = new FeedBackObj(), forB = new FeedBackObj();
        forA.setEvent(FeedBackEventTypeEnum.START.getType());
        forB.setEvent(FeedBackEventTypeEnum.START.getType());
        forA.setFirst(isAFirst);
        forB.setFirst(!isAFirst);
        forA.setOpponentId(b.getUserId());
        forB.setOpponentId(a.getUserId());
        forA.setOpponentName(b.getUserName());
        forB.setOpponentName(a.getUserName());
        sendAMessage(JSONObject.toJSONString(forA));
        sendBMessage(JSONObject.toJSONString(forB));
    }

    /**
     * 要求LMM输出
     */
    private void sendLMMCode(GamePlayer player) {
        MultiValueMap<String, String> data = new LinkedMultiValueMap<>();
        data.add("currentMap", getStringMap());
        data.add("userId", playerA.getUserId().toString());
        data.add("LLMFlag", player == playerA ? CellRoleEnum.PLAYER_A.getCode() : CellRoleEnum.PLAYER_B.getCode());
        data.add("gameType", gameType.getGameName());
        data.add("gameRole", gameType.getGameRule());
        data.add("historySteps", "稍后实现，测试数据");
        WebSocketServer.restTemplate.postForObject(addBotUrl, data, String.class);
    }

    /**
     * 游戏推进到下一步，如果没在规定时间内输入或输入非法则返回false
     */
    private boolean nextStep() {
        boolean askA = (round % 2) == 0;
        //如果LMM输入则则发送信息
        if (!askA && forLMM) {
            sendLMMCode(playerB);
        }

        //最大等待时间60秒
        for (int i = 0; i < 600; i++) {
            try {
                Thread.sleep(100);
                lock.lock();
                try {
                    //检查A玩家是否有输入
                    if (askA && nextStepA != null) {
                        if (map.get(nextStepA.getX()).get(nextStepA.getY()) != CellRoleEnum.EMPTY) {
                            //非法输入不记录，之间判负
                            return false;
                        }
                        map.get(nextStepA.getY()).set(nextStepA.getY(), CellRoleEnum.PLAYER_A);
                        playerA.getSteps().add(nextStepA);
                        return true;
                    }//检查B玩家是否有输入
                    else if (!askA && nextStepB != null) {
                        if (map.get(nextStepB.getX()).get(nextStepB.getY()) != CellRoleEnum.EMPTY) {
                            return false;
                        }
                        map.get(nextStepB.getY()).set(nextStepB.getY(), CellRoleEnum.PLAYER_B);
                        playerB.getSteps().add(nextStepB);
                    }
                    return true;
                } finally {
                    lock.unlock();
                }
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        }
        return false;
    }


    @Override
    public void run() {
        //步数收到棋盘大小限制
        for (int i = 0; i < 1000; i++) {
            //内部回等待50秒，超时视为失败
            if (!nextStep()) {
                //没有输入或输入非法
                lock.lock();
                try {
                    boolean askA = (round % 2) == 0;
                    if (askA) {
                        status = GameStatusEnum.PLAYER_B_WIN;
                    } else {
                        status = GameStatusEnum.PLAYER_A_WIN;
                    }
                    sendResult();
                } finally {
                    lock.unlock();
                }
                break;
            } else {
                //合法输入
                judge();
                if (status.equals(GameStatusEnum.UNFINISHED)) {
                    //游戏还在继续，广播上一步操作
                    sendMove(round % 2 == 0 ? nextStepA : nextStepB);
                } else {
                    //广播结果-游戏正常结束
                    sendResult();
                    break;
                }
                round++;
            }
        }
    }

    /**
     * 判断操作后局面
     */
    private void judge() {
        status = gameStrategy.checkGameOver(map);
    }

    /**
     * 广播操作信息
     */
    private void sendMove(Cell move) {
        lock.lock();
        try {
            FeedBackObj data = new FeedBackObj();
            data.setEvent(FeedBackEventTypeEnum.MOVE.getType());
            data.setGameStatus(GameStatusEnum.UNFINISHED.getName());
            data.setX(move.getX());
            data.setY(move.getY());
            sendAllMessage(JSONObject.toJSONString(data));
            nextStepA = nextStepB = null;
        } finally {
            lock.unlock();
        }
    }

    /**
     * 广播对局结束信息
     */
    private void sendResult() {
        FeedBackObj result = new FeedBackObj();
        result.setEvent(FeedBackEventTypeEnum.RESULT.getType());
        result.setWinStatus(status.getName());
        saveToDataBase();
    }

    //*****工具函数*****//

    /**
     * 对局存入数据库
     */
    private void saveToDataBase() {
        JSONObject item = new JSONObject();
        //过滤步数小于对应步数次的对局
        if (round < gameType.getMinValidPieces()) {
            return;
        }

        //筛选出两位玩家
        Player userA = WebSocketServer.playerMapper.selectById(playerA.getUserId());
        Player userB = WebSocketServer.playerMapper.selectById(playerB.getUserId());
        Long ratingA = userA.getRaking();
        Long ratingB = userB.getRaking();

        //更新积分
        if (status.equals(GameStatusEnum.PLAYER_B_WIN)) {
            ratingA -= 4;
            ratingB += 5;
        } else if (status.equals(GameStatusEnum.PLAYER_A_WIN)) {
            ratingB -= 4;
            ratingA += 5;
        }
        updateUserRating(playerA, ratingA);
        if (!forLMM) {
            updateUserRating(playerB, ratingB);
        }
        //写入对局表
        Record record = new Record();
        record.setGameTypeId(gameType.getGameId());
        record.setGameTypeName(gameType.getGameName());
        record.setAlgorithmId(forLMM ? -2L : -1L);
        record.setAlgorithmName(forLMM ? "LMM" : "联机对战");
        record.setWinner((long) status.getCode());
        //先后手
        record.setFirstPlayerId(playerA.getUserId());
        record.setFirstPlayer(playerA.getUserName());
        record.setSecondPlayerId(playerB.getUserId());
        record.setSecondPlayerName(playerB.getUserName());
        //操作序列
        record.setFirstPlayerPieces(JSONObject.toJSONString(playerA.getSteps()));
        record.setPlayerBPieces(JSONObject.toJSONString(playerB.getSteps()));
        WebSocketServer.recordMapper.insert(record);
    }

    //设置next
    public synchronized void setNextStepA(Cell nextStepA) {
        this.nextStepA = nextStepA;
    }

    public synchronized void setNextStepB(Cell nextStepB) {
        this.nextStepB = nextStepB;
    }

    public synchronized void setLMMNextMove(Integer x, Integer y) {
        this.nextStepB = new Cell(x, y);
    }


    private void sendAMessage(String x) {
        if (WebSocketServer.users.get(playerA.getUserId()) != null) {
            WebSocketServer.users.get(playerA.getUserId()).sendMessage(x);
        }
    }

    private void sendBMessage(String x) {
        //大模型只会被获取输入，其余信息都不同步大模型
        if (!forLMM && WebSocketServer.users.get(playerB.getUserId()) != null) {
            WebSocketServer.users.get(playerB.getUserId()).sendMessage(x);
        }
    }

    private void sendAllMessage(String x) {
        sendAMessage(x);
        sendBMessage(x);
    }

    /**
     * 更新玩家积分
     */
    private void updateUserRating(GamePlayer player, Long rating) {
        Player user = WebSocketServer.playerMapper.selectById(player.getUserId());
        user.setRaking(rating);
        WebSocketServer.playerMapper.updateById(user);
    }

    /**
     * 局面转为字符串，用于发送给大模型
     *
     * @return 由012组成的字符串，表示棋盘状态，每行用换行符分隔。未下棋位置0，人类下棋位置1，大模型为2
     */
    private String getStringMap() {
        StringBuilder sb = new StringBuilder();
        for (List<CellRoleEnum> row : map) {
            for (CellRoleEnum cell : row) {
                sb.append(cell.getCode()); //使用枚举的code值
            }
            sb.append("\n"); //每行结束后添加换行符
        }
        return sb.toString();
    }
}
