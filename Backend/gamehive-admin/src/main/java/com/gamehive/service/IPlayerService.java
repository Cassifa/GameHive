package com.gamehive.service;

import java.util.List;
import com.gamehive.pojo.Player;

/**
 * 玩家Service接口
 * 
 * @author Cassifa
 * @date 2025-05-05
 */
public interface IPlayerService 
{
    /**
     * 查询玩家
     * 
     * @param userId 玩家主键
     * @return 玩家
     */
    public Player selectPlayerByUserId(Long userId);

    /**
     * 查询玩家列表
     * 
     * @param player 玩家
     * @return 玩家集合
     */
    public List<Player> selectPlayerList(Player player);

    /**
     * 新增玩家
     * 
     * @param player 玩家
     * @return 结果
     */
    public int insertPlayer(Player player);

    /**
     * 修改玩家
     * 
     * @param player 玩家
     * @return 结果
     */
    public int updatePlayer(Player player);

    /**
     * 批量删除玩家
     * 
     * @param userIds 需要删除的玩家主键集合
     * @return 结果
     */
    public int deletePlayerByUserIds(Long[] userIds);

    /**
     * 删除玩家信息
     * 
     * @param userId 玩家主键
     * @return 结果
     */
    public int deletePlayerByUserId(Long userId);
}
