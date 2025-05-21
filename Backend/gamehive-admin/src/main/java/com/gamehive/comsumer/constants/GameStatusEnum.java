package com.gamehive.comsumer.constants;

import lombok.AllArgsConstructor;
import lombok.Getter;

/**
 * @Description 游戏胜利状态枚举
 * @Author calciferli
 * @Date 2025/5/21 16:36
 */
@Getter
@AllArgsConstructor
public enum GameStatusEnum {
    /**
     * 游戏未结束
     */
    UNFINISHED(0, "unfinished"),

    /**
     * 游戏平局
     */
    DRAW(1, "draw"),

    /**
     * 玩家A获胜
     */
    PLAYER_A_WIN(2, "player a win"),

    /**
     * 玩家B获胜
     */
    PLAYER_B_WIN(3, "player b win");

    private final int code;
    private final String name;
}
