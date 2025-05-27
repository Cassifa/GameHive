package com.gamehive.constants;

import lombok.AllArgsConstructor;
import lombok.Getter;

/**
 * 游戏模式枚举
 *
 * @author Cassifa
 */
@Getter
@AllArgsConstructor
public enum GameModeEnum {
    LOCAL_GAME(0, "本地对战", "Local Game"),
    LMM_GAME(1, "与大模型对战", "AI Game"),
    ONLINE_GAME(2, "联机对战", "Online Game");

    private final int code;
    private final String chineseName;
    private final String englishName;

    /**
     * 根据代码获取枚举
     *
     * @param code 代码
     * @return 对应的枚举值
     * @throws IllegalArgumentException 如果找不到匹配的枚举值
     */
    public static GameModeEnum fromCode(int code) {
        for (GameModeEnum value : values()) {
            if (value.getCode() == code) {
                return value;
            }
        }
        throw new IllegalArgumentException("未知游戏模式: " + code);
    }

    /**
     * 根据中文名称获取枚举
     *
     * @param chineseName 中文名称
     * @return 对应的枚举值
     * @throws IllegalArgumentException 如果找不到匹配的枚举值
     */
    public static GameModeEnum fromChineseName(String chineseName) {
        for (GameModeEnum value : values()) {
            if (value.getChineseName().equals(chineseName)) {
                return value;
            }
        }
        throw new IllegalArgumentException("未知游戏模式: " + chineseName);
    }

    /**
     * 根据英文名称获取枚举
     *
     * @param englishName 英文名称
     * @return 对应的枚举值
     * @throws IllegalArgumentException 如果找不到匹配的枚举值
     */
    public static GameModeEnum fromEnglishName(String englishName) {
        for (GameModeEnum value : values()) {
            if (value.getEnglishName().equals(englishName)) {
                return value;
            }
        }
        throw new IllegalArgumentException("未知游戏模式: " + englishName);
    }
} 