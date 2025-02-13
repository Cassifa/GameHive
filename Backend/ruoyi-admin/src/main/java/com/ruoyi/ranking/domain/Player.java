package com.ruoyi.ranking.domain;

import org.apache.commons.lang3.builder.ToStringBuilder;
import org.apache.commons.lang3.builder.ToStringStyle;
import com.ruoyi.common.annotation.Excel;
import com.ruoyi.common.core.domain.BaseEntity;

/**
 * 天梯排行对象 player
 * 
 * @author Cassifa
 * @date 2025-02-13
 */
public class Player extends BaseEntity
{
    private static final long serialVersionUID = 1L;

    /** 用户编号 */
    @Excel(name = "用户编号")
    private Long userId;

    /** 天梯积分 */
    @Excel(name = "天梯积分")
    private Long raking;

    /** 与AI对战记录 */
    @Excel(name = "与AI对战记录")
    private String recordWithAi;

    /** 与玩家对战记录 */
    @Excel(name = "与玩家对战记录")
    private String recordWithPlayer;

    public void setUserId(Long userId) 
    {
        this.userId = userId;
    }

    public Long getUserId() 
    {
        return userId;
    }
    public void setRaking(Long raking) 
    {
        this.raking = raking;
    }

    public Long getRaking() 
    {
        return raking;
    }
    public void setRecordWithAi(String recordWithAi) 
    {
        this.recordWithAi = recordWithAi;
    }

    public String getRecordWithAi() 
    {
        return recordWithAi;
    }
    public void setRecordWithPlayer(String recordWithPlayer) 
    {
        this.recordWithPlayer = recordWithPlayer;
    }

    public String getRecordWithPlayer() 
    {
        return recordWithPlayer;
    }

    @Override
    public String toString() {
        return new ToStringBuilder(this,ToStringStyle.MULTI_LINE_STYLE)
            .append("userId", getUserId())
            .append("raking", getRaking())
            .append("recordWithAi", getRecordWithAi())
            .append("recordWithPlayer", getRecordWithPlayer())
            .toString();
    }
}
