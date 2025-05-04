package com.gamehive.mapper;

import java.util.List;
import com.gamehive.pojo.AlgorithmType;
import com.baomidou.mybatisplus.core.mapper.BaseMapper;

/**
 * 算法类型Mapper接口
 *
 * @author Cassifa
 * @date 2025-05-05
 */
public interface AlgorithmTypeMapper extends BaseMapper<AlgorithmType> {

    /**
     * 查询算法类型
     *
     * @param algorithmId 算法类型主键
     * @return 算法类型
     */
    public AlgorithmType selectAlgorithmTypeByAlgorithmId(Long algorithmId);

    /**
     * 查询算法类型列表
     *
     * @param algorithmType 算法类型
     * @return 算法类型集合
     */
    public List<AlgorithmType> selectAlgorithmTypeList(AlgorithmType algorithmType);

    /**
     * 新增算法类型
     *
     * @param algorithmType 算法类型
     * @return 结果
     */
    public int insertAlgorithmType(AlgorithmType algorithmType);

    /**
     * 修改算法类型
     *
     * @param algorithmType 算法类型
     * @return 结果
     */
    public int updateAlgorithmType(AlgorithmType algorithmType);

    /**
     * 删除算法类型
     *
     * @param algorithmId 算法类型主键
     * @return 结果
     */
    public int deleteAlgorithmTypeByAlgorithmId(Long algorithmId);

    /**
     * 批量删除算法类型
     *
     * @param algorithmIds 需要删除的数据主键集合
     * @return 结果
     */
    public int deleteAlgorithmTypeByAlgorithmIds(Long[] algorithmIds);
}
