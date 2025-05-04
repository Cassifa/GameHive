package com.gamehive.service.impl;

import java.util.List;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import com.gamehive.mapper.RecordMapper;
import com.gamehive.pojo.Record;
import com.gamehive.service.IRecordService;

/**
 * 对局记录Service业务层处理
 *
 * @author Cassifa
 * @date 2025-05-05
 */
@Service
public class RecordServiceImpl implements IRecordService {

    @Autowired
    private RecordMapper recordMapper;

    /**
     * 查询对局记录
     *
     * @param recordId 对局记录主键
     * @return 对局记录
     */
    @Override
    public Record selectRecordByRecordId(Long recordId) {
        return recordMapper.selectRecordByRecordId(recordId);
    }

    /**
     * 查询对局记录列表
     *
     * @param record 对局记录
     * @return 对局记录
     */
    @Override
    public List<Record> selectRecordList(Record record) {
        return recordMapper.selectRecordList(record);
    }

    /**
     * 新增对局记录
     *
     * @param record 对局记录
     * @return 结果
     */
    @Override
    public int insertRecord(Record record) {
        return recordMapper.insertRecord(record);
    }

    /**
     * 修改对局记录
     *
     * @param record 对局记录
     * @return 结果
     */
    @Override
    public int updateRecord(Record record) {
        return recordMapper.updateRecord(record);
    }

    /**
     * 批量删除对局记录
     *
     * @param recordIds 需要删除的对局记录主键
     * @return 结果
     */
    @Override
    public int deleteRecordByRecordIds(Long[] recordIds) {
        return recordMapper.deleteRecordByRecordIds(recordIds);
    }

    /**
     * 删除对局记录信息
     *
     * @param recordId 对局记录主键
     * @return 结果
     */
    @Override
    public int deleteRecordByRecordId(Long recordId) {
        return recordMapper.deleteRecordByRecordId(recordId);
    }
}
