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
import com.gamehive.constants.GameModeEnum;
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
        data.add("historySteps", buildHistorySteps());
        data.add("gridSize", gameType.getBoardSize().toString());
        System.out.println("尝试像LMM运行服务发送信息" + data);
        WebSocketServer.restTemplate.postForObject(addBotUrl, data, String.class);
    }

    /**
     * 游戏推进到下一步，如果没在规定时间内输入或输入非法则返回false
     */
    private boolean nextStep() {
        boolean askA = isNextA();
        //最大等待时间60秒
        int waitTime = 600;
        //如果LMM输入则则发送信息
        if (!askA && forLMM) {
            sendLMMCode(playerB);
            //对于大模型放宽时间限制到600秒
            waitTime = 6000;
        }
        for (int i = 0; i < waitTime; i++) {
            try {
                Thread.sleep(100);
                lock.lock();
                try {
                    //检查A玩家是否有输入
                    if (askA && nextStepA != null) {
                        if (map.get(nextStepA.getX()).get(nextStepA.getY()) != CellRoleEnum.EMPTY) {
                            //非法输入不记录，直接判负
                            return false;
                        }
                        map.get(nextStepA.getX()).set(nextStepA.getY(), CellRoleEnum.PLAYER_A);
                        playerA.getSteps().add(nextStepA);
                        return true;
                    }//检查B玩家是否有输入
                    else if (!askA && nextStepB != null) {
                        if (map.get(nextStepB.getX()).get(nextStepB.getY()) != CellRoleEnum.EMPTY) {
                            return false;
                        }
                        map.get(nextStepB.getX()).set(nextStepB.getY(), CellRoleEnum.PLAYER_B);
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


    @Override
    public void run() {
        //步数收到棋盘大小限制
        for (int i = 0; i < 1000; i++) {
            //内部回等待50秒，超时视为失败
            if (!nextStep()) {
                //没有输入或输入非法
                lock.lock();
                try {
                    boolean askA = isNextA();
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
                //先广播这一步操作
                System.out.println(isNextA() ? "玩家A" : "玩家B" + "在" + (isNextA() ? nextStepA : nextStepB) + "下棋了");
                sendMove(isNextA() ? nextStepA : nextStepB);

                //然后判断游戏是否结束
                judge();
                if (!status.equals(GameStatusEnum.UNFINISHED)) {
                    //游戏结束，广播结果
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
            data.setGameStatus(status.getName()); // 使用当前游戏状态
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
        sendAllMessage(JSONObject.toJSONString(result));
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
        Player userA = WebSocketServer.playerMapper.selectPlayerByUserId(playerA.getUserId());
        Player userB;
        Long ratingA = userA.getRaking();
        Long ratingB = 0L;

        if (forLMM) {
            // 与LMM对战时，创建虚拟的LMM玩家信息
            userB = new Player();
            userB.setUserId((long) SpecialPlayerEnum.LMM.getCode());
            userB.setUserName(SpecialPlayerEnum.LMM.getChineseName());
            userB.setRaking(0L);
            ratingB = 0L;
        } else {
            // 正常联机对战
            userB = WebSocketServer.playerMapper.selectPlayerByUserId(playerB.getUserId());
            ratingB = userB.getRaking();
        }

        //更新积分
        if (status.equals(GameStatusEnum.PLAYER_B_WIN)) {
            ratingA -= 4;
            if (!forLMM) {
                ratingB += 5;
            }
        } else if (status.equals(GameStatusEnum.PLAYER_A_WIN)) {
            if (!forLMM) {
                ratingB -= 4;
            }
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
        // 设置游戏模式
        record.setGameMode(forLMM ? GameModeEnum.LMM_GAME.getCode() : GameModeEnum.ONLINE_GAME.getCode());
        //先后手
        record.setFirstPlayerId(playerA.getUserId());
        record.setFirstPlayerName(playerA.getUserName());
        record.setSecondPlayerId(userB.getUserId());
        record.setSecondPlayerName(userB.getUserName());
        //操作序列
        record.setFirstPlayerPieces(JSONObject.toJSONString(playerA.getSteps()));
        record.setSecondPlayerPieces(JSONObject.toJSONString(playerB.getSteps()));
        WebSocketServer.recordMapper.insertRecord(record);
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
        System.out.println("向A发消息：" + x);
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
        Player user = WebSocketServer.playerMapper.selectPlayerByUserId(player.getUserId());
        user.setRaking(rating);
        WebSocketServer.playerMapper.updatePlayer(user);
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

    private boolean isNextA() {
        return ((round + (first == playerA ? 0 : 1)) % 2) == 0;
    }

    /**
     * 构造玩家历史操作信息
     * 按照时间顺序生成格式化的历史步骤字符串
     *
     * @return 格式化的历史步骤字符串，如："step1:玩家在(1,1)下棋\n step2:AI在(2,2)下棋"
     */
    private String buildHistorySteps() {
        if (round == 0) {
            return "游戏刚开始，暂无历史步骤";
        }

        StringBuilder history = new StringBuilder();
        List<Cell> stepsA = playerA.getSteps();
        List<Cell> stepsB = playerB.getSteps();

        // 确定先手玩家
        boolean isAFirst = (first == playerA);

        // 按照实际下棋顺序重建历史
        int stepCountA = 0; // 玩家A已下的步数
        int stepCountB = 0; // 玩家B已下的步数

        // 遍历所有已完成的回合
        for (int stepNumber = 1; stepNumber <= round; stepNumber++) {
            // 判断当前步骤是哪个玩家下的
            boolean isAStep;
            if (isAFirst) {
                // A先手：奇数步是A，偶数步是B
                isAStep = (stepNumber % 2 == 1);
            } else {
                // B先手：奇数步是B，偶数步是A
                isAStep = (stepNumber % 2 == 0);
            }

            if (isAStep && stepCountA < stepsA.size()) {
                // 玩家A的步骤
                Cell step = stepsA.get(stepCountA);
                String playerName = forLMM ? "玩家" : "玩家A";
                history.append("step").append(stepNumber).append(":")
                        .append(playerName).append("在(")
                        .append(step.getX()).append(",").append(step.getY())
                        .append(")下棋");
                stepCountA++;
            } else if (!isAStep && stepCountB < stepsB.size()) {
                // 玩家B的步骤
                Cell step = stepsB.get(stepCountB);
                String playerName = forLMM ? "AI" : "玩家B";
                history.append("step").append(stepNumber).append(":")
                        .append(playerName).append("在(")
                        .append(step.getX()).append(",").append(step.getY())
                        .append(")下棋");
                stepCountB++;
            }

            // 如果不是最后一步，添加换行符
            if (stepNumber < round) {
                history.append("\n");
            }
        }

        return history.toString();
    }
}
