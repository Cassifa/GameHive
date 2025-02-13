package com.ruoyi.ranking.controller;

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
import com.ruoyi.common.annotation.Log;
import com.ruoyi.common.core.controller.BaseController;
import com.ruoyi.common.core.domain.AjaxResult;
import com.ruoyi.common.enums.BusinessType;
import com.ruoyi.ranking.domain.Player;
import com.ruoyi.ranking.service.IPlayerService;
import com.ruoyi.common.utils.poi.ExcelUtil;
import com.ruoyi.common.core.page.TableDataInfo;

/**
 * 天梯排行Controller
 * 
 * @author Cassifa
 * @date 2025-02-13
 */
@RestController
@RequestMapping("/ranking/ranking")
public class PlayerController extends BaseController
{
    @Autowired
    private IPlayerService playerService;

    /**
     * 查询天梯排行列表
     */
    @PreAuthorize("@ss.hasPermi('ranking:ranking:list')")
    @GetMapping("/list")
    public TableDataInfo list(Player player)
    {
        startPage();
        List<Player> list = playerService.selectPlayerList(player);
        return getDataTable(list);
    }

    /**
     * 导出天梯排行列表
     */
    @PreAuthorize("@ss.hasPermi('ranking:ranking:export')")
    @Log(title = "天梯排行", businessType = BusinessType.EXPORT)
    @PostMapping("/export")
    public void export(HttpServletResponse response, Player player)
    {
        List<Player> list = playerService.selectPlayerList(player);
        ExcelUtil<Player> util = new ExcelUtil<Player>(Player.class);
        util.exportExcel(response, list, "天梯排行数据");
    }

    /**
     * 获取天梯排行详细信息
     */
    @PreAuthorize("@ss.hasPermi('ranking:ranking:query')")
    @GetMapping(value = "/{userId}")
    public AjaxResult getInfo(@PathVariable("userId") Long userId)
    {
        return success(playerService.selectPlayerByUserId(userId));
    }

    /**
     * 新增天梯排行
     */
    @PreAuthorize("@ss.hasPermi('ranking:ranking:add')")
    @Log(title = "天梯排行", businessType = BusinessType.INSERT)
    @PostMapping
    public AjaxResult add(@RequestBody Player player)
    {
        return toAjax(playerService.insertPlayer(player));
    }

    /**
     * 修改天梯排行
     */
    @PreAuthorize("@ss.hasPermi('ranking:ranking:edit')")
    @Log(title = "天梯排行", businessType = BusinessType.UPDATE)
    @PutMapping
    public AjaxResult edit(@RequestBody Player player)
    {
        return toAjax(playerService.updatePlayer(player));
    }

    /**
     * 删除天梯排行
     */
    @PreAuthorize("@ss.hasPermi('ranking:ranking:remove')")
    @Log(title = "天梯排行", businessType = BusinessType.DELETE)
	@DeleteMapping("/{userIds}")
    public AjaxResult remove(@PathVariable Long[] userIds)
    {
        return toAjax(playerService.deletePlayerByUserIds(userIds));
    }
}
