package com.gamehive.lmmrunningsystem.service.impl;

import com.alibaba.fastjson2.JSON;
import com.alibaba.fastjson2.JSONObject;
import com.gamehive.lmmrunningsystem.constants.ValidationResultEnum;
import com.gamehive.lmmrunningsystem.dto.LMMDecisionResult;
import com.gamehive.lmmrunningsystem.dto.LMMRequest;
import org.springframework.ai.chat.client.ChatClient;
import org.springframework.beans.factory.annotation.Qualifier;
import org.springframework.beans.factory.annotation.Value;
import org.springframework.stereotype.Service;

/**
 * DeepSeek AI服务类
 * 负责与DeepSeek大模型进行交互，获取游戏决策结果
 * 包含智能重试机制和结果验证功能
 *
 * @author Cassifa
 * @since 1.0.0
 */
@Service
public class DeepSeekAIService {

    /**
     * Spring AI ChatClient实例，用于与大模型通信
     */
    private final ChatClient chatClient;

    /**
     * 最大重试次数配置
     * 从配置文件中读取，默认值为3次
     */
    @Value("${lmm.max-retry-count:3}")
    private int maxRetryCount;

    /**
     * 构造函数，初始化ChatClient
     *
     * @param gameDecisionChatClient Spring AI配置的游戏决策ChatClient实例
     */
    public DeepSeekAIService(@Qualifier("gameDecisionChatClient") ChatClient gameDecisionChatClient) {
        this.chatClient = gameDecisionChatClient;
    }

    /**
     * 获取大模型决策的主要方法
     * 包含重试机制，确保返回有效的决策结果
     *
     * @param lmmRequest 大模型请求对象，包含游戏状态和配置信息
     * @return LMMDecisionResult 大模型的决策结果，包含坐标和理由
     */
    public LMMDecisionResult getDecision(LMMRequest lmmRequest) {
        String prompt = buildPrompt(lmmRequest);
        LMMDecisionResult result = null;
        int retryCount = 0;

        while (retryCount < maxRetryCount) {
            try {
                String response = chatClient.prompt()
                        .user(prompt)
                        .call()
                        .content();

                System.out.println("大模型响应 (第" + (retryCount + 1) + "次): " + response);

                result = parseDecisionResult(response);

                if (result != null && result.isValid(lmmRequest)) {
                    System.out.println("大模型决策成功: x=" + result.getX() + ", y=" + result.getY());
                    break;
                } else {
                    retryCount++;
                    if (retryCount < maxRetryCount) {
                        prompt = buildRetryPrompt(lmmRequest, result);
                        System.out.println("第" + retryCount + "次重试，原因：结果格式不正确");
                    }
                }
            } catch (Exception e) {
                retryCount++;
                System.out.println("大模型调用失败 (第" + retryCount + "次): " + e.getMessage());
            }
        }

        if (result == null || !result.isValid(lmmRequest)) {
            System.out.println("大模型决策失败，使用默认策略");
            result = getDefaultDecision(lmmRequest);
        }

        return result;
    }

    /**
     * 构建发送给大模型的初始提示词
     * 包含游戏规则、棋盘状态和输出格式要求
     *
     * @param lmmRequest 大模型请求对象
     * @return String 格式化的提示词字符串
     */
    private String buildPrompt(LMMRequest lmmRequest) {
        return "你是一个专业的" + lmmRequest.getGameType() + "游戏AI。请分析当前棋盘状态并给出最佳决策。\n\n" +
                "游戏规则：\n" +
                "- 棋盘大小：" + lmmRequest.getGridSize() + " x " + lmmRequest.getGridSize() + "\n" +
                "- 0表示空位，1表示玩家1，2表示玩家2\n" +
                "- 你需要选择一个空位放置棋子\n\n" +
                "当前棋盘状态：\n" +
                lmmRequest.getCurrentMap() + "\n\n" +
                "请严格按照以下JSON格式返回决策：\n" +
                "{\n" +
                "  \"x\": 行坐标(从0开始),\n" +
                "  \"y\": 列坐标(从0开始),\n" +
                "  \"reason\": \"详细的决策理由\"\n" +
                "}\n\n" +
                "注意：\n" +
                "1. 必须返回有效的JSON格式\n" +
                "2. x和y必须是数字，且在棋盘范围内且没有落子过\n" +
                "3. 选择的位置必须是空位（值为0）";
    }

