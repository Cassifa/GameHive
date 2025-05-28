package com.gamehive.dto;

import com.fasterxml.jackson.annotation.JsonIgnoreProperties;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.HashMap;
import java.util.Map;

/**
 * 玩家对局统计信息DTO
 * 用于保存用户在各种游戏模式下的对局统计数据
 * 结构：游戏模式 -> 游戏类型 -> 算法类型（仅本地对战）
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
     * 结构：游戏类型 -> 算法类型 -> 统计数据
     */
    private LocalGameStatistics localGameStats;

    /**
     * 与大模型对战统计
     * 结构：游戏类型 -> 统计数据
     */
    private LmmGameStatistics lmmGameStats;

    /**
     * 联机对战统计（与其他玩家对战）
     * 结构：游戏类型 -> 统计数据
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
         * 按游戏类型分组的统计
         * Key: 游戏类型名称（中文）, Value: 该游戏类型的统计
         */
        private Map<String, GameTypeStatistics> gameTypeStats = new HashMap<>();

        /**
         * 本地对战总统计
         */
        private BasicStatistics totalStats = new BasicStatistics();
    }

    /**
     * 与大模型对战统计
     */
    @Data
    @AllArgsConstructor
    @NoArgsConstructor
    public static class LmmGameStatistics {
        /**
         * 按游戏类型分组的统计
         * Key: 游戏类型名称（中文）, Value: 该游戏类型的统计
         */
        private Map<String, BasicStatistics> gameTypeStats = new HashMap<>();

        /**
         * 与大模型对战总统计
         */
        private BasicStatistics totalStats = new BasicStatistics();
    }

    /**
     * 联机对战统计（与其他玩家对战）
     */
    @Data
    @AllArgsConstructor
    @NoArgsConstructor
    public static class OnlineGameStatistics {
        /**
         * 按游戏类型分组的统计
         * Key: 游戏类型名称（中文）, Value: 该游戏类型的统计
         */
        private Map<String, BasicStatistics> gameTypeStats = new HashMap<>();

        /**
         * 联机对战总统计
         */
        private BasicStatistics totalStats = new BasicStatistics();
    }

    /**
     * 游戏类型统计信息（仅用于本地对战，包含算法分组）
     */
    @Data
    @AllArgsConstructor
    @NoArgsConstructor
    public static class GameTypeStatistics {
        /**
         * 按算法类型分组的统计
         * Key: 算法名称, Value: 该算法的对局统计
         */
        private Map<String, AlgorithmStatistics> algorithmStats = new HashMap<>();

        /**
         * 该游戏类型的总统计
         */
        private BasicStatistics totalStats = new BasicStatistics();
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
        private BasicStatistics stats = new BasicStatistics();
    }

    /**
     * 基础统计信息
     */
    @Data
    @AllArgsConstructor
    @NoArgsConstructor
    @JsonIgnoreProperties(ignoreUnknown = true)
    public static class BasicStatistics {
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
         * 终止游戏次数
         */
        private int aborts = 0;

        /**
         * 总对局数 = 胜利次数 + 失败次数 + 平局次数 + 终止游戏次数
         */
        public int getTotalGames() {
            return wins + losses + draws + aborts;
        }

        /**
         * 添加对局结果
         *
         * @param winner        赢家 (0=先手赢, 1=后手赢, 2=平局, 3=终止)
         * @param isFirstPlayer 当前玩家是否为先手
         */
        public void addGameResult(Long winner, boolean isFirstPlayer) {
            if (winner == null || winner == 2) {
                // 平局
                draws++;
            } else if (winner == 3) {
                // 终止
                aborts++;
            } else if ((winner == 0 && isFirstPlayer) || (winner == 1 && !isFirstPlayer)) {
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
         * 所有模式总终止次数
         */
        private Integer totalAborts = 0;

        /**
         * 所有模式总对局次数
         */
        public Integer getTotalGames() {
            return totalWins + totalLosses + totalDraws + totalAborts;
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
        overallStats.setTotalWins(0);
        overallStats.setTotalLosses(0);
        overallStats.setTotalDraws(0);
        overallStats.setTotalAborts(0);

        // 累加本地对战统计
        if (localGameStats != null && localGameStats.getTotalStats() != null) {
            BasicStatistics localStats = localGameStats.getTotalStats();
            overallStats.setTotalWins(overallStats.getTotalWins() + localStats.getWins());
            overallStats.setTotalLosses(overallStats.getTotalLosses() + localStats.getLosses());
            overallStats.setTotalDraws(overallStats.getTotalDraws() + localStats.getDraws());
            overallStats.setTotalAborts(overallStats.getTotalAborts() + localStats.getAborts());
        }

        // 累加大模型对战统计
        if (lmmGameStats != null && lmmGameStats.getTotalStats() != null) {
            BasicStatistics lmmStats = lmmGameStats.getTotalStats();
            overallStats.setTotalWins(overallStats.getTotalWins() + lmmStats.getWins());
            overallStats.setTotalLosses(overallStats.getTotalLosses() + lmmStats.getLosses());
            overallStats.setTotalDraws(overallStats.getTotalDraws() + lmmStats.getDraws());
            overallStats.setTotalAborts(overallStats.getTotalAborts() + lmmStats.getAborts());
        }

        // 累加联机对战统计
        if (onlineGameStats != null && onlineGameStats.getTotalStats() != null) {
            BasicStatistics onlineStats = onlineGameStats.getTotalStats();
            overallStats.setTotalWins(overallStats.getTotalWins() + onlineStats.getWins());
            overallStats.setTotalLosses(overallStats.getTotalLosses() + onlineStats.getLosses());
            overallStats.setTotalDraws(overallStats.getTotalDraws() + onlineStats.getDraws());
            overallStats.setTotalAborts(overallStats.getTotalAborts() + onlineStats.getAborts());
        }
    }
} 