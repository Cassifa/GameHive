package com.gamehive.matchingsystem.service.impl.utils;

import com.gamehive.matchingsystem.constants.GameTypeEnum;
import java.util.ArrayList;
import java.util.EnumMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.locks.ReentrantLock;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import org.springframework.util.LinkedMultiValueMap;
import org.springframework.util.MultiValueMap;
import org.springframework.web.client.RestTemplate;

@Component
public class MatchingPool extends Thread {

    // 按游戏类型分组的匹配队列
    private static final Map<GameTypeEnum, List<Player>> gameTypeQueues = new EnumMap<>(GameTypeEnum.class);
    private final ReentrantLock lock = new ReentrantLock();

    public static RestTemplate restTemplate;

    //注入RestTemplate
    @Autowired
    public void setRestTemplate(RestTemplate restTemplate) {
        MatchingPool.restTemplate = restTemplate;
    }

    private final static String startGameUrl =
            "http://127.0.0.1:3000/api/pk/start/game/";

    /**
     * 添加玩家到匹配池
     *
     * @param userId 用户ID
     * @param rating 玩家评分
     * @param gameType 游戏类型枚举
     */
    /**
     * 添加玩家到对应游戏类型的匹配队列
     */
    public void addPlayer(Integer userId, Integer rating, GameTypeEnum gameType) {
        lock.lock();
        try {
            List<Player> queue = gameTypeQueues.computeIfAbsent(gameType, k -> new ArrayList<>());
            queue.add(new Player(userId, rating, 0, gameType));
        } finally {
            lock.unlock();
        }
    }

    /**
     * 从所有游戏队列中移除指定玩家
     */
    public void removePlayer(Integer userId) {
        lock.lock();
        try {
            for (List<Player> queue : gameTypeQueues.values()) {
                queue.removeIf(player -> player.getUserId().equals(userId));
            }
        } finally {
            lock.unlock();
        }
    }

    /**
     * 增加所有队列中玩家的等待时间
     */
    private void increaseWaitingTime() {
        for (List<Player> queue : gameTypeQueues.values()) {
            for (Player player : queue) {
                player.setWaitingTime(1 + player.getWaitingTime());
            }
        }
    }

    //两者可否匹配
    private boolean checkMatched(Player a, Player b) {
        int ratingDelta = Math.abs(a.getRating() - b.getRating());
        int waitingTime = Math.min(a.getWaitingTime(), b.getWaitingTime());
        return ratingDelta <= waitingTime * 10;
    }

    //返回匹配对
    private void sendResult(Player a, Player b) {
        MultiValueMap<String, String> data = new LinkedMultiValueMap<>();
        System.out.println(a + " 与 " + b + " 匹配了");
        data.add("a_id", a.getUserId().toString());
        data.add("b_id", b.getUserId().toString());//反射函数
        System.out.println(startGameUrl + data);
        restTemplate.postForObject(startGameUrl, data, String.class);
    }

    /**
     * 检查所有游戏类型的队列，尝试匹配玩家
     * 实现思路：
     * 1. 遍历所有游戏类型的匹配队列
     * 2. 对每个队列中的玩家进行两两匹配
     * 3. 将匹配成功的玩家从队列中标记并移除
     */
    private void matchPlayers() {
        //遍历所有游戏类型的队列
        for (Map.Entry<GameTypeEnum, List<Player>> entry : gameTypeQueues.entrySet()) {
            List<Player> queue = entry.getValue();
            //记录是否已被匹配
            boolean[] used = new boolean[queue.size()];

            //优先匹配等待时间长的玩家
            for (int i = 0; i < queue.size(); i++) {
                if (used[i]) {
                    continue;
                }
                //从当前玩家的下一个位置开始寻找匹配
                for (int j = i + 1; j < queue.size(); j++) {
                    if (used[j]) {
                        continue;
                    }
                    //检查两个玩家是否匹配
                    if (checkMatched(queue.get(i), queue.get(j))) {
                        used[i] = used[j] = true; //标记为已匹配
                        sendResult(queue.get(i), queue.get(j)); //发送匹配结果
                        break; //找到一个匹配后跳出内层循环
                    }
                }
            }

            // 移除已匹配的玩家，构建新队列
            List<Player> newQueue = new ArrayList<>();
            for (int i = 0; i < queue.size(); i++) {
                if (!used[i]) {
                    newQueue.add(queue.get(i)); //只保留未匹配的玩家
                }
            }
            //更新当前游戏类型的队列
            gameTypeQueues.put(entry.getKey(), newQueue);
        }
    }

    @Override
    public void run() {
        while (true) {
            try {
                lock.lock();
                try {
                    increaseWaitingTime();//增加当前匹配池所有人等待时间
                    matchPlayers();//尝试匹配
                } finally {
                    lock.unlock();
                }
                Thread.sleep(1000);
            } catch (InterruptedException e) {
                e.printStackTrace();
                break;
            }
        }
    }
}
