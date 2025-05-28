package com.gamehive.lmmrunningsystem;

import com.gamehive.lmmrunningsystem.service.impl.LMMRunningServiceImpl;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

/**
 * LMM服务启动类
 * 独立的大模型运行服务
 * 
 * @author Cassifa
 * @since 1.0.0
 */
@SpringBootApplication
public class LmmServiceApplication {
    
    public static void main(String[] args) {
        // 启动LMM线程池
        LMMRunningServiceImpl.LMM_POOL.start();
        
        // 启动Spring Boot应用
        SpringApplication.run(LmmServiceApplication.class, args);
        
        System.out.println("=================================");
        System.out.println("LMM Service 启动成功！");
        System.out.println("端口: 3002");
        System.out.println("=================================");
    }
}