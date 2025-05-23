package com.gamehive.service.impl.pk;


import com.gamehive.comsumer.WebSocketServer;
import com.gamehive.constants.GameTypeEnum;
import com.gamehive.constants.SpecialPlayerEnum;
import com.gamehive.service.pk.StartGameService;
import org.springframework.stereotype.Service;

@Service
public class StartGameServiceImpl implements StartGameService {

    @Override
    public String startGame(Long aId, Long bId, GameTypeEnum gameTypeEnum) {
        System.out.println("start game" + aId + bId);
        WebSocketServer.startGame(aId, SpecialPlayerEnum.PLAYER, bId, SpecialPlayerEnum.PLAYER, gameTypeEnum, false);
        return "StartGame success";
    }
}
