package com.gamehive.lmmrunningsystem.controller;

import com.gamehive.lmmrunningsystem.service.LMMRunningService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.util.MultiValueMap;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import java.util.Objects;

/**
 * LMMRunningController 控制器类
 */
@RestController
public class LMMRunningController {

    @Autowired
    private LMMRunningService LMMRunningService;  // 注入LMM运行服务

    /**
     * 添加LMM的POST请求处理方法
     *
     * @param data 包含请求参数的MultiValueMap，需要包含以下参数：
     *         - userId: 玩家ID
     *         - currentMap: 当前棋盘状态，字符串格式表示棋盘各位置状态
     *         - LLMFlag: 大模型落子标志，标识大模型当前代表哪一方(如"0"或"1")
     *         - gameType: 游戏类型，如"五子棋"、"围棋"等
     *         - gameRule: 游戏规则，描述游戏的具体规则
     *         - historySteps: 历史步骤，记录之前的所有落子信息
     *         - gridSize: 游戏棋盘格数
     * @return String 返回处理结果字符串，通常为操作成功或失败信息
     */
    @PostMapping("/bot/add/")
    public String addLMM(@RequestParam MultiValueMap<String, String> data) {
        System.out.println("接受的大模型运行请求");
        // 解析新增参数
        Long userId = Long.parseLong(Objects.requireNonNull(data.getFirst("userId")));      // 玩家ID
        String currentMap = data.getFirst("currentMap");  // 当前棋盘状态
        String LLMFlag = data.getFirst("LLMFlag");  // 大模型落子用什么代表
        String gameType = data.getFirst("gameType");     // 游戏类型
        String gameRule = data.getFirst("gameRule");     // 游戏规则
        String historySteps = data.getFirst("historySteps"); // 历史步骤
        String gridSize = data.getFirst("gridSize");     // 游戏棋盘格数

        // 调用服务层方法，传入所有参数
        return LMMRunningService.addLMM(
                userId,
                currentMap,
                LLMFlag,
                gameType,
                gameRule,
                historySteps,
                gridSize
        );
    }
}
