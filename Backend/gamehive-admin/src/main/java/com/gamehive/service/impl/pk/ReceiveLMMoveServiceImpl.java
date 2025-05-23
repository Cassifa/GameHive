package com.gamehive.service.impl.pk;

import com.gamehive.comsumer.Game.Game;
import com.gamehive.comsumer.WebSocketServer;
import com.gamehive.service.pk.ReceiveLMMoveService;
import org.springframework.stereotype.Service;

@Service
public class ReceiveLMMoveServiceImpl implements ReceiveLMMoveService {

    @Override
    public String receiveBotMove(Long userId, Integer x, Integer y, String modelName, String reason) {
        // 根据模型名称和原因处理机器人移动逻辑
        if (WebSocketServer.users.get(userId) != null) {
            Game game = WebSocketServer.users.get(userId).game;
            if (game != null) {
                game.setLMMNextMove(x, y);
            }
        }
        return "receive bot move success";
    }
}
