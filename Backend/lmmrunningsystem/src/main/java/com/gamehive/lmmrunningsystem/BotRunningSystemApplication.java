package com.gamehive.lmmrunningsystem;

import com.gamehive.lmmrunningsystem.service.impl.LMMRunningServiceImpl;
import org.springframework.boot.SpringApplication;
import org.springframework.boot.autoconfigure.SpringBootApplication;

@SpringBootApplication
public class BotRunningSystemApplication {
    public static void main(String[] args) {
        LMMRunningServiceImpl.LMM_POOL.start();
        SpringApplication.run(BotRunningSystemApplication.class,args);
    }
}