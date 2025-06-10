package com.gamehive.lmmrunningsystem;

import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;
import org.springframework.boot.context.properties.EnableConfigurationProperties;
import lombok.extern.slf4j.Slf4j;

/**
 * LMM服务启动类
 * 独立的大模型运行服务
 *
 * @author Cassifa
 * @since 1.0.0
 */
@SpringBootApplication
@EnableConfigurationProperties
@Slf4j
public class LMMServiceApplication {

    public static void main(String[] args) {
        // 启动Spring Boot应用
        SpringApplication.run(LMMServiceApplication.class, args);

        log.info("=================================");
        log.info("LMM Service 启动成功");
        log.info("端口: 3002");
        log.info("=================================");
    }
}
