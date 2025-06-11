package com.gamehive.lmmrunningsystem.service.agent;

import com.gamehive.lmmrunningsystem.dto.LMMDecisionResult;
import com.gamehive.lmmrunningsystem.dto.LMMRequestDTO;
import com.gamehive.lmmrunningsystem.dto.MultiAgentResult;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.boot.autoconfigure.condition.ConditionalOnProperty;
import org.springframework.stereotype.Service;

import java.util.HashMap;
import java.util.List;
import java.util.Map;
import java.util.concurrent.CompletableFuture;
import java.util.concurrent.ConcurrentHashMap;
import java.util.concurrent.TimeUnit;

/**
 * 多代理协调器服务
 * 负责异步执行多个代理并进行投票决策
 * 使用AgentFactory统一管理Agent的创建和获取
 *
 * @author Cassifa
 * @since 1.0.0
 */
@Service
@Slf4j
@ConditionalOnProperty(name = "lmm.multi-agent.enabled", havingValue = "true", matchIfMissing = true)
public class MultiAgentCoordinatorService {

    private final AgentFactory agentFactory;

    @Autowired
    public MultiAgentCoordinatorService(AgentFactory agentFactory) {
        this.agentFactory = agentFactory;
    }

    /**
     * 多代理决策方法
     * 异步执行所有可用的代理，然后进行投票决策
     *
     * @param lmmRequest 大模型请求对象
     * @return MultiAgentResult 多代理投票结果
     */
    public MultiAgentResult getMultiAgentDecision(LMMRequestDTO lmmRequest) {
        long startTime = System.currentTimeMillis();

        // 获取超时时间，如果未设置则使用默认值30秒
        long timeoutMs = lmmRequest.getAllowedTimeout();

        // 从工厂获取所有可用的代理
        List<GameAgent> availableAgents = agentFactory.getAvailableAgents();

        log.info("开始多代理决策，游戏ID: {}, 可用代理数: {}, 超时时间: {}ms",
                lmmRequest.getGameId(), availableAgents.size(), timeoutMs);

        // 异步执行所有代理
        Map<String, CompletableFuture<AgentExecutionResult>> futures = new ConcurrentHashMap<>();

        for (GameAgent agent : availableAgents) {
            String agentName = agent.getAgentName();
            CompletableFuture<AgentExecutionResult> future = CompletableFuture.supplyAsync(() -> {
                long agentStartTime = System.currentTimeMillis();
                try {
                    LMMDecisionResult result = agent.getDecision(lmmRequest);
                    long agentEndTime = System.currentTimeMillis();
                    return new AgentExecutionResult(agentName, result, agentEndTime - agentStartTime, null);
                } catch (Exception e) {
                    long agentEndTime = System.currentTimeMillis();
                    log.error("[{}] 代理执行失败: {}", agentName, e.getMessage(), e);
                    return new AgentExecutionResult(agentName, null, agentEndTime - agentStartTime, e);
                }
            });

            futures.put(agentName, future);
        }

        // 等待所有代理完成或超时
        Map<String, AgentExecutionResult> results = new ConcurrentHashMap<>();

        for (Map.Entry<String, CompletableFuture<AgentExecutionResult>> entry : futures.entrySet()) {
            try {
                AgentExecutionResult result = entry.getValue().get(timeoutMs, TimeUnit.MILLISECONDS);
                results.put(entry.getKey(), result);
            } catch (Exception e) {
                log.error("[{}] 代理执行超时或失败: {}", entry.getKey(), e.getMessage());
                results.put(entry.getKey(), new AgentExecutionResult(entry.getKey(), null, timeoutMs, e));
            }
        }

        long endTime = System.currentTimeMillis();

        // 统计结果并进行投票
        return processResults(results, lmmRequest, endTime - startTime);
    }

