package com.gamehive.constants;

/**
 * 游戏类型枚举
 * @author Cassifa
 */
public enum GameTypeEnum {
    GOBANG(1, "五子棋", "Gobang"),
    GOBANG_88(2, "8*8五子棋", "8*8Gobang"),
    ANTI_GO(3, "不围棋", "ReverseGo"),
    TIC_TAC_TOE(4, "井字棋", "Tic-Tac-Toe"),
    MISERE_TIC_TAC_TOE(5, "反井字棋", "Misère Tic-Tac-Toe");

    private final int code;
    private final String chineseName;
    private final String englishName;

    GameTypeEnum(int code, String chineseName, String englishName) {
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

    public static GameTypeEnum fromCode(int code) {
        for (GameTypeEnum value : values()) {
            if (value.getCode() == code) {
                return value;
            }
        }
        throw new IllegalArgumentException("未知游戏类型: " + code);
    }
} 