package com.kob.botrunningsystem.service.impl.utils;

import org.joor.Reflect;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import org.springframework.util.LinkedMultiValueMap;
import org.springframework.util.MultiValueMap;
import org.springframework.web.client.RestTemplate;

import java.io.File;
import java.io.FileNotFoundException;
import java.io.PrintWriter;
import java.util.UUID;
import java.util.function.Supplier;

@SuppressWarnings("all")
@Component
public class Consumer extends Thread{
    private Bot bot;
    private static RestTemplate restTemplate;
    private final static String receiveBotMoveUrl=
            "http://127.0.0.1:3000/api/pk/receive/bot/move/";

    @Autowired
    public void setRestTemplate(RestTemplate restTemplate){
        Consumer.restTemplate=restTemplate;
    }
    public void startTimeout(Long timeout,Bot bot){
        this.bot=bot;
        this.start();
        try {
            this.join(timeout);//等最多timeout
        } catch (InterruptedException e) {
            e.printStackTrace();
        }
        this.interrupt();//终端当前线程
    }

    private String addUid(String code ,String uuid){
        int k=code.indexOf(" implements java.util.function.Supplier<Integer>");
        return code.substring(0,k)+uuid+code.substring(k);
    }
    @Override
    public void run(){
        UUID uuid=UUID.randomUUID();//返回随机字符串
        String uid=uuid.toString().substring(0,8);
        //动态编译
//        BotInterface botInterface= Reflect.compile(
        Supplier<Integer> botInterface= Reflect.compile(
                "com.kob.botrunningsystem.utils.Bot"+uid,//保证类名不一样
                addUid(bot.getBotCode(),uid)
        ).create().get();//获取一个类

        //文件操作
        File file=new File("input.txt");
        try(PrintWriter fout=new PrintWriter(file)) {
            fout.println(bot.getInput());
            fout.flush();
        } catch (FileNotFoundException e) {
            throw new RuntimeException(e);
        }

//        Integer direction=botInterface.nextMove(bot.getInput());//局面传给Bot
        Integer direction=botInterface.get();

//        System.out.println("move id: "+bot.getUserId()+" direction "+direction);
        MultiValueMap<String,String> data=new LinkedMultiValueMap<>();
        data.add("user_id",bot.getUserId().toString());
        data.add("direction",direction.toString());
        restTemplate.postForObject(receiveBotMoveUrl,data,String.class);
    }
}
