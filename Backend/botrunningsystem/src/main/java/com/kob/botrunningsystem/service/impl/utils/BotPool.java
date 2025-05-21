package com.kob.botrunningsystem.service.impl.utils;

import java.util.LinkedList;
import java.util.Queue;
import java.util.concurrent.locks.Condition;
import java.util.concurrent.locks.ReentrantLock;

public class BotPool extends Thread{
    private final ReentrantLock lock=new ReentrantLock();
    private final Condition condition =lock.newCondition();

    private Queue<Bot> bots =new LinkedList<>();

    //消息队列生产者
    public void addBot(Integer userId,String botCode,String input){
        lock.lock();
        try{
            bots.add(new Bot(userId,botCode,input));
            condition.signal();//唤醒线程
        }finally {
            lock.unlock();
        }
    }

    private void consume(Bot bot){
        //仅支持java
        Consumer consumer =new Consumer();
        consumer.startTimeout(4000L,bot);//Bot最多等四秒
    }

    public void run(){
        while (true){//模拟消息队列
            lock.lock();
            if(bots.isEmpty()){
                try {
                    condition.await();//阻塞当前进程,直到唤醒/结束 自动释放锁
                } catch (InterruptedException e) {
                    e.printStackTrace();
                    lock.unlock();
                    break;
                }
            }else {
                Bot bot=bots.remove();
                lock.unlock();
                consume(bot);//耗时高
            }

        }
    }
}
