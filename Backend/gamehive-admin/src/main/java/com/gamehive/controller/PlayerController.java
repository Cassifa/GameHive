package com.gamehive.controller;

import com.gamehive.common.annotation.Log;
import com.gamehive.common.core.controller.BaseController;
import com.gamehive.common.core.domain.AjaxResult;
import com.gamehive.common.core.page.TableDataInfo;
import com.gamehive.common.enums.BusinessType;
import com.gamehive.common.utils.poi.ExcelUtil;
import com.gamehive.pojo.Player;
import com.gamehive.service.IPlayerService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.*;

import javax.servlet.http.HttpServletResponse;
import java.util.List;

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
    @GetMapping("/list")
    public TableDataInfo list(Player player,
                              @RequestParam(value = "pageNum", required = false) Integer pageNum,
                              @RequestParam(value = "pageSize", required = false) Integer pageSize) {
        // 如果传递了分页参数，则进行分页查询
        if (pageNum != null && pageSize != null && pageSize > 0) {
            startPage();
        }
        List<Player> list = playerService.selectPlayerList(player);
        return getDataTable(list);
    }

    /**
     * 导出玩家列表
     */
    @Log(title = "玩家", businessType = BusinessType.EXPORT)
    @PostMapping("/export")
    public void export(HttpServletResponse response, Player player) {
        List<Player> list = playerService.selectPlayerList(player);
        ExcelUtil<Player> util = new ExcelUtil<Player>(Player.class);
        util.exportExcel(response, list, "排行榜");
    }

    /**
     * 获取玩家详细信息
     */
    @GetMapping(value = "/{userId}")
    public AjaxResult getInfo(@PathVariable("userId") Long userId) {
        return success(playerService.selectPlayerByUserId(userId));
    }

    /**
     * 新增玩家
     */
    @Log(title = "玩家", businessType = BusinessType.INSERT)
    @PostMapping
    public AjaxResult add(@RequestBody Player player) {
        return toAjax(playerService.insertPlayer(player));
    }

    /**
     * 修改玩家
     */
    @Log(title = "玩家", businessType = BusinessType.UPDATE)
    @PutMapping
    public AjaxResult edit(@RequestBody Player player) {
        return toAjax(playerService.updatePlayer(player));
    }

    /**
     * 删除玩家
     */
    @Log(title = "玩家", businessType = BusinessType.DELETE)
    @DeleteMapping("/{userIds}")
    public AjaxResult remove(@PathVariable Long[] userIds) {
        return toAjax(playerService.deletePlayerByUserIds(userIds));
    }
}
