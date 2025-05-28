package com.gamehive.service.impl.utils;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.gamehive.constants.GameModeEnum;
import com.gamehive.dto.PlayerGameStatisticsDTO;
import com.gamehive.mapper.AlgorithmTypeMapper;
import com.gamehive.pojo.AlgorithmType;
import com.gamehive.pojo.Record;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;

import java.util.List;

/**
 * 玩家统计数据处理工具类
 * 用于处理玩家对局统计数据的序列化、反序列化和计算
 *
 * @author Cassifa
 */
@Slf4j
@Component
public class PlayerStatisticsUtils {

    private static final ObjectMapper objectMapper = new ObjectMapper();
    
    private static AlgorithmTypeMapper algorithmTypeMapper;
    
    @Autowired
    public void setAlgorithmTypeMapper(AlgorithmTypeMapper algorithmTypeMapper) {
        PlayerStatisticsUtils.algorithmTypeMapper = algorithmTypeMapper;
    }

    /**
     * 将统计DTO转换为JSON字符串
     *
     * @param statistics 统计DTO对象
     * @return JSON字符串
     */
    public static String toJsonString(PlayerGameStatisticsDTO statistics) {
        try {
            return objectMapper.writeValueAsString(statistics);
        } catch (JsonProcessingException e) {
            log.error("统计数据序列化失败", e);
            return "{}";
        }
    }

    /**
     * 从JSON字符串解析统计DTO
     *
     * @param jsonString JSON字符串
     * @return 统计DTO对象
     */
    public static PlayerGameStatisticsDTO fromJsonString(String jsonString) {
        try {
            if (jsonString == null || jsonString.trim().isEmpty()) {
                return new PlayerGameStatisticsDTO();
            }
            return objectMapper.readValue(jsonString, PlayerGameStatisticsDTO.class);
        } catch (JsonProcessingException e) {
            log.error("统计数据反序列化失败: {}", jsonString, e);
            return new PlayerGameStatisticsDTO();
        }
    }

    /**
     * 根据对局记录列表计算玩家统计数据
     *
     * @param userId 用户ID
     * @param userName 用户名
     * @param records 对局记录列表
     * @return 计算后的统计DTO
     */
    public static PlayerGameStatisticsDTO calculateStatistics(Long userId, String userName, List<Record> records) {
        PlayerGameStatisticsDTO statistics = new PlayerGameStatisticsDTO();
        statistics.setUserId(userId);
        statistics.setUserName(userName);
        statistics.initializeStats();
        
        // 初始化所有算法的统计对象
        initializeAllAlgorithmStats(statistics);

        // 遍历所有对局记录
        for (Record record : records) {
            processRecord(statistics, record, userId);
        }

        // 更新总体统计
        statistics.updateOverallStats();

        return statistics;
    }

    /**
     * 处理单条对局记录
     *
     * @param statistics 统计对象
     * @param record 对局记录
     * @param userId 用户ID
     */
    private static void processRecord(PlayerGameStatisticsDTO statistics, Record record, Long userId) {
        // 判断玩家是先手还是后手
        boolean isFirstPlayer = userId.equals(record.getFirstPlayerId());
        
        // 根据游戏模式分类处理
        GameModeEnum gameMode = GameModeEnum.fromCode(record.getGameMode());
        
        switch (gameMode) {
            case LOCAL_GAME:
                processLocalGameRecord(statistics, record, isFirstPlayer);
                break;
            case LMM_GAME:
                processLmmGameRecord(statistics, record, isFirstPlayer);
                break;
            case ONLINE_GAME:
                processOnlineGameRecord(statistics, record, isFirstPlayer);
                break;
            default:
                log.warn("未知游戏模式: {}", record.getGameMode());
                break;
        }
    }

