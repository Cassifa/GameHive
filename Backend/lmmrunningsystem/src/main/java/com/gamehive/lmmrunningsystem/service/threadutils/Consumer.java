package com.gamehive.lmmrunningsystem.service.threadutils;

import com.gamehive.lmmrunningsystem.config.ApiConfig;
import com.gamehive.lmmrunningsystem.dto.LMMDecisionResult;
import com.gamehive.lmmrunningsystem.dto.LMMRequestDTO;
import com.gamehive.lmmrunningsystem.service.agent.DeepSeekAIServiceImpl;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import org.springframework.util.LinkedMultiValueMap;
import org.springframework.util.MultiValueMap;
import org.springframework.web.client.RestTemplate;

/**
 * 大模型决策消费者线程类
 * 负责处理大模型决策请求，调用AI服务获取决策结果并发送到游戏服务
 *
 * @author Cassifa
 * @since 1.0.0
 */
@Component
@Slf4j
public class Consumer extends Thread {

    private LMMRequestDTO lmmRequest;
    private volatile boolean isCompleted = false;

    private static RestTemplate restTemplate;
    private static DeepSeekAIServiceImpl deepSeekAIService;
    private static ApiConfig apiConfig;

    @Autowired
    public void setRestTemplate(RestTemplate restTemplate) {
        Consumer.restTemplate = restTemplate;
    }

    @Autowired
    public void setDeepSeekAIService(DeepSeekAIServiceImpl deepSeekAIService) {
        Consumer.deepSeekAIService = deepSeekAIService;
    }

    @Autowired
    public void setApiConfig(ApiConfig apiConfig) {
        Consumer.apiConfig = apiConfig;
    }

    /**
     * 构造函数，通过参数注入依赖
     */
    public Consumer() {
    }

    /**
     * 创建新线程处理决策请求，并在指定时间后强制中断
     *
     * @param timeout    超时时间（毫秒）
     * @param lmmRequest 大模型请求对象
     */
    public void startTimeout(Long timeout, LMMRequestDTO lmmRequest) {
        this.lmmRequest = lmmRequest;
        this.start();
        try {
            this.join(timeout);//等最多timeout
            if (!isCompleted) {
                log.warn("大模型决策请求超时，返回随机移动 (游戏ID: {}, 超时时间: {}ms)",
                        lmmRequest.getGameId(), timeout);
                sendRandomMove();
            }
        } catch (InterruptedException e) {
            log.warn("等待线程超时被中断", e);
            sendRandomMove();
        }
        this.interrupt();//终端当前线程
    }

    @Override
    public void run() {
        try {
            log.info("开始处理大模型决策请求 (游戏ID: {})", lmmRequest.getGameId());

            // 使用DeepSeek AI服务获取决策
            LMMDecisionResult decision = deepSeekAIService.getDecision(lmmRequest);

            // 检查是否已经被中断
            if (Thread.currentThread().isInterrupted()) {
                log.warn("决策请求被中断，不发送结果 (游戏ID: {})", lmmRequest.getGameId());
                return;
            }

            // 使用统一的发送方法
            sendMoveResult(decision, "LMM移动");
            isCompleted = true;

        } catch (Exception e) {
            log.error("Consumer执行过程中发生错误: {}", e.getMessage(), e);
        }
    }

    /**
     * 发送移动结果的通用方法
     *
     * @param decision 决策结果
     * @param moveType 移动类型（用于日志）
     */
    private void sendMoveResult(LMMDecisionResult decision, String moveType) {
        MultiValueMap<String, String> data = new LinkedMultiValueMap<>();
        data.add("user_id", lmmRequest.getUserId().toString());
        data.add("x", String.valueOf(decision.getX()));
        data.add("y", String.valueOf(decision.getY()));
        data.add("model_name", apiConfig.getModelName());
        data.add("reason", decision.getReason());

        log.info("发送{}结果: x={}, y={}, 理由: {}", moveType, decision.getX(), decision.getY(), decision.getReason());

        try {
            String response = restTemplate.postForObject(apiConfig.getReceiveBotMoveUrl(), data, String.class);
            log.info("{}请求响应: {}", moveType, response);
        } catch (Exception e) {
            log.error("发送{}请求失败: {}", moveType, e.getMessage(), e);
        }
    }


    /**
     * 发送随机移动
     */
    private void sendRandomMove() {
        try {
            LMMDecisionResult randomDecision = getRandomDecision();
            sendMoveResult(randomDecision, "随机移动");
        } catch (Exception e) {
            log.error("生成随机移动失败: {}", e.getMessage(), e);
        }
    }

    /**
     * 生成随机决策
     */
    private LMMDecisionResult getRandomDecision() {
        String[] rows = lmmRequest.getCurrentMap().split("\n");
        int gridSize = Integer.parseInt(lmmRequest.getGridSize());

        // 收集所有空位
        java.util.List<int[]> emptyPositions = new java.util.ArrayList<>();
        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                if (i < rows.length && j < rows[i].length() && rows[i].charAt(j) == '0') {
                    emptyPositions.add(new int[]{i, j});
                }
            }
        }

        if (!emptyPositions.isEmpty()) {
            // 随机选择一个空位
            int randomIndex = (int) (Math.random() * emptyPositions.size());
            int[] position = emptyPositions.get(randomIndex);
            return new LMMDecisionResult(position[0], position[1], "超时随机移动");
        } else {
            // 如果没有空位，返回(0,0)
            return new LMMDecisionResult(0, 0, "超时随机移动：无可用位置");
        }
    }
}
