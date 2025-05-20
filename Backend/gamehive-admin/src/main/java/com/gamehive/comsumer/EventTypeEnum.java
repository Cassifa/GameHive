package com.gamehive.comsumer;

/*
 * @ Author     ：Li Feifei
 * @ Date       ：Created in 23:14 2025/5/20
 * @ Description：客户端事件类型
 */
public enum EventTypeEnum {
    START(0, "start-matching"),
    STOP(1, "stop-matching"),
    MOVE(2, "move");

    private final int code;
    private final String type;

    EventTypeEnum(int code, String type) {
        this.code = code;
        this.type = type;
    }

    public long getCode() {
        return code;
    }

    public String getType() {
        return type;
    }

    public static EventTypeEnum fromCode(int code) {
        for (EventTypeEnum value : values()) {
            if (value.getCode() == code) {
                return value;
            }
        }
        return null;
    }

    public static EventTypeEnum fromType(String type) {
        for (EventTypeEnum value : values()) {
            if (value.getType() == type) {
                return value;
            }
        }
        return null;
    }
}
