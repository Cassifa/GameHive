package com.gamehive.controller;

import java.util.List;
import javax.servlet.http.HttpServletResponse;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RestController;
import com.gamehive.common.annotation.Log;
import com.gamehive.common.core.controller.BaseController;
import com.gamehive.common.core.domain.AjaxResult;
import com.gamehive.common.enums.BusinessType;
import com.gamehive.pojo.Player;
import com.gamehive.service.IPlayerService;
import com.gamehive.common.utils.poi.ExcelUtil;
import com.gamehive.common.core.page.TableDataInfo;

/**
 * 玩家Controller
 *
 * @author Cassifa
 * @date 2025-05-05
 */
@RestController
@RequestMapping("/Player/Player")
public class PlayerController extends BaseController {

    @Autowired
    private IPlayerService playerService;

    /**
     * 查询玩家列表
     */
    @PreAuthorize("@ss.hasPermi('system:player:list')")
    @GetMapping("/list")
    public TableDataInfo list(Player player) {
        startPage();
        List<Player> list = playerService.selectPlayerList(player);
        return getDataTable(list);
    }

    /**
     * 导出玩家列表
     */
    @PreAuthorize("@ss.hasPermi('system:player:export')")
    @Log(title = "玩家", businessType = BusinessType.EXPORT)
    @PostMapping("/export")
    public void export(HttpServletResponse response, Player player) {
        List<Player> list = playerService.selectPlayerList(player);
        ExcelUtil<Player> util = new ExcelUtil<Player>(Player.class);
        util.exportExcel(response, list, "玩家数据");
    }

    /**
     * 获取玩家详细信息
     */
    @PreAuthorize("@ss.hasPermi('system:player:query')")
    @GetMapping(value = "/{userId}")
    public AjaxResult getInfo(@PathVariable("userId") Long userId) {
        return success(playerService.selectPlayerByUserId(userId));
    }

    /**
     * 新增玩家
     */
    @PreAuthorize("@ss.hasPermi('system:player:add')")
    @Log(title = "玩家", businessType = BusinessType.INSERT)
    @PostMapping
    public AjaxResult add(@RequestBody Player player) {
        return toAjax(playerService.insertPlayer(player));
    }

    /**
     * 修改玩家
     */
    @PreAuthorize("@ss.hasPermi('system:player:edit')")
    @Log(title = "玩家", businessType = BusinessType.UPDATE)
    @PutMapping
    public AjaxResult edit(@RequestBody Player player) {
        return toAjax(playerService.updatePlayer(player));
    }

    /**
     * 删除玩家
     */
    @PreAuthorize("@ss.hasPermi('system:player:remove')")
    @Log(title = "玩家", businessType = BusinessType.DELETE)
    @DeleteMapping("/{userIds}")
    public AjaxResult remove(@PathVariable Long[] userIds) {
        return toAjax(playerService.deletePlayerByUserIds(userIds));
    }
}
