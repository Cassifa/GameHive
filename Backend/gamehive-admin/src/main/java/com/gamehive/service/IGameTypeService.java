package com.gamehive.service;

import java.util.List;
import com.gamehive.pojo.GameType;

/**
 * 游戏类型Service接口
 * 
 * @author ruoyi
 * @date 2025-02-13
 */
public interface IGameTypeService 
{
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
     * 批量删除游戏类型
     * 
     * @param gameIds 需要删除的游戏类型主键集合
     * @return 结果
     */
    public int deleteGameTypeByGameIds(Long[] gameIds);

    /**
     * 删除游戏类型信息
     * 
     * @param gameId 游戏类型主键
     * @return 结果
     */
    public int deleteGameTypeByGameId(Long gameId);
}
