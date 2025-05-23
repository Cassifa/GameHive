package com.gamehive.matchingsystem;

import com.gamehive.matchingsystem.service.impl.MatchingServiceImpl;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

@SpringBootApplication
public class MatchingSystemApplication {
    public static void main(String[] args) {
        //启动匹配线程
        MatchingServiceImpl.matchingPool.start();
        SpringApplication.run(MatchingSystemApplication.class,args);
    }
} 