package com.gamehive.comsumer.stratey;

import com.gamehive.comsumer.constants.CellRoleEnum;
import com.gamehive.comsumer.constants.GameStatusEnum;
import java.util.List;

/**
 * @Description 五子棋
 * @Author calciferli
 * @Date 2025/5/21 16:43
 */

public class GoBangStrategy implements GameStrategy {

    @Override
    public void initGameMap(Integer rows, Integer cols, List<List<CellRoleEnum>> map) {
        
    }

    @Override
    public GameStatusEnum checkGameOver(List<List<CellRoleEnum>> map) {
        return null;
    }
}
