package com.gamehive.service.impl;

import java.util.List;
import java.util.Map;
import java.util.Set;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;

import com.baomidou.mybatisplus.core.conditions.query.LambdaQueryWrapper;
import com.baomidou.mybatisplus.core.conditions.query.QueryWrapper;
import com.gamehive.common.core.domain.entity.SysUser;
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
        // 使用Wrapper条件构建器
        QueryWrapper<Record> queryWrapper = new QueryWrapper<>();

        // 1. 添加用户过滤条件
        queryWrapper.and(wrapper -> wrapper
                .eq("first_player_id", userId)
                .or()
                .eq("second_player_id", userId));

        // 2. 添加其他查询条件
        if (record.getGameTypeId() != null) {
            queryWrapper.eq("game_type_id", record.getGameTypeId());
        }
        if (record.getGameTypeName() != null && !record.getGameTypeName().isEmpty()) {
            queryWrapper.like("game_type_name", record.getGameTypeName());
        }
        if (record.getIsPkAi() != null) {
            queryWrapper.eq("is_pk_ai", record.getIsPkAi());
        }
        if (record.getAlgorithmId() != null) {
            queryWrapper.eq("algorithm_id", record.getAlgorithmId());
        }
        if (record.getAlgorithmName() != null && !record.getAlgorithmName().isEmpty()) {
            queryWrapper.like("algorithm_name", record.getAlgorithmName());
        }
        if (record.getWinner() != null) {
            queryWrapper.eq("winner", record.getWinner());
        }
        if (record.getFirstPlayer() != null && !record.getFirstPlayer().isEmpty()) {
            queryWrapper.like("first_player", record.getFirstPlayer());
        }
        if (record.getSecondPlayerName() != null && !record.getSecondPlayerName().isEmpty()) {
            queryWrapper.like("second_player_name", record.getSecondPlayerName());
        }

        // 3. 处理playerName参数
        String playerName = (String) record.getParams().get("playerName");
        if (playerName != null && !playerName.isEmpty()) {
            // 普通用户传playerName时，需要匹配对手名称
            queryWrapper.and(wrapper -> wrapper
                    .and(w -> w
                            .eq("first_player_id", userId)
                            .like("second_player_name", playerName))
                    .or(w -> w
                            .eq("second_player_id", userId)
                            .like("first_player", playerName)));
        }

        // 执行查询
        return recordMapper.selectList(queryWrapper);
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
        // 在Service层构建查询条件

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

        // 构建QueryWrapper
        QueryWrapper<Record> queryWrapper = new QueryWrapper<>();

        // 根据不同条件过滤数据
        // 1. 如果不是管理员，只显示与当前用户相关的对局记录
        if (!isAdmin && userId != null) {
            queryWrapper.and(wrapper -> wrapper
                    .eq("first_player_id", userId)
                    .or()
                    .eq("second_player_id", userId));
        }

        // 2. 根据游戏类型、AI对局、算法ID、赢家等过滤
        if (gameTypeId != null) {
            queryWrapper.eq("game_type_id", gameTypeId);
        }
        if (isPkAi != null) {
            queryWrapper.eq("is_pk_ai", isPkAi);
        }
        if (algorithmId != null) {
            queryWrapper.eq("algorithm_id", algorithmId);
        }
        if (winner != null) {
            queryWrapper.eq("winner", winner);
        }

        // 3. 根据玩家名称过滤
        if (playerName != null && !playerName.isEmpty()) {
            if (isAdmin) {
                // 管理员传playerName时，匹配任意一方玩家
                queryWrapper.and(wrapper -> wrapper
                        .like("first_player", playerName)
                        .or()
                        .like("second_player_name", playerName));
            } else if (userId != null) {
                // 普通用户传playerName时，匹配对手名称
                queryWrapper.and(wrapper -> wrapper
                        .and(w -> w
                                .eq("first_player_id", userId)
                                .like("second_player_name", playerName))
                        .or(w -> w
                                .eq("second_player_id", userId)
                                .like("first_player", playerName)));
            }
        }

        // 添加日期格式化和分组
        queryWrapper.select("DATE_FORMAT(record_time, '%Y-%m-%d') as date", "COUNT(*) as count");
        queryWrapper.groupBy("DATE_FORMAT(record_time, '%Y-%m-%d')");
        queryWrapper.orderByAsc("date");

        // 执行查询
        List<Map<String, Object>> heatmapData = recordMapper.selectMaps(queryWrapper);

        return heatmapData;
    }
}
