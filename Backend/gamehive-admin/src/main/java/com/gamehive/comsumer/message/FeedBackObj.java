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
    //本次事件类型
    private String event;
    private int x;
    private int y;
    String gameStatus;
}
