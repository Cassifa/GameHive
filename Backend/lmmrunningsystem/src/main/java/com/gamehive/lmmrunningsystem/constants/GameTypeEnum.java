package com.gamehive.lmmrunningsystem.constants;

import lombok.AllArgsConstructor;
import lombok.Getter;

/**
 * 游戏类型枚举
 *
 * @author Cassifa
 */
@Getter
@AllArgsConstructor
public enum GameTypeEnum {
    GOBANG(1, "五子棋", "Gobang"),
    GOBANG_88(2, "8*8五子棋", "8*8Gobang"),
    ANTI_GO(3, "不围棋", "ReverseGo"),
    TIC_TAC_TOE(4, "井字棋", "Tic-Tac-Toe"),
    MISERE_TIC_TAC_TOE(5, "反井字棋", "Misère Tic-Tac-Toe");

    private final int code;
    private final String chineseName;
    private final String englishName;


    public static GameTypeEnum fromCode(int code) {
        for (GameTypeEnum value : values()) {
            if (value.getCode() == code) {
                return value;
            }
        }
        throw new IllegalArgumentException("未知游戏类型: " + code);
    }

    /**
     * 根据中文名称获取枚举
     *
     * @param chineseName 中文名称
     * @return 对应的枚举值
     * @throws IllegalArgumentException 如果找不到匹配的枚举值
     */
    public static GameTypeEnum fromChineseName(String chineseName) {
        for (GameTypeEnum value : values()) {
            if (value.getChineseName().equals(chineseName)) {
                return value;
            }
        }
        throw new IllegalArgumentException("未知游戏类型: " + chineseName);
    }
} 