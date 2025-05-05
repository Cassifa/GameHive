package com.gamehive.service.impl;

import java.util.List;
import java.util.Map;
import java.util.Set;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.baomidou.mybatisplus.core.conditions.query.QueryWrapper;
import com.gamehive.common.core.domain.model.LoginUser;
import com.gamehive.common.utils.SecurityUtils;
import com.gamehive.framework.web.service.SysPermissionService;
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

    @Autowired
    private SysPermissionService permissionService;

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
        // 获取当前用户ID
        Long userId = (Long) record.getParams().getOrDefault("userId", null);

        // 如果userId为空，直接返回原始查询结果
        if (userId == null) {
            return recordMapper.selectRecordList(record);
        }

        // 判断当前用户是否为管理员
        boolean isAdmin = false;
        LoginUser loginUser = SecurityUtils.getLoginUser();
        if (loginUser != null && loginUser.getUser() != null) {
            // 获取用户角色集合
            Set<String> roles = permissionService.getRolePermission(loginUser.getUser());
            if (roles.contains("admin")) {
                isAdmin = true;
            }
        }

        // 如果是管理员，返回原始查询结果
        if (isAdmin) {
            return recordMapper.selectRecordList(record);
        }

        // 如果不是管理员，则只查询与当前用户相关的记录
        record.getParams().put("currentUserId", userId);

        // 处理playerName参数
        String playerName = (String) record.getParams().get("playerName");
        if (playerName != null && !playerName.isEmpty()) {
            record.getParams().put("opponentName", playerName);
        }

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

    /**
     * 获取对局记录热力图数据
     *
     * @param userId 用户ID
     * @param gameTypeId 游戏类型ID
     * @param isPkAi 是否与AI对局
     * @param algorithmId 算法ID
     * @param winner 赢家
     * @param playerName 玩家名称
     * @return 热力图数据
     */
    @Override
    public List<Map<String, Object>> getHeatmapData(Long userId, Long gameTypeId, Boolean isPkAi,
            Long algorithmId, Long winner, String playerName) {
        
        Record record = new Record();
        record.setGameTypeId(gameTypeId);
        record.setIsPkAi(isPkAi);
        record.setAlgorithmId(algorithmId);
        record.setWinner(winner);
        
        // 判断当前用户是否为管理员
        boolean isAdmin = false;
        LoginUser loginUser = SecurityUtils.getLoginUser();
        if (loginUser != null && loginUser.getUser() != null) {
            // 获取用户角色集合
            Set<String> roles = permissionService.getRolePermission(loginUser.getUser());
            if (roles.contains("admin")) {
                isAdmin = true;
            }
        }

        // 如果不是管理员，添加用户ID过滤
        if (!isAdmin && userId != null) {
            record.getParams().put("currentUserId", userId);
        }

        // 处理playerName参数
        if (playerName != null && !playerName.isEmpty()) {
            if (isAdmin) {
                record.getParams().put("playerName", playerName);
            } else {
                record.getParams().put("opponentName", playerName);
            }
        }

        return recordMapper.selectRecordHeatmap(record);
    }
}
