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
     * 玩家A获胜
     */
    PLAYER_A_WIN(0, "aWin"),

    /**
     * 玩家B获胜
     */
    PLAYER_B_WIN(1, "bWin"),

    /**
     * 游戏平局
     */
    DRAW(2, "draw"),
    /**
     * 游戏未结束
     */
    UNFINISHED(3, "unfinished");


    private final int code;
    private final String name;
}
