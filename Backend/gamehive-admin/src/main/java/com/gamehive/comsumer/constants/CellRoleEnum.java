package com.gamehive.comsumer.constants;

import lombok.AllArgsConstructor;
import lombok.Getter;

/**
 * @Description 棋盘角色枚举
 * @Author calciferli
 * @Date 2025/5/21 15:27
 */
@Getter
@AllArgsConstructor
public enum CellRoleEnum {
    /**
     * 空白格子
     */
    EMPTY,
    /**
     * 玩家A的棋子
     */
    PLAYER_A,
    /**
     * 玩家B的棋子
     */
    PLAYER_B
}
