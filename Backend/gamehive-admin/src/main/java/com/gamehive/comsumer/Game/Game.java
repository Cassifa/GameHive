package com.gamehive.comsumer.Game;

import com.alibaba.fastjson2.JSONObject;
import com.gamehive.comsumer.WebSocketServer;
import com.gamehive.comsumer.constants.CellRoleEnum;
import com.gamehive.comsumer.constants.FeedBackEventTypeEnum;
import com.gamehive.comsumer.constants.GameStatusEnum;
import com.gamehive.comsumer.message.FeedBackObj;
import com.gamehive.comsumer.stratey.AntiGoStrategy;
import com.gamehive.comsumer.stratey.GameStrategy;
import com.gamehive.comsumer.stratey.GoBangStrategy;
import com.gamehive.comsumer.stratey.MisereTicTacToeStrategy;
import com.gamehive.comsumer.stratey.TicTocToeStrategy;
import com.gamehive.constants.GameTypeEnum;
import com.gamehive.constants.SpecialPlayerEnum;
import com.gamehive.pojo.Player;
import com.gamehive.pojo.Record;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.locks.ReentrantLock;
import lombok.Getter;
import org.springframework.util.LinkedMultiValueMap;
import org.springframework.util.MultiValueMap;

@Getter
public class Game extends Thread {

    private Integer rows, cols;
    //落子次数
    private Integer round = 0;
    private GamePlayer invalidStepPlayer = null;
    //是否A先手
    private final GamePlayer first;
    private List<List<CellRoleEnum>> map;
    private final GamePlayer playerA, playerB;
    private Cell nextStepA = null, nextStepB = null;
    private GameStatusEnum status = GameStatusEnum.UNFINISHED;
    private final ReentrantLock lock = new ReentrantLock();
    private GameStrategy gameStrategy;
    private final static String addBotUrl =
            "http://127.0.0.1:3002/bot/add/";

    public Game(Long aId, SpecialPlayerEnum playerAType, SpecialPlayerEnum playerBType, Long bId,
            GameTypeEnum gameTypeEnum
    ) {
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
        gameStrategy.initGameMap(rows, cols);
        map = new ArrayList<>(rows);
        for (int i = 0; i < rows; i++) {
            List<CellRoleEnum> row = new ArrayList<>(cols);
            for (int j = 0; j < cols; j++) {
                row.add(CellRoleEnum.EMPTY);
            }
            map.add(row);
        }
        playerA = new GamePlayer(aId, playerAType,
                new ArrayList<>());
        playerB = new GamePlayer(bId, playerBType,
                new ArrayList<>());
        boolean isAFirst = Math.random() < 0.5;
        if (isAFirst) {
            first = playerA;
        } else {
            first = playerB;
        }
    }

    //设置next
    public synchronized void setNextStepA(Cell nextStepA) {
        this.nextStepA = nextStepA;
    }

    public synchronized void setNextStepB(Cell nextStepB) {
        this.nextStepB = nextStepB;
    }

    /**
     * 局面转为字符串，用于发送给大模型
     *
     * @return 由012组成的字符串，表示棋盘状态，每行用换行符分隔
     */
    private String getStringMap() {
        StringBuilder sb = new StringBuilder();
        for (List<CellRoleEnum> row : map) {
            for (CellRoleEnum cell : row) {
                sb.append(cell.getCode()); // 使用枚举的code值
            }
            sb.append("\n"); // 每行结束后添加换行符
        }
        return sb.toString();
    }

    private void sendLMMCode(GamePlayer player) {
        if (!player.getPlayerType().equals(SpecialPlayerEnum.LMM)) {
            return;//亲自出马
        }
        MultiValueMap<String, String> data = new LinkedMultiValueMap<>();
        data.add("input", getStringMap());
        data.add("LLMFlag", player == playerA ? CellRoleEnum.PLAYER_A.getCode() : CellRoleEnum.PLAYER_B.getCode());
        WebSocketServer.restTemplate.postForObject(addBotUrl, data, String.class);
    }

