package com.ruoyi.aiSys.controller;

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
import com.ruoyi.aiSys.domain.AiType;
import com.ruoyi.aiSys.service.IAiTypeService;
import com.ruoyi.common.utils.poi.ExcelUtil;
import com.ruoyi.common.core.page.TableDataInfo;

/**
 * AI类型Controller
 *
 * @author Cassifa
 * @date 2025-02-13
 */
@RestController
@RequestMapping("/aiSys/aiType")
public class AiTypeController extends BaseController {
    @Autowired
    private IAiTypeService aiTypeService;

    /**
     * 查询AI类型列表
     */
    @PreAuthorize("@ss.hasPermi('aiSys:aiType:list')")
    @GetMapping("/list")
    public TableDataInfo list(AiType aiType) {
        startPage();
        List<AiType> list = aiTypeService.selectAiTypeList(aiType);
        return getDataTable(list);
    }

    /**
     * 导出AI类型列表
     */
    @PreAuthorize("@ss.hasPermi('aiSys:aiType:export')")
    @Log(title = "AI类型", businessType = BusinessType.EXPORT)
    @PostMapping("/export")
    public void export(HttpServletResponse response, AiType aiType) {
        List<AiType> list = aiTypeService.selectAiTypeList(aiType);
        ExcelUtil<AiType> util = new ExcelUtil<AiType>(AiType.class);
        util.exportExcel(response, list, "AI类型数据");
    }

    /**
     * 获取AI类型详细信息
     */
    @PreAuthorize("@ss.hasPermi('aiSys:aiType:query')")
    @GetMapping(value = "/{aiId}")
    public AjaxResult getInfo(@PathVariable("aiId") Long aiId) {
        return success(aiTypeService.selectAiTypeByAiId(aiId));
    }

    /**
     * 新增AI类型
     */
    @PreAuthorize("@ss.hasPermi('aiSys:aiType:add')")
    @Log(title = "AI类型", businessType = BusinessType.INSERT)
    @PostMapping
    public AjaxResult add(@RequestBody AiType aiType) {
        return toAjax(aiTypeService.insertAiType(aiType));
    }

    /**
     * 修改AI类型
     */
    @PreAuthorize("@ss.hasPermi('aiSys:aiType:edit')")
    @Log(title = "AI类型", businessType = BusinessType.UPDATE)
    @PutMapping
    public AjaxResult edit(@RequestBody AiType aiType) {
        return toAjax(aiTypeService.updateAiType(aiType));
    }

    /**
     * 删除AI类型
     */
    @PreAuthorize("@ss.hasPermi('aiSys:aiType:remove')")
    @Log(title = "AI类型", businessType = BusinessType.DELETE)
    @DeleteMapping("/{aiIds}")
    public AjaxResult remove(@PathVariable Long[] aiIds) {
        return toAjax(aiTypeService.deleteAiTypeByAiIds(aiIds));
    }
}
