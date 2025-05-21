package com.gamehive.comsumer.stratey;

import com.gamehive.comsumer.constants.CellRoleEnum;
import com.gamehive.comsumer.constants.GameStatusEnum;
import java.util.List;

/**
 * @Description 反井字棋
 * @Author calciferli
 * @Date 2025/5/21 16:47
 */

public class MisereTicTacToeStrategy implements GameStrategy {

    @Override
    public void initGameMap(Integer rows, Integer cols) {

    }

    @Override
    public GameStatusEnum checkGameOver(List<List<CellRoleEnum>> map) {
        return null;
    }

    @Override
    public int getMinRecordStepCnt() {
        return 0;
    }
}