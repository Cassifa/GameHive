package com.ruoyi.aiSys.service.impl;

import java.util.List;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import java.util.ArrayList;
import com.ruoyi.common.utils.StringUtils;
import org.springframework.transaction.annotation.Transactional;
import com.ruoyi.aiSys.domain.AiGame;
import com.ruoyi.aiSys.mapper.GameTypeMapper;
import com.ruoyi.aiSys.domain.GameType;
import com.ruoyi.aiSys.service.IGameTypeService;

/**
 * 游戏类型Service业务层处理
 * 
 * @author ruoyi
 * @date 2025-02-13
 */
@Service
public class GameTypeServiceImpl implements IGameTypeService 
{
    @Autowired
    private GameTypeMapper gameTypeMapper;

    /**
     * 查询游戏类型
     * 
     * @param gameId 游戏类型主键
     * @return 游戏类型
     */
    @Override
    public GameType selectGameTypeByGameId(Long gameId)
    {
        return gameTypeMapper.selectGameTypeByGameId(gameId);
    }

    /**
     * 查询游戏类型列表
     * 
     * @param gameType 游戏类型
     * @return 游戏类型
     */
    @Override
    public List<GameType> selectGameTypeList(GameType gameType)
    {
        return gameTypeMapper.selectGameTypeList(gameType);
    }

    /**
     * 新增游戏类型
     * 
     * @param gameType 游戏类型
     * @return 结果
     */
    @Transactional
    @Override
    public int insertGameType(GameType gameType)
    {
        int rows = gameTypeMapper.insertGameType(gameType);
        insertAiGame(gameType);
        return rows;
    }

    /**
     * 修改游戏类型
     * 
     * @param gameType 游戏类型
     * @return 结果
     */
    @Transactional
    @Override
    public int updateGameType(GameType gameType)
    {
        gameTypeMapper.deleteAiGameByGameTypeId(gameType.getGameId());
        insertAiGame(gameType);
        return gameTypeMapper.updateGameType(gameType);
    }

    /**
     * 批量删除游戏类型
     * 
     * @param gameIds 需要删除的游戏类型主键
     * @return 结果
     */
    @Transactional
    @Override
    public int deleteGameTypeByGameIds(Long[] gameIds)
    {
        gameTypeMapper.deleteAiGameByGameTypeIds(gameIds);
        return gameTypeMapper.deleteGameTypeByGameIds(gameIds);
    }

    /**
     * 删除游戏类型信息
     * 
     * @param gameId 游戏类型主键
     * @return 结果
     */
    @Transactional
    @Override
    public int deleteGameTypeByGameId(Long gameId)
    {
        gameTypeMapper.deleteAiGameByGameTypeId(gameId);
        return gameTypeMapper.deleteGameTypeByGameId(gameId);
    }

    /**
     * 新增AI-Game具体产品信息
     * 
     * @param gameType 游戏类型对象
     */
    public void insertAiGame(GameType gameType)
    {
        List<AiGame> aiGameList = gameType.getAiGameList();
        Long gameId = gameType.getGameId();
        if (StringUtils.isNotNull(aiGameList))
        {
            List<AiGame> list = new ArrayList<AiGame>();
            for (AiGame aiGame : aiGameList)
            {
                aiGame.setGameTypeId(gameId);
                list.add(aiGame);
            }
            if (list.size() > 0)
            {
                gameTypeMapper.batchAiGame(list);
            }
        }
    }
}
