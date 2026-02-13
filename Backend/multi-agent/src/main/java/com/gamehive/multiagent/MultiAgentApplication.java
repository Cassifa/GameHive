package com.gamehive.multiagent;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.context.properties.EnableConfigurationProperties;
import lombok.extern.slf4j.Slf4j;

/**
 * 多Agent协作博弈决策服务启动类
 * 基于多Agent协作架构的游戏AI决策服务
 *
 * @author Cassifa
 * @since 2.0.0
 */
@SpringBootApplication
@EnableConfigurationProperties
@Slf4j
public class MultiAgentApplication {

    public static void main(String[] args) {
        // 启动Spring Boot应用
        SpringApplication.run(MultiAgentApplication.class, args);

        log.info("=================================");
        log.info("Multi-Agent Service 启动成功");
        log.info("端口: 3003");
        log.info("=================================");
    }
}
