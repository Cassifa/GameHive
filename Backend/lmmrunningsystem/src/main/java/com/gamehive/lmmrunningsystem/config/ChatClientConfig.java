package com.gamehive.lmmrunningsystem.config;

import org.springframework.ai.chat.client.ChatClient;
import org.springframework.beans.factory.annotation.Qualifier;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/**
 * ChatClient配置类
 * 负责配置Spring AI的ChatClient相关Bean
 * 为DeepSeek AI服务提供必要的聊天客户端构建器
 *
 * @author Cassifa
 * @since 1.0.0
 */
@Configuration
public class ChatClientConfig {

    /**
     * 创建专用于游戏决策的ChatClient Bean
     * 使用Spring AI Alibaba自动配置的ChatClient.Builder
     *
     * @param chatClientBuilder Spring AI Alibaba自动配置的ChatClient.Builder
     * @return ChatClient 配置好的游戏决策聊天客户端实例
     */
    @Bean
    @Qualifier("gameDecisionChatClient")
    public ChatClient gameDecisionChatClient(ChatClient.Builder chatClientBuilder) {
        return chatClientBuilder
                .defaultSystem("你是一个棋类游戏专家。请严格按照JSON格式返回决策结果。")
                .build();
    }
} 