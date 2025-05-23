package com.gamehive.comsumer.Game;

import com.gamehive.common.annotation.Excel;
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
     * 玩家的编号，用户编号或SpecialPlayerEnum.code(对于LMM为-2)
     */
    private Long userId;
    /**
     * 名称
     */
    private String userName;
    /**
     * 玩家角色类型
     */
    private SpecialPlayerEnum playerType;
    /**
     * 玩家操作序列
     */
    private List<Cell> steps;

}
