package com.gamehive.service.impl;

import java.util.List;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import com.gamehive.mapper.AlgorithmTypeMapper;
import com.gamehive.pojo.AlgorithmType;
import com.gamehive.service.IAlgorithmTypeService;

/**
 * 算法类型Service业务层处理
 * 
 * @author Cassifa
 * @date 2025-05-05
 */
@Service
public class AlgorithmTypeServiceImpl implements IAlgorithmTypeService 
{
    @Autowired
    private AlgorithmTypeMapper algorithmTypeMapper;

    /**
     * 查询算法类型
     * 
     * @param algorithmId 算法类型主键
     * @return 算法类型
     */
    @Override
    public AlgorithmType selectAlgorithmTypeByAlgorithmId(Long algorithmId)
    {
        return algorithmTypeMapper.selectAlgorithmTypeByAlgorithmId(algorithmId);
    }

    /**
     * 查询算法类型列表
     * 
     * @param algorithmType 算法类型
     * @return 算法类型
     */
    @Override
    public List<AlgorithmType> selectAlgorithmTypeList(AlgorithmType algorithmType)
    {
        return algorithmTypeMapper.selectAlgorithmTypeList(algorithmType);
    }

    /**
     * 新增算法类型
     * 
     * @param algorithmType 算法类型
     * @return 结果
     */
    @Override
    public int insertAlgorithmType(AlgorithmType algorithmType)
    {
        return algorithmTypeMapper.insertAlgorithmType(algorithmType);
    }

    /**
     * 修改算法类型
     * 
     * @param algorithmType 算法类型
     * @return 结果
     */
    @Override
    public int updateAlgorithmType(AlgorithmType algorithmType)
    {
        return algorithmTypeMapper.updateAlgorithmType(algorithmType);
    }

    /**
     * 批量删除算法类型
     * 
     * @param algorithmIds 需要删除的算法类型主键
     * @return 结果
     */
    @Override
    public int deleteAlgorithmTypeByAlgorithmIds(Long[] algorithmIds)
    {
        return algorithmTypeMapper.deleteAlgorithmTypeByAlgorithmIds(algorithmIds);
    }

    /**
     * 删除算法类型信息
     * 
     * @param algorithmId 算法类型主键
     * @return 结果
     */
    @Override
    public int deleteAlgorithmTypeByAlgorithmId(Long algorithmId)
    {
        return algorithmTypeMapper.deleteAlgorithmTypeByAlgorithmId(algorithmId);
    }
}
