package com.ruoyi.aiSys.domain;

import org.apache.commons.lang3.builder.ToStringBuilder;
import org.apache.commons.lang3.builder.ToStringStyle;
import com.ruoyi.common.annotation.Excel;
import com.ruoyi.common.core.domain.BaseEntity;

/**
 * AI-Game具体产品对象 ai_game
 * 
 * @author Cassifa
 * @date 2025-02-13
 */
public class AiGame extends BaseEntity
{
    private static final long serialVersionUID = 1L;

    /** 编号 */
    @Excel(name = "编号")
    private Long id;

    /** AI类别 */
    @Excel(name = "AI类别")
    private Long aiTypeId;

    /** 游戏类别 */
    @Excel(name = "游戏类别")
    private Long gameTypeId;

    /** 难度 */
    @Excel(name = "难度")
    private Long level;

    /** 被挑战次数 */
    @Excel(name = "被挑战次数")
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
    public void setAiTypeId(Long aiTypeId) 
    {
        this.aiTypeId = aiTypeId;
    }

    public Long getAiTypeId() 
    {
        return aiTypeId;
    }
    public void setGameTypeId(Long gameTypeId) 
    {
        this.gameTypeId = gameTypeId;
    }

    public Long getGameTypeId() 
    {
        return gameTypeId;
    }
    public void setLevel(Long level) 
    {
        this.level = level;
    }

    public Long getLevel() 
    {
        return level;
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
            .append("aiTypeId", getAiTypeId())
            .append("gameTypeId", getGameTypeId())
            .append("level", getLevel())
            .append("challengedCount", getChallengedCount())
            .append("winCount", getWinCount())
            .toString();
    }
}
