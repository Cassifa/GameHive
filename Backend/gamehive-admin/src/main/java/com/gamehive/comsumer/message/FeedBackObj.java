package com.gamehive.comsumer.message;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

/**
 * @Description 回传客户端数据
 * @Author calciferli
 * @Date 2025/5/21 18:58
 */
@Data
@AllArgsConstructor
@NoArgsConstructor
public class FeedBackObj {

    /**
     * 事件类型，对应FeedBackEventTypeEnum中的类型值
     */
    private String event;

    /**
     * 是否先手，true表示当前玩家是先手
     */
    private boolean isFirst;

    /**
     * GameStatusEnum.name
     * draw、aWin、bWin
     */
    private String winStatus;

    /**
     * 落子/操作的x坐标
     */
    private int x;

    /**
     * 落子/操作的y坐标
     */
    private int y;

    /**
     * 对手玩家ID
     */
    private Long opponentId;

    /**
     * 对手玩家名称
     */
    private String opponentName;

    /**
     * 游戏状态信息，JSON格式字符串
     */
    private String gameStatus;
}
