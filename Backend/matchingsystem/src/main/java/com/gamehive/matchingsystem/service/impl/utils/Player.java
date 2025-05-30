package com.gamehive.matchingsystem.service.impl.utils;

import com.gamehive.matchingsystem.constants.GameTypeEnum;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

@Data
@AllArgsConstructor
@NoArgsConstructor
public class Player {

    private Integer userId;
    private Integer rating;
    private Integer waitingTime;
    private GameTypeEnum gameType;
}
