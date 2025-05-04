package com.gamehive.mapper;

import java.util.List;
import com.gamehive.pojo.Player;
import com.baomidou.mybatisplus.core.mapper.BaseMapper;
import org.apache.ibatis.annotations.Mapper;

/**
 * 天梯排行Mapper接口
 * 
 * @author Cassifa
 * @date 2025-02-13
 */
@Mapper
public interface PlayerMapper extends BaseMapper<Player>
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
     * 删除天梯排行
     * 
     * @param userId 天梯排行主键
     * @return 结果
     */
    public int deletePlayerByUserId(Long userId);

    /**
     * 批量删除天梯排行
     * 
     * @param userIds 需要删除的数据主键集合
     * @return 结果
     */
    public int deletePlayerByUserIds(Long[] userIds);
}
