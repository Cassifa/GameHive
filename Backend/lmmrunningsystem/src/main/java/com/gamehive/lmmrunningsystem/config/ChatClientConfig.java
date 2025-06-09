package com.gamehive.lmmrunningsystem.config;

import com.gamehive.lmmrunningsystem.utils.PromptTemplateBuilder;
import org.springframework.ai.chat.client.ChatClient;
import org.springframework.ai.chat.client.advisor.MessageChatMemoryAdvisor;
import org.springframework.ai.chat.client.advisor.SimpleLoggerAdvisor;
import org.springframework.ai.chat.memory.ChatMemory;
import org.springframework.ai.chat.memory.InMemoryChatMemory;
import org.springframework.beans.factory.annotation.Qualifier;
import org.springframework.context.annotation.Bean;
import org.springframework.context.annotation.Configuration;

/**
 * ChatClient配置类
 * 负责配置Spring AI的ChatClient相关Bean
 * 为DeepSeek AI服务提供必要的聊天客户端构建器和对话记忆功能
 *
 * @author Cassifa
 * @since 1.0.0
 */
@Configuration
public class ChatClientConfig {

    @Bean
    @Qualifier("gameChatMemory")
    public ChatMemory gameChatMemory() {
        return new InMemoryChatMemory();
    }

    /**
     * @param chatClientBuilder Spring AI Alibaba自动配置的ChatClient.Builder
     * @param gameChatMemory    游戏对话记忆实例
     * @return ChatClient 配置好的游戏决策聊天客户端实例
     */
    @Bean
    @Qualifier("gameDecisionChatClient")
    public ChatClient gameDecisionChatClient(ChatClient.Builder chatClientBuilder,
                                             @Qualifier("gameChatMemory") ChatMemory gameChatMemory) {
        return chatClientBuilder
                .defaultSystem(PromptTemplateBuilder.buildBaseSystemPrompt())
                .defaultAdvisors(MessageChatMemoryAdvisor.builder(gameChatMemory).build())
                .defaultAdvisors(new SimpleLoggerAdvisor())
                .build();
    }
} 
