package com.gamehive.matchingsystem.controller;

import com.gamehive.matchingsystem.constants.GameTypeEnum;
import com.gamehive.matchingsystem.service.MatchingService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.util.MultiValueMap;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import java.util.Objects;

@RestController
public class MatchingController {
    @Autowired
    private MatchingService matchingService;

    /**
     * 添加玩家到匹配池
     * @param data 请求参数，包含：
     *             - user_id: 用户ID
     *             - rating: 玩家评分
     *             - game_type: 游戏类型中文名称(如"五子棋"、"井字棋"等)
     * @return 匹配结果信息
     */
    @PostMapping("/player/add/")
    public String addPlayer(@RequestParam MultiValueMap<String,String> data){
        Integer userId = Integer.parseInt(Objects.requireNonNull(data.getFirst("user_id")));
        Integer rating = Integer.parseInt(Objects.requireNonNull(data.getFirst("rating")));
        String gameTypeName = Objects.requireNonNull(data.getFirst("game_type"));
        GameTypeEnum gameType = GameTypeEnum.fromChineseName(gameTypeName);
        return matchingService.addPlayer(userId, rating, gameType);
    }

    @PostMapping("/player/remove/")
    public String removePlayer(@RequestParam MultiValueMap<String,String> data){
        Integer userId=Integer.parseInt(Objects.requireNonNull(data.getFirst("user_id")));
        Integer rating=Integer.parseInt(Objects.requireNonNull(data.getFirst("rating")));
        return matchingService.removePlayer(userId,rating);
    }
}
