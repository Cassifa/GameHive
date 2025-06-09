package com.gamehive.lmmrunningsystem.dto;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

/**
 * LMM请求数据传输对象
 * 封装向LMM运行服务发送的所有参数，包含游戏ID和游戏状态信息
 *
 * @author Li Feifei
 * @since 1.0.0
 */
@Data
@NoArgsConstructor
@AllArgsConstructor
public class LMMRequestDTO {

    /**
     * 游戏唯一标识符
     */
    private Integer gameId;

    /**
     * 玩家用户ID，用于标识请求来源
     */
    private Long userId;

    /**
     * 当前棋盘状态字符串
     * 格式：每行用换行符分隔，每个位置用数字表示
     * 0表示空位，1表示玩家1的棋子，2表示玩家2的棋子
     * 例如："000\n010\n000" 表示3x3棋盘，中心位置有玩家1的棋子
     */
    private String currentMap;

    /**
     * 大模型落子标志
     * 标识当前大模型代表哪一方进行游戏
     * 通常为"1"或"2"，对应棋盘上的数字标识
     */
    private String LLMFlag;

    /**
     * 游戏类型名称
     * 如"井字棋"、"五子棋"、"围棋"等
     */
    private String gameType;

    /**
     * 游戏规则描述
     * 详细描述当前游戏的规则和获胜条件
     */
    private String gameRule;

    /**
     * 历史步骤记录
     * 记录之前所有的落子历史，用于大模型分析局势
     * 格式可以是JSON字符串或其他约定格式
     */
    private String historySteps;

    /**
     * 棋盘网格大小
     * 表示棋盘的边长，如"3"表示3x3棋盘，"15"表示15x15棋盘
     */
    private String gridSize;

    /**
     * 转换为原有的LMMRequest对象
     * 为了保持与现有代码的兼容性
     *
     * @return LMMRequest 原有的请求对象
     */
    public LMMRequest toLMMRequest() {
        return new LMMRequest(userId, currentMap, LLMFlag, gameType, gameRule, historySteps, gridSize);
    }
} 