package com.gamehive.service;

import java.util.List;
import java.util.Map;
import com.gamehive.pojo.Record;

/**
 * 对局记录Service接口
 *
 * @author Cassifa
 * @date 2025-05-05
 */
public interface IRecordService {

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
     * @param userId 用户ID
     * @param gameTypeId 游戏类型ID
     * @param gameMode 游戏模式（0-本地对战，1-与大模型对战，2-联机对战）
     * @param algorithmId 算法ID
     * @param winner 赢家
     * @param playerName 玩家名称
     * @return 热力图数据
     */
    public List<Map<String, Object>> getHeatmapData(Long userId, Long gameTypeId, Integer gameMode, 
                                                   Long algorithmId, Long winner, String playerName);

    /**
     * 根据对局ID获取对局详情（包含对局记录和游戏类型信息）
     *
     * @param recordId 对局记录主键
     * @return 对局详情（包含Record和GameType信息）
     */
    public Map<String, Object> selectRecordDetailByRecordId(Long recordId);
}
