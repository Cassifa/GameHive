package com.gamehive.comsumer.Game;

import com.alibaba.fastjson2.JSON;
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
import java.util.Date;
import java.util.List;
import java.util.concurrent.locks.ReentrantLock;
import lombok.Getter;
import org.springframework.util.LinkedMultiValueMap;
import org.springframework.util.MultiValueMap;

@Getter
public class Game extends Thread {
    private  Integer rows, cols;
    //落子次数
    private  Integer round=0;
    private List<List<CellRoleEnum>> map;
    private final GamePlayer playerA, playerB;
    private Cell nextStepA = null, nextStepB = null;
    private GameStatusEnum status = GameStatusEnum.UNFINISHED;
    private final ReentrantLock lock = new ReentrantLock();
    private GameStrategy gameStrategy;
    private final static String addBotUrl =
            "http://127.0.0.1:3002/bot/add/";

    public Game(Long aId, SpecialPlayerEnum playerAType,SpecialPlayerEnum playerBType, Integer bId, GameTypeEnum gameTypeEnum
    ) {
        switch (gameTypeEnum){
            case GOBANG:
            case GOBANG_88:
                gameStrategy=new GoBangStrategy();
                break;
            case ANTI_GO:
                gameStrategy=new AntiGoStrategy();
                break;
            case TIC_TAC_TOE:
                gameStrategy=new TicTocToeStrategy();
                break;
            case MISERE_TIC_TAC_TOE:
                gameStrategy=new MisereTicTacToeStrategy();
                break;
        }
        gameStrategy.initGameMap(rows,cols,map);

        playerA = new GamePlayer(aId,playerAType,
                new ArrayList<>());
        playerB = new GamePlayer(bId,playerBType,
                new ArrayList<>());
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
     */
    private String getStringMap( ) {

    }

    private void sendLMMCode(GamePlayer player) {
        if (player.getBotId().equals(-1)) return;//亲自出马
        MultiValueMap<String, String> data = new LinkedMultiValueMap<>();
        data.add("user_id", player.getId().toString());
        data.add("bot_code", player.getBotCode());
        data.add("input", getStringMap());
        WebSocketServer.restTemplate.postForObject(addBotUrl, data, String.class);
    }

    private boolean nextStep() {//等待下次操作
        try {//等待前端动画
            Thread.sleep(333);
        } catch (InterruptedException e) {
            throw new RuntimeException(e);
        }

        sendLMMCode(playerA);
        sendLMMCode(playerB);

        for (int i = 0; i < 50; i++) {//最大等待时间5秒
            try {
                Thread.sleep(100);
                lock.lock();
                try {
                    if (nextStepA != null && nextStepB != null) {
                        playerA.getSteps().add(nextStepA);
                        playerB.getSteps().add(nextStepB);
                        return true;
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
        status=gameStrategy.checkGameOver(map);
    }

    private void sendAllMessage(String x) {
        //一方断开链接则无视其操作
        if (WebSocketServer.users.get(playerA.getUserId()) != null)
            WebSocketServer.users.get(playerA.getUserId()).sendMessage(x);
        if (WebSocketServer.users.get(playerB.getUserId()) != null)
            WebSocketServer.users.get(playerB.getUserId()).sendMessage(x);
    }

    private void sendMove() {//广播操作
        lock.lock();
        try {
            FeedBackObj data=new FeedBackObj(FeedBackEventTypeEnum.MOVE.getType(), x,y,GameStatusEnum.UNFINISHED.getName());
            sendAllMessage(JSONObject.toJSONString(data));
            nextStepA = nextStepB = null;
        } finally {
            lock.unlock();
        }
    }

    private String getMapString() {//获取字符串形地图
        StringBuilder ans = new StringBuilder();
        for (int i = 0; i < rows; i++)
            for (int j = 0; j < cols; j++)
                ans.append(g[i][j]);
        return ans.toString();
    }

    private void updateUserRating(GamePlayer player, Long rating) {
        Player user = WebSocketServer.playerMapper.selectById(player.getUserId());
        user.setRaking(rating);
        WebSocketServer.playerMapper.updateById(user);
    }

    private JSONObject saveToDataBase() {

        JSONObject item = new JSONObject();
        //过滤步数小于对应步数次的对局
        if (playerA.getStepsString().length() < gameStrategy.getMinRecordStepCnt()) {
            item.put("msg", false);
            return item;
        }

        Player userA = WebSocketServer.playerMapper.selectById(playerA.getId());
        Player userB = WebSocketServer.playerMapper.selectById(playerB.getId());
        Integer ratingA = userA.getRating();
        Integer ratingB = userB.getRating();

        if ("a".equals(loser)) {
            ratingA -= 4;
            ratingB += 5;
        } else if ("b".equals(loser)) {
            ratingA += 5;
            ratingB -= 4;
        }
        updateUserRating(playerA, ratingA);
        updateUserRating(playerB, ratingB);
        Record record = new Record(
                null,
                playerA.getId(),
                playerA.getSx(),
                playerA.getSy(),
                playerB.getId(),
                playerB.getSx(),
                playerB.getSy(),
                playerA.getStepsString(),
                playerB.getStepsString(),
                getMapString(),
                loser,
                new Date()
        );
        WebSocketServer.recordMapper.insert(record);
//        String data=JSON.toJSONString(record);
        item.put("msg",true);
        item.put("a_photo", userA.getPhoto());
        item.put("a_username", userA.getUsername());
        item.put("b_photo", userB.getPhoto());
        item.put("b_username", userB.getUsername());
        item.put("record", record);
        String result = "平局";
        if ("a".equals(record.getLoser())) result = "B胜";
        else if ("b".equals(record.getLoser())) result = "A胜";
        item.put("result", result);

        return item;
    }

    private void sendResult() {//广播结果
        JSONObject resp = new JSONObject();
        JSONObject record = saveToDataBase();
        resp.put("event", "result");
        resp.put("loser", loser);
        resp.put("recordItem", record);
        sendAllMessage(JSON.toJSONString(resp));
    }

    @Override
    public void run() {
        //最多会有600步
        for (int i = 0; i < 1000; i++) {
            if (!nextStep()) {//游戏终止
                lock.lock();
                try {
                    if (nextStepA == null && nextStepB == null) {
                        status=GameStatusEnum.DRAW;
                    } else if (nextStepA == null) {
                        status=GameStatusEnum.PLAYER_B_WIN;
                    } else if (nextStepB == null) {
                        status=GameStatusEnum.PLAYER_A_WIN;
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
