package com.gamehive.controller;

import com.gamehive.common.core.controller.BaseController;
import com.gamehive.common.core.domain.AjaxResult;
import com.gamehive.dto.PlayerGameStatisticsDTO;
import com.gamehive.service.IPlayerStatisticsService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;

/**
 * 玩家统计信息Controller
 *
 * @author Cassifa
 */
@RestController
@RequestMapping("/PlayerStatistics/PlayerStatistics")
public class PlayerStatisticsController extends BaseController {

    @Autowired
    private IPlayerStatisticsService playerStatisticsService;

    /**
     * 获取玩家统计信息
     *
     * @param userId 用户ID
     * @return 玩家统计信息
     */
    @GetMapping("/{userId}")
    public AjaxResult getPlayerStatistics(@PathVariable("userId") Long userId) {
        try {
            PlayerGameStatisticsDTO statistics = playerStatisticsService.getPlayerStatistics(userId);
            if (statistics == null) {
                return error("玩家不存在或统计信息获取失败");
            }
            return success(statistics);
        } catch (Exception e) {
            return error("获取统计信息失败：" + e.getMessage());
        }
    }
} 