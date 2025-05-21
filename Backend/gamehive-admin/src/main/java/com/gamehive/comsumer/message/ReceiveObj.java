package com.gamehive.comsumer.message;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

/*
 * @ Author     ：Li Feifei
 * @ Date       ：Created in 23:21 2025/5/20
 * @ Description：客户端与服务器通信类
 */
@Data
@AllArgsConstructor
@NoArgsConstructor
public class ReceiveObj {
    //本次事件类型
    private String event;
    //玩家选择的游戏类型
    private String gameType;
    //玩家是否选择与大模型下棋
    private Boolean playWithLMM;
    private int x;
    private int y;
    private int userId;
    private String userName;
}