    /**
     * 处理本地对战记录
     */
    private static void processLocalGameRecord(PlayerGameStatisticsDTO statistics, Record record, boolean isFirstPlayer) {
        PlayerGameStatisticsDTO.LocalGameStatistics localStats = statistics.getLocalGameStats();
        
        // 更新总统计
        localStats.getTotalStats().addGameResult(record.getWinner(), isFirstPlayer);
        
        // 更新算法统计
        String algorithmName = record.getAlgorithmName();
        if (algorithmName != null && !algorithmName.trim().isEmpty()) {
            PlayerGameStatisticsDTO.AlgorithmStatistics algorithmStats = 
                localStats.getAlgorithmStats().computeIfAbsent(algorithmName, 
                    k -> {
                        PlayerGameStatisticsDTO.AlgorithmStatistics stats = new PlayerGameStatisticsDTO.AlgorithmStatistics();
                        stats.setAlgorithmId(record.getAlgorithmId());
                        stats.setAlgorithmName(algorithmName);
                        return stats;
                    });
            
            algorithmStats.getStats().addGameResult(record.getWinner(), isFirstPlayer);
        }
    }

    /**
     * 初始化所有算法的统计对象
     */
    public static void initializeAllAlgorithmStats(PlayerGameStatisticsDTO statistics) {
        if (algorithmTypeMapper == null) {
            log.warn("AlgorithmTypeMapper未注入，跳过算法统计初始化");
            return;
        }
        
        try {
            // 获取所有算法类型
            List<AlgorithmType> allAlgorithms = algorithmTypeMapper.selectAlgorithmTypeList(new AlgorithmType());
            
            // 为每个算法创建空的统计对象
            for (AlgorithmType algorithm : allAlgorithms) {
                String algorithmName = algorithm.getAlgorithmName();
                if (algorithmName != null && !algorithmName.trim().isEmpty()) {
                    statistics.getLocalGameStats().getAlgorithmStats().computeIfAbsent(algorithmName, 
                        k -> {
                            PlayerGameStatisticsDTO.AlgorithmStatistics stats = new PlayerGameStatisticsDTO.AlgorithmStatistics();
                            stats.setAlgorithmId(algorithm.getAlgorithmId());
                            stats.setAlgorithmName(algorithmName);
                            return stats;
                        });
                }
            }
        } catch (Exception e) {
            log.error("初始化算法统计失败", e);
        }
    }

    /**
     * 处理大模型对战记录
     */
    private static void processLmmGameRecord(PlayerGameStatisticsDTO statistics, Record record, boolean isFirstPlayer) {
        PlayerGameStatisticsDTO.LmmGameStatistics lmmStats = statistics.getLmmGameStats();
        lmmStats.getStats().addGameResult(record.getWinner(), isFirstPlayer);
    }

    /**
     * 处理联机对战记录
     */
    private static void processOnlineGameRecord(PlayerGameStatisticsDTO statistics, Record record, boolean isFirstPlayer) {
        PlayerGameStatisticsDTO.OnlineGameStatistics onlineStats = statistics.getOnlineGameStats();
        onlineStats.getStats().addGameResult(record.getWinner(), isFirstPlayer);
    }

    /**
     * 更新玩家统计数据（增量更新）
     *
     * @param existingStats 现有统计数据
     * @param newRecord 新的对局记录
     * @param userId 用户ID
     * @return 更新后的统计数据
     */
    public static PlayerGameStatisticsDTO updateStatistics(PlayerGameStatisticsDTO existingStats, Record newRecord, Long userId) {
        if (existingStats == null) {
            existingStats = new PlayerGameStatisticsDTO();
            existingStats.setUserId(userId);
            existingStats.initializeStats();
            
            // 初始化所有算法的统计对象
            initializeAllAlgorithmStats(existingStats);
        }

        // 处理新记录
        processRecord(existingStats, newRecord, userId);
        
        // 更新总体统计
        existingStats.updateOverallStats();

        return existingStats;
    }

    /**
     * 创建空的统计对象
     *
     * @param userId 用户ID
     * @param userName 用户名
     * @return 初始化的统计对象
     */
    public static PlayerGameStatisticsDTO createEmptyStatistics(Long userId, String userName) {
        PlayerGameStatisticsDTO statistics = new PlayerGameStatisticsDTO();
        statistics.setUserId(userId);
        statistics.setUserName(userName);
        statistics.initializeStats();
        
        // 初始化所有算法的统计对象
        initializeAllAlgorithmStats(statistics);
        
        return statistics;
    }
} 