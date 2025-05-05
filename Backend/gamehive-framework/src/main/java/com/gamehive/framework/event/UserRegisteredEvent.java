package com.gamehive.framework.event;

import org.springframework.context.ApplicationEvent;
import com.gamehive.common.core.domain.entity.SysUser;

/**
 * 用户注册成功事件
 */
public class UserRegisteredEvent extends ApplicationEvent {
    private static final long serialVersionUID = 1L;
    
    private final SysUser user;
    
    public UserRegisteredEvent(Object source, SysUser user) {
        super(source);
        this.user = user;
    }
    
    public SysUser getUser() {
        return user;
    }
} 