package com.gamehive.lmmrunningsystem.service;

import com.gamehive.lmmrunningsystem.dto.LMMRequestDTO;

public interface LMMRunningService {

    /**
     * 添加LMM决策请求
     *
     * @param requestDTO LMM请求数据传输对象，包含游戏状态和配置信息
     * @return String 处理结果消息
     */
    String addLMM(LMMRequestDTO requestDTO);
}
