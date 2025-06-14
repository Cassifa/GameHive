package com.gamehive.lmmrunningsystem.service.agent;

import com.gamehive.lmmrunningsystem.constants.GameTypeEnum;
import com.gamehive.lmmrunningsystem.constants.ValidationResultEnum;
import com.gamehive.lmmrunningsystem.dto.LMMDecisionResult;
import com.gamehive.lmmrunningsystem.dto.LMMRequestDTO;
import com.gamehive.lmmrunningsystem.service.agent.factory.RAGChatClientFactory;
import com.gamehive.lmmrunningsystem.service.agent.utils.PromptTemplateBuilder;
import lombok.extern.slf4j.Slf4j;
import org.springframework.ai.chat.client.ChatClient;
import org.springframework.ai.chat.memory.ChatMemory;
import org.springframework.ai.chat.messages.AssistantMessage;
import org.springframework.ai.vectorstore.SearchRequest;
import org.springframework.ai.vectorstore.VectorStore;
import org.springframework.beans.factory.annotation.Qualifier;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

import static org.springframework.ai.chat.client.advisor.AbstractChatMemoryAdvisor.CHAT_MEMORY_CONVERSATION_ID_KEY;

/**
 * 单智能体服务
 * 包含智能重试机制、结果验证功能、对话记忆功能
 *
 * @author Cassifa
 * @since 1.0.0
 */
@Service("originalDeepSeekAIService")
@Slf4j
public class SingleAgentService {

    // 基础ChatClient
    private final ChatClient baseChatClient;

    // 聊天记忆实例，存储对话历史
    protected final ChatMemory chatMemory;

    // RAG ChatClient工厂
    protected final RAGChatClientFactory ragChatClientFactory;

    // 最大重试次数配置
    @Value("${lmm.max-retry-count:3}")
    private int maxRetryCount;

    /**
     * 构造函数，初始化ChatClient和ChatMemory
     *
     * @param gameDecisionChatClient Spring AI配置的游戏决策ChatClient实例
     * @param gameChatMemory         游戏对话记忆实例
     * @param ragChatClientFactory   RAG ChatClient工厂
     */
    public SingleAgentService(@Qualifier("gameDecisionChatClient") ChatClient gameDecisionChatClient,
                              @Qualifier("gameChatMemory") ChatMemory gameChatMemory,
                              RAGChatClientFactory ragChatClientFactory) {
        this.baseChatClient = gameDecisionChatClient;
        this.chatMemory = gameChatMemory;
        this.ragChatClientFactory = ragChatClientFactory;
    }

    /**
     * 获取大模型决策的主要方法
     * 包含重试机制，确保返回有效的决策结果
     * 使用gameId作为对话ID来维护游戏会话的对话记忆
     * 集成决策记忆存储功能，将每次决策结果自动存储到对话历史中
     *
     * @param lmmRequest 大模型请求对象，包含游戏状态和配置信息
     * @return LMMDecisionResult 大模型的决策结果，包含坐标和理由
     */
    public LMMDecisionResult getDecision(LMMRequestDTO lmmRequest) {
        Integer gameId = lmmRequest.getGameId();
        String conversationId = gameId.toString();
        LMMDecisionResult result = null;
        ValidationResultEnum validationResult = null;
        int retryCount = 0;

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

                if (systemPrompt != null) {
                    result = ragChatClient.prompt()
                            .system(systemPrompt)
                            .user(userPrompt)
                            .advisors(a -> a.param(CHAT_MEMORY_CONVERSATION_ID_KEY, conversationId))
                            .call()
                            .entity(LMMDecisionResult.class);
                } else {
                    result = ragChatClient.prompt()
                            .user(userPrompt)
                            .advisors(a -> a.param(CHAT_MEMORY_CONVERSATION_ID_KEY, conversationId))
                            .call()
                            .entity(LMMDecisionResult.class);
                }

                log.info("大模型响应(第{}次, 游戏ID: {}): {}", retryCount + 1, gameId, result);

                //获取到响应
                if (result != null) {
                    //验证响应
                    validationResult = result.validate(lmmRequest);

                    if (validationResult == ValidationResultEnum.VALID) {
                        log.info("大模型决策成功(游戏ID: {}): x={}, y={}", gameId, result.getX(), result.getY());
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
                            log.warn("第{}次重试(游戏ID: {})，原因：{}", retryCount, gameId, validationResult.getDescription());
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
                        log.warn("第{}次重试(游戏ID: {})，原因：结果解析失败", retryCount, gameId);
                    }
                }
            } catch (Exception e) {
                retryCount++;
                log.error("大模型调用失败(第{}次, 游戏ID: {}): {}", retryCount, gameId, e.getMessage(), e);
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
            log.warn("大模型决策失败(游戏ID: {})，使用默认策略", gameId);
            result = getDefaultDecision(lmmRequest);
            validationResult = ValidationResultEnum.VALID;
        }

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
            String memoryText = PromptTemplateBuilder.buildDecisionMemory(
                    lmmRequest.getCurrentMap(),
                    decision,
                    validationResult);
            AssistantMessage memoryMessage = new AssistantMessage(memoryText);
            chatMemory.add(conversationId, memoryMessage);
            log.debug("决策记忆已存储(游戏ID: {}): {}", conversationId, memoryText);
        } catch (Exception e) {
            log.error("存储决策记忆失败 (游戏ID: {}): {}", conversationId, e.getMessage(), e);
        }
    }

    /**
     * 获取默认决策结果 - 随机选择一个空位
     *
     * @param lmmRequest 大模型请求对象
     * @return LMMDecisionResult 默认的决策结果
     */
    protected LMMDecisionResult getDefaultDecision(LMMRequestDTO lmmRequest) {
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
            java.util.Random random = new java.util.Random();
            int[] randomPosition = emptyPositions.get(random.nextInt(emptyPositions.size()));
            return new LMMDecisionResult(randomPosition[0], randomPosition[1],
                    "默认策略：随机选择空位 (" + randomPosition[0] + "," + randomPosition[1] + ")");
        }

        return new LMMDecisionResult(0, 0, "默认策略：无可用位置，选择(0,0)");
    }

    /**
     * 测试向量存储是否能正确检索到相关文档 - 用于RAG调试
     */
    private void testVectorStoreForGame(GameTypeEnum gameType, String userQuery) {
        try {
            VectorStore vectorStore = ragChatClientFactory.getVectorStore(gameType);
            var searchResult = vectorStore.similaritySearch(SearchRequest.builder()
                    .query(userQuery)
                    .topK(5)
                    .similarityThreshold(0.1)
                    .build());

            log.info("RAG调试 - 游戏: {}, 查询: {}, 检索到文档数: {}",
                    gameType.getChineseName(), userQuery, searchResult.size());

            if (!searchResult.isEmpty()) {
                for (int i = 0; i < Math.min(searchResult.size(), 3); i++) {
                    var doc = searchResult.get(i);
                    String preview = doc.getText().length() > 100 ?
                            doc.getText().substring(0, 100) + "..." : doc.getText();
                    log.info("检索文档 {}: 相似度={}, 内容预览: {}",
                            i + 1, doc.getMetadata().get("distance"), preview);
                }
            } else {
                log.warn("RAG调试 - 没有检索到任何相关文档，这可能表示向量存储为空或查询不匹配");
            }
        } catch (Exception e) {
            log.error("RAG调试失败: {}", e.getMessage(), e);
        }
    }
} 
