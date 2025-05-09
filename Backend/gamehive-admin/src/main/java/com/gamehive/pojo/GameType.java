package com.gamehive.pojo;

import org.apache.commons.lang3.builder.ToStringBuilder;
import org.apache.commons.lang3.builder.ToStringStyle;
import com.gamehive.common.annotation.Excel;
import com.gamehive.common.core.domain.BaseEntity;

/**
 * 游戏类型对象 game_type
 *
 * @author Cassifa
 * @date 2025-05-05
 */
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

    public void setGameId(Long gameId) {
        this.gameId = gameId;
    }

    public Long getGameId() {
        return gameId;
    }

    public void setGameName(String gameName) {
        this.gameName = gameName;
    }

    public String getGameName() {
        return gameName;
    }

    public void setGameIntroduction(String gameIntroduction) {
        this.gameIntroduction = gameIntroduction;
    }

    public String getGameIntroduction() {
        return gameIntroduction;
    }

    public void setGameRule(String gameRule) {
        this.gameRule = gameRule;
    }

    public String getGameRule() {
        return gameRule;
    }

    public void setBoardSize(Long boardSize) {
        this.boardSize = boardSize;
    }

    public Long getBoardSize() {
        return boardSize;
    }

    public void setMinValidPieces(Long minValidPieces) {
        this.minValidPieces = minValidPieces;
    }

    public Long getMinValidPieces() {
        return minValidPieces;
    }

    public void setIsCellCenter(Boolean isCellCenter) {
        this.isCellCenter = isCellCenter;
    }

    public Boolean getIsCellCenter() {
        return isCellCenter;
    }

    @Override
    public String toString() {
        return new ToStringBuilder(this, ToStringStyle.MULTI_LINE_STYLE)
                .append("gameId", getGameId())
                .append("gameName", getGameName())
                .append("gameIntroduction", getGameIntroduction())
                .append("gameRule", getGameRule())
                .append("boardSize", getBoardSize())
                .append("minValidPieces", getMinValidPieces())
                .append("isCellCenter", getIsCellCenter())
                .toString();
    }
}
