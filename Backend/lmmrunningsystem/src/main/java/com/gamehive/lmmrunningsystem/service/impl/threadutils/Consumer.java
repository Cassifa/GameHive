package com.gamehive.lmmrunningsystem.service.impl.threadutils;

import com.gamehive.lmmrunningsystem.dto.LMMDecisionResult;
import com.gamehive.lmmrunningsystem.dto.LMMRequestDTO;
import com.gamehive.lmmrunningsystem.service.impl.DeepSeekAIServiceImpl;
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

    private static RestTemplate restTemplate;
    private static DeepSeekAIServiceImpl deepSeekAIService;
    private final static String receiveBotMoveUrl =
            "http://127.0.0.1:3000/api/pk/receive/LMM/move/";

    @Autowired
    public void setRestTemplate(RestTemplate restTemplate) {
        Consumer.restTemplate = restTemplate;
    }

    @Autowired
    public void setDeepSeekAIService(DeepSeekAIServiceImpl deepSeekAIService) {
        Consumer.deepSeekAIService = deepSeekAIService;
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
        } catch (InterruptedException e) {
            log.warn("等待线程超时被中断", e);
        }
        this.interrupt();//终端当前线程
    }

    @Override
    public void run() {
        try {
            log.info("开始处理大模型决策请求 (游戏ID: {})", lmmRequest.getGameId());

            // 使用DeepSeek AI服务获取决策，传递gameId用于对话记忆
            LMMDecisionResult decision = deepSeekAIService.getDecision(lmmRequest, lmmRequest.getGameId());

            // 发送移动结果
            MultiValueMap<String, String> data = new LinkedMultiValueMap<>();
            data.add("user_id", lmmRequest.getUserId().toString());
            data.add("x", String.valueOf(decision.getX()));
            data.add("y", String.valueOf(decision.getY()));
            data.add("model_name", "deepseek");
            data.add("reason", decision.getReason());

            log.info("准备发送LMM移动结果到: {}", receiveBotMoveUrl);
            log.debug("请求参数: {}", data);

            try {
                String response = restTemplate.postForObject(receiveBotMoveUrl, data, String.class);
                log.info("LMM移动请求响应: {}", response);
            } catch (Exception e) {
                log.error("发送LMM移动请求失败: {}", e.getMessage(), e);
            }
        } catch (Exception e) {
            log.error("Consumer执行过程中发生错误: {}", e.getMessage(), e);
        }
    }
}
