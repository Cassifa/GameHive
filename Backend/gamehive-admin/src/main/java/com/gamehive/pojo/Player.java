package com.gamehive.pojo;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;
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
@Data
@AllArgsConstructor
@NoArgsConstructor
public class Player extends BaseEntity {
    private static final long serialVersionUID = 1L;

    /**
     * 用户id
     */
    private Long userId;

    /**
     * 用户昵称
     */
    @Excel(name = "用户昵称")
    private String userName;

    /**
     * 天梯积分
     */
    @Excel(name = "天梯积分")
    private Long raking;

    /**
     * 对局统计信息
     */
    private String gameStatistics;
}
