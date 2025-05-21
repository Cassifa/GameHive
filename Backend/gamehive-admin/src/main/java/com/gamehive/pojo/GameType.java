package com.gamehive.pojo;

import com.gamehive.common.annotation.Excel;
import com.gamehive.common.core.domain.BaseEntity;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

/**
 * 游戏类型对象 game_type
 *
 * @author Cassifa
 * @date 2025-05-05
 */
@Data
@AllArgsConstructor
@NoArgsConstructor
public class GameType extends BaseEntity {

    private static final long serialVersionUID = 1L;

    /**
     * 游戏ID
     */
    private Long gameId;

    /**
     * 游戏名称
     */
    @Excel(name = "游戏名称")
    private String gameName;

    /**
     * 游戏规则简介
     */
    @Excel(name = "游戏规则简介")
    private String gameIntroduction;

    /**
     * 游戏规则
     */
    private String gameRule;

    /**
     * 棋盘格数
     */
    @Excel(name = "棋盘格数")
    private Long boardSize;

    /**
     * 最低记录有效步数
     */
    @Excel(name = "最低记录有效步数")
    private Long minValidPieces;

    /**
     * 是否在格子中落子
     */
    @Excel(name = "是否在格子中落子")
    private Boolean isCellCenter;
}
