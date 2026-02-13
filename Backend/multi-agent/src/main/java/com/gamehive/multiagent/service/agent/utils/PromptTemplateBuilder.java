package com.gamehive.multiagent.service.agent.utils;

import com.gamehive.multiagent.constants.ValidationResultEnum;
import com.gamehive.multiagent.dto.LMMDecisionResult;

import java.util.Map;

/**
 * 提示词模板构建器
 * 提供各种静态方法来构建不同场景下的AI提示词模板
 * 包括系统提示词、用户提示词、重试提示词、记忆存储提示词
 *
 * @author Cassifa
 * @since 1.0.0
 */
public class PromptTemplateBuilder {

    // 基础系统提示词，定义AI角色和基本要求
    private static final String SYSTEM_PROMPT_TEMPLATE =
            "你是一个专业的棋类游戏专家级AI。请严格按照JSON格式返回决策结果。" +
                    "你需要根据对话历史来理解游戏进程并做出更好的决策。";

    // 游戏规则模板，包含游戏类型、规则说明和棋子标识
    private static final String GAME_RULES_TEMPLATE =
            "你是一个专业的{gameType}游戏专家级AI。请分析当前棋盘状态并给出最佳决策。\n\n" +
                    "游戏规则：\n{gameRule}\n" +
                    "- 0表示空位，1表示玩家已落子位置，{llmFlag}表示你的棋子\n" +
                    "- 你需要选择一个空位放置棋子\n\n";

    // 输出格式说明模板，定义返回的JSON格式要求
    private static final String OUTPUT_FORMAT_TEMPLATE =
            "请返回你的决策，包含以下字段：\n" +
                    "- x: 行坐标(从0开始的整数)\n" +
                    "- y: 列坐标(从0开始的整数)\n" +
                    "- reason: 详细的决策理由(中文)，如果使用了知识库内容需要说明依据\n\n";

    // 注意事项模板，强调决策的重要约束条件
    private static final String ATTENTION_TEMPLATE =
            "注意：\n" +
                    "1. x和y必须是数字，且在棋盘范围内且没有落子过\n" +
                    "2. 选择的位置必须是空位（值为0）\n" +
                    "3. 请给出清晰的决策理由\n" +
                    "4. 你需要特别注意'游戏规则'因为它很可能和你熟知的其它游戏规则看起来类似但实际完全相反";

    // 历史步骤模板，用于展示之前的落子记录
    private static final String HISTORY_TEMPLATE =
            "历史下棋记录，可以作为参考：\n{historySteps}\n\n";

    // 当前棋盘状态模板，用于展示当前局面
    private static final String BOARD_STATE_TEMPLATE =
            "当前棋盘状态：\n{currentMap}\n\n";

    // 无效决策记忆模板，用于存储失败的决策结果和错误信息
    private static final String INVALID_DECISION_MEMORY_TEMPLATE =
            "局面状态：\n{currentMap}\n" +
                    "AI决策：位置({x},{y})\n" +
                    "决策理由：{reason}\n" +
                    "验证结果：{validationResult}";

    // 错误反馈模板，用于指出上次决策的具体错误
    private static final String ERROR_FEEDBACK_TEMPLATE =
            "上次回答不符合要求。{specificError}请重新分析棋盘并给出正确的决策！\n\n";

    // 多代理汇总记忆模板，用于存储所有代理的响应和投票结果
    private static final String MULTI_AGENT_SUMMARY_TEMPLATE =
            "【多代理决策汇总】\n" +
                    "局面状态：\n{currentMap}\n\n" +
                    "各代理响应：\n{agentResponses}\n" +
                    "投票统计：\n{voteStatistics}\n" +
                    "最终决策：位置({finalX},{finalY}) - {finalReason}\n";

    /**
     * 构建系统提示词
     * 包含游戏规则、棋盘大小、输出格式说明和注意事项，用于系统级别的配置
     *
     * @param gameType 游戏类型名称
     * @param gameRule 游戏规则描述
     * @param llmFlag  大模型落子标识
     * @param gridSize 棋盘网格大小
     * @return 完整的系统提示词
     */
    public static String buildSystemPrompt(String gameType, String gameRule, String llmFlag, String gridSize) {
        StringBuilder systemPrompt = new StringBuilder();

        systemPrompt.append(SYSTEM_PROMPT_TEMPLATE);
        systemPrompt.append("\n\n");

        systemPrompt.append(fillTemplate(GAME_RULES_TEMPLATE,
                "gameType", gameType,
                "gameRule", gameRule,
                "llmFlag", llmFlag));

        systemPrompt.append("棋盘大小：").append(gridSize).append("x").append(gridSize).append("\n\n");

        systemPrompt.append(OUTPUT_FORMAT_TEMPLATE);
        systemPrompt.append(ATTENTION_TEMPLATE);

        return systemPrompt.toString();
    }

    /**
     * 构建基础系统提示词
     * 仅包含AI角色定义，用于配置ChatClient的默认系统提示词
     */
    public static String buildBaseSystemPrompt() {
        return SYSTEM_PROMPT_TEMPLATE;
    }

