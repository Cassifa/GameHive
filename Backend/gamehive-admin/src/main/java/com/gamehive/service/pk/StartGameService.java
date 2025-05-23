package com.gamehive.service.pk;

import com.gamehive.constants.GameTypeEnum;

public interface StartGameService {

    String startGame(Long aId, Long bId, GameTypeEnum gameTypeEnum);
}
