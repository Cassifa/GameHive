package com.gamehive.lmmrunningsystem.utils;

import com.gamehive.lmmrunningsystem.constants.ValidationResultEnum;
import com.gamehive.lmmrunningsystem.dto.LMMDecisionResult;
import com.gamehive.lmmrunningsystem.dto.LMMRequestDTO;

/**
 * 提示词模板构建器
 * 提供各种静态方法来构建不同场景下的AI提示词模板
 * 包括初始提示词、重试提示词、记忆存储提示词
 *
 * @author Cassifa
 * @since 1.0.0
 */
public class PromptTemplateBuilder {

    private static final String SYSTEM_PROMPT_TEMPLATE = 
        "你是一个专业的棋类游戏专家级AI。请严格按照JSON格式返回决策结果。" +
        "你需要根据对话历史来理解游戏进程并做出更好的决策。";

    private static final String GAME_RULES_TEMPLATE = 
        "你是一个专业的{gameType}游戏专家级AI。请分析当前棋盘状态并给出最佳决策。\n\n" +
        "游戏规则：\n{gameRule}\n" +
        "- 0表示空位，1表示玩家已落子位置，{llmFlag}表示你的棋子\n" +
        "- 你需要选择一个空位放置棋子\n\n";

    private static final String HISTORY_TEMPLATE = 
        "历史下棋记录，可以作为参考：\n{historySteps}\n\n";

    private static final String BOARD_STATE_TEMPLATE = 
        "当前棋盘状态：\n{currentMap}\n\n";

    private static final String OUTPUT_FORMAT_TEMPLATE = 
        "请返回你的决策，包含以下字段：\n" +
        "- x: 行坐标(从0开始的整数)\n" +
        "- y: 列坐标(从0开始的整数)\n" +
        "- reason: 详细的决策理由（中文形式）\n\n";

    private static final String ATTENTION_TEMPLATE = 
        "注意：\n" +
        "1. x和y必须是数字，且在棋盘范围内且没有落子过\n" +
        "2. 选择的位置必须是空位（值为0）\n" +
        "3. 请给出清晰的决策理由\n" +
        "4. 你需要特别注意'游戏规则'因为它很可能和你熟知的其它游戏规则看起来类似但实际完全相反";

    private static final String DECISION_MEMORY_TEMPLATE = 
        "局面状态：\n{currentMap}\n" +
        "AI决策：位置({x},{y})\n" +
        "决策理由：{reason}";

    private static final String INVALID_DECISION_MEMORY_TEMPLATE = 
        "局面状态：\n{currentMap}\n" +
        "AI决策：位置({x},{y})\n" +
        "决策理由：{reason}\n" +
        "验证结果：{validationResult}";

    private static final String ERROR_FEEDBACK_TEMPLATE = 
        "上次回答不符合要求。{specificError}请重新分析棋盘并给出正确的决策！\n\n";

    public static String buildInitialPrompt(LMMRequestDTO lmmRequest) {
        StringBuilder prompt = new StringBuilder();
        
        prompt.append(fillTemplate(GAME_RULES_TEMPLATE, 
            "gameType", lmmRequest.getGameType(),
            "gameRule", lmmRequest.getGameRule(),
            "llmFlag", lmmRequest.getLLMFlag()));
        
        if (lmmRequest.getHistorySteps() != null && !lmmRequest.getHistorySteps().trim().isEmpty()) {
            prompt.append(fillTemplate(HISTORY_TEMPLATE, 
                "historySteps", lmmRequest.getHistorySteps()));
        }
        
        prompt.append(fillTemplate(BOARD_STATE_TEMPLATE, 
            "currentMap", lmmRequest.getCurrentMap()));
        
        prompt.append(OUTPUT_FORMAT_TEMPLATE);
        prompt.append(ATTENTION_TEMPLATE);
        
        return prompt.toString();
    }

    public static String buildRetryPrompt(LMMRequestDTO lmmRequest, LMMDecisionResult previousResult) {
        String specificError = buildErrorFeedback(lmmRequest, previousResult);
        
        StringBuilder prompt = new StringBuilder();
        prompt.append(fillTemplate(ERROR_FEEDBACK_TEMPLATE, "specificError", specificError));
        prompt.append(buildInitialPrompt(lmmRequest));
        
        return prompt.toString();
    }

    public static String buildDecisionMemory(LMMRequestDTO lmmRequest, 
                                           LMMDecisionResult decision, 
                                           ValidationResultEnum validationResult) {
        if (validationResult == ValidationResultEnum.VALID) {
            return fillTemplate(DECISION_MEMORY_TEMPLATE,
                "currentMap", lmmRequest.getCurrentMap(),
                "x", String.valueOf(decision.getX()),
                "y", String.valueOf(decision.getY()),
                "reason", decision.getReason() != null ? decision.getReason() : "无理由");
        } else {
            return fillTemplate(INVALID_DECISION_MEMORY_TEMPLATE,
                "currentMap", lmmRequest.getCurrentMap(),
                "x", String.valueOf(decision.getX()),
                "y", String.valueOf(decision.getY()),
                "reason", decision.getReason() != null ? decision.getReason() : "无理由",
                "validationResult", getValidationResultDescription(validationResult));
        }
    }

    public static String buildSystemPrompt() {
        return SYSTEM_PROMPT_TEMPLATE;
    }

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

    private static String buildErrorFeedback(LMMRequestDTO lmmRequest, LMMDecisionResult previousResult) {
        if (previousResult == null) {
            return "解析失败，请确保返回正确的格式。";
        }
        
        ValidationResultEnum validation = previousResult.validate(lmmRequest);
        switch (validation) {
            case POSITION_OCCUPIED:
                return String.format("位置(%d,%d)已经有棋子了。", 
                    previousResult.getX(), previousResult.getY());
            case OUT_OF_BOUNDS:
                return String.format("位置(%d,%d)超出棋盘范围。", 
                    previousResult.getX(), previousResult.getY());
            case INVALID_FORMAT:
                return "返回格式不正确，缺少x或y坐标。";
            default:
                return "未知错误，请重新尝试。";
        }
    }

    private static String getValidationResultDescription(ValidationResultEnum validationResult) {
        switch (validationResult) {
            case VALID:
                return "有效决策";
            case INVALID_FORMAT:
                return "格式无效";
            case OUT_OF_BOUNDS:
                return "超出边界";
            case POSITION_OCCUPIED:
                return "位置已占用";
            default:
                return "未知状态";
        }
    }
} 
