package com.gamehive.service.impl;

import java.util.List;
import java.util.Map;
import java.util.ArrayList;
import java.util.HashMap;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import com.gamehive.mapper.GameTypeMapper;
import com.gamehive.pojo.GameType;
import com.gamehive.service.IGameTypeService;

/**
 * 游戏类型Service业务层处理
 * 
 * @author Cassifa
 * @date 2025-05-05
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
    @Override
    public int insertGameType(GameType gameType)
    {
        return gameTypeMapper.insertGameType(gameType);
    }

    /**
     * 修改游戏类型
     * 
     * @param gameType 游戏类型
     * @return 结果
     */
    @Override
    public int updateGameType(GameType gameType)
    {
        return gameTypeMapper.updateGameType(gameType);
    }

    /**
     * 批量删除游戏类型
     * 
     * @param gameIds 需要删除的游戏类型主键
     * @return 结果
     */
    @Override
    public int deleteGameTypeByGameIds(Long[] gameIds)
    {
        return gameTypeMapper.deleteGameTypeByGameIds(gameIds);
    }

    /**
     * 删除游戏类型信息
     * 
     * @param gameId 游戏类型主键
     * @return 结果
     */
    @Override
    public int deleteGameTypeByGameId(Long gameId)
    {
        return gameTypeMapper.deleteGameTypeByGameId(gameId);
    }

    /**
     * 获取游戏类型下拉框选项
     * 
     * @return 游戏类型下拉框选项列表
     */
    @Override
    public List<Map<String, Object>> selectGameTypeOptions() {
        // 使用已有的查询方法
        GameType gameType = new GameType();
        List<GameType> list = gameTypeMapper.selectGameTypeList(gameType);
        
        // 转换为前端需要的格式
        List<Map<String, Object>> options = new ArrayList<>();
        for (GameType type : list) {
            Map<String, Object> option = new HashMap<>();
            option.put("value", type.getGameId());
            option.put("label", type.getGameName());
            options.add(option);
        }
        
        return options;
    }
}
