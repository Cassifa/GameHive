package com.gamehive.comsumer.Game;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

/**
 * 游戏单元格实体类
 */
@Data
@NoArgsConstructor
@AllArgsConstructor
public class Cell {

    /**
     * 单元格坐标位置
     * x: 横坐标
     * y: 纵坐标
     */
    private Integer x, y;
}