    /**
     * 构建重试时的提示词
     * 根据上次失败的原因给出具体的错误反馈
     *
     * @param lmmRequest     大模型请求对象
     * @param previousResult 上次的决策结果
     * @return String 包含错误反馈的重试提示词
     */
    private String buildRetryPrompt(LMMRequest lmmRequest, LMMDecisionResult previousResult) {
        StringBuilder feedback = new StringBuilder();
        feedback.append("上次回答不符合要求。");

        if (previousResult != null) {
            ValidationResultEnum validation = previousResult.validate(lmmRequest);
            switch (validation) {
                case POSITION_OCCUPIED:
                    feedback.append("位置(").append(previousResult.getX()).append(",").append(previousResult.getY()).append(")已经有棋子了。");
                    break;
                case OUT_OF_BOUNDS:
                    feedback.append("位置(").append(previousResult.getX()).append(",").append(previousResult.getY()).append(")超出棋盘范围。");
                    break;
                case INVALID_FORMAT:
                    feedback.append("返回格式不正确，缺少x或y坐标。");
                    break;
            }
        }

        feedback.append("请重新分析棋盘并给出正确的决策！\n\n");
        feedback.append(buildPrompt(lmmRequest));

        return feedback.toString();
    }

    /**
     * 解析大模型返回结果
     *
     * @param content 大模型返回的原始文本内容
     * @return LMMDecisionResult 解析后的决策结果，解析失败时返回null
     */
    private LMMDecisionResult parseDecisionResult(String content) {
        try {
            // 提取JSON部分
            String jsonStr = extractJson(content);
            if (jsonStr == null) {
                return null;
            }

            // 使用FastJSON解析
            JSONObject jsonObject = JSON.parseObject(jsonStr);
            Integer x = jsonObject.getInteger("x");
            Integer y = jsonObject.getInteger("y");
            String reason = jsonObject.getString("reason");

            if (x != null && y != null) {
                return new LMMDecisionResult(x, y, reason != null ? reason : "AI决策");
            }
        } catch (Exception e) {
            System.out.println("JSON解析失败: " + e.getMessage());
        }
        return null;
    }

    /**
     * 从文本中提取JSON字符串
     * 查找第一个'{'和最后一个'}'之间的内容
     *
     * @param content 包含JSON的文本内容
     * @return String 提取的JSON字符串，未找到时返回null
     */
    private String extractJson(String content) {
        int start = content.indexOf('{');
        int end = content.lastIndexOf('}');
        if (start >= 0 && end > start) {
            return content.substring(start, end + 1);
        }
        return null;
    }

    /**
     * 获取默认决策结果
     * 当大模型决策失败时，使用简单策略选择第一个可用位置
     *
     * @param lmmRequest 大模型请求对象
     * @return LMMDecisionResult 默认的决策结果
     */
    private LMMDecisionResult getDefaultDecision(LMMRequest lmmRequest) {
        String[] rows = lmmRequest.getCurrentMap().split("\n");
        int gridSize = Integer.parseInt(lmmRequest.getGridSize());

        for (int i = 0; i < gridSize; i++) {
            for (int j = 0; j < gridSize; j++) {
                if (i < rows.length && j < rows[i].length() && rows[i].charAt(j) == '0') {
                    return new LMMDecisionResult(i, j, "默认策略：选择第一个空位");
                }
            }
        }
        return new LMMDecisionResult(0, 0, "默认策略：无可用位置");
    }
} 