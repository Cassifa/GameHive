package com.gamehive.constants;

/**
 * 角色类型枚举
 * @author Claude
 */
public enum RoleEnum {
    AI("AI", "AI"),
    PLAYER("玩家", "Player"),
    EMPTY("空", "Empty"),
    DRAW("平局", "Draw");

    private final String chineseName;
    private final String englishName;

    RoleEnum(String chineseName, String englishName) {
        this.chineseName = chineseName;
        this.englishName = englishName;
    }

    public String getChineseName() {
        return chineseName;
    }

    public String getEnglishName() {
        return englishName;
    }

    public boolean isVictory() {
        return this == AI || this == PLAYER;
    }
} 