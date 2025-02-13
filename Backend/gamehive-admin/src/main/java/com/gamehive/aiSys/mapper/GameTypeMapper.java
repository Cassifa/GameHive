package com.gamehive.aiSys.mapper;

import java.util.List;
import com.gamehive.aiSys.domain.GameType;
import com.gamehive.aiSys.domain.AiGame;

/**
 * 游戏类型Mapper接口
 * 
 * @author ruoyi
 * @date 2025-02-13
 */
public interface GameTypeMapper 
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

    /**
     * 批量删除AI-Game具体产品
     * 
     * @param gameIds 需要删除的数据主键集合
     * @return 结果
     */
    public int deleteAiGameByGameTypeIds(Long[] gameIds);
    
    /**
     * 批量新增AI-Game具体产品
     * 
     * @param aiGameList AI-Game具体产品列表
     * @return 结果
     */
    public int batchAiGame(List<AiGame> aiGameList);
    

    /**
     * 通过游戏类型主键删除AI-Game具体产品信息
     * 
     * @param gameId 游戏类型ID
     * @return 结果
     */
    public int deleteAiGameByGameTypeId(Long gameId);
}
