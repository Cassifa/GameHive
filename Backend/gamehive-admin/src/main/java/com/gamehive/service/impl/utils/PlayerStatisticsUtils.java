package com.gamehive.service.impl.utils;

import com.fasterxml.jackson.core.JsonProcessingException;
import com.fasterxml.jackson.databind.ObjectMapper;
import com.gamehive.constants.GameModeEnum;
import com.gamehive.constants.GameTypeEnum;
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
 * 结构：游戏模式 -> 游戏类型 -> 算法类型（仅本地对战）
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
        
        // 初始化所有游戏类型和算法的统计对象
        initializeAllGameTypeAndAlgorithmStats(statistics);

        // 遍历所有对局记录
        for (Record record : records) {
            processRecord(statistics, record, userId);
        }

        // 更新各层级的总体统计
        updateAllLevelStats(statistics);

        return statistics;
    }

    /**
     * 初始化所有游戏类型和算法的统计对象
     */
    public static void initializeAllGameTypeAndAlgorithmStats(PlayerGameStatisticsDTO statistics) {
        // 初始化所有游戏类型的统计
        for (GameTypeEnum gameType : GameTypeEnum.values()) {
            String gameTypeName = gameType.getChineseName();
            
            // 本地对战：初始化游戏类型和算法统计
            PlayerGameStatisticsDTO.GameTypeStatistics localGameTypeStats = 
                statistics.getLocalGameStats().getGameTypeStats().computeIfAbsent(gameTypeName, 
                    k -> new PlayerGameStatisticsDTO.GameTypeStatistics());
            
            // 为本地对战的每个游戏类型初始化所有算法统计
            initializeAlgorithmStatsForGameType(localGameTypeStats);
            
            // 大模型对战：初始化游戏类型统计
            statistics.getLmmGameStats().getGameTypeStats().computeIfAbsent(gameTypeName, 
                k -> new PlayerGameStatisticsDTO.BasicStatistics());
            
            // 联机对战：初始化游戏类型统计
            statistics.getOnlineGameStats().getGameTypeStats().computeIfAbsent(gameTypeName, 
                k -> new PlayerGameStatisticsDTO.BasicStatistics());
        }
    }

    /**
     * 为指定游戏类型初始化所有算法统计
     */
    private static void initializeAlgorithmStatsForGameType(PlayerGameStatisticsDTO.GameTypeStatistics gameTypeStats) {
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
                    gameTypeStats.getAlgorithmStats().computeIfAbsent(algorithmName, 
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
     * 处理单条对局记录
     *
     * @param statistics 统计对象
     * @param record 对局记录
     * @param userId 用户ID
     */
    private static void processRecord(PlayerGameStatisticsDTO statistics, Record record, Long userId) {
        // 判断玩家是先手还是后手
        boolean isFirstPlayer = userId.equals(record.getFirstPlayerId());
        
        // 获取游戏类型名称
        String gameTypeName = record.getGameTypeName();
        if (gameTypeName == null || gameTypeName.trim().isEmpty()) {
            log.warn("对局记录缺少游戏类型名称: {}", record.getRecordId());
            return;
        }
        
        // 根据游戏模式分类处理
        GameModeEnum gameMode = GameModeEnum.fromCode(record.getGameMode());
        
        switch (gameMode) {
            case LOCAL_GAME:
                processLocalGameRecord(statistics, record, gameTypeName, isFirstPlayer);
                break;
            case LMM_GAME:
                processLmmGameRecord(statistics, record, gameTypeName, isFirstPlayer);
                break;
            case ONLINE_GAME:
                processOnlineGameRecord(statistics, record, gameTypeName, isFirstPlayer);
                break;
            default:
                log.warn("未知游戏模式: {}", record.getGameMode());
                break;
        }
    }

    /**
     * 处理本地对战记录
     */
    private static void processLocalGameRecord(PlayerGameStatisticsDTO statistics, Record record, 
                                             String gameTypeName, boolean isFirstPlayer) {
        PlayerGameStatisticsDTO.LocalGameStatistics localStats = statistics.getLocalGameStats();
        
        // 获取或创建游戏类型统计
        PlayerGameStatisticsDTO.GameTypeStatistics gameTypeStats = 
            localStats.getGameTypeStats().computeIfAbsent(gameTypeName, 
                k -> new PlayerGameStatisticsDTO.GameTypeStatistics());
        
        // 更新游戏类型总统计
        gameTypeStats.getTotalStats().addGameResult(record.getWinner(), isFirstPlayer);
        
        // 更新算法统计
        String algorithmName = record.getAlgorithmName();
        if (algorithmName != null && !algorithmName.trim().isEmpty()) {
            PlayerGameStatisticsDTO.AlgorithmStatistics algorithmStats = 
                gameTypeStats.getAlgorithmStats().computeIfAbsent(algorithmName, 
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
     * 处理大模型对战记录
     */
    private static void processLmmGameRecord(PlayerGameStatisticsDTO statistics, Record record, 
                                           String gameTypeName, boolean isFirstPlayer) {
        PlayerGameStatisticsDTO.LmmGameStatistics lmmStats = statistics.getLmmGameStats();
        
        // 获取或创建游戏类型统计
        PlayerGameStatisticsDTO.BasicStatistics gameTypeStats = 
            lmmStats.getGameTypeStats().computeIfAbsent(gameTypeName, 
                k -> new PlayerGameStatisticsDTO.BasicStatistics());
        
        // 更新游戏类型统计
        gameTypeStats.addGameResult(record.getWinner(), isFirstPlayer);
    }

    /**
     * 处理联机对战记录
     */
    private static void processOnlineGameRecord(PlayerGameStatisticsDTO statistics, Record record, 
                                              String gameTypeName, boolean isFirstPlayer) {
        PlayerGameStatisticsDTO.OnlineGameStatistics onlineStats = statistics.getOnlineGameStats();
        
        // 获取或创建游戏类型统计
        PlayerGameStatisticsDTO.BasicStatistics gameTypeStats = 
            onlineStats.getGameTypeStats().computeIfAbsent(gameTypeName, 
                k -> new PlayerGameStatisticsDTO.BasicStatistics());
        
        // 更新游戏类型统计
        gameTypeStats.addGameResult(record.getWinner(), isFirstPlayer);
    }

    /**
     * 更新所有层级的统计数据
     */
    private static void updateAllLevelStats(PlayerGameStatisticsDTO statistics) {
        // 更新本地对战总统计
        updateLocalGameTotalStats(statistics.getLocalGameStats());
        
        // 更新大模型对战总统计
        updateLmmGameTotalStats(statistics.getLmmGameStats());
        
        // 更新联机对战总统计
        updateOnlineGameTotalStats(statistics.getOnlineGameStats());
        
        // 更新总体统计
        statistics.updateOverallStats();
    }

    /**
     * 更新本地对战总统计
     */
    private static void updateLocalGameTotalStats(PlayerGameStatisticsDTO.LocalGameStatistics localStats) {
        PlayerGameStatisticsDTO.BasicStatistics totalStats = localStats.getTotalStats();
        totalStats.setWins(0);
        totalStats.setLosses(0);
        totalStats.setDraws(0);
        totalStats.setAborts(0);
        
        // 累加所有游戏类型的统计
        for (PlayerGameStatisticsDTO.GameTypeStatistics gameTypeStats : localStats.getGameTypeStats().values()) {
            PlayerGameStatisticsDTO.BasicStatistics gameTypeTotal = gameTypeStats.getTotalStats();
            totalStats.setWins(totalStats.getWins() + gameTypeTotal.getWins());
            totalStats.setLosses(totalStats.getLosses() + gameTypeTotal.getLosses());
            totalStats.setDraws(totalStats.getDraws() + gameTypeTotal.getDraws());
            totalStats.setAborts(totalStats.getAborts() + gameTypeTotal.getAborts());
        }
    }

    /**
     * 更新大模型对战总统计
     */
    private static void updateLmmGameTotalStats(PlayerGameStatisticsDTO.LmmGameStatistics lmmStats) {
        PlayerGameStatisticsDTO.BasicStatistics totalStats = lmmStats.getTotalStats();
        totalStats.setWins(0);
        totalStats.setLosses(0);
        totalStats.setDraws(0);
        totalStats.setAborts(0);
        
        // 累加所有游戏类型的统计
        for (PlayerGameStatisticsDTO.BasicStatistics gameTypeStats : lmmStats.getGameTypeStats().values()) {
            totalStats.setWins(totalStats.getWins() + gameTypeStats.getWins());
            totalStats.setLosses(totalStats.getLosses() + gameTypeStats.getLosses());
            totalStats.setDraws(totalStats.getDraws() + gameTypeStats.getDraws());
            totalStats.setAborts(totalStats.getAborts() + gameTypeStats.getAborts());
        }
    }

    /**
     * 更新联机对战总统计
     */
    private static void updateOnlineGameTotalStats(PlayerGameStatisticsDTO.OnlineGameStatistics onlineStats) {
        PlayerGameStatisticsDTO.BasicStatistics totalStats = onlineStats.getTotalStats();
        totalStats.setWins(0);
        totalStats.setLosses(0);
        totalStats.setDraws(0);
        totalStats.setAborts(0);
        
        // 累加所有游戏类型的统计
        for (PlayerGameStatisticsDTO.BasicStatistics gameTypeStats : onlineStats.getGameTypeStats().values()) {
            totalStats.setWins(totalStats.getWins() + gameTypeStats.getWins());
            totalStats.setLosses(totalStats.getLosses() + gameTypeStats.getLosses());
            totalStats.setDraws(totalStats.getDraws() + gameTypeStats.getDraws());
            totalStats.setAborts(totalStats.getAborts() + gameTypeStats.getAborts());
        }
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
            
            // 初始化所有游戏类型和算法的统计对象
            initializeAllGameTypeAndAlgorithmStats(existingStats);
        }

        // 处理新记录
        processRecord(existingStats, newRecord, userId);
        
        // 更新所有层级的统计
        updateAllLevelStats(existingStats);

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
        
        // 初始化所有游戏类型和算法的统计对象
        initializeAllGameTypeAndAlgorithmStats(statistics);
        
        return statistics;
    }
} 