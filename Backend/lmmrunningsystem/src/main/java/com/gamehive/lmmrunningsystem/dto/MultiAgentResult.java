package com.gamehive.lmmrunningsystem.dto;

import lombok.Data;
import lombok.AllArgsConstructor;
import lombok.NoArgsConstructor;

import java.util.List;
import java.util.Map;

/**
 * 多代理投票结果DTO
 * 包含最终决策结果和各代理的投票详情
 *
 * @author Cassifa
 * @since 1.0.0
 */
@Data
@AllArgsConstructor
@NoArgsConstructor
public class MultiAgentResult {

    /**
     * 最终决策结果
     */
    private LMMDecisionResult finalDecision;

    /**
     * 各代理的决策结果
     * key: 代理名称, value: 决策结果
     */
    private Map<String, LMMDecisionResult> agentDecisions;

    /**
     * 投票统计
     * key: 坐标(格式: "x,y"), value: 票数
     */
    private Map<String, Integer> voteCount;

    /**
     * 执行时间统计(毫秒)
     * key: 代理名称, value: 执行时间
     */
    private Map<String, Long> executionTimes;

    /**
     * 总执行时间(毫秒)
     */
    private long totalExecutionTime;

    /**
     * 成功的代理数量
     */
    private int successfulAgents;

    /**
     * 失败的代理数量
     */
    private int failedAgents;

    /**
     * 投票算法
     */
    private String votingAlgorithm;
} 