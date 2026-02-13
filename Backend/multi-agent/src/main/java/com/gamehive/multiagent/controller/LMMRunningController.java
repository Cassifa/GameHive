package com.gamehive.multiagent.controller;

import com.gamehive.multiagent.dto.LMMRequestDTO;
import com.gamehive.multiagent.service.LMMRunningService;
import lombok.extern.slf4j.Slf4j;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RestController;

/**
 * LMMRunningController 控制器类
 */
@RestController
@Slf4j
public class LMMRunningController {

    @Autowired
    private LMMRunningService LMMRunningService;  // 注入LMM运行服务

    /**
     * 添加LMM的POST请求处理方法
     *
     * @param requestDTO LMM请求数据传输对象，包含以下参数：
     *                   - gameId: 游戏唯一标识
     *                   - userId: 玩家ID
     *                   - currentMap: 当前棋盘状态，字符串格式表示棋盘各位置状态
     *                   - LLMFlag: 大模型落子标志，标识大模型当前代表哪一玩家(0"0"?1")
     *                   - gameType: 游戏类型，如"五子棋"?围棋"
     *                   - gameRule: 游戏规则，描述游戏的具体规则
     *                   - historySteps: 历史步骤，记录之前的所有落子信息
     *                   - gridSize: 游戏棋盘格数
     * @return String 返回处理结果字符串，通常为操作成功或失败信息
     */
    @PostMapping("/LMMRunning/add/")
    public String addLMM(@RequestBody LMMRequestDTO requestDTO) {
        log.info("LMMRunningController接收大模型运行请求");

        // 调用服务层方法，传入DTO对象
        return LMMRunningService.addLMM(requestDTO);
    }
}
