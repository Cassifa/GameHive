package com.gamehive.constants;

/**
 * 特殊玩家枚举
 * @author Claude
 */
public enum SpecialPlayerEnum {
    GUEST(0, "游客", "Guest"),
    AI(-1, "人工智能", "Artificial Intelligence");

    private final int code;
    private final String chineseName;
    private final String englishName;

    SpecialPlayerEnum(int code, String chineseName, String englishName) {
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

    public static SpecialPlayerEnum fromCode(int code) {
        for (SpecialPlayerEnum value : values()) {
            if (value.getCode() == code) {
                return value;
            }
        }
        return null;
    }

    public static boolean isSpecialPlayer(Integer playerId) {
        if (playerId == null) {
            return false;
        }
        return playerId == GUEST.getCode() || playerId == AI.getCode();
    }
} 