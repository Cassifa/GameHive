package com.gamehive.service.impl;

import com.gamehive.dto.PlayerGameStatisticsDTO;
import com.gamehive.mapper.PlayerMapper;
import com.gamehive.mapper.RecordMapper;
import com.gamehive.pojo.Player;
import com.gamehive.pojo.Record;
import com.gamehive.service.IPlayerStatisticsService;
import com.gamehive.service.impl.utils.PlayerStatisticsUtils;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import org.springframework.transaction.annotation.Transactional;

import java.util.List;

/**
 * 玩家统计数据服务实现类
 *
 * @author Cassifa
 */
@Slf4j
@Service
public class PlayerStatisticsServiceImpl implements IPlayerStatisticsService {

    @Autowired
    private PlayerMapper playerMapper;

    @Autowired
    private RecordMapper recordMapper;

    @Autowired
    private PlayerStatisticsUtils playerStatisticsUtils;

    @Override
    public PlayerGameStatisticsDTO getPlayerStatistics(Long userId) {
        Player player = playerMapper.selectPlayerByUserId(userId);
        if (player == null) {
            log.warn("玩家不存在: {}", userId);
            return null;
        }

        String statisticsJson = player.getGameStatistics();
        PlayerGameStatisticsDTO statistics;
        
        // 如果统计信息为null或空，重新计算统计数据
        if (statisticsJson == null || statisticsJson.trim().isEmpty()) {
            log.info("玩家统计信息为空，重新计算统计数据: userId={}", userId);
            statistics = recalculatePlayerStatistics(userId);
        } else {
            statistics = PlayerStatisticsUtils.fromJsonString(statisticsJson);
            // 确保用户信息正确
            statistics.setUserId(userId);
            statistics.setUserName(player.getUserName());
            statistics.initializeStats();
            
            // 确保所有算法都有统计对象（处理新增算法的情况）
            PlayerStatisticsUtils.initializeAllAlgorithmStats(statistics);
        }

        return statistics;
    }

    @Override
    @Transactional
    public void updatePlayerStatistics(Record record) {
        // 更新先手玩家统计
        if (record.getFirstPlayerId() != null && record.getFirstPlayerId() > 0) {
            updateSinglePlayerStatistics(record.getFirstPlayerId(), record);
        }

        // 更新后手玩家统计
        if (record.getSecondPlayerId() != null && record.getSecondPlayerId() > 0) {
            updateSinglePlayerStatistics(record.getSecondPlayerId(), record);
        }
    }

    /**
     * 更新单个玩家的统计数据
     */
    private void updateSinglePlayerStatistics(Long userId, Record record) {
        try {
            Player player = playerMapper.selectPlayerByUserId(userId);
            if (player == null) {
                log.warn("玩家不存在，跳过统计更新: {}", userId);
                return;
            }

            // 获取现有统计数据，如果为null则重新计算
            PlayerGameStatisticsDTO existingStats;
            String statisticsJson = player.getGameStatistics();
            
            if (statisticsJson == null || statisticsJson.trim().isEmpty()) {
                log.info("玩家统计信息为空，重新计算后更新: userId={}", userId);
                // 先重新计算现有的所有对局记录
                existingStats = recalculatePlayerStatistics(userId);
            } else {
                existingStats = PlayerStatisticsUtils.fromJsonString(statisticsJson);
                existingStats.setUserId(userId);
                existingStats.setUserName(player.getUserName());
            }

            // 更新统计数据（添加新的对局记录）
            PlayerGameStatisticsDTO updatedStats = PlayerStatisticsUtils.updateStatistics(existingStats, record, userId);

            // 保存更新后的统计数据
            player.setGameStatistics(PlayerStatisticsUtils.toJsonString(updatedStats));
            playerMapper.updatePlayer(player);

            log.debug("玩家统计数据更新成功: userId={}", userId);
        } catch (Exception e) {
            log.error("更新玩家统计数据失败: userId={}", userId, e);
        }
    }

    @Override
    @Transactional
    public PlayerGameStatisticsDTO recalculatePlayerStatistics(Long userId) {
        Player player = playerMapper.selectPlayerByUserId(userId);
        if (player == null) {
            log.warn("玩家不存在: {}", userId);
            return null;
        }

        // 使用专门的方法查询该玩家的所有对局记录
        List<Record> records = recordMapper.selectRecordsByPlayerId(userId);

        // 重新计算统计数据
        PlayerGameStatisticsDTO statistics = PlayerStatisticsUtils.calculateStatistics(
            userId, player.getUserName(), records);

        // 保存计算结果
        player.setGameStatistics(PlayerStatisticsUtils.toJsonString(statistics));
        playerMapper.updatePlayer(player);

        log.info("玩家统计数据重新计算完成: userId={}, totalGames={}", 
            userId, statistics.getOverallStats().getTotalGames());

        return statistics;
    }

    @Override
    @Transactional
    public PlayerGameStatisticsDTO initializePlayerStatistics(Long userId, String userName) {
        Player player = playerMapper.selectPlayerByUserId(userId);
        if (player == null) {
            log.warn("玩家不存在: {}", userId);
            return null;
        }

        // 创建空的统计数据
        PlayerGameStatisticsDTO statistics = PlayerStatisticsUtils.createEmptyStatistics(userId, userName);

        // 保存初始化的统计数据
        player.setGameStatistics(PlayerStatisticsUtils.toJsonString(statistics));
        playerMapper.updatePlayer(player);

        log.info("玩家统计数据初始化完成: userId={}", userId);

        return statistics;
    }
} 