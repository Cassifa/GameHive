package com.gamehive.lmmrunningsystem.service.impl;

import com.gamehive.lmmrunningsystem.service.LMMRunningService;
import com.gamehive.lmmrunningsystem.service.impl.utils.LMMPool;
import org.springframework.stereotype.Service;

@Service
public class LMMRunningServiceImpl implements LMMRunningService {

    public final static LMMPool LMM_POOL = new LMMPool();

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
        LMM_POOL.addBot(userId, currentMap, LLMFlag, gameType, gameRule, historySteps, gridSize);
        return "大模型请求处理成功";
    }
}
