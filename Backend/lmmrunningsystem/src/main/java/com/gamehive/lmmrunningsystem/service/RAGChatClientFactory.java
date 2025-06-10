package com.gamehive.lmmrunningsystem.service;

import com.alibaba.cloud.ai.dashscope.api.DashScopeApi;
import com.alibaba.cloud.ai.dashscope.embedding.DashScopeEmbeddingModel;
import com.gamehive.lmmrunningsystem.config.VectorStoreProperties;
import com.gamehive.lmmrunningsystem.constants.GameTypeEnum;
import jakarta.annotation.PostConstruct;
import lombok.extern.slf4j.Slf4j;
import org.springframework.ai.chat.client.ChatClient;
import org.springframework.ai.chat.client.advisor.MessageChatMemoryAdvisor;
import org.springframework.ai.chat.client.advisor.QuestionAnswerAdvisor;
import org.springframework.ai.chat.client.advisor.SimpleLoggerAdvisor;
import org.springframework.ai.chat.memory.ChatMemory;
import org.springframework.ai.chat.model.ChatModel;
import org.springframework.ai.embedding.EmbeddingModel;
import org.springframework.ai.document.Document;
import org.springframework.ai.reader.TextReader;
import org.springframework.ai.vectorstore.SearchRequest;
import org.springframework.ai.vectorstore.SimpleVectorStore;
import org.springframework.ai.vectorstore.VectorStore;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.beans.factory.annotation.Qualifier;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.core.io.Resource;
import org.springframework.core.io.ResourceLoader;
import org.springframework.stereotype.Service;

import java.io.File;
import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.ConcurrentHashMap;

/**
 * RAG ChatClient工厂类
 * 为每个游戏类型创建独立的ChatClient，每个使用不同的嵌入模型和向量存储
 *
 * @author Cassifa
 */
@Slf4j
@Service
public class RAGChatClientFactory {
    // ChatClient映射表
    private final ConcurrentHashMap<String, ChatClient> chatClients = new ConcurrentHashMap<>();
    // VectorStore映射表
    private final ConcurrentHashMap<String, VectorStore> vectorStores = new ConcurrentHashMap<>();
    private final ChatModel chatModel;
    private final VectorStoreProperties vectorStoreProperties;
    private final ChatMemory gameChatMemory;
    private final ResourceLoader resourceLoader;

    @Value("${spring.ai.dashscope.api-key:}")
    private String apiKey;

    @Autowired
    public RAGChatClientFactory(ChatModel chatModel,
                                VectorStoreProperties vectorStoreProperties,
                                @Qualifier("gameChatMemory") ChatMemory gameChatMemory,
                                ResourceLoader resourceLoader) {
        this.chatModel = chatModel;
        this.vectorStoreProperties = vectorStoreProperties;
        this.gameChatMemory = gameChatMemory;
        this.resourceLoader = resourceLoader;
    }

    @PostConstruct
    public void initializeChatClients() {
        log.info("开始初始化RAG ChatClient工厂...");

        // 为每个游戏类型创建ChatClient
        createChatClientForGame("gobang", "五子棋专用RAG ChatClient");
        createChatClientForGame("reverse-go", "不围棋专用RAG ChatClient");
        createChatClientForGame("tic-tac-toe", "井字棋专用RAG ChatClient");
        createChatClientForGame("misere-tic-tac-toe", "反井字棋专用RAG ChatClient");

        log.info("RAG ChatClient工厂初始化完成，共创建{}个ChatClient", chatClients.size());
    }

    /**
     * 为指定游戏类型创建ChatClient
     */
    private void createChatClientForGame(String gameKey, String description) {
        try {
            log.info("创建ChatClient: {} - {}", gameKey, description);

            // 1. 创建独立的嵌入模型
            DashScopeApi dashScopeApi = new DashScopeApi(apiKey);
            EmbeddingModel embeddingModel = new DashScopeEmbeddingModel(dashScopeApi);

            // 2. 创建向量存储
            VectorStore vectorStore = createVectorStore(gameKey, embeddingModel);
            vectorStores.put(gameKey, vectorStore);  // 保存向量存储引用

            // 3. 创建RAG advisor，使用配置文件中的参数
            VectorStoreProperties.RAGConfig ragConfig = vectorStoreProperties.getRag();
            QuestionAnswerAdvisor questionAnswerAdvisor = QuestionAnswerAdvisor.builder(vectorStore)
                    .searchRequest(SearchRequest.builder()
                            .topK(ragConfig.getMaxDocuments())
                            .similarityThreshold(ragConfig.getSimilarityThreshold())
                            .build())
                    .build();

            // 4. 创建ChatClient with RAG，模仿gameDecisionChatClient的构建方式
            ChatClient chatClient = ChatClient.builder(chatModel)
                    .defaultSystem("你是一个" + getGameTypeDescription(gameKey) + "游戏专家，请基于提供的知识回答问题。")
                    .defaultAdvisors(MessageChatMemoryAdvisor.builder(gameChatMemory).build())
                    .defaultAdvisors(questionAnswerAdvisor)  // RAG增强
                    .defaultAdvisors(new SimpleLoggerAdvisor())
                    .build();

            chatClients.put(gameKey, chatClient);
            log.info("ChatClient创建成功: {}", gameKey);

        } catch (Exception e) {
            log.error("创建ChatClient失败: {}", gameKey, e);
            throw new RuntimeException("创建ChatClient失败: " + gameKey, e);
        }
    }

