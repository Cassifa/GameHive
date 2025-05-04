package com.gamehive.service.impl;

import java.util.List;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import com.gamehive.mapper.PlayerMapper;
import com.gamehive.pojo.Player;
import com.gamehive.service.IPlayerService;

/**
 * 天梯排行Service业务层处理
 * 
 * @author Cassifa
 * @date 2025-02-13
 */
@Service
public class PlayerServiceImpl implements IPlayerService 
{
    @Autowired
    private PlayerMapper playerMapper;

    /**
     * 查询天梯排行
     * 
     * @param userId 天梯排行主键
     * @return 天梯排行
     */
    @Override
    public Player selectPlayerByUserId(Long userId)
    {
        return playerMapper.selectPlayerByUserId(userId);
    }

    /**
     * 查询天梯排行列表
     * 
     * @param player 天梯排行
     * @return 天梯排行
     */
    @Override
    public List<Player> selectPlayerList(Player player)
    {
        return playerMapper.selectPlayerList(player);
    }

    /**
     * 新增天梯排行
     * 
     * @param player 天梯排行
     * @return 结果
     */
    @Override
    public int insertPlayer(Player player)
    {
        return playerMapper.insertPlayer(player);
    }

    /**
     * 修改天梯排行
     * 
     * @param player 天梯排行
     * @return 结果
     */
    @Override
    public int updatePlayer(Player player)
    {
        return playerMapper.updatePlayer(player);
    }

    /**
     * 批量删除天梯排行
     * 
     * @param userIds 需要删除的天梯排行主键
     * @return 结果
     */
    @Override
    public int deletePlayerByUserIds(Long[] userIds)
    {
        return playerMapper.deletePlayerByUserIds(userIds);
    }

    /**
     * 删除天梯排行信息
     * 
     * @param userId 天梯排行主键
     * @return 结果
     */
    @Override
    public int deletePlayerByUserId(Long userId)
    {
        return playerMapper.deletePlayerByUserId(userId);
    }
}
