package com.gamehive.lmmrunningsystem.service.impl;

import com.gamehive.lmmrunningsystem.service.LMMRunningService;
import com.gamehive.lmmrunningsystem.service.impl.utils.LMMPool;
import org.springframework.stereotype.Service;

@Service
public class LMMRunningServiceImpl implements LMMRunningService {

    public final static LMMPool LMM_POOL = new LMMPool();

    @Override
    public String addLMM(
            String currentMap,
            String LLMFlag,
            String gameType,
            String gameRule,
            String historySteps
    ) {
        // 构建完整的游戏上下文信息
        String context = String.format(
                "游戏类型: %s\n" +
                        "游戏规则: %s\n" +
                        "玩家标志: %s\n" +
                        "棋盘状态:\n%s\n" +
                        "历史步骤: %s",
                gameType,
                gameRule,
                LLMFlag,
                currentMap,
                historySteps
        );

        // 调用LMM池处理(使用固定用户ID和空botCode)
        LMM_POOL.addBot(0, "", context);
        return "大模型请求处理成功";
    }
}
