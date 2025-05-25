package com.gamehive.comsumer.stratey;

import com.gamehive.comsumer.constants.CellRoleEnum;
import com.gamehive.comsumer.constants.GameStatusEnum;

import java.util.ArrayList;
import java.util.List;

/**
 * @Description 不围棋
 * @Author calciferli
 * @Date 2025/5/21 16:44
 */

public class AntiGoStrategy implements GameStrategy {
    @Override
    public GameStatusEnum checkGameOver(List<List<CellRoleEnum>> map) {
        int size = map.size();
        List<List<Boolean>> visited = new ArrayList<>();

        // 初始化访问数组
        for (int i = 0; i < size; i++) {
            List<Boolean> row = new ArrayList<>();
            for (int j = 0; j < size; j++) {
                row.add(false);
            }
            visited.add(row);
        }

        // 检查每个位置
        for (int i = 0; i < size; i++) {
            for (int j = 0; j < size; j++) {
                CellRoleEnum currentRole = map.get(i).get(j);
                if (currentRole == CellRoleEnum.EMPTY) continue;

                // 检查周围五个位置
                for (int[] dir : DIRECTIONS) {
                    int newI = i + dir[0];
                    int newJ = j + dir[1];

                    if (newI < 0 || newI >= size || newJ < 0 || newJ >= size
                            || map.get(newI).get(newJ) == CellRoleEnum.EMPTY) {
                        continue;
                    }

                    // 清除访问状态
                    clearVisited(visited);

                    // 如果导致某个位置无气，则对方获胜
                    if (!isAlive(map, visited, newI, newJ)) {
                        return currentRole == CellRoleEnum.PLAYER_A ?
                                GameStatusEnum.PLAYER_B_WIN : GameStatusEnum.PLAYER_A_WIN;
                    }
                }
            }
        }

        return GameStatusEnum.UNFINISHED;
    }

    private void clearVisited(List<List<Boolean>> visited) {
        for (List<Boolean> row : visited) {
            for (int i = 0; i < row.size(); i++) {
                row.set(i, false);
            }
        }
    }

    private boolean isAlive(List<List<CellRoleEnum>> board, List<List<Boolean>> visited, int x, int y) {
        visited.get(x).set(y, true);
        int size = board.size();

        // 检查四个方向
        int[][] directions = {{-1, 0}, {1, 0}, {0, -1}, {0, 1}};
        for (int[] dir : directions) {
            int newX = x + dir[0];
            int newY = y + dir[1];

            if (newX < 0 || newX >= size || newY < 0 || newY >= size) {
                continue;
            }

            // 如果旁边有空位，说明有气
            if (board.get(newX).get(newY) == CellRoleEnum.EMPTY) {
                return true;
            }

            // 如果是同色且未访问过的棋子，检查它是否有气
            if (board.get(newX).get(newY) == board.get(x).get(y)
                    && !visited.get(newX).get(newY)
                    && isAlive(board, visited, newX, newY)) {
                return true;
            }
        }

        return false;
    }
}
