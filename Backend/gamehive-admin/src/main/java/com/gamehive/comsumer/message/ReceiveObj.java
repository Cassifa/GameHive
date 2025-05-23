package com.gamehive.comsumer.message;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

/**
 * 客户端与服务器通信数据对象
 *
 * <p>用于封装客户端发送给服务器的游戏操作和请求信息</p>
 *
 * @author Li Feifei
 * @date 2025/5/20 23:21
 */
@Data
@AllArgsConstructor
@NoArgsConstructor
public class ReceiveObj {

    /**
     * 事件类型，对应ReceiveEventTypeEnum中的类型值
     */
    private String event;

    /**
     * 玩家选择的游戏类型名称
     */
    private String gameType;

    /**
     * 是否选择与语言大模型(LMM)对战
     */
    private Boolean playWithLMM;

    /**
     * 操作/移动的x坐标
     */
    private int x;

    /**
     * 操作/移动的y坐标
     */
    private int y;

    /**
     * 玩家用户ID
     */
    private int userId;

    /**
     * 玩家用户名
     */
    private String userName;
}
