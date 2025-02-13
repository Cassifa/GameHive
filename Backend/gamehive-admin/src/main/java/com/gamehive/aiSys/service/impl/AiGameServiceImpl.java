package com.gamehive.aiSys.service.impl;

import java.util.List;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import com.gamehive.aiSys.mapper.AiGameMapper;
import com.gamehive.aiSys.domain.AiGame;
import com.gamehive.aiSys.service.IAiGameService;

/**
 * AI-Game具体产品Service业务层处理
 * 
 * @author Cassifa
 * @date 2025-02-13
 */
@Service
public class AiGameServiceImpl implements IAiGameService 
{
    @Autowired
    private AiGameMapper aiGameMapper;

    /**
     * 查询AI-Game具体产品
     * 
     * @param id AI-Game具体产品主键
     * @return AI-Game具体产品
     */
    @Override
    public AiGame selectAiGameById(Long id)
    {
        return aiGameMapper.selectAiGameById(id);
    }

    /**
     * 查询AI-Game具体产品列表
     * 
     * @param aiGame AI-Game具体产品
     * @return AI-Game具体产品
     */
    @Override
    public List<AiGame> selectAiGameList(AiGame aiGame)
    {
        return aiGameMapper.selectAiGameList(aiGame);
    }

    /**
     * 新增AI-Game具体产品
     * 
     * @param aiGame AI-Game具体产品
     * @return 结果
     */
    @Override
    public int insertAiGame(AiGame aiGame)
    {
        return aiGameMapper.insertAiGame(aiGame);
    }

    /**
     * 修改AI-Game具体产品
     * 
     * @param aiGame AI-Game具体产品
     * @return 结果
     */
    @Override
    public int updateAiGame(AiGame aiGame)
    {
        return aiGameMapper.updateAiGame(aiGame);
    }

    /**
     * 批量删除AI-Game具体产品
     * 
     * @param ids 需要删除的AI-Game具体产品主键
     * @return 结果
     */
    @Override
    public int deleteAiGameByIds(Long[] ids)
    {
        return aiGameMapper.deleteAiGameByIds(ids);
    }

    /**
     * 删除AI-Game具体产品信息
     * 
     * @param id AI-Game具体产品主键
     * @return 结果
     */
    @Override
    public int deleteAiGameById(Long id)
    {
        return aiGameMapper.deleteAiGameById(id);
    }
}
