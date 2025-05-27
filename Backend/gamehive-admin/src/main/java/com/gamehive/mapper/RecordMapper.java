package com.gamehive.mapper;

import com.baomidou.mybatisplus.core.mapper.BaseMapper;
import com.gamehive.pojo.Record;
import java.util.List;
import java.util.Map;
import org.apache.ibatis.annotations.Mapper;
import org.apache.ibatis.annotations.Param;

/**
 * 对局记录Mapper接口
 *
 * @author Cassifa
 * @date 2025-05-05
 */
@Mapper
public interface RecordMapper extends BaseMapper<Record> {

    /**
     * 查询对局记录
     *
     * @param recordId 对局记录主键
     * @return 对局记录
     */
    public Record selectRecordByRecordId(Long recordId);

    /**
     * 查询对局记录列表
     *
     * @param record 对局记录
     * @return 对局记录集合
     */
    public List<Record> selectRecordList(Record record);

    /**
     * 新增对局记录
     *
     * @param record 对局记录
     * @return 结果
     */
    public int insertRecord(Record record);
    
    /**
     * 获取对局记录热力图数据
     * 
     * @param record 查询条件
     * @return 热力图数据
     */
    public List<Map<String, Object>> selectRecordHeatmap(Record record);

    /**
     * 根据对局ID获取对局详情（包含对局记录和游戏类型信息）
     * 
     * @param recordId 对局记录主键
     * @return 对局详情
     */
    public Map<String, Object> selectRecordDetailByRecordId(@Param("recordId") Long recordId);
}
