package com.gamehive.comsumer.message;

import lombok.Data;
import lombok.AllArgsConstructor;
import lombok.NoArgsConstructor;

/**
 * LMM请求数据传输对象
 * 封装向LMM运行服务发送的所有参数
 *
 * @author Li Feifei
 * @date Created in 22:43 2025/6/9
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
     * 当前棋盘状态，字符串格式表示棋盘各位置状态
     */
    private String currentMap;

    /**
     * 玩家ID
     */
    private Long userId;

    /**
     * 大模型落子标志，标识大模型当前代表哪一方
     */
    private String LLMFlag;

    /**
     * 游戏类型，如"五子棋"、"围棋"等
     */
    private String gameType;

    /**
     * 游戏规则，描述游戏的具体规则
     */
    private String gameRule;

    /**
     * 历史步骤，记录之前的所有落子信息
     */
    private String historySteps;

    /**
     * 游戏棋盘格数
     */
    private String gridSize;
}
