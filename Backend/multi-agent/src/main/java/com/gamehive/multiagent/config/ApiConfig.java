package com.gamehive.multiagent.config;

import lombok.Data;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

/**
 * API配置类
 *
 * @author Cassifa
 * @since 1.0.0
 */
@Component
@Data
public class ApiConfig {

    @Value("${api.urls.receive-bot-move}")
    private String receiveBotMoveUrl;

    @Value("${spring.ai.dashscope.chat.options.model}")
    private String modelName;
}