    /**
     * 处理代理执行结果并进行投票
     */
    private MultiAgentResult processResults(Map<String, AgentExecutionResult> results,
                                            LMMRequestDTO lmmRequest,
                                            long totalTime) {
        Map<String, LMMDecisionResult> agentDecisions = new HashMap<>();
        Map<String, Long> executionTimes = new HashMap<>();
        Map<String, Integer> voteCount = new HashMap<>();

        int successfulAgents = 0;
        int failedAgents = 0;

        // 统计结果
        for (AgentExecutionResult result : results.values()) {
            executionTimes.put(result.agentName, result.executionTime);

            if (result.decision != null && result.decision.isValid(lmmRequest)) {
                agentDecisions.put(result.agentName, result.decision);
                successfulAgents++;

                // 统计投票
                String voteKey = result.decision.getX() + "," + result.decision.getY();
                voteCount.put(voteKey, voteCount.getOrDefault(voteKey, 0) + 1);
            } else {
                failedAgents++;
                log.warn("代理 {} 决策失败或无效", result.agentName);
            }
        }

        // 进行投票决策
        LMMDecisionResult finalDecision = performVoting(agentDecisions, voteCount, lmmRequest);

        log.info("多代理决策完成，游戏ID: {}, 成功: {}, 失败: {}, 总耗时: {}ms, 最终决策: ({}, {})",
                lmmRequest.getGameId(), successfulAgents, failedAgents, totalTime,
                finalDecision.getX(), finalDecision.getY());

        return new MultiAgentResult(
                finalDecision,
                agentDecisions,
                voteCount,
                executionTimes,
                totalTime,
                successfulAgents,
                failedAgents,
                "MAJORITY"
        );
    }

    /**
     * 执行投票算法
     */
    private LMMDecisionResult performVoting(Map<String, LMMDecisionResult> agentDecisions,
                                            Map<String, Integer> voteCount,
                                            LMMRequestDTO lmmRequest) {
        if (agentDecisions.isEmpty()) {
            log.warn("没有有效的代理决策，使用默认策略");
            return getDefaultDecision(lmmRequest);
        }

        if (agentDecisions.size() == 1) {
            // 只有一个有效决策，直接返回
            return agentDecisions.values().iterator().next();
        }

        // 简单多数投票
        return performMajorityVoting(voteCount, agentDecisions, lmmRequest);
    }

    /**
     * 多数投票算法
     */
    private LMMDecisionResult performMajorityVoting(Map<String, Integer> voteCount,
                                                    Map<String, LMMDecisionResult> agentDecisions,
                                                    LMMRequestDTO lmmRequest) {
        if (voteCount.isEmpty()) {
            return getDefaultDecision(lmmRequest);
        }

        // 找到得票最多的决策
        String winningVote = null;
        int maxVotes = 0;

        for (Map.Entry<String, Integer> entry : voteCount.entrySet()) {
            if (entry.getValue() > maxVotes) {
                maxVotes = entry.getValue();
                winningVote = entry.getKey();
            }
        }

        if (winningVote != null) {
            // 解析坐标
            String[] coords = winningVote.split(",");
            int x = Integer.parseInt(coords[0]);
            int y = Integer.parseInt(coords[1]);

            // 找到对应的决策理由
            String reason = "多数投票决策，得票数: " + maxVotes;
            for (LMMDecisionResult decision : agentDecisions.values()) {
                if (decision.getX() == x && decision.getY() == y) {
                    reason = decision.getReason() + " (多数投票，得票: " + maxVotes + ")";
                    break;
                }
            }

            return new LMMDecisionResult(x, y, reason);
        }

        return getDefaultDecision(lmmRequest);
    }

    /**
     * 获取默认决策 - 随机选择一个空位
     */
    private LMMDecisionResult getDefaultDecision(LMMRequestDTO lmmRequest) {
        String[] rows = lmmRequest.getCurrentMap().split("\n");
        int gridSize = Integer.parseInt(lmmRequest.getGridSize());
        
        // 收集所有空位
        java.util.List<int[]> emptyPositions = new java.util.ArrayList<>();
        
        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                if (i < rows.length && j < rows[i].length() && rows[i].charAt(j) == '0') {
                    emptyPositions.add(new int[]{i, j});
                }
            }
        }
        
        if (!emptyPositions.isEmpty()) {
            // 随机选择一个空位
            java.util.Random random = new java.util.Random();
            int[] randomPosition = emptyPositions.get(random.nextInt(emptyPositions.size()));
            return new LMMDecisionResult(randomPosition[0], randomPosition[1], 
                "多代理默认策略：随机选择空位 (" + randomPosition[0] + "," + randomPosition[1] + ")");
        }

        return new LMMDecisionResult(0, 0, "多代理默认策略：无可用位置，选择(0,0)");
    }

    /**
     * 代理执行结果内部类
     */
    private static class AgentExecutionResult {
        final String agentName;
        final LMMDecisionResult decision;
        final long executionTime;
        final Exception exception;

        AgentExecutionResult(String agentName, LMMDecisionResult decision, long executionTime, Exception exception) {
            this.agentName = agentName;
            this.decision = decision;
            this.executionTime = executionTime;
            this.exception = exception;
        }
    }
} 