package com.gamehive.comsumer.constants;

import lombok.AllArgsConstructor;
import lombok.Getter;

/**
 * @Description
 * @Author calciferli
 * @Date 2025/5/21 19:01
 */

@Getter
@AllArgsConstructor
public enum FeedBackEventTypeEnum {
    MOVE(0, "move"),
    RESULT(1, "result"),
    START(2, "start");

    private final int code;
    private final String type;

    public static FeedBackEventTypeEnum fromCode(int code) {
        for (FeedBackEventTypeEnum value : values()) {
            if (value.getCode() == code) {
                return value;
            }
        }
        return null;
    }

    public static FeedBackEventTypeEnum fromType(String type) {
        for (FeedBackEventTypeEnum value : values()) {
            if (value.getType() == type) {
                return value;
            }
        }
        return null;
    }
}
