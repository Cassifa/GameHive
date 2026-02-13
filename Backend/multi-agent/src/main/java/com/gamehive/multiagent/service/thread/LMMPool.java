package com.gamehive.multiagent.service.thread;

import com.gamehive.multiagent.dto.LMMRequestDTO;
import org.springframework.stereotype.Component;

import java.util.LinkedList;
import java.util.Queue;
import java.util.concurrent.locks.Condition;
import java.util.concurrent.locks.ReentrantLock;

/**
 * 大模型线程池类
 * 作为消费者线程持续处理队列中的决策请求
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
