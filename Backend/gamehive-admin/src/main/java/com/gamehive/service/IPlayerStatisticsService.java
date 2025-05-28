package com.gamehive.service;

import com.gamehive.dto.PlayerGameStatisticsDTO;
import com.gamehive.pojo.Record;

/**
 * 玩家统计数据服务接口
 *
 * @author Cassifa
 */
public interface IPlayerStatisticsService {

    /**
     * 获取玩家统计数据
     *
     * @param userId 用户ID
     * @return 玩家统计数据
     */
    PlayerGameStatisticsDTO getPlayerStatistics(Long userId);

    /**
     * 更新玩家统计数据（基于新的对局记录）
     *
     * @param record 新的对局记录
     */
    void updatePlayerStatistics(Record record);

    /**
     * 重新计算玩家统计数据（基于所有对局记录）
     *
     * @param userId 用户ID
     * @return 重新计算后的统计数据
     */
    PlayerGameStatisticsDTO recalculatePlayerStatistics(Long userId);

    /**
     * 初始化玩家统计数据
     *
     * @param userId 用户ID
     * @param userName 用户名
     * @return 初始化的统计数据
     */
    PlayerGameStatisticsDTO initializePlayerStatistics(Long userId, String userName);
} 