package com.gamehive.lmmrunningsystem.dto;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

/**
 * LMM请求数据传输对象
 *
 * @author Li Feifei
 * @since 1.0.0
 */
@Data
@NoArgsConstructor
@AllArgsConstructor
public class LMMRequestDTO {

    // 游戏唯一标识符
    private Integer gameId;

    // 玩家用户ID
    private Long userId;

    // 当前棋盘状态字符串，格式：每行用换行符分隔，0表示空位，1表示玩家1，2表示玩家2
    private String currentMap;

    // 大模型落子标志，通常为"1"或"2"
    private String LLMFlag;

    // 游戏类型名称
    private String gameType;

    // 游戏规则描述
    private String gameRule;

    // 历史步骤记录
    private String historySteps;

    // 棋盘网格大小，如"3"表示3x3棋盘
    private String gridSize;

    // 大模型允许的最大运行时间（毫秒）
    private Long allowedTimeout;
} 
