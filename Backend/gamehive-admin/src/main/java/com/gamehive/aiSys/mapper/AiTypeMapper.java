package com.gamehive.aiSys.mapper;

import java.util.List;
import com.gamehive.aiSys.domain.AiType;
import com.gamehive.aiSys.domain.AiGame;

/**
 * AI类型Mapper接口
 * 
 * @author Cassifa
 * @date 2025-02-13
 */
public interface AiTypeMapper 
{
    /**
     * 查询AI类型
     * 
     * @param aiId AI类型主键
     * @return AI类型
     */
    public AiType selectAiTypeByAiId(Long aiId);

    /**
     * 查询AI类型列表
     * 
     * @param aiType AI类型
     * @return AI类型集合
     */
    public List<AiType> selectAiTypeList(AiType aiType);

    /**
     * 新增AI类型
     * 
     * @param aiType AI类型
     * @return 结果
     */
    public int insertAiType(AiType aiType);

    /**
     * 修改AI类型
     * 
     * @param aiType AI类型
     * @return 结果
     */
    public int updateAiType(AiType aiType);

    /**
     * 删除AI类型
     * 
     * @param aiId AI类型主键
     * @return 结果
     */
    public int deleteAiTypeByAiId(Long aiId);

    /**
     * 批量删除AI类型
     * 
     * @param aiIds 需要删除的数据主键集合
     * @return 结果
     */
    public int deleteAiTypeByAiIds(Long[] aiIds);

    /**
     * 批量删除AI-Game具体产品
     * 
     * @param aiIds 需要删除的数据主键集合
     * @return 结果
     */
    public int deleteAiGameByAiTypeIds(Long[] aiIds);
    
    /**
     * 批量新增AI-Game具体产品
     * 
     * @param aiGameList AI-Game具体产品列表
     * @return 结果
     */
    public int batchAiGame(List<AiGame> aiGameList);
    

    /**
     * 通过AI类型主键删除AI-Game具体产品信息
     * 
     * @param aiId AI类型ID
     * @return 结果
     */
    public int deleteAiGameByAiTypeId(Long aiId);
}
