package com.gamehive.comsumer.constants;

import lombok.AllArgsConstructor;
import lombok.Getter;

/*
 * @ Author     ：Li Feifei
 * @ Date       ：Created in 23:14 2025/5/20
 * @ Description：客户端事件类型
 */
@Getter
@AllArgsConstructor
public enum ReceiveEventTypeEnum {
    START(0, "start-matching"),
    STOP(1, "stop-matching"),
    MOVE(2, "move");

    private final int code;
    private final String type;

    public static ReceiveEventTypeEnum fromCode(int code) {
        for (ReceiveEventTypeEnum value : values()) {
            if (value.getCode() == code) {
                return value;
            }
        }
        return null;
    }

    public static ReceiveEventTypeEnum fromType(String type) {
        for (ReceiveEventTypeEnum value : values()) {
            if (value.getType() == type) {
                return value;
            }
        }
        return null;
    }
}
