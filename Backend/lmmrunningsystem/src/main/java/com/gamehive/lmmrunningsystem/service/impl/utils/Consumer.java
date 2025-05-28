package com.gamehive.lmmrunningsystem.service.impl.utils;

import com.gamehive.lmmrunningsystem.service.impl.DeepSeekAIService;
import com.gamehive.lmmrunningsystem.dto.LMMDecisionResult;
import com.gamehive.lmmrunningsystem.dto.LMMRequest;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import org.springframework.util.LinkedMultiValueMap;
import org.springframework.util.MultiValueMap;
import org.springframework.web.client.RestTemplate;

/**
 * 大模型决策消费者线程类
 * 负责处理大模型决策请求，调用AI服务获取决策结果并发送到游戏服务器
 * 继承Thread类，支持超时控制和异步处理
 * 
 * @author Cassifa
 * @since 1.0.0
 */
@Component
public class Consumer extends Thread {

    private LMMRequest lmmRequest;

    private static RestTemplate restTemplate;
    private static DeepSeekAIService deepSeekAIService;
    private final static String receiveBotMoveUrl =
            "http://127.0.0.1:3000/api/pk/receive/LMM/move/";

    @Autowired
    public void setRestTemplate(RestTemplate restTemplate) {
        Consumer.restTemplate = restTemplate;
    }

    @Autowired
    public void setDeepSeekAIService(DeepSeekAIService deepSeekAIService) {
        Consumer.deepSeekAIService = deepSeekAIService;
    }

    /**
     * 创建新线程处理决策请求，并在指定时间后强制中断
     * 
     * @param timeout 超时时间（毫秒），超过此时间将中断线程
     * @param lmmRequest 大模型请求对象，包含游戏状态和配置信息
     */
    public void startTimeout(Long timeout, LMMRequest lmmRequest) {
        this.lmmRequest = lmmRequest;
        this.start();
        try {
            this.join(timeout);//等最多timeout
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
        this.interrupt();//终端当前线程
    }

    @Override
    public void run() {
        try {
            System.out.println("开始处理大模型决策请求...");
            
            // 使用DeepSeek AI服务获取决策
            LMMDecisionResult decision = deepSeekAIService.getDecision(lmmRequest);
            
            // 发送移动结果
            MultiValueMap<String, String> data = new LinkedMultiValueMap<>();
            data.add("user_id", lmmRequest.getUserId().toString());
            data.add("x", String.valueOf(decision.getX()));
            data.add("y", String.valueOf(decision.getY()));
            data.add("model_name", "deepseek");
            data.add("reason", decision.getReason());

            System.out.println("准备发送LMM移动结果到: " + receiveBotMoveUrl);
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
