package com.gamehive.controller;

import java.util.List;
import java.util.Map;
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
import com.gamehive.pojo.GameType;
import com.gamehive.service.IGameTypeService;
import com.gamehive.common.utils.poi.ExcelUtil;
import com.gamehive.common.core.page.TableDataInfo;

/**
 * 游戏类型Controller
 *
 * @author Cassifa
 * @date 2025-05-05
 */
@RestController
@RequestMapping("/GameType/GameType")
public class GameTypeController extends BaseController {

    @Autowired
    private IGameTypeService gameTypeService;

    /**
     * 查询游戏类型列表
     */
    @PreAuthorize("@ss.hasPermi('system:type:list')")
    @GetMapping("/list")
    public TableDataInfo list(GameType gameType) {
        startPage();
        List<GameType> list = gameTypeService.selectGameTypeList(gameType);
        return getDataTable(list);
    }

    /**
     * 导出游戏类型列表
     */
    @PreAuthorize("@ss.hasPermi('system:type:export')")
    @Log(title = "游戏类型", businessType = BusinessType.EXPORT)
    @PostMapping("/export")
    public void export(HttpServletResponse response, GameType gameType) {
        List<GameType> list = gameTypeService.selectGameTypeList(gameType);
        ExcelUtil<GameType> util = new ExcelUtil<>(GameType.class);
        util.exportExcel(response, list, "游戏类型数据");
    }

    /**
     * 获取游戏类型详细信息
     */
    @PreAuthorize("@ss.hasPermi('system:type:query')")
    @GetMapping(value = "/{gameId}")
    public AjaxResult getInfo(@PathVariable("gameId") Long gameId) {
        return success(gameTypeService.selectGameTypeByGameId(gameId));
    }

    /**
     * 新增游戏类型
     */
    @PreAuthorize("@ss.hasPermi('system:type:add')")
    @Log(title = "游戏类型", businessType = BusinessType.INSERT)
    @PostMapping
    public AjaxResult add(@RequestBody GameType gameType) {
        return toAjax(gameTypeService.insertGameType(gameType));
    }

    /**
     * 修改游戏类型
     */
    @PreAuthorize("@ss.hasPermi('system:type:edit')")
    @Log(title = "游戏类型", businessType = BusinessType.UPDATE)
    @PutMapping
    public AjaxResult edit(@RequestBody GameType gameType) {
        return toAjax(gameTypeService.updateGameType(gameType));
    }

    /**
     * 删除游戏类型
     */
    @PreAuthorize("@ss.hasPermi('system:type:remove')")
    @Log(title = "游戏类型", businessType = BusinessType.DELETE)
    @DeleteMapping("/{gameIds}")
    public AjaxResult remove(@PathVariable Long[] gameIds) {
        return toAjax(gameTypeService.deleteGameTypeByGameIds(gameIds));
    }

    /**
     * 获取游戏类型下拉框选项
     */
    @GetMapping("/options")
    public AjaxResult options() {
        List<Map<String, Object>> options = gameTypeService.selectGameTypeOptions();
        return success(options);
    }
}
