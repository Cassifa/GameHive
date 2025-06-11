package com.gamehive.lmmrunningsystem.service.agent.utils;

import com.alibaba.cloud.ai.dashscope.chat.DashScopeChatOptions;
import com.gamehive.lmmrunningsystem.constants.GameTypeEnum;
import com.gamehive.lmmrunningsystem.constants.ValidationResultEnum;
import com.gamehive.lmmrunningsystem.dto.LMMDecisionResult;
import com.gamehive.lmmrunningsystem.dto.LMMRequestDTO;
import com.gamehive.lmmrunningsystem.service.agent.SingleAgentService;
import com.gamehive.lmmrunningsystem.service.agent.factory.RAGChatClientFactory;
import lombok.extern.slf4j.Slf4j;
import org.springframework.ai.chat.client.ChatClient;
import org.springframework.ai.chat.memory.ChatMemory;
import org.springframework.ai.chat.messages.AssistantMessage;
import org.springframework.ai.vectorstore.SearchRequest;
import org.springframework.ai.vectorstore.VectorStore;

import static org.springframework.ai.chat.client.advisor.AbstractChatMemoryAdvisor.CHAT_MEMORY_CONVERSATION_ID_KEY;

/**
 * 通用游戏智能体实现
 * 继承DeepSeekAIServiceImpl，通过不同的ID实现内存隔离
 * 由AgentFactory创建和管理
 *
 * @author Cassifa
 * @since 1.0.0
 */
@Slf4j
public class GameAgent extends SingleAgentService {

    private final Integer id;
    private final double temperature;

    /**
     * 构造函数
     *
     * @param gameDecisionChatClient 游戏决策ChatClient
     * @param gameChatMemory         游戏聊天记忆
     * @param ragChatClientFactory   RAG ChatClient工厂
     * @param id                     代理ID，用于日志标识和内存隔离
     * @param temperature            温度参数，控制模型输出的随机性
     */
    public GameAgent(ChatClient gameDecisionChatClient,
                     ChatMemory gameChatMemory,
                     RAGChatClientFactory ragChatClientFactory,
                     Integer id,
                     double temperature) {
        super(gameDecisionChatClient, gameChatMemory, ragChatClientFactory);
        this.id = id;
        this.temperature = temperature;
    }

    /**
     * 重写getDecision方法，使用独立的conversationId确保内存隔离，并应用温度参数
     */
    @Override
    public LMMDecisionResult getDecision(LMMRequestDTO lmmRequest) {
        // 创建内存隔离的请求对象
        LMMRequestDTO isolatedRequest = createIsolatedRequest(lmmRequest);

        log.info("[Agent{}] 开始处理决策请求，游戏ID: {}, 原始游戏ID: {}, 温度: {}",
                id, isolatedRequest.getGameId(), lmmRequest.getGameId(), temperature);

        // 使用独立的决策逻辑，包含温度参数
        LMMDecisionResult result = getDecisionWithTemperature(isolatedRequest);

        log.info("[Agent{}] 决策完成，结果: ({}, {}), 理由: {}",
                id, result.getX(), result.getY(), result.getReason());

        return result;
    }

