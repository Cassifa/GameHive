package com.gamehive.service.impl;

import java.util.List;
import java.util.Map;
import java.util.Set;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import com.baomidou.mybatisplus.core.conditions.query.QueryWrapper;
import com.gamehive.common.core.domain.model.LoginUser;
import com.gamehive.common.utils.SecurityUtils;
import com.gamehive.framework.web.service.SysPermissionService;
import com.gamehive.mapper.RecordMapper;
import com.gamehive.mapper.PlayerMapper;
import com.gamehive.pojo.Record;
import com.gamehive.pojo.Player;
import com.gamehive.constants.GameModeEnum;
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
    private PlayerMapper playerMapper;

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
        // 获取当前用户
        LoginUser loginUser = SecurityUtils.getLoginUser();
        Record record = recordMapper.selectRecordByRecordId(recordId);

        // 如果记录不存在，直接返回null
        if (record == null) {
            return null;
        }

        // 如果是管理员，可以查看所有记录
        if (loginUser != null && loginUser.getUser() != null) {
            Set<String> roles = permissionService.getRolePermission(loginUser.getUser());
            if (roles.contains("admin")) {
                return record;
            }
        }

        // 非管理员只能查看自己的记录
        Long userId = loginUser != null && loginUser.getUser() != null ? loginUser.getUser().getUserId() : null;
        if (userId != null && (record.getFirstPlayerId().equals(userId) || record.getSecondPlayerId().equals(userId))) {
            return record;
        }

        return null;
    }

    /**
     * 查询对局记录列表
     *
     * @param record 对局记录
     * @return 对局记录
     */
    @Override
    public List<Record> selectRecordList(Record record) {
        // 获取当前用户
        LoginUser loginUser = SecurityUtils.getLoginUser();
        if (loginUser == null || loginUser.getUser() == null) {
            return null;
        }

        // 判断当前用户是否为管理员
        boolean isAdmin = false;
        Set<String> roles = permissionService.getRolePermission(loginUser.getUser());
        if (roles.contains("admin")) {
            isAdmin = true;
        }

        // 如果不是管理员，则只查询与当前用户相关的记录
        if (!isAdmin) {
            record.getParams().put("currentUserId", loginUser.getUser().getUserId());
        }

        // 处理playerName参数
        String playerName = (String) record.getParams().get("playerName");
        if (playerName != null && !playerName.isEmpty()) {
            if (isAdmin) {
                record.getParams().put("playerName", playerName);
            } else {
                record.getParams().put("opponentName", playerName);
            }
        }

        return recordMapper.selectRecordList(record);
    }

    /**
     * 查询对局记录总数
     *
     * @param record 对局记录
     * @return 总数
     */
    @Override
    public long selectRecordCount(Record record) {
        // 获取当前用户
        LoginUser loginUser = SecurityUtils.getLoginUser();
        if (loginUser == null || loginUser.getUser() == null) {
            return 0;
        }

        // 判断当前用户是否为管理员
        boolean isAdmin = false;
        Set<String> roles = permissionService.getRolePermission(loginUser.getUser());
        if (roles.contains("admin")) {
            isAdmin = true;
        }

        // 如果不是管理员，则只查询与当前用户相关的记录
        if (!isAdmin) {
            record.getParams().put("currentUserId", loginUser.getUser().getUserId());
        }

        // 处理playerName参数
        String playerName = (String) record.getParams().get("playerName");
        if (playerName != null && !playerName.isEmpty()) {
            if (isAdmin) {
                record.getParams().put("playerName", playerName);
            } else {
                record.getParams().put("opponentName", playerName);
            }
        }

        return recordMapper.selectRecordCount(record);
    }

    /**
     * 新增对局记录
     *
     * @param record 对局记录
     * @return 结果
     */
    @Override
    @Transactional
    public int insertRecord(Record record) {
        int result = recordMapper.insertRecord(record);
        
        // 如果是与AI对局，更新玩家积分
        if (record.getGameMode() != null && record.getGameMode() == GameModeEnum.LMM_GAME.getCode() && result > 0) {
            updatePlayerRaking(record);
        }
        
        return result;
    }

    /**
     * 更新玩家积分
     * 胜利 +5分，失败 -5分，平局或终止不变
     */
    private void updatePlayerRaking(Record record) {
        // 获取玩家ID和是否为先手
        Long playerId;
        boolean isFirstPlayer;
        
        if (record.getFirstPlayerId() > 0) { // 玩家是先手
            playerId = record.getFirstPlayerId();
            isFirstPlayer = true;
        } else if (record.getSecondPlayerId() > 0) { // 玩家是后手
            playerId = record.getSecondPlayerId();
            isFirstPlayer = false;
        } else {
            return; // 如果都不是有效玩家ID，直接返回
        }

        // 查询玩家信息
        Player player = playerMapper.selectPlayerByUserId(playerId);
        if (player == null) {
            return;
        }

        // 计算积分变化
        int scoreChange = 0;
        if (record.getWinner() != null) {
            if ((isFirstPlayer && record.getWinner() == 0) || // 玩家是先手且先手赢
                (!isFirstPlayer && record.getWinner() == 1)) { // 玩家是后手且后手赢
                scoreChange = 5; // 胜利加5分
            } else if (record.getWinner() != 2 && record.getWinner() != 3) { // 不是平局且不是终止
                scoreChange = -5; // 失败减5分
            }
        }

        // 更新积分
        if (scoreChange != 0) {
            player.setRaking(player.getRaking() + scoreChange);
            playerMapper.updatePlayer(player);
        }
    }

    /**
     * 获取对局记录热力图数据
     */
    @Override
    public List<Map<String, Object>> getHeatmapData(Long userId, Long gameTypeId, Integer gameMode,
                                                    Long algorithmId, Long winner, String playerName) {

        if (userId == null) {
            return null;
        }

        // 判断当前用户是否为管理员
        boolean isAdmin = false;
        LoginUser loginUser = SecurityUtils.getLoginUser();
        if (loginUser != null && loginUser.getUser() != null) {
            Set<String> roles = permissionService.getRolePermission(loginUser.getUser());
            if (roles.contains("admin")) {
                isAdmin = true;
            }
        }

        Record record = new Record();
        record.setGameTypeId(gameTypeId);
        // 直接设置gameMode
        if (gameMode != null) {
            record.setGameMode(gameMode);
        }
        record.setAlgorithmId(algorithmId);
        record.setWinner(winner);

        // 如果不是管理员，添加用户ID过滤
        if (!isAdmin) {
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

    /**
     * 根据对局ID获取对局详情（包含对局记录和游戏类型信息）
     */
    @Override
    public Map<String, Object> selectRecordDetailByRecordId(Long recordId) {
        if (recordId == null) {
            return null;
        }

        // 直接获取对局详情，不进行权限验证
        return recordMapper.selectRecordDetailByRecordId(recordId);
    }
}
