package com.gamehive.constants;

import lombok.AllArgsConstructor;
import lombok.Getter;

/**
 * 特殊玩家枚举,若不是玩家则code为其标识符，写入对局记录数据库
 *
 * @author lff
 */
@Getter
@AllArgsConstructor
public enum SpecialPlayerEnum {
    GUEST(0, "游客", "Guest"),
    AI(-1, "AI", "Artificial Intelligence"),
    LMM(-2, "大模型", "LMM"),
    PLAYER(1, "玩家", "Guest");

    private final int code;
    private final String chineseName;
    private final String englishName;


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