package com.gamehive.matchingsystem.service;

import com.gamehive.matchingsystem.constants.GameTypeEnum;

public interface MatchingService {

    /**
     * 添加玩家到匹配池
     *
     * @param userId 用户ID
     * @param rating 玩家评分
     * @param gameType 游戏类型枚举
     * @return 操作结果信息
     */
    String addPlayer(Integer userId, Integer rating, GameTypeEnum gameType);

    /**
     * 从匹配池移除玩家
     *
     * @param userId 用户ID
     * @param rating 玩家评分
     * @return 操作结果信息
     */
    String removePlayer(Integer userId, Integer rating);
}
