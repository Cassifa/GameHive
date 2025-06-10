package com.gamehive.lmmrunningsystem.service.impl.threadutils;

import com.gamehive.lmmrunningsystem.dto.LMMRequestDTO;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Component;

import java.util.LinkedList;
import java.util.Queue;
import java.util.concurrent.locks.Condition;
import java.util.concurrent.locks.ReentrantLock;

/**
 * 大模型线程池类
 * 实现生产者-消费者模式的线程池，用于管理大模型决策任务
 * 继承Thread类，作为消费者线程持续处理队列中的决策请求
 *
 * @author Cassifa
 * @since 1.0.0
 */
@Component
public class LMMPool extends Thread {

    private final ReentrantLock lock = new ReentrantLock();

    private final Condition condition = lock.newCondition();

    //大模型请求队列
    private final Queue<LMMRequestDTO> lmmRequests = new LinkedList<>();

    public void addLMMRequest(LMMRequestDTO lmmRequest) {
        lock.lock();
        try {
            lmmRequests.add(lmmRequest);
            condition.signal();//唤醒线程
        } finally {
            lock.unlock();
        }
    }

    private void consume(LMMRequestDTO lmmRequest) {
        Consumer consumer = new Consumer();
        consumer.startTimeout(lmmRequest.getAllowedTimeout(), lmmRequest);
    }

    /**
     * 线程执行的主要方法
     * 实现消费者逻辑，持续监听队列中的新任务
     * 使用生产者-消费者模式：
     * 1. 当队列为空时，线程阻塞等待新任务
     * 2. 当有新任务时，取出任务并创建Consumer处理
     * 3. 使用锁机制确保线程安全
     */
    public void run() {
        while (true) {//模拟消息队列
            lock.lock();
            if (lmmRequests.isEmpty()) {
                try {
                    condition.await();//阻塞当前进程,直到唤醒/结束 自动释放锁
                } catch (InterruptedException e) {
                    e.printStackTrace();
                    lock.unlock();
                    break;
                }
            } else {
                LMMRequestDTO lmmRequest = lmmRequests.remove();
                lock.unlock();
                consume(lmmRequest);//耗时高
            }
        }
    }
}
