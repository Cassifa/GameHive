package com.gamehive.constants;

/**
 * AI算法类型枚举
 * @author Claude
 */
public enum AIAlgorithmTypeEnum {
    MINIMAX(1, "极小值极大搜索", "Minimax"),
    MCTS(2, "蒙特卡洛树搜索", "Monte Carlo Tree Search"),
    DEEP_RL(4, "深度强化学习", "Deep Reinforcement Learning"),
    NEGAMAX(5, "负极大值搜索", "Negamax Algorithm");

    private final int code;
    private final String chineseName;
    private final String englishName;

    AIAlgorithmTypeEnum(int code, String chineseName, String englishName) {
        this.code = code;
        this.chineseName = chineseName;
        this.englishName = englishName;
    }

    public int getCode() {
        return code;
    }

    public String getChineseName() {
        return chineseName;
    }

    public String getEnglishName() {
        return englishName;
    }

    public static AIAlgorithmTypeEnum fromCode(int code) {
        for (AIAlgorithmTypeEnum value : values()) {
            if (value.getCode() == code) {
                return value;
            }
        }
        throw new IllegalArgumentException("未知AI算法类型: " + code);
    }
} 