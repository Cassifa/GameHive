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

    @Value("${lmm.timeout}")
    private Long timeout;

    @Value("${lmm.max-retry-count}")
    private Long tryTime;

    /**
     * 大模型请求队列
     * 使用LinkedList实现的FIFO队列，存储待处理的大模型决策请求
     */
    private final Queue<LMMRequestDTO> lmmRequests = new LinkedList<>();

    /**
     * 消息队列生产者方法
     * 接收游戏参数，创建LMMRequestDTO对象并添加到处理队列中
     * 使用锁机制确保线程安全，并通过条件变量唤醒消费者线程
     *
     * @param gameId       游戏唯一标识符，用于对话记忆的会话ID
     * @param userId       玩家用户ID，用于标识请求来源
     * @param currentMap   当前棋盘状态字符串，格式为每行用换行符分隔的数字矩阵  
     * @param LLMFlag      大模型落子标志，标识大模型代表哪一方（如"1"或"2"）
     * @param gameType     游戏类型名称，如"井字棋"、"五子棋"等
     * @param gameRule     游戏规则描述，详细说明游戏规则和获胜条件
     * @param historySteps 历史步骤记录，包含之前所有落子的历史信息
     * @param gridSize     棋盘网格大小，表示棋盘的边长（如"3"表示3x3棋盘）
     */
    public void addLMMRequest(Integer gameId, Long userId, String currentMap, String LLMFlag,
                              String gameType, String gameRule, String historySteps, String gridSize) {
        lock.lock();
        try {
            lmmRequests.add(new LMMRequestDTO(gameId, userId, currentMap, LLMFlag, gameType, gameRule, historySteps, gridSize));
            condition.signal();//唤醒线程
        } finally {
            lock.unlock();
        }
    }

    /**
     * 消费者处理方法
     * 为每个大模型请求创建独立的Consumer线程进行处理
     * 使用配置文件中的超时时间，防止单个请求阻塞整个系统
     *
     * @param lmmRequest 待处理的大模型请求对象
     */
    private void consume(LMMRequestDTO lmmRequest) {
        Consumer consumer = new Consumer();
        consumer.startTimeout(timeout * tryTime, lmmRequest);
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
