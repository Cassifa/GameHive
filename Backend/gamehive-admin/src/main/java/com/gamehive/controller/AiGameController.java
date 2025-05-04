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
import com.gamehive.pojo.AiGame;
import com.gamehive.service.IAiGameService;
import com.gamehive.common.utils.poi.ExcelUtil;
import com.gamehive.common.core.page.TableDataInfo;

/**
 * AI-Game具体产品Controller
 * 
 * @author Cassifa
 * @date 2025-02-13
 */
@RestController
@RequestMapping("/aiSys/aiProduct")
public class AiGameController extends BaseController
{
    @Autowired
    private IAiGameService aiGameService;

    /**
     * 查询AI-Game具体产品列表
     */
    @PreAuthorize("@ss.hasPermi('aiSys:aiProduct:list')")
    @GetMapping("/list")
    public TableDataInfo list(AiGame aiGame)
    {
        startPage();
        List<AiGame> list = aiGameService.selectAiGameList(aiGame);
        return getDataTable(list);
    }

    /**
     * 导出AI-Game具体产品列表
     */
    @PreAuthorize("@ss.hasPermi('aiSys:aiProduct:export')")
    @Log(title = "AI-Game具体产品", businessType = BusinessType.EXPORT)
    @PostMapping("/export")
    public void export(HttpServletResponse response, AiGame aiGame)
    {
        List<AiGame> list = aiGameService.selectAiGameList(aiGame);
        ExcelUtil<AiGame> util = new ExcelUtil<AiGame>(AiGame.class);
        util.exportExcel(response, list, "AI-Game具体产品数据");
    }

    /**
     * 获取AI-Game具体产品详细信息
     */
    @PreAuthorize("@ss.hasPermi('aiSys:aiProduct:query')")
    @GetMapping(value = "/{id}")
    public AjaxResult getInfo(@PathVariable("id") Long id)
    {
        return success(aiGameService.selectAiGameById(id));
    }

    /**
     * 新增AI-Game具体产品
     */
    @PreAuthorize("@ss.hasPermi('aiSys:aiProduct:add')")
    @Log(title = "AI-Game具体产品", businessType = BusinessType.INSERT)
    @PostMapping
    public AjaxResult add(@RequestBody AiGame aiGame)
    {
        return toAjax(aiGameService.insertAiGame(aiGame));
    }

    /**
     * 修改AI-Game具体产品
     */
    @PreAuthorize("@ss.hasPermi('aiSys:aiProduct:edit')")
    @Log(title = "AI-Game具体产品", businessType = BusinessType.UPDATE)
    @PutMapping
    public AjaxResult edit(@RequestBody AiGame aiGame)
    {
        return toAjax(aiGameService.updateAiGame(aiGame));
    }

    /**
     * 删除AI-Game具体产品
     */
    @PreAuthorize("@ss.hasPermi('aiSys:aiProduct:remove')")
    @Log(title = "AI-Game具体产品", businessType = BusinessType.DELETE)
	@DeleteMapping("/{ids}")
    public AjaxResult remove(@PathVariable Long[] ids)
    {
        return toAjax(aiGameService.deleteAiGameByIds(ids));
    }
}