    private boolean nextStep() {
        invalidStepPlayer = null;
        boolean askA = (round % 2) == 0;
        if (askA) {
            sendLMMCode(playerA);
        } else {
            sendLMMCode(playerB);
        }

        for (int i = 0; i < 500; i++) {//最大等待时间50秒
            try {
                Thread.sleep(100);
                lock.lock();
                try {
                    if (askA && nextStepA != null) {
                        if (map.get(nextStepA.getX()).get(nextStepA.getY()) != CellRoleEnum.EMPTY) {
                            invalidStepPlayer = playerA;
                            return false;
                        }
                        map.get(nextStepA.getY()).set(nextStepA.getY(), CellRoleEnum.PLAYER_A);
                        playerA.getSteps().add(nextStepA);
                    } else if (!askA && nextStepB != null) {
                        if (map.get(nextStepB.getX()).get(nextStepB.getY()) != CellRoleEnum.EMPTY) {
                            invalidStepPlayer = playerB;
                            return false;
                        }
                        map.get(nextStepB.getY()).set(nextStepB.getY(), CellRoleEnum.PLAYER_B);
                        playerB.getSteps().add(nextStepB);

                    }
                } finally {
                    lock.unlock();
                }
            } catch (InterruptedException e) {
                e.printStackTrace();
            }
        }
        return false;
    }

    private void judge() {//判断操作后局面
        //如果有人进行非法操作，直接判断负
        if (invalidStepPlayer != null) {
            status = invalidStepPlayer == playerA ? GameStatusEnum.PLAYER_B_WIN : GameStatusEnum.PLAYER_A_WIN;
        }
        status = gameStrategy.checkGameOver(map);
    }

    private void sendAllMessage(String x) {
        //一方断开链接则无视其操作
        if (WebSocketServer.users.get(playerA.getUserId()) != null) {
            WebSocketServer.users.get(playerA.getUserId()).sendMessage(x);
        }
        if (WebSocketServer.users.get(playerB.getUserId()) != null) {
            WebSocketServer.users.get(playerB.getUserId()).sendMessage(x);
        }
    }

    private void sendMove() {//广播操作
        lock.lock();
        try {
            FeedBackObj data = new FeedBackObj(FeedBackEventTypeEnum.MOVE.getType(), x, y,
                    GameStatusEnum.UNFINISHED.getName());
            sendAllMessage(JSONObject.toJSONString(data));
            nextStepA = nextStepB = null;
        } finally {
            lock.unlock();
        }
    }


    private void updateUserRating(GamePlayer player, Long rating) {
        Player user = WebSocketServer.playerMapper.selectById(player.getUserId());
        user.setRaking(rating);
        WebSocketServer.playerMapper.updateById(user);
    }

    private JSONObject saveToDataBase() {
        JSONObject item = new JSONObject();
        //过滤步数小于对应步数次的对局
        if (round < gameStrategy.getMinRecordStepCnt()) {
            item.put("msg", false);
            return item;
        }

        Player userA = WebSocketServer.playerMapper.selectById(playerA.getUserId());
        Player userB = WebSocketServer.playerMapper.selectById(playerB.getUserId());
        Long ratingA = userA.getRaking();
        Long ratingB = userB.getRaking();

        if (status.equals(GameStatusEnum.PLAYER_B_WIN)) {
            ratingA -= 4;
            ratingB += 5;
        } else if (status.equals(GameStatusEnum.PLAYER_A_WIN)) {
            ratingB -= 4;
        }
        updateUserRating(playerA, ratingA);
        updateUserRating(playerB, ratingB);
        Record record = new Record();
        WebSocketServer.recordMapper.insert(record);
//        String data=JSON.toJSONString(record);
        item.put("msg", true);
        item.put("a_photo", userA.getPhoto());
        item.put("a_username", userA.getUsername());
        item.put("b_photo", userB.getPhoto());
        item.put("b_username", userB.getUsername());
        item.put("record", record);
        String result = "平局";
        if ("a".equals(record.getLoser())) {
            result = "B胜";
        } else if ("b".equals(record.getLoser())) {
            result = "A胜";
        }
        item.put("result", result);

        return item;
    }

    private void sendResult() {//广播结果
        FeedBackObj feedBackObj = new FeedBackObj();
        feedBackObj.setEvent(FeedBackEventTypeEnum.RESULT.getType());
        feedBackObj.setGameStatus(status.getName());
        sendAllMessage(JSONObject.toJSONString(feedBackObj));
    }

    @Override
    public void run() {
        //最多会有600步
        for (int i = 0; i < 1000; i++) {
            if (!nextStep()) {//游戏终止
                lock.lock();
                try {
                    if (nextStepA == null && nextStepB == null) {
                        status = GameStatusEnum.DRAW;
                    } else if (nextStepA == null) {
                        status = GameStatusEnum.PLAYER_B_WIN;
                    } else if (nextStepB == null) {
                        status = GameStatusEnum.PLAYER_A_WIN;
                    }
                } finally {
                    lock.unlock();
                }
                //广播结果
                sendResult();
                break;
            } else {
                judge();
                if (status.equals(GameStatusEnum.UNFINISHED)) {
                    sendMove();
                } else {
                    sendResult();
                    break;
                }
            }
        }
    }
}
