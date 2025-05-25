package com.gamehive.lmmrunningsystem.service.impl.utils;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import org.springframework.util.LinkedMultiValueMap;
import org.springframework.util.MultiValueMap;
import org.springframework.web.client.RestTemplate;

import java.util.ArrayList;
import java.util.List;
import java.util.Random;

@SuppressWarnings("all")
@Component
public class Consumer extends Thread {
    private Bot bot;
    private static RestTemplate restTemplate;
    private final static String receiveBotMoveUrl =
            "http://127.0.0.1:3000/api/pk/receive/LMM/move/";

    @Autowired
    public void setRestTemplate(RestTemplate restTemplate) {
        Consumer.restTemplate = restTemplate;
    }

    public void startTimeout(Long timeout, Bot bot) {
        this.bot = bot;
        this.start();
        try {
            this.join(timeout);//等最多timeout
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
        this.interrupt();//终端当前线程
    }

    private String addUid(String code, String uuid) {
        int k = code.indexOf(" implements java.util.function.Supplier<Integer>");
        return code.substring(0, k) + uuid + code.substring(k);
    }

    @Override
    public void run() {
        try {
            // 解析棋盘字符串
            String[] rows = bot.getCurrentMap().split("\n");
            int gridSize = Integer.parseInt(bot.getGridSize());
            List<Integer> emptyPositions = new ArrayList<>();

            // 遍历棋盘找到所有空位置
            for (int i = 0; i < gridSize; i++) {
                for (int j = 0; j < gridSize; j++) {
                    if (rows[i].charAt(j) == '0') {
                        // 将二维坐标转换为一维索引
                        emptyPositions.add(i * gridSize + j);
                    }
                }
            }

            // 随机选择一个空位置
            Random random = new Random();
            int randomIndex = random.nextInt(emptyPositions.size());
            int position = emptyPositions.get(randomIndex);

            // 将一维索引转换回二维坐标
            int x = position / gridSize;
            int y = position % gridSize;

            // 发送移动结果
            MultiValueMap<String, String> data = new LinkedMultiValueMap<>();
            data.add("user_id", bot.getUserId().toString());
            data.add("x", String.valueOf(x));
            data.add("y", String.valueOf(y));
            data.add("model_name", "deepseek");
            data.add("reason", "测试");

            System.out.println("准备发送LMM移动请求到: " + receiveBotMoveUrl);
            System.out.println("请求参数: " + data);

            try {
                String response = restTemplate.postForObject(receiveBotMoveUrl, data, String.class);
                System.out.println("LMM移动请求响应: " + response);
            } catch (Exception e) {
                System.out.println("发送LMM移动请求失败: " + e.getMessage());
                e.printStackTrace();
            }
        } catch (Exception e) {
            System.out.println("Consumer执行过程中发生错误: " + e.getMessage());
            e.printStackTrace();
        }
    }
}
