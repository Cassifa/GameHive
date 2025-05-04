package com.gamehive.constants;

import java.util.ArrayList;
import java.util.List;

/**
 * 游戏难度等级枚举
 * @author Claude
 */
public enum DifficultyLevelEnum {
    LEVEL_1(1, "一级", "Level 1"),
    LEVEL_2(2, "二级", "Level 2"),
    LEVEL_3(3, "三级", "Level 3"),
    LEVEL_4(4, "四级", "Level 4"),
    LEVEL_5(5, "五级", "Level 5");

    private final int level;
    private final String chineseName;
    private final String englishName;

    DifficultyLevelEnum(int level, String chineseName, String englishName) {
        this.level = level;
        this.chineseName = chineseName;
        this.englishName = englishName;
    }

    public int getLevel() {
        return level;
    }

    public String getChineseName() {
        return chineseName;
    }

    public String getEnglishName() {
        return englishName;
    }

    public static List<DifficultyLevelEnum> getLevelRange(int maxLevel) {
        if (maxLevel < 1 || maxLevel > 5) {
            throw new IllegalArgumentException("难度等级必须是1-5");
        }

        List<DifficultyLevelEnum> levels = new ArrayList<>();
        for (DifficultyLevelEnum level : values()) {
            if (level.getLevel() <= maxLevel) {
                levels.add(level);
            }
        }
        return levels;
    }

    public boolean isGreaterThan(DifficultyLevelEnum other) {
        return this.level > other.level;
    }

    public boolean isLessThan(DifficultyLevelEnum other) {
        return this.level < other.level;
    }

    public boolean isEqualTo(DifficultyLevelEnum other) {
        return this.level == other.level;
    }

    public static DifficultyLevelEnum fromLevel(int level) {
        if (level < 1 || level > 5) {
            throw new IllegalArgumentException("难度等级必须是1-5");
        }
        
        for (DifficultyLevelEnum value : values()) {
            if (value.getLevel() == level) {
                return value;
            }
        }
        throw new IllegalArgumentException("未知难度等级: " + level);
    }
} 