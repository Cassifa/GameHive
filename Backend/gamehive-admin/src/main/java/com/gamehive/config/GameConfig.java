package com.gamehive.config;

import lombok.Data;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

/**
 * 游戏配置类
 *
 * @author Cassifa
 * @since 1.0.0
 */
@Component
@Data
public class GameConfig {

    @Value("${game.wait-time.normal:600}")
    private int normalWaitTime;

    @Value("${game.wait-time.lmm:6000}")
    private int lmmWaitTime;

    @Value("${api.urls.add-bot:http://127.0.0.1:3002/LMMRunning/add/}")
    private String addBotUrl;
} 