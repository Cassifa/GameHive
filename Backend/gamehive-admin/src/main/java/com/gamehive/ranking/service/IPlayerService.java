package com.gamehive.ranking.service;

import java.util.List;
import com.gamehive.ranking.domain.Player;

/**
 * 天梯排行Service接口
 * 
 * @author Cassifa
 * @date 2025-02-13
 */
public interface IPlayerService 
{
    /**
     * 查询天梯排行
     * 
     * @param userId 天梯排行主键
     * @return 天梯排行
     */
    public Player selectPlayerByUserId(Long userId);

    /**
     * 查询天梯排行列表
     * 
     * @param player 天梯排行
     * @return 天梯排行集合
     */
    public List<Player> selectPlayerList(Player player);

    /**
     * 新增天梯排行
     * 
     * @param player 天梯排行
     * @return 结果
     */
    public int insertPlayer(Player player);

    /**
     * 修改天梯排行
     * 
     * @param player 天梯排行
     * @return 结果
     */
    public int updatePlayer(Player player);

    /**
     * 批量删除天梯排行
     * 
     * @param userIds 需要删除的天梯排行主键集合
     * @return 结果
     */
    public int deletePlayerByUserIds(Long[] userIds);

    /**
     * 删除天梯排行信息
     * 
     * @param userId 天梯排行主键
     * @return 结果
     */
    public int deletePlayerByUserId(Long userId);
}