    /**
     * 创建向量存储
     */
    private VectorStore createVectorStore(String gameKey, EmbeddingModel embeddingModel) {
        try {
            VectorStoreProperties.VectorStoreConfig config = vectorStoreProperties.getStores().get(gameKey);
            String storagePath = config.getPath();
            String knowledgeFile = config.getKnowledgeFile();

            if (storagePath == null || storagePath.isEmpty()) {
                throw new IllegalArgumentException("向量存储路径未配置: " + gameKey);
            }

            // 创建SimpleVectorStore
            SimpleVectorStore vectorStore = SimpleVectorStore.builder(embeddingModel).build();

            // 如果存储文件存在，则加载
            File storageFile = new File(storagePath);
            if (storageFile.exists()) {
                vectorStore.load(storageFile);
                log.info("向量存储 {} 已从文件加载: {}", gameKey, storagePath);
            } else {
                log.info("向量存储 {} 创建新的存储文件: {}", gameKey, storagePath);
                // 确保父目录存在
                File parentDir = storageFile.getParentFile();
                if (parentDir != null && !parentDir.exists()) {
                    boolean created = parentDir.mkdirs();
                    if (created) {
                        log.info("创建存储目录: {}", parentDir.getAbsolutePath());
                    }
                }
                
                // 加载知识库文件
                if (knowledgeFile != null && !knowledgeFile.isEmpty()) {
                    loadKnowledgeFile(vectorStore, knowledgeFile, gameKey);
                    
                    // 保存向量存储到文件
                    vectorStore.save(storageFile);
                    log.info("向量存储 {} 已保存到文件: {}", gameKey, storagePath);
                }
            }

            return vectorStore;
        } catch (Exception e) {
            log.error("创建向量存储失败: {}", gameKey, e);
            throw new RuntimeException("创建向量存储失败: " + gameKey, e);
        }
    }

    /**
     * 加载知识库文件到向量存储
     */
    private void loadKnowledgeFile(VectorStore vectorStore, String knowledgeFile, String gameKey) {
        try {
            log.info("开始加载知识库文件: {} for {}", knowledgeFile, gameKey);
            
            // 加载资源文件
            Resource resource = resourceLoader.getResource(knowledgeFile);
            if (!resource.exists()) {
                log.warn("知识库文件不存在: {}", knowledgeFile);
                return;
            }
            
            // 读取文件内容
            String content = new String(resource.getInputStream().readAllBytes(), java.nio.charset.StandardCharsets.UTF_8);
            
            // 将大文档分割成小块（每块最大1800字符，留200字符缓冲）
            List<Document> documents = splitTextIntoChunks(content, 1800, gameKey, knowledgeFile);
            
            log.info("知识库文件 {} 分割为 {} 个文档片段", knowledgeFile, documents.size());
            
            // 添加到向量存储
            vectorStore.add(documents);
            
            log.info("成功加载知识库文件 {} 到向量存储 {}", knowledgeFile, gameKey);
            
        } catch (Exception e) {
            log.error("加载知识库文件失败: {} for {}", knowledgeFile, gameKey, e);
            throw new RuntimeException("加载知识库文件失败: " + knowledgeFile, e);
        }
    }

