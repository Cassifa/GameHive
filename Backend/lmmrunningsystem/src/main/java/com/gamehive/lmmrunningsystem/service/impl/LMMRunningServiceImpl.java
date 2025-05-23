package com.gamehive.lmmrunningsystem.service.impl;

import com.gamehive.lmmrunningsystem.service.LMMRunningService;
import com.gamehive.lmmrunningsystem.service.impl.utils.LMMPool;
import org.springframework.stereotype.Service;

@Service
public class LMMRunningServiceImpl implements LMMRunningService {
    public final static LMMPool LMM_POOL =new LMMPool();
    @Override
    public String addBot(Integer userId, String botCode, String input) {
        LMM_POOL.addBot(userId,botCode,input);
        return "add bot success";
    }
}