    /**
     * 构建用户查询提示词
     * 包含当前局面和历史步骤信息，用于单次查询
     *
     * @param currentMap   当前棋盘状态
     * @param historySteps 历史步骤记录
     */
    public static String buildUserPrompt(String currentMap, String historySteps) {
        StringBuilder userPrompt = new StringBuilder();

        if (historySteps != null && !historySteps.trim().isEmpty()) {
            userPrompt.append(fillTemplate(HISTORY_TEMPLATE,
                    "historySteps", historySteps));
        }

        userPrompt.append(fillTemplate(BOARD_STATE_TEMPLATE,
                "currentMap", currentMap));

        userPrompt.append("请分析当前局面并给出你的最佳决策。");

        return userPrompt.toString();
    }

    /**
     * 构建重试提示词
     *
     * @param previousResult 上次的决策结果
     * @param currentMap     当前棋盘状态（用于验证）
     * @param gridSize       棋盘网格大小（用于验证）
     */
    public static String buildRetryPrompt(LMMDecisionResult previousResult, String currentMap, String gridSize) {
        String specificError = buildErrorFeedback(previousResult, currentMap, gridSize);
        return fillTemplate(ERROR_FEEDBACK_TEMPLATE, "specificError", specificError);
    }

    /**
     * 构建失败决策记忆存储文本
     *
     * @param currentMap       当前棋盘状态
     * @param decision         失败的决策结果
     * @param validationResult 验证失败结果
     */
    public static String buildDecisionMemory(String currentMap, LMMDecisionResult decision,
                                             ValidationResultEnum validationResult) {
        return fillTemplate(INVALID_DECISION_MEMORY_TEMPLATE,
                "currentMap", currentMap,
                "x", String.valueOf(decision.getX()),
                "y", String.valueOf(decision.getY()),
                "reason", decision.getReason() != null ? decision.getReason() : "无理由",
                "validationResult", getValidationResultDescription(validationResult));
    }

    /**
     * 构建多代理汇总记忆
     * 将所有成功响应、投票结果、模型名称和温度信息汇总
     *
     * @param currentMap            当前棋盘状态
     * @param agentDecisions        各代理的决策结果
     * @param voteCount            投票统计
     * @param finalDecision        最终决策结果
     * @param agentModelInfo       各代理的模型信息
     * @param agentTemperatureInfo 各代理的温度信息
     */
    public static String buildMultiAgentSummaryMemory(String currentMap,
                                                      Map<String, LMMDecisionResult> agentDecisions,
                                                      Map<String, Integer> voteCount,
                                                      LMMDecisionResult finalDecision,
                                                      Map<String, String> agentModelInfo,
                                                      Map<String, Double> agentTemperatureInfo) {
        
        // 构建各代理响应信息
        StringBuilder agentResponses = new StringBuilder();
        for (Map.Entry<String, LMMDecisionResult> entry : agentDecisions.entrySet()) {
            String agentName = entry.getKey();
            LMMDecisionResult decision = entry.getValue();
            String modelName = agentModelInfo.getOrDefault(agentName, "未知模型");
            Double temperature = agentTemperatureInfo.getOrDefault(agentName, 0.7);
            
            agentResponses.append(String.format("- %s (模型: %s, 温度: %.1f): 位置(%d,%d) - %s\n",
                    agentName, modelName, temperature, 
                    decision.getX(), decision.getY(), decision.getReason()));
        }
        
        // 构建投票统计信息
        StringBuilder voteStatistics = new StringBuilder();
        for (Map.Entry<String, Integer> entry : voteCount.entrySet()) {
            String position = entry.getKey();
            int votes = entry.getValue();
            voteStatistics.append(String.format("- 位置%s: %d票\n", position, votes));
        }
        
        return fillTemplate(MULTI_AGENT_SUMMARY_TEMPLATE,
                "currentMap", currentMap,
                "agentResponses", agentResponses.toString(),
                "voteStatistics", voteStatistics.toString(),
                "finalX", String.valueOf(finalDecision.getX()),
                "finalY", String.valueOf(finalDecision.getY()),
                "finalReason", finalDecision.getReason());
    }

    // 模板变量替换工具方法
    private static String fillTemplate(String template, String... keyValuePairs) {
        String result = template;
        for (int i = 0; i < keyValuePairs.length; i += 2) {
            if (i + 1 < keyValuePairs.length) {
                String key = "{" + keyValuePairs[i] + "}";
                String value = keyValuePairs[i + 1];
                result = result.replace(key, value);
            }
        }
        return result;
    }

    // 构建错误反馈信息的工具方法
    private static String buildErrorFeedback(LMMDecisionResult previousResult, String currentMap, String gridSize) {
        if (previousResult == null) {
            return "解析失败，请确保返回正确的格式。";
        }

        ValidationResultEnum validation = previousResult.validate(currentMap, gridSize);
        return switch (validation) {
            case POSITION_OCCUPIED -> String.format("位置(%d,%d)已经有棋子了。",
                    previousResult.getX(), previousResult.getY());
            case OUT_OF_BOUNDS -> String.format("位置(%d,%d)超出棋盘范围。",
                    previousResult.getX(), previousResult.getY());
            case INVALID_FORMAT -> "返回格式不正确，缺少x或y坐标。";
            default -> "未知错误，请重新尝试。";
        };
    }

    // 获取验证结果描述的工具方法
    private static String getValidationResultDescription(ValidationResultEnum validationResult) {
        return switch (validationResult) {
            case INVALID_FORMAT -> "格式无效";
            case OUT_OF_BOUNDS -> "超出边界";
            case POSITION_OCCUPIED -> "位置已占用";
            default -> "未知错误";
        };
    }
}
