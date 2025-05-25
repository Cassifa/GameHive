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
    public GameStatusEnum checkGameOver(List<List<CellRoleEnum>> map) {
        int size = map.size();

        // 检查每个位置
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                CellRoleEnum currentRole = map.get(i).get(j);
                if (currentRole == CellRoleEnum.EMPTY) continue;

                // 检查四个方向
                for (int[] dir : DIRECTIONS) {
                    int count = 1;
                    // 正向检查
                    for (int step = 1; step < 5; step++) {
                        int newI = i + dir[0] * step;
                        int newJ = j + dir[1] * step;
                        if (newI < 0 || newI >= size || newJ < 0 || newJ >= size) break;
                        if (map.get(newI).get(newJ) != currentRole) break;
                        count++;
                    }
                    // 反向检查
                    for (int step = 1; step < 5; step++) {
                        int newI = i - dir[0] * step;
                        int newJ = j - dir[1] * step;
                        if (newI < 0 || newI >= size || newJ < 0 || newJ >= size) break;
                        if (map.get(newI).get(newJ) != currentRole) break;
                        count++;
                    }

                    if (count >= 5) {
                        return currentRole == CellRoleEnum.PLAYER_A ?
                                GameStatusEnum.PLAYER_A_WIN : GameStatusEnum.PLAYER_B_WIN;
                    }
                }
            }
        }

        // 检查是否平局
        boolean isFull = true;
        for (List<CellRoleEnum> row : map) {
            for (CellRoleEnum cell : row) {
                if (cell == CellRoleEnum.EMPTY) {
                    isFull = false;
                    break;
                }
            }
        }

        return isFull ? GameStatusEnum.DRAW : GameStatusEnum.UNFINISHED;
    }
}