    /**
     * 使用温度参数进行决策的实现
     * 复制父类逻辑但应用自定义温度参数
     */
    private LMMDecisionResult getDecisionWithTemperature(LMMRequestDTO lmmRequest) {
        Integer gameId = lmmRequest.getGameId();
        String conversationId = gameId.toString();
        LMMDecisionResult result = null;
        ValidationResultEnum validationResult = null;
        int retryCount = 0;
        int maxRetryCount = 3; // 从父类复制

        //是否是新对话
        boolean isNewConversation;
        try {
            isNewConversation = chatMemory.get(conversationId, -1).isEmpty();
        } catch (Exception e) {
            isNewConversation = true;
        }

        // 构建提示词
        String systemPrompt = null;
        String userPrompt;

        if (isNewConversation) {
            //首次对话构造系统提示词
            systemPrompt = PromptTemplateBuilder.buildSystemPrompt(
                    lmmRequest.getGameType(),
                    lmmRequest.getGameRule(),
                    lmmRequest.getLLMFlag(),
                    lmmRequest.getGridSize());
        }
        //构造用户查询信息
        userPrompt = PromptTemplateBuilder.buildUserPrompt(
                lmmRequest.getCurrentMap(),
                lmmRequest.getHistorySteps());

        while (retryCount < maxRetryCount) {
            try {
                // 获取游戏专用的RAG ChatClient
                GameTypeEnum gameTypeEnum = GameTypeEnum.fromChineseName(lmmRequest.getGameType());
                ChatClient ragChatClient = ragChatClientFactory.getChatClient(gameTypeEnum);

                //测试向量存储是否工作
                testVectorStoreForGame(gameTypeEnum, userPrompt);

                // 创建带温度参数的ChatOptions
                DashScopeChatOptions chatOptions = DashScopeChatOptions.builder()
                        .withTemperature(temperature)
                        .build();

                if (systemPrompt != null) {
                    result = ragChatClient.prompt()
                            .system(systemPrompt)
                            .user(userPrompt)
                            .options(chatOptions)  // 应用温度参数
                            .advisors(a -> a.param(CHAT_MEMORY_CONVERSATION_ID_KEY, conversationId))
                            .call()
                            .entity(LMMDecisionResult.class);
                } else {
                    result = ragChatClient.prompt()
                            .user(userPrompt)
                            .options(chatOptions)  // 应用温度参数
                            .advisors(a -> a.param(CHAT_MEMORY_CONVERSATION_ID_KEY, conversationId))
                            .call()
                            .entity(LMMDecisionResult.class);
                }

                log.info("[Agent{}] 大模型响应(第{}次, 游戏ID: {}, 温度: {}): {}",
                        id, retryCount + 1, gameId, temperature, result);

                //获取到响应
                if (result != null) {
                    //验证响应
                    validationResult = result.validate(lmmRequest);

                    if (validationResult == ValidationResultEnum.VALID) {
                        log.info("[Agent{}] 大模型决策成功(游戏ID: {}): x={}, y={}",
                                id, gameId, result.getX(), result.getY());
                        break;
                    } else {
                        //存储失败的决策记忆
                        storeDecisionMemory(lmmRequest, result, validationResult, conversationId);
                        retryCount++;
                        if (retryCount < maxRetryCount) {
                            userPrompt = PromptTemplateBuilder.buildRetryPrompt(
                                    result,
                                    lmmRequest.getCurrentMap(),
                                    lmmRequest.getGridSize());
                            systemPrompt = null;
                            log.warn("[Agent{}] 第{}次重试(游戏ID: {})，原因：{}",
                                    id, retryCount, gameId, validationResult.getDescription());
                        }
                    }
                } else {
                    retryCount++;
                    if (retryCount < maxRetryCount) {
                        userPrompt = PromptTemplateBuilder.buildRetryPrompt(
                                null,
                                lmmRequest.getCurrentMap(),
                                lmmRequest.getGridSize());
                        systemPrompt = null;
                        log.warn("[Agent{}] 第{}次重试(游戏ID: {})，原因：结果解析失败", id, retryCount, gameId);
                    }
                }
            } catch (Exception e) {
                retryCount++;
                log.error("[Agent{}] 大模型调用失败(第{}次, 游戏ID: {}): {}", id, retryCount, gameId, e.getMessage(), e);
                if (retryCount < maxRetryCount) {
                    userPrompt = PromptTemplateBuilder.buildRetryPrompt(
                            null,
                            lmmRequest.getCurrentMap(),
                            lmmRequest.getGridSize());
                    systemPrompt = null;
                }
            }
        }

        if (result == null || !result.isValid(lmmRequest)) {
            log.warn("[Agent{}] 大模型决策失败(游戏ID: {})，使用默认策略", id, gameId);
            result = getDefaultDecision(lmmRequest);
            // 修改reason来包含Agent信息
            result = new LMMDecisionResult(result.getX(), result.getY(),
                    "[Agent" + id + "] " + result.getReason());
            validationResult = ValidationResultEnum.VALID;
        }

        return result;
    }

    /**
     * 存储决策记忆的代理版本
     */
    private void storeDecisionMemory(LMMRequestDTO lmmRequest,
                                     LMMDecisionResult decision,
                                     ValidationResultEnum validationResult,
                                     String conversationId) {
        try {
            String memoryText = PromptTemplateBuilder.buildDecisionMemory(
                    lmmRequest.getCurrentMap(),
                    decision,
                    validationResult);
            AssistantMessage memoryMessage = new AssistantMessage(memoryText);
            chatMemory.add(conversationId, memoryMessage);
            log.debug("[Agent{}] 决策记忆已存储(游戏ID: {}): {}", id, conversationId, memoryText);
        } catch (Exception e) {
            log.error("[Agent{}] 存储决策记忆失败 (游戏ID: {}): {}", id, conversationId, e.getMessage(), e);
        }
    }

    /**
     * 测试向量存储的代理版本
     */
    private void testVectorStoreForGame(GameTypeEnum gameType, String userQuery) {
        try {
            VectorStore vectorStore = ragChatClientFactory.getVectorStore(gameType);
            var searchResult = vectorStore.similaritySearch(SearchRequest.builder()
                    .query(userQuery)
                    .topK(5)
                    .similarityThreshold(0.1)
                    .build());

            log.info("[Agent{}] RAG调试 - 游戏: {}, 查询: {}, 检索到文档数: {}",
                    id, gameType.getChineseName(), userQuery, searchResult.size());

            if (!searchResult.isEmpty()) {
                for (int i = 0; i < Math.min(searchResult.size(), 3); i++) {
                    var doc = searchResult.get(i);
                    String preview = doc.getText().length() > 100 ?
                            doc.getText().substring(0, 100) + "..." : doc.getText();
                    log.info("[Agent{}] 检索文档 {}: 相似度={}, 内容预览: {}",
                            id, i + 1, doc.getMetadata().get("distance"), preview);
                }
            } else {
                log.warn("[Agent{}] RAG调试 - 没有检索到任何相关文档，这可能表示向量存储为空或查询不匹配", id);
            }
        } catch (Exception e) {
            log.error("[Agent{}] RAG调试失败: {}", id, e.getMessage(), e);
        }
    }


    /**
     * 创建内存隔离的请求对象
     */
    private LMMRequestDTO createIsolatedRequest(LMMRequestDTO originalRequest) {
        LMMRequestDTO isolatedRequest = new LMMRequestDTO();
        isolatedRequest.setGameId(generateIsolatedGameId(originalRequest.getGameId()));
        isolatedRequest.setUserId(originalRequest.getUserId());
        isolatedRequest.setCurrentMap(originalRequest.getCurrentMap());
        isolatedRequest.setLLMFlag(originalRequest.getLLMFlag());
        isolatedRequest.setGameType(originalRequest.getGameType());
        isolatedRequest.setGameRule(originalRequest.getGameRule());
        isolatedRequest.setHistorySteps(originalRequest.getHistorySteps());
        isolatedRequest.setGridSize(originalRequest.getGridSize());
        isolatedRequest.setAllowedTimeout(originalRequest.getAllowedTimeout());
        return isolatedRequest;
    }

    /**
     * 生成隔离的游戏ID
     * 通过加上代理ID来确保每个代理有独立的conversationId
     */
    private Integer generateIsolatedGameId(Integer originalGameId) {
        return originalGameId + id;
    }

    public Integer getId() {
        return id;
    }

    public String getAgentName() {
        return "Agent" + id;
    }

    public double getTemperature() {
        return temperature;
    }
} 