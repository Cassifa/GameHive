package com.gamehive.service.impl;

import java.util.List;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import com.gamehive.mapper.PlayerMapper;
import com.gamehive.pojo.Player;
import com.gamehive.service.IPlayerService;

/**
 * 玩家Service业务层处理
 * 
 * @author Cassifa
 * @date 2025-05-05
 */
@Service
public class PlayerServiceImpl implements IPlayerService 
{
    @Autowired
    private PlayerMapper playerMapper;

    /**
     * 查询玩家
     * 
     * @param userId 玩家主键
     * @return 玩家
     */
    @Override
    public Player selectPlayerByUserId(Long userId)
    {
        return playerMapper.selectPlayerByUserId(userId);
    }

    /**
     * 查询玩家列表
     * 
     * @param player 玩家
     * @return 玩家
     */
    @Override
    public List<Player> selectPlayerList(Player player)
    {
        return playerMapper.selectPlayerList(player);
    }

    /**
     * 新增玩家
     * 
     * @param player 玩家
     * @return 结果
     */
    @Override
    public int insertPlayer(Player player)
    {
        return playerMapper.insertPlayer(player);
    }

    /**
     * 修改玩家
     * 
     * @param player 玩家
     * @return 结果
     */
    @Override
    public int updatePlayer(Player player)
    {
        return playerMapper.updatePlayer(player);
    }

    /**
     * 批量删除玩家
     * 
     * @param userIds 需要删除的玩家主键
     * @return 结果
     */
    @Override
    public int deletePlayerByUserIds(Long[] userIds)
    {
        return playerMapper.deletePlayerByUserIds(userIds);
    }

    /**
     * 删除玩家信息
     * 
     * @param userId 玩家主键
     * @return 结果
     */
    @Override
    public int deletePlayerByUserId(Long userId)
    {
        return playerMapper.deletePlayerByUserId(userId);
    }
}
