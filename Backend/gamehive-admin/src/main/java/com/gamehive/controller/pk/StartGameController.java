package com.gamehive.controller.pk;

import com.gamehive.constants.GameTypeEnum;
import com.gamehive.service.pk.StartGameService;
import java.util.Objects;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.util.MultiValueMap;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

@RestController
public class StartGameController {

    @Autowired
    private StartGameService startGameService;

    @PostMapping("/api/pk/start/game/")
    public String startGame(@RequestParam MultiValueMap<String, String> data) {
        // 解析玩家ID
        Long aId = Long.parseLong(Objects.requireNonNull(data.getFirst("a_id")));
        Long bId = Long.parseLong(Objects.requireNonNull(data.getFirst("b_id")));
        // 从请求参数中获取游戏类型中文名称并转换为枚举
        String gameTypeName = Objects.requireNonNull(data.getFirst("game_type"));
        GameTypeEnum gameTypeEnum = GameTypeEnum.fromChineseName(gameTypeName);

        // 调用服务开始游戏
        return startGameService.startGame(aId, bId, gameTypeEnum);
    }
}