    /**
     * 将文本分割成小块
     */
    private List<Document> splitTextIntoChunks(String text, int maxChunkSize, String gameKey, String source) {
        List<Document> documents = new ArrayList<>();
        
        // 按段落分割（以双换行为分界）
        String[] paragraphs = text.split("\n\n");
        
        StringBuilder currentChunk = new StringBuilder();
        int chunkIndex = 0;
        
        for (String paragraph : paragraphs) {
            // 如果当前段落本身就超过限制，进一步分割
            if (paragraph.length() > maxChunkSize) {
                // 先保存之前积累的内容
                if (currentChunk.length() > 0) {
                    documents.add(createDocumentFromChunk(currentChunk.toString(), gameKey, source, chunkIndex++));
                    currentChunk.setLength(0);
                }
                
                // 按句子分割长段落
                String[] sentences = paragraph.split("\\n");
                for (String sentence : sentences) {
                    if (sentence.trim().isEmpty()) continue;
                    
                    // 如果单个句子仍然太长，强制按字符数分割
                    if (sentence.length() > maxChunkSize) {
                        for (int i = 0; i < sentence.length(); i += maxChunkSize) {
                            int endIndex = Math.min(i + maxChunkSize, sentence.length());
                            String subSentence = sentence.substring(i, endIndex);
                            documents.add(createDocumentFromChunk(subSentence, gameKey, source, chunkIndex++));
                        }
                    } else if (currentChunk.length() + sentence.length() + 1 > maxChunkSize) {
                        // 当前chunk放不下这个句子，先保存当前chunk
                        if (currentChunk.length() > 0) {
                            documents.add(createDocumentFromChunk(currentChunk.toString(), gameKey, source, chunkIndex++));
                            currentChunk.setLength(0);
                        }
                        currentChunk.append(sentence).append("\n");
                    } else {
                        // 句子可以放入当前chunk
                        if (currentChunk.length() > 0) {
                            currentChunk.append("\n");
                        }
                        currentChunk.append(sentence);
                    }
                }
            } else if (currentChunk.length() + paragraph.length() + 2 > maxChunkSize) {
                // 当前chunk放不下这个段落，先保存当前chunk
                if (currentChunk.length() > 0) {
                    documents.add(createDocumentFromChunk(currentChunk.toString(), gameKey, source, chunkIndex++));
                    currentChunk.setLength(0);
                }
                currentChunk.append(paragraph);
            } else {
                // 段落可以放入当前chunk
                if (currentChunk.length() > 0) {
                    currentChunk.append("\n\n");
                }
                currentChunk.append(paragraph);
            }
        }
        
        // 保存最后一个chunk
        if (currentChunk.length() > 0) {
            documents.add(createDocumentFromChunk(currentChunk.toString(), gameKey, source, chunkIndex));
        }
        
        return documents;
    }
    
    /**
     * 从文本块创建Document
     */
    private Document createDocumentFromChunk(String content, String gameKey, String source, int chunkIndex) {
        Document doc = new Document(content.trim());
        doc.getMetadata().put("game_type", gameKey);
        doc.getMetadata().put("source", source);
        doc.getMetadata().put("chunk_index", chunkIndex);
        doc.getMetadata().put("chunk_size", content.length());
        return doc;
    }

    /**
     * 工厂模式方法：根据游戏类型获取对应的ChatClient
     */
    public ChatClient getChatClient(GameTypeEnum gameType) {
        String gameKey = getGameKey(gameType);
        ChatClient chatClient = chatClients.get(gameKey);
        if (chatClient == null) {
            throw new IllegalArgumentException("未找到对应的ChatClient: " + gameType.getChineseName());
        }
        
        log.info("🎯 使用RAG ChatClient: {} ({}) - 包含知识库增强功能", 
                 gameType.getChineseName(), gameKey);
        
        return chatClient;
    }

    /**
     * 根据游戏类型字符串获取对应的VectorStore
     */
    public VectorStore getVectorStore(String gameType) {
        // 根据游戏类型字符串转换为gameKey
        String gameKey = convertGameTypeToKey(gameType);
        VectorStore vectorStore = vectorStores.get(gameKey);
        if (vectorStore == null) {
            throw new IllegalArgumentException("未找到对应的VectorStore: " + gameType);
        }
        return vectorStore;
    }

    /**
     * 将游戏类型字符串转换为存储键
     */
    private String convertGameTypeToKey(String gameType) {
        switch (gameType.toLowerCase()) {
            case "gobang":
            case "五子棋":
            case "8*8五子棋":
                return "gobang";
            case "reverse-go":
            case "anti-go":
            case "不围棋":
                return "reverse-go";
            case "tic-tac-toe":
            case "井字棋":
                return "tic-tac-toe";
            case "misere-tic-tac-toe":
            case "反井字棋":
                return "misere-tic-tac-toe";
            default:
                throw new IllegalArgumentException("不支持的游戏类型: " + gameType);
        }
    }

    /**
     * 根据游戏类型获取存储键
     */
    private String getGameKey(GameTypeEnum gameType) {
        // 五子棋和8*8五子棋共用一个ChatClient
        if (gameType == GameTypeEnum.GOBANG || gameType == GameTypeEnum.GOBANG_88) {
            return "gobang";
        }
        switch (gameType) {
            case ANTI_GO:
                return "reverse-go";
            case TIC_TAC_TOE:
                return "tic-tac-toe";
            case MISERE_TIC_TAC_TOE:
                return "misere-tic-tac-toe";
            default:
                throw new IllegalArgumentException("不支持的游戏类型: " + gameType.getChineseName());
        }
    }

    /**
     * 获取游戏类型描述
     */
    private String getGameTypeDescription(String gameKey) {
        switch (gameKey) {
            case "gobang":
                return "五子棋";
            case "reverse-go":
                return "不围棋";
            case "tic-tac-toe":
                return "井字棋";
            case "misere-tic-tac-toe":
                return "反井字棋";
            default:
                return "未知游戏";
        }
    }
}