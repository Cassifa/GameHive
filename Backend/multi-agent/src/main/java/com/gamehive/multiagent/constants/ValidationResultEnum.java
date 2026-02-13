package com.gamehive.multiagent.constants;

/**
 * 大模型决策结果验证枚举
 *
 * @author Cassifa
 * @since 1.0.0
 */
public enum ValidationResultEnum {
    /**
     * 验证通过，决策结果正确
     */
    VALID("验证通过"),

    /**
     * 坐标超出棋盘边界范围
     */
    OUT_OF_BOUNDS("坐标超出边界"),

    /**
     * 目标位置已经被占用，不能重复落子
     */
    POSITION_OCCUPIED("位置已被占用"),

    /**
     * 决策结果格式错误，缺少必要的坐标信息
     */
    INVALID_FORMAT("返回格式不正确");

    private final String description;

    ValidationResultEnum(String description) {
        this.description = description;
    }

    public String getDescription() {
        return description;
    }
}
