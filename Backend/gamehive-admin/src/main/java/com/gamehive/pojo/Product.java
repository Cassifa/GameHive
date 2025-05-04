package com.gamehive.pojo;

import org.apache.commons.lang3.builder.ToStringBuilder;
import org.apache.commons.lang3.builder.ToStringStyle;
import com.gamehive.common.annotation.Excel;
import com.gamehive.common.core.domain.BaseEntity;

/**
 * Algorithm-Game具体产品对象 product
 * 
 * @author Cassifa
 * @date 2025-05-05
 */
public class Product extends BaseEntity
{
    private static final long serialVersionUID = 1L;

    /** $column.columnComment */
    private Long id;

    /** 算法id */
    private Long algorithmTypeId;

    /** 算法名称 */
    @Excel(name = "算法名称")
    private String algorithmTypeName;

    /** Game类型外键 */
    private Long gameTypeId;

    /** 游戏类型中文名 */
    @Excel(name = "游戏类型中文名")
    private String gameTypeName;

    /** 难度等级 */
    @Excel(name = "难度等级")
    private Long maximumLevel;

    /** 被玩家挑战次数 */
    @Excel(name = "被玩家挑战次数")
    private Long challengedCount;

    /** 胜利次数 */
    @Excel(name = "胜利次数")
    private Long winCount;

    public void setId(Long id) 
    {
        this.id = id;
    }

    public Long getId() 
    {
        return id;
    }
    public void setAlgorithmTypeId(Long algorithmTypeId) 
    {
        this.algorithmTypeId = algorithmTypeId;
    }

    public Long getAlgorithmTypeId() 
    {
        return algorithmTypeId;
    }
    public void setAlgorithmTypeName(String algorithmTypeName) 
    {
        this.algorithmTypeName = algorithmTypeName;
    }

    public String getAlgorithmTypeName() 
    {
        return algorithmTypeName;
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
    public void setMaximumLevel(Long maximumLevel) 
    {
        this.maximumLevel = maximumLevel;
    }

    public Long getMaximumLevel() 
    {
        return maximumLevel;
    }
    public void setChallengedCount(Long challengedCount) 
    {
        this.challengedCount = challengedCount;
    }

    public Long getChallengedCount() 
    {
        return challengedCount;
    }
    public void setWinCount(Long winCount) 
    {
        this.winCount = winCount;
    }

    public Long getWinCount() 
    {
        return winCount;
    }

    @Override
    public String toString() {
        return new ToStringBuilder(this,ToStringStyle.MULTI_LINE_STYLE)
            .append("id", getId())
            .append("algorithmTypeId", getAlgorithmTypeId())
            .append("algorithmTypeName", getAlgorithmTypeName())
            .append("gameTypeId", getGameTypeId())
            .append("gameTypeName", getGameTypeName())
            .append("maximumLevel", getMaximumLevel())
            .append("challengedCount", getChallengedCount())
            .append("winCount", getWinCount())
            .toString();
    }
}
