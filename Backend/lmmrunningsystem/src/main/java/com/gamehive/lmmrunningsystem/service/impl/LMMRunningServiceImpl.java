package com.gamehive.lmmrunningsystem.service.impl;

import com.gamehive.lmmrunningsystem.dto.LMMRequestDTO;
import com.gamehive.lmmrunningsystem.service.LMMRunningService;
import com.gamehive.lmmrunningsystem.service.impl.threadutils.LMMPool;
import jakarta.annotation.PostConstruct;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

/**
 * 大模型运行服务实现类
 * 实现LMMRunningService接口，提供大模型决策服务的核心功能
 * 负责接收游戏请求并委托给LMMPool进行处理
 *
 * @author Cassifa
 * @since 1.0.0
 */
@Service
@Slf4j
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
     * 接收LMM请求DTO，转换为内部处理格式并提交到线程池处理
     *
     * @param requestDTO LMM请求数据传输对象，包含游戏状态和配置信息
     * @return String 处理结果消息，通常返回"大模型请求处理成功"
     */
    @Override
    public String addLMM(LMMRequestDTO requestDTO) {
        log.info("接收到游戏ID：{} 的LMM请求", requestDTO.getGameId());

        lmmPool.addLMMRequest(requestDTO);

        return "大模型请求处理成功";
    }
}
