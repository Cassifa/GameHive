package com.gamehive.record.mapper;

import java.util.List;
import com.gamehive.record.domain.Record;

/**
 * 对局记录Mapper接口
 * 
 * @author Cassifa
 * @date 2025-02-13
 */
public interface RecordMapper 
{
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
     * 修改对局记录
     * 
     * @param record 对局记录
     * @return 结果
     */
    public int updateRecord(Record record);

    /**
     * 删除对局记录
     * 
     * @param recordId 对局记录主键
     * @return 结果
     */
    public int deleteRecordByRecordId(Long recordId);

    /**
     * 批量删除对局记录
     * 
     * @param recordIds 需要删除的数据主键集合
     * @return 结果
     */
    public int deleteRecordByRecordIds(Long[] recordIds);
}
