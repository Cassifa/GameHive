package com.gamehive.pojo;

import org.apache.commons.lang3.builder.ToStringBuilder;
import org.apache.commons.lang3.builder.ToStringStyle;
import com.gamehive.common.annotation.Excel;
import com.gamehive.common.core.domain.BaseEntity;

/**
 * 玩家对象 player
 * 
 * @author Cassifa
 * @date 2025-05-05
 */
public class Player extends BaseEntity
{
    private static final long serialVersionUID = 1L;

    /** 用户id */
    private Long userId;

    /** 用户昵称 */
    @Excel(name = "用户昵称")
    private String userName;

    /** 天梯积分 */
    @Excel(name = "天梯积分")
    private Long raking;

    /** 玩家与AI对战统计信息 */
    private String recordWithAi;

    /** 玩家与其它玩家对局记录 */
    private String recordWithPlayer;

    public void setUserId(Long userId) 
    {
        this.userId = userId;
    }

    public Long getUserId() 
    {
        return userId;
    }
    public void setUserName(String userName) 
    {
        this.userName = userName;
    }

    public String getUserName() 
    {
        return userName;
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
            .append("userName", getUserName())
            .append("raking", getRaking())
            .append("recordWithAi", getRecordWithAi())
            .append("recordWithPlayer", getRecordWithPlayer())
            .toString();
    }
}
