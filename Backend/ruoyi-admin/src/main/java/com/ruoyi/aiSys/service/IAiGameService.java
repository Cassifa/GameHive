package com.ruoyi.aiSys.service;

import java.util.List;
import com.ruoyi.aiSys.domain.AiGame;

/**
 * AI-Game具体产品Service接口
 * 
 * @author Cassifa
 * @date 2025-02-13
 */
public interface IAiGameService 
{
    /**
     * 查询AI-Game具体产品
     * 
     * @param id AI-Game具体产品主键
     * @return AI-Game具体产品
     */
    public AiGame selectAiGameById(Long id);

    /**
     * 查询AI-Game具体产品列表
     * 
     * @param aiGame AI-Game具体产品
     * @return AI-Game具体产品集合
     */
    public List<AiGame> selectAiGameList(AiGame aiGame);

    /**
     * 新增AI-Game具体产品
     * 
     * @param aiGame AI-Game具体产品
     * @return 结果
     */
    public int insertAiGame(AiGame aiGame);

    /**
     * 修改AI-Game具体产品
     * 
     * @param aiGame AI-Game具体产品
     * @return 结果
     */
    public int updateAiGame(AiGame aiGame);

    /**
     * 批量删除AI-Game具体产品
     * 
     * @param ids 需要删除的AI-Game具体产品主键集合
     * @return 结果
     */
    public int deleteAiGameByIds(Long[] ids);

    /**
     * 删除AI-Game具体产品信息
     * 
     * @param id AI-Game具体产品主键
     * @return 结果
     */
    public int deleteAiGameById(Long id);
}
