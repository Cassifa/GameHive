package com.gamehive.dto;

import com.fasterxml.jackson.annotation.JsonIgnore;
import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.HashMap;
import java.util.Map;

/**
 * 玩家对局统计信息DTO
 * 用于保存用户在各种游戏模式下的对局统计数据
 *
 * @author Cassifa
 */
@Data
@AllArgsConstructor
@NoArgsConstructor
public class PlayerGameStatisticsDTO {

    /**
     * 用户ID
     */
    private Long userId;

    /**
     * 用户名
     */
    private String userName;

    /**
     * 本地对战统计（与AI算法对战）
     */
    private LocalGameStatistics localGameStats;

    /**
     * 与大模型对战统计
     */
    private LmmGameStatistics lmmGameStats;

    /**
     * 联机对战统计（与其他玩家对战）
     */
    private OnlineGameStatistics onlineGameStats;

    /**
     * 总体统计
     */
    private OverallStatistics overallStats;

    /**
     * 本地对战统计（与AI算法对战）
     */
    @Data
    @AllArgsConstructor
    @NoArgsConstructor
    public static class LocalGameStatistics {
        /**
         * 按算法类型分组的统计
         * Key: 算法名称, Value: 该算法的对局统计
         */
        private Map<String, AlgorithmStatistics> algorithmStats = new HashMap<>();

        /**
         * 本地对战总统计
         */
        private GameModeStatistics totalStats = new GameModeStatistics();
    }

    /**
     * 与大模型对战统计
     */
    @Data
    @AllArgsConstructor
    @NoArgsConstructor
    public static class LmmGameStatistics {
        /**
         * 与大模型对战的统计
         */
        private GameModeStatistics stats = new GameModeStatistics();
    }

    /**
     * 联机对战统计（与其他玩家对战）
     */
    @Data
    @AllArgsConstructor
    @NoArgsConstructor
    public static class OnlineGameStatistics {
        /**
         * 联机对战统计
         */
        private GameModeStatistics stats = new GameModeStatistics();
    }

    /**
     * 算法统计信息
     */
    @Data
    @AllArgsConstructor
    @NoArgsConstructor
    public static class AlgorithmStatistics {
        /**
         * 算法ID
         */
        private Long algorithmId;

        /**
         * 算法名称
         */
        private String algorithmName;

        /**
         * 该算法的对局统计
         */
        private GameModeStatistics stats = new GameModeStatistics();
    }

    /**
     * 游戏模式统计
     */
    @Data
    @AllArgsConstructor
    @NoArgsConstructor
    @JsonIgnoreProperties(ignoreUnknown = true)
    public static class GameModeStatistics {
        /**
         * 总对局数
         */
        private int totalGames = 0;

        /**
         * 胜利次数
         */
        private int wins = 0;

        /**
         * 失败次数
         */
        private int losses = 0;

        /**
         * 平局次数
         */
        private int draws = 0;

        /**
         * 胜率（为了兼容旧数据）
         */
        @JsonIgnore
        private double winRate = 0.0;

        /**
         * 计算胜率
         */
        public double getWinRate() {
            if (totalGames == 0) {
                return 0.0;
            }
            return (double) wins / totalGames * 100;
        }

        /**
         * 添加对局结果
         */
        public void addGameResult(Long winner, boolean isFirstPlayer) {
            totalGames++;
            if (winner == null) {
                // 平局
                draws++;
            } else if ((winner == 1 && isFirstPlayer) || (winner == 2 && !isFirstPlayer)) {
                // 胜利
                wins++;
            } else {
                // 失败
                losses++;
            }
        }
    }

    /**
     * 总体统计信息
     */
    @Data
    @AllArgsConstructor
    @NoArgsConstructor
    @JsonIgnoreProperties(ignoreUnknown = true)
    public static class OverallStatistics {
        /**
         * 所有模式总对局次数
         */
        private Integer totalGames = 0;

        /**
         * 所有模式总胜利次数
         */
        private Integer totalWins = 0;

        /**
         * 所有模式总失败次数
         */
        private Integer totalLosses = 0;

        /**
         * 所有模式总平局次数
         */
        private Integer totalDraws = 0;

        /**
         * 总胜率（百分比）
         */
        public Double getTotalWinRate() {
            if (totalGames == 0) {
                return 0.0;
            }
            return (double) totalWins / totalGames * 100;
        }
    }

    /**
     * 初始化统计对象
     */
    public void initializeStats() {
        if (localGameStats == null) {
            localGameStats = new LocalGameStatistics();
        }
        if (lmmGameStats == null) {
            lmmGameStats = new LmmGameStatistics();
        }
        if (onlineGameStats == null) {
            onlineGameStats = new OnlineGameStatistics();
        }
        if (overallStats == null) {
            overallStats = new OverallStatistics();
        }
    }

    /**
     * 更新总体统计
     */
    public void updateOverallStats() {
        if (overallStats == null) {
            overallStats = new OverallStatistics();
        }

        // 重置总体统计
        overallStats.setTotalGames(0);
        overallStats.setTotalWins(0);
        overallStats.setTotalLosses(0);
        overallStats.setTotalDraws(0);

        // 累加本地对战统计
        if (localGameStats != null && localGameStats.getTotalStats() != null) {
            GameModeStatistics localStats = localGameStats.getTotalStats();
            overallStats.setTotalGames(overallStats.getTotalGames() + localStats.getTotalGames());
            overallStats.setTotalWins(overallStats.getTotalWins() + localStats.getWins());
            overallStats.setTotalLosses(overallStats.getTotalLosses() + localStats.getLosses());
            overallStats.setTotalDraws(overallStats.getTotalDraws() + localStats.getDraws());
        }

        // 累加大模型对战统计
        if (lmmGameStats != null && lmmGameStats.getStats() != null) {
            GameModeStatistics lmmStats = lmmGameStats.getStats();
            overallStats.setTotalGames(overallStats.getTotalGames() + lmmStats.getTotalGames());
            overallStats.setTotalWins(overallStats.getTotalWins() + lmmStats.getWins());
            overallStats.setTotalLosses(overallStats.getTotalLosses() + lmmStats.getLosses());
            overallStats.setTotalDraws(overallStats.getTotalDraws() + lmmStats.getDraws());
        }

        // 累加联机对战统计
        if (onlineGameStats != null && onlineGameStats.getStats() != null) {
            GameModeStatistics onlineStats = onlineGameStats.getStats();
            overallStats.setTotalGames(overallStats.getTotalGames() + onlineStats.getTotalGames());
            overallStats.setTotalWins(overallStats.getTotalWins() + onlineStats.getWins());
            overallStats.setTotalLosses(overallStats.getTotalLosses() + onlineStats.getLosses());
            overallStats.setTotalDraws(overallStats.getTotalDraws() + onlineStats.getDraws());
        }
    }
} 