package com.gamehive.comsumer.stratey;

import com.gamehive.comsumer.constants.CellRoleEnum;
import com.gamehive.comsumer.constants.GameStatusEnum;
import java.util.List;

/**
 * 游戏策略接口 - 定义游戏状态检查的核心行为
 *
 * @Description 检查游戏是否结束策略接口
 * @Author calciferli
 * @Date 2025/5/21 16:31
 */
public interface GameStrategy {

    /**
     * 初始化游戏地图
     *
     * @param rows 地图行数
     * @param cols 地图列数
     * @param map 二维单元格列表表示的地图
     */
    void initGameMap(Integer rows, Integer cols, List<List<CellRoleEnum>> map);

    /**
     * 检查游戏是否结束
     *
     * @param map 当前游戏地图状态
     * @return 游戏状态枚举值(如胜利 / 失败 / 进行中等)
     */
    GameStatusEnum checkGameOver(List<List<CellRoleEnum>> map);
}
