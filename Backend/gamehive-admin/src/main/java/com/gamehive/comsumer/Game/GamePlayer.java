package com.gamehive.comsumer.Game;

import com.gamehive.constants.SpecialPlayerEnum;
import java.util.List;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@AllArgsConstructor
@NoArgsConstructor
public class GamePlayer {

    /**
     * 单玩家编号
     */
    private Long userId;
    /**
     * 玩家角色类型
     */
    private SpecialPlayerEnum playerType;
    /**
     * 玩家操作序列
     */
    private List<Cell> steps;

}
