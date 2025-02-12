package com.ruoyi.aiSys.service;

import java.util.List;
import com.ruoyi.aiSys.domain.AiType;

/**
 * AI类型Service接口
 * 
 * @author Cassifa
 * @date 2025-02-13
 */
public interface IAiTypeService 
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
     * 批量删除AI类型
     * 
     * @param aiIds 需要删除的AI类型主键集合
     * @return 结果
     */
    public int deleteAiTypeByAiIds(Long[] aiIds);

    /**
     * 删除AI类型信息
     * 
     * @param aiId AI类型主键
     * @return 结果
     */
    public int deleteAiTypeByAiId(Long aiId);
}
