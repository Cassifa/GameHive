package com.gamehive.lmmrunningsystem.dto;

import com.gamehive.lmmrunningsystem.constants.ValidationResultEnum;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

/**
 * 大模型决策结果实体类
 * 封装了大模型返回的落子决策信息，包括坐标位置和决策理由
 *
 * @author Cassifa
 * @since 1.0.0
 */
@Data
@NoArgsConstructor
@AllArgsConstructor
public class LMMDecisionResult {

    /**
     * X坐标（行坐标）
     * 表示在棋盘上的行位置，从0开始计数
     */
    private Integer x;

    /**
     * Y坐标（列坐标）
     * 表示在棋盘上的列位置，从0开始计数
     */
    private Integer y;

    /**
     * 决策理由或深度思考内容
     * 大模型给出此次落子决策的详细分析和理由
     */
    private String reason;

    /**
     * 验证结果是否有效（基础验证）
     * 仅检查坐标是否为空，不涉及游戏逻辑验证
     *
     * @return 如果x和y都不为null则返回true，否则返回false
     */
    public boolean isValid() {
        return x != null && y != null;
    }

    /**
     * 验证结果是否有效（完整验证）
     * 结合游戏状态进行完整的决策有效性验证
     *
     * @param lmmRequest 大模型请求对象，包含当前游戏状态信息
     * @return 如果坐标有效且位置可用则返回true，否则返回false
     */
    public boolean isValid(LMMRequest lmmRequest) {
        return validate(lmmRequest) == ValidationResultEnum.VALID;
    }

    /**
     * 验证决策结果的详细验证方法
     * 对决策结果进行全面验证，包括格式、边界和位置占用检查
     *
     * @param lmmRequest 大模型请求对象，包含棋盘状态和游戏配置信息
     * @return ValidationResult 详细的验证结果枚举值
     */
    public ValidationResultEnum validate(LMMRequest lmmRequest) {
        if (x == null || y == null) {
            return ValidationResultEnum.INVALID_FORMAT;
        }
        int gridSize = Integer.parseInt(lmmRequest.getGridSize());
        if (x < 0 || x >= gridSize || y < 0 || y >= gridSize) {
            return ValidationResultEnum.OUT_OF_BOUNDS;
        }
        String[] rows = lmmRequest.getCurrentMap().split("\n");
        if (x < rows.length && y < rows[x].length()) {
            char position = rows[x].charAt(y);
            if (position != '0') {
                return ValidationResultEnum.POSITION_OCCUPIED;
            }
        }
        return ValidationResultEnum.VALID;
    }
} 