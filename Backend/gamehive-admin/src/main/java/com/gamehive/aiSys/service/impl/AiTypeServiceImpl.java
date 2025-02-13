package com.gamehive.aiSys.service.impl;

import java.util.List;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import java.util.ArrayList;

import com.gamehive.common.utils.StringUtils;
import org.springframework.transaction.annotation.Transactional;
import com.gamehive.aiSys.domain.AiGame;
import com.gamehive.aiSys.mapper.AiTypeMapper;
import com.gamehive.aiSys.domain.AiType;
import com.gamehive.aiSys.service.IAiTypeService;

/**
 * AI类型Service业务层处理
 *
 * @author Cassifa
 * @date 2025-02-13
 */
@Service
public class AiTypeServiceImpl implements IAiTypeService {
    @Autowired
    private AiTypeMapper aiTypeMapper;

    /**
     * 查询AI类型
     *
     * @param aiId AI类型主键
     * @return AI类型
     */
    @Override
    public AiType selectAiTypeByAiId(Long aiId) {
        return aiTypeMapper.selectAiTypeByAiId(aiId);
    }

    /**
     * 查询AI类型列表
     *
     * @param aiType AI类型
     * @return AI类型
     */
    @Override
    public List<AiType> selectAiTypeList(AiType aiType) {
        return aiTypeMapper.selectAiTypeList(aiType);
    }

    /**
     * 新增AI类型
     *
     * @param aiType AI类型
     * @return 结果
     */
    @Transactional
    @Override
    public int insertAiType(AiType aiType) {
        int rows = aiTypeMapper.insertAiType(aiType);

        insertAiGame(aiType);
        return rows;
    }

    /**
     * 修改AI类型
     *
     * @param aiType AI类型
     * @return 结果
     */
    @Transactional
    @Override
    public int updateAiType(AiType aiType) {
        aiTypeMapper.deleteAiGameByAiTypeId(aiType.getAiId());
        insertAiGame(aiType);
        return aiTypeMapper.updateAiType(aiType);
    }

    /**
     * 批量删除AI类型
     *
     * @param aiIds 需要删除的AI类型主键
     * @return 结果
     */
    @Transactional
    @Override
    public int deleteAiTypeByAiIds(Long[] aiIds) {
        aiTypeMapper.deleteAiGameByAiTypeIds(aiIds);
        return aiTypeMapper.deleteAiTypeByAiIds(aiIds);
    }

    /**
     * 删除AI类型信息
     *
     * @param aiId AI类型主键
     * @return 结果
     */
    @Transactional
    @Override
    public int deleteAiTypeByAiId(Long aiId) {
        aiTypeMapper.deleteAiGameByAiTypeId(aiId);
        return aiTypeMapper.deleteAiTypeByAiId(aiId);
    }

    /**
     * 新增AI-Game具体产品信息
     *
     * @param aiType AI类型对象
     */
    public void insertAiGame(AiType aiType) {
        List<AiGame> aiGameList = aiType.getAiGameList();
        Long aiId = aiType.getAiId();
        if (StringUtils.isNotNull(aiGameList)) {
            List<AiGame> list = new ArrayList<AiGame>();
            for (AiGame aiGame : aiGameList) {
                aiGame.setAiTypeId(aiId);
                list.add(aiGame);
            }
            if (list.size() > 0) {
                aiTypeMapper.batchAiGame(list);
            }
        }
    }
}
