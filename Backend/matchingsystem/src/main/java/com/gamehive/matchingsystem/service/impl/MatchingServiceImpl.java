package com.gamehive.matchingsystem.service.impl;

import com.gamehive.matchingsystem.constants.GameTypeEnum;
import com.gamehive.matchingsystem.service.MatchingService;
import com.gamehive.matchingsystem.service.impl.utils.MatchingPool;
import org.springframework.stereotype.Service;

@Service
public class MatchingServiceImpl implements MatchingService {

    public final static MatchingPool matchingPool = new MatchingPool();
    
    @Override
    public String addPlayer(Integer userId, Integer rating, GameTypeEnum gameType) {
        System.out.println("addPlayer: " + userId + ", gameType: " + gameType.getChineseName());
        matchingPool.addPlayer(userId, rating, gameType);
        return "add success";
    }

    @Override
    public String removePlayer(Integer userId, Integer rating) {
        System.out.println("removePlayer: " + userId);
        matchingPool.removePlayer(userId);
        return "remove success";
    }
}
