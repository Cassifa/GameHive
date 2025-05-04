package com.gamehive.pojo;

import java.util.Date;
import com.fasterxml.jackson.annotation.JsonFormat;
import org.apache.commons.lang3.builder.ToStringBuilder;
import org.apache.commons.lang3.builder.ToStringStyle;
import com.gamehive.common.annotation.Excel;
import com.gamehive.common.core.domain.BaseEntity;

/**
 * 对局记录对象 record
 * 
 * @author Cassifa
 * @date 2025-02-13
 */
public class Record extends BaseEntity
{
    private static final long serialVersionUID = 1L;

    /** 对局编号 */
    private Long recordId;

    /** 游戏类别 */
    @Excel(name = "游戏类别")
    private Long gameTypeId;

    /** 对局时间 */
    @JsonFormat(pattern = "yyyy-MM-dd")
    @Excel(name = "对局时间", width = 30, dateFormat = "yyyy-MM-dd")
    private Date recordTime;

    /** 是否与AI对战 */
    @Excel(name = "是否与AI对战")
    private Boolean isPkAi;

    /** 对战AI */
    @Excel(name = "对战AI")
    private Long aiGameId;

    /** 玩家A是否先手 */
    @Excel(name = "玩家A是否先手")
    private Boolean isAFirst;

    /** 赢家 */
    @Excel(name = "赢家")
    private Long winner;

    /** A玩家ID */
    @Excel(name = "A玩家ID")
    private Long playerAId;

    /** B玩家ID */
    @Excel(name = "B玩家ID")
    private Long playerBId;

    /** 玩家A操作序列 */
    @Excel(name = "玩家A操作序列")
    private String playerAPieces;

    /** 玩家B操作序列 */
    @Excel(name = "玩家B操作序列")
    private String playerBPieces;

    public void setRecordId(Long recordId) 
    {
        this.recordId = recordId;
    }

    public Long getRecordId() 
    {
        return recordId;
    }
    public void setGameTypeId(Long gameTypeId) 
    {
        this.gameTypeId = gameTypeId;
    }

    public Long getGameTypeId() 
    {
        return gameTypeId;
    }
    public void setRecordTime(Date recordTime) 
    {
        this.recordTime = recordTime;
    }

    public Date getRecordTime() 
    {
        return recordTime;
    }
    public void setIsPkAi(Boolean isPkAi) 
    {
        this.isPkAi = isPkAi;
    }

    public Boolean getIsPkAi() 
    {
        return isPkAi;
    }
    public void setAiGameId(Long aiGameId) 
    {
        this.aiGameId = aiGameId;
    }

    public Long getAiGameId() 
    {
        return aiGameId;
    }
    public void setIsAFirst(Boolean isAFirst) 
    {
        this.isAFirst = isAFirst;
    }

    public Boolean getIsAFirst() 
    {
        return isAFirst;
    }
    public void setWinner(Long winner) 
    {
        this.winner = winner;
    }

    public Long getWinner() 
    {
        return winner;
    }
    public void setPlayerAId(Long playerAId) 
    {
        this.playerAId = playerAId;
    }

    public Long getPlayerAId() 
    {
        return playerAId;
    }
    public void setPlayerBId(Long playerBId) 
    {
        this.playerBId = playerBId;
    }

    public Long getPlayerBId() 
    {
        return playerBId;
    }
    public void setPlayerAPieces(String playerAPieces) 
    {
        this.playerAPieces = playerAPieces;
    }

    public String getPlayerAPieces() 
    {
        return playerAPieces;
    }
    public void setPlayerBPieces(String playerBPieces) 
    {
        this.playerBPieces = playerBPieces;
    }

    public String getPlayerBPieces() 
    {
        return playerBPieces;
    }

    @Override
    public String toString() {
        return new ToStringBuilder(this,ToStringStyle.MULTI_LINE_STYLE)
            .append("recordId", getRecordId())
            .append("gameTypeId", getGameTypeId())
            .append("recordTime", getRecordTime())
            .append("isPkAi", getIsPkAi())
            .append("aiGameId", getAiGameId())
            .append("isAFirst", getIsAFirst())
            .append("winner", getWinner())
            .append("playerAId", getPlayerAId())
            .append("playerBId", getPlayerBId())
            .append("playerAPieces", getPlayerAPieces())
            .append("playerBPieces", getPlayerBPieces())
            .toString();
    }
}
