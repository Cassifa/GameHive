package com.gamehive.mapper;

import java.util.List;
import com.gamehive.pojo.GameType;
import com.baomidou.mybatisplus.core.mapper.BaseMapper;

/**
 * 游戏类型Mapper接口
 *
 * @author Cassifa
 * @date 2025-05-05
 */
public interface GameTypeMapper extends BaseMapper<GameType> {

    /**
     * 查询游戏类型
     *
     * @param gameId 游戏类型主键
     * @return 游戏类型
     */
    public GameType selectGameTypeByGameId(Long gameId);

    /**
     * 查询游戏类型列表
     *
     * @param gameType 游戏类型
     * @return 游戏类型集合
     */
    public List<GameType> selectGameTypeList(GameType gameType);

    /**
     * 新增游戏类型
     *
     * @param gameType 游戏类型
     * @return 结果
     */
    public int insertGameType(GameType gameType);

    /**
     * 修改游戏类型
     *
     * @param gameType 游戏类型
     * @return 结果
     */
    public int updateGameType(GameType gameType);

    /**
     * 删除游戏类型
     *
     * @param gameId 游戏类型主键
     * @return 结果
     */
    public int deleteGameTypeByGameId(Long gameId);

    /**
     * 批量删除游戏类型
     *
     * @param gameIds 需要删除的数据主键集合
     * @return 结果
     */
    public int deleteGameTypeByGameIds(Long[] gameIds);
}
