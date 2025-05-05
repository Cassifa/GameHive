package com.gamehive.controller;

import java.util.List;
import java.util.Map;
import javax.servlet.http.HttpServletResponse;
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
import com.gamehive.pojo.AlgorithmType;
import com.gamehive.service.IAlgorithmTypeService;
import com.gamehive.common.utils.poi.ExcelUtil;
import com.gamehive.common.core.page.TableDataInfo;

/**
 * 算法类型Controller
 *
 * @author Cassifa
 * @date 2025-05-05
 */
@RestController
@RequestMapping("/Algorithm/Algorithm")
public class AlgorithmTypeController extends BaseController {

    @Autowired
    private IAlgorithmTypeService algorithmTypeService;

    /**
     * 查询算法类型列表
     */
    @GetMapping("/list")
    public TableDataInfo list(AlgorithmType algorithmType) {
        startPage();
        List<AlgorithmType> list = algorithmTypeService.selectAlgorithmTypeList(algorithmType);
        return getDataTable(list);
    }

    /**
     * 导出算法类型列表
     */
    @Log(title = "算法类型", businessType = BusinessType.EXPORT)
    @PostMapping("/export")
    public void export(HttpServletResponse response, AlgorithmType algorithmType) {
        List<AlgorithmType> list = algorithmTypeService.selectAlgorithmTypeList(algorithmType);
        ExcelUtil<AlgorithmType> util = new ExcelUtil<AlgorithmType>(AlgorithmType.class);
        util.exportExcel(response, list, "算法类型数据");
    }

    /**
     * 获取算法类型详细信息
     */
    @GetMapping(value = "/{algorithmId}")
    public AjaxResult getInfo(@PathVariable("algorithmId") Long algorithmId) {
        return success(algorithmTypeService.selectAlgorithmTypeByAlgorithmId(algorithmId));
    }

    /**
     * 新增算法类型
     */
    @Log(title = "算法类型", businessType = BusinessType.INSERT)
    @PostMapping
    public AjaxResult add(@RequestBody AlgorithmType algorithmType) {
        return toAjax(algorithmTypeService.insertAlgorithmType(algorithmType));
    }

    /**
     * 修改算法类型
     */
    @Log(title = "算法类型", businessType = BusinessType.UPDATE)
    @PutMapping
    public AjaxResult edit(@RequestBody AlgorithmType algorithmType) {
        return toAjax(algorithmTypeService.updateAlgorithmType(algorithmType));
    }

    /**
     * 删除算法类型
     */
    @Log(title = "算法类型", businessType = BusinessType.DELETE)
    @DeleteMapping("/{algorithmIds}")
    public AjaxResult remove(@PathVariable Long[] algorithmIds) {
        return toAjax(algorithmTypeService.deleteAlgorithmTypeByAlgorithmIds(algorithmIds));
    }

    /**
     * 获取算法类型下拉框选项
     */
    @GetMapping("/options")
    public AjaxResult options() {
        List<Map<String, Object>> options = algorithmTypeService.selectAlgorithmTypeOptions();
        return success(options);
    }
}
