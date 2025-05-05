package com.gamehive.listener;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.context.event.EventListener;
import org.springframework.stereotype.Component;
import com.gamehive.framework.event.UserRegisteredEvent;
import com.gamehive.pojo.Player;
import com.gamehive.service.IPlayerService;
import com.gamehive.common.core.domain.entity.SysUser;

/**
 * 用户注册事件监听器
 */
@Component
public class UserRegistrationListener {

    @Autowired
    private IPlayerService playerService;

    @EventListener
    public void handleUserRegisteredEvent(UserRegisteredEvent event) {
        SysUser user = event.getUser();

        // 创建玩家记录
        Player player = new Player();
        player.setUserId(user.getUserId());
        player.setUserName(user.getUserName());

        playerService.insertPlayer(player);
    }
} 