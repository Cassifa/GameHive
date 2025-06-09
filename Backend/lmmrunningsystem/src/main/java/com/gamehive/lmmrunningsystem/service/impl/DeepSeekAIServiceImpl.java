package com.gamehive.lmmrunningsystem.service.impl;

import com.gamehive.lmmrunningsystem.constants.ValidationResultEnum;
import com.gamehive.lmmrunningsystem.dto.LMMDecisionResult;
import com.gamehive.lmmrunningsystem.dto.LMMRequestDTO;
import com.gamehive.lmmrunningsystem.utils.PromptTemplateBuilder;
import lombok.extern.slf4j.Slf4j;
import org.springframework.ai.chat.client.ChatClient;
import org.springframework.ai.chat.client.advisor.AbstractChatMemoryAdvisor;
import org.springframework.ai.chat.memory.ChatMemory;
import org.springframework.ai.chat.messages.AssistantMessage;
import org.springframework.beans.factory.annotation.Qualifier;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

/**
 * DeepSeek AI服务实现
 * 负责与DeepSeek大模型进行交互，获取游戏决策结果
 * 包含智能重试机制、结果验证功能和对话记忆功能
 *
 * @author Cassifa
 * @since 1.0.0
 */
@Service
@Slf4j
public class DeepSeekAIServiceImpl {

    // Spring AI ChatClient实例，用于与大模型通信
    private final ChatClient chatClient;

    // 聊天记忆实例，用于存储对话历史
    private final ChatMemory chatMemory;

    // 最大重试次数配置
    @Value("${lmm.max-retry-count:3}")
    private int maxRetryCount;

    /**
     * 构造函数，初始化ChatClient和ChatMemory
     *
     * @param gameDecisionChatClient Spring AI配置的游戏决策ChatClient实例
     * @param gameChatMemory         游戏对话记忆实例
     */
    public DeepSeekAIServiceImpl(@Qualifier("gameDecisionChatClient") ChatClient gameDecisionChatClient,
                                 @Qualifier("gameChatMemory") ChatMemory gameChatMemory) {
        this.chatClient = gameDecisionChatClient;
        this.chatMemory = gameChatMemory;
    }

    /**
     * 获取大模型决策的主要方法
     * 包含重试机制，确保返回有效的决策结果
     * 使用gameId作为对话ID来维护游戏会话的对话记忆
     * 集成决策记忆存储功能，将每次决策结果自动存储到对话历史中
     *
     * @param lmmRequest 大模型请求对象，包含游戏状态和配置信息
     * @param gameId     游戏唯一标识符，用作对话记忆的会话ID
     * @return LMMDecisionResult 大模型的决策结果，包含坐标和理由
     */
    public LMMDecisionResult getDecision(LMMRequestDTO lmmRequest, Integer gameId) {
        String conversationId = gameId.toString();
        String prompt = PromptTemplateBuilder.buildInitialPrompt(lmmRequest);
        LMMDecisionResult result = null;
        ValidationResultEnum validationResult = null;
        int retryCount = 0;

        while (retryCount < maxRetryCount) {
            try {
                result = chatClient.prompt()
                        .advisors(advisorSpec -> advisorSpec.param(AbstractChatMemoryAdvisor.CHAT_MEMORY_CONVERSATION_ID_KEY, conversationId))
                        .user(prompt)
                        .call()
                        .entity(LMMDecisionResult.class);

                log.info("大模型响应(第{}次, 游戏ID: {}): {}", retryCount + 1, gameId, result);

                //获取到相应
                if (result != null) {
                    //验证相应
                    validationResult = result.validate(lmmRequest);

                    if (validationResult == ValidationResultEnum.VALID) {
                        log.info("大模型决策成功(游戏ID: {}): x={}, y={}", gameId, result.getX(), result.getY());
                        break;
                    } else {
                        //计入记忆
                        storeDecisionMemory(lmmRequest, result, validationResult, conversationId);
                        retryCount++;
                        if (retryCount < maxRetryCount) {
                            prompt = PromptTemplateBuilder.buildRetryPrompt(lmmRequest, result);
                            log.warn("第{}次重试(游戏ID: {})，原因：{}", retryCount, gameId, validationResult.getDescription());
                        }
                    }
                } else {
                    retryCount++;
                    if (retryCount < maxRetryCount) {
                        prompt = PromptTemplateBuilder.buildRetryPrompt(lmmRequest, null);
                        log.warn("第{}次重试(游戏ID: {})，原因：结果解析失败", retryCount, gameId);
                    }
                }
            } catch (Exception e) {
                retryCount++;
                log.error("大模型调用失败(第{}次, 游戏ID: {}): {}", retryCount, gameId, e.getMessage(), e);
                if (retryCount < maxRetryCount) {
                    prompt = PromptTemplateBuilder.buildRetryPrompt(lmmRequest, null);
                }
            }
        }

        if (result == null || !result.isValid(lmmRequest)) {
            log.warn("大模型决策失败(游戏ID: {})，使用默认策略", gameId);
            result = getDefaultDecision(lmmRequest);
            validationResult = ValidationResultEnum.VALID;
        }

        storeDecisionMemory(lmmRequest, result, validationResult, conversationId);

        return result;
    }

    /**
     * 将决策结果存储到对话记忆中
     * 包括局面状态、AI决策、决策理由和验证结果
     *
     * @param lmmRequest       大模型请求对象
     * @param decision         决策结果
     * @param validationResult 验证结果
     * @param conversationId   对话ID
     */
    private void storeDecisionMemory(LMMRequestDTO lmmRequest,
                                     LMMDecisionResult decision,
                                     ValidationResultEnum validationResult,
                                     String conversationId) {
        try {
            String memoryText = PromptTemplateBuilder.buildDecisionMemory(lmmRequest, decision, validationResult);
            AssistantMessage memoryMessage = new AssistantMessage(memoryText);
            chatMemory.add(conversationId, memoryMessage);
            log.debug("决策记忆已存储(游戏ID: {}): {}", conversationId, memoryText);
        } catch (Exception e) {
            log.error("存储决策记忆失败 (游戏ID: {}): {}", conversationId, e.getMessage(), e);
        }
    }

    /**
     * 获取默认决策结果
     * 当大模型决策失败时，使用简单策略选择第一个可用位置
     *
     * @param lmmRequest 大模型请求对象
     * @return LMMDecisionResult 默认的决策结果
     */
    private LMMDecisionResult getDefaultDecision(LMMRequestDTO lmmRequest) {
        String[] rows = lmmRequest.getCurrentMap().split("\n");
        int gridSize = Integer.parseInt(lmmRequest.getGridSize());

        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                if (i < rows.length && j < rows[i].length() && rows[i].charAt(j) == '0') {
                    return new LMMDecisionResult(i, j, "默认策略：选择第一个可用位置");
                }
            }
        }

        return new LMMDecisionResult(0, 0, "默认策略：无可用位置，选择(0,0)");
    }
} 
