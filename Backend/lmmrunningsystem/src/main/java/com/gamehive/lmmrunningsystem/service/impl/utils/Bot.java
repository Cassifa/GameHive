package com.gamehive.lmmrunningsystem.service.impl.utils;

import lombok.AllArgsConstructor;
import lombok.Data;

@Data
@AllArgsConstructor
public class Bot {
    private Long userId;
    private String currentMap;
    private String LLMFlag;
    private String gameType;
    private String gameRule;
    private String historySteps;
    private String gridSize;

}
