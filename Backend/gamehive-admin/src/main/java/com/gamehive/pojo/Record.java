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
 * @date 2025-05-05
 */
public class Record extends BaseEntity
{
    private static final long serialVersionUID = 1L;

    /** 对局id */
    private Long recordId;

    /** 本局对局对应的game_id */
    private Long gameTypeId;

    /** 本局游戏名称 */
    @Excel(name = "本局游戏名称")
    private String gameTypeName;

    /** 对局结束时间 */
    @JsonFormat(pattern = "yyyy-MM-dd")
    @Excel(name = "对局结束时间", width = 30, dateFormat = "yyyy-MM-dd")
    private Date recordTime;

    /** 是否是与ai对局 */
    @Excel(name = "是否是与ai对局", readConverterExp = "true=是,false=否")
    private Boolean isPkAi;

    /** 对战的算法编号,如果为匹配对战则为-1 */
    private Long algorithmId;

    /** 算法名称 */
    @Excel(name = "算法名称")
    private String algorithmName;

    /** 赢家（0-先手 1-后手 2-平局 3-无） */
    @Excel(name = "赢家", readConverterExp = "0=先手,1=后手,2=平局,3=无")
    private Long winner;

    /** 先手id */
    private Long firstPlayerId;

    /** 先手玩家 */
    @Excel(name = "先手玩家")
    private String firstPlayer;

    /** 后手 */
    private Long secondPlayerId;

    /** 后手玩家 */
    @Excel(name = "后手玩家")
    private String secondPlayerName;

    /** 先手玩家操作序列，json格式 */
    private String firstPlayerPieces;

    /** 后手玩家操作序列，json格式 */
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
    public void setGameTypeName(String gameTypeName) 
    {
        this.gameTypeName = gameTypeName;
    }

    public String getGameTypeName() 
    {
        return gameTypeName;
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
    public void setAlgorithmId(Long algorithmId) 
    {
        this.algorithmId = algorithmId;
    }

    public Long getAlgorithmId() 
    {
        return algorithmId;
    }
    public void setAlgorithmName(String algorithmName) 
    {
        this.algorithmName = algorithmName;
    }

    public String getAlgorithmName() 
    {
        return algorithmName;
    }
    public void setWinner(Long winner) 
    {
        this.winner = winner;
    }

    public Long getWinner() 
    {
        return winner;
    }
    public void setFirstPlayerId(Long firstPlayerId) 
    {
        this.firstPlayerId = firstPlayerId;
    }

    public Long getFirstPlayerId() 
    {
        return firstPlayerId;
    }
    public void setFirstPlayer(String firstPlayer) 
    {
        this.firstPlayer = firstPlayer;
    }

    public String getFirstPlayer() 
    {
        return firstPlayer;
    }
    public void setSecondPlayerId(Long secondPlayerId) 
    {
        this.secondPlayerId = secondPlayerId;
    }

    public Long getSecondPlayerId() 
    {
        return secondPlayerId;
    }
    public void setSecondPlayerName(String secondPlayerName) 
    {
        this.secondPlayerName = secondPlayerName;
    }

    public String getSecondPlayerName() 
    {
        return secondPlayerName;
    }
    public void setFirstPlayerPieces(String firstPlayerPieces) 
    {
        this.firstPlayerPieces = firstPlayerPieces;
    }

    public String getFirstPlayerPieces() 
    {
        return firstPlayerPieces;
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
            .append("gameTypeName", getGameTypeName())
            .append("recordTime", getRecordTime())
            .append("isPkAi", getIsPkAi())
            .append("algorithmId", getAlgorithmId())
            .append("algorithmName", getAlgorithmName())
            .append("winner", getWinner())
            .append("firstPlayerId", getFirstPlayerId())
            .append("firstPlayer", getFirstPlayer())
            .append("secondPlayerId", getSecondPlayerId())
            .append("secondPlayerName", getSecondPlayerName())
            .append("firstPlayerPieces", getFirstPlayerPieces())
            .append("playerBPieces", getPlayerBPieces())
            .toString();
    }
}
