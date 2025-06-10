package com.gamehive.lmmrunningsystem.config;

import lombok.Data;
import org.springframework.boot.context.properties.ConfigurationProperties;
import org.springframework.stereotype.Component;

import java.util.Map;

/**
 * 向量存储配置属性
 *
 * @author Cassifa
 */
@Data
@Component
@ConfigurationProperties(prefix = "vector.store")
public class VectorStoreProperties {
    
    /**
     * 各游戏类型的向量存储配置
     */
    private Map<String, VectorStoreConfig> stores;
    
    /**
     * RAG检索配置
     */
    private RAGConfig rag = new RAGConfig();
    
    /**
     * 向量存储单个配置
     */
    @Data
    public static class VectorStoreConfig {
        /**
         * 存储路径
         */
        private String path;
        
        /**
         * 向量维度
         */
        private Integer dimension;
        
        /**
         * 知识库文件路径
         */
        private String knowledgeFile;
    }
    
    /**
     * 根据游戏类型获取配置
     */
    public VectorStoreConfig getConfig(String gameType) {
        return stores.get(gameType);
    }
    
    /**
     * RAG检索配置类
     */
    @Data
    public static class RAGConfig {
        /**
         * 检索的最大文档数量
         */
        private Integer maxDocuments = 10;
        
        /**
         * 相似度阈值
         */
        private Double similarityThreshold = 0.1;
        
        /**
         * 是否启用重排序
         */
        private Boolean enableRerank = false;
    }
}
