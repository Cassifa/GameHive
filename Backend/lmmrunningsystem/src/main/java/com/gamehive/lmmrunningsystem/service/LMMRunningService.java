package com.gamehive.lmmrunningsystem.service;

public interface LMMRunningService {

    String addLMM(
            Long userId,
            String currentMap,
            String LLMFlag,
            String gameType,
            String gameRule,
            String historySteps,
            String gridSize
    );
}
