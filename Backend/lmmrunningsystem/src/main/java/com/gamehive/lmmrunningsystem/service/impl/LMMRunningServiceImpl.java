package com.gamehive.lmmrunningsystem.service.impl;

import com.gamehive.lmmrunningsystem.service.LMMRunningService;
import com.gamehive.lmmrunningsystem.service.impl.utils.LMMPool;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import jakarta.annotation.PostConstruct;

/**
 * 大模型运行服务实现类
 * 实现LMMRunningService接口，提供大模型决策服务的核心功能
 * 负责接收游戏请求并委托给LMMPool进行处理
 *
 * @author Cassifa
 * @since 1.0.0
 */
@Service
public class LMMRunningServiceImpl implements LMMRunningService {

    /**
     * 大模型线程池实例
     * 通过Spring依赖注入，用于管理和调度大模型决策任务的执行
     */
    @Autowired
    private LMMPool lmmPool;

    /**
     * 初始化方法，在Bean创建后启动线程池
     */
    @PostConstruct
    public void init() {
        lmmPool.start();
    }

    /**
     * 添加大模型决策请求的核心方法
     * 接收游戏状态参数，创建大模型决策任务并提交到线程池处理
     *
     * @param userId       玩家用户ID，用于标识请求来源
     * @param currentMap   当前棋盘状态字符串，格式为每行用换行符分隔的数字矩阵
     * @param LLMFlag      大模型落子标志，标识大模型代表哪一方（如"1"或"2"）
     * @param gameType     游戏类型名称，如"井字棋"、"五子棋"等
     * @param gameRule     游戏规则描述，详细说明游戏规则和获胜条件
     * @param historySteps 历史步骤记录，包含之前所有落子的历史信息
     * @param gridSize     棋盘网格大小，表示棋盘的边长（如"3"表示3x3棋盘）
     * @return String 处理结果消息，通常返回"大模型请求处理成功"
     */
    @Override
    public String addLMM(
            Long userId,
            String currentMap,
            String LLMFlag,
            String gameType,
            String gameRule,
            String historySteps,
            String gridSize
    ) {
        // 直接传递所有参数到 LMMPool
        lmmPool.addLMMRequest(userId, currentMap, LLMFlag, gameType, gameRule, historySteps, gridSize);
        return "大模型请求处理成功";
    }
}
