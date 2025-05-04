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
import org.springframework.web.bind.annotation.RequestParam;
import com.gamehive.common.annotation.Log;
import com.gamehive.common.core.controller.BaseController;
import com.gamehive.common.core.domain.AjaxResult;
import com.gamehive.common.enums.BusinessType;
import com.gamehive.pojo.Product;
import com.gamehive.service.IProductService;
import com.gamehive.common.utils.poi.ExcelUtil;
import com.gamehive.common.core.page.TableDataInfo;
import com.github.pagehelper.PageHelper;

/**
 * AI产品Controller
 *
 * @author Cassifa
 * @date 2025-05-05
 */
@RestController
@RequestMapping("/Product/Product")
public class ProductController extends BaseController {

    @Autowired
    private IProductService productService;

    /**
     * 查询AI产品列表
     * 支持的查询条件:
     * - pageNum: 当前页码
     * - pageSize: 每页记录数（如果为空则返回所有数据）
     * - algorithmTypeId: 算法类型ID
     * - gameTypeId: 游戏类型ID
     * - algorithmTypeName: 算法名称（模糊查询）
     * - gameTypeName: 游戏名称（模糊查询）
     */
    @PreAuthorize("@ss.hasPermi('system:product:list')")
    @GetMapping("/list")
    public TableDataInfo list(
            @RequestParam(value = "pageNum", required = false) Integer pageNum,
            @RequestParam(value = "pageSize", required = false) Integer pageSize,
            Product product) {
        
        // 如果pageSize不为空，使用分页查询
        if (pageSize != null && pageSize > 0) {
            // 使用PageHelper进行分页
            int pageNumValue = (pageNum != null && pageNum > 0) ? pageNum : 1;
            PageHelper.startPage(pageNumValue, pageSize);
        } 
        // 否则不分页，返回所有数据
        
        List<Product> list = productService.selectProductList(product);
        return getDataTable(list);
    }

    /**
     * 导出AI产品列表
     */
    @PreAuthorize("@ss.hasPermi('system:product:export')")
    @Log(title = "AI产品", businessType = BusinessType.EXPORT)
    @PostMapping("/export")
    public void export(HttpServletResponse response, Product product) {
        List<Product> list = productService.selectProductList(product);
        ExcelUtil<Product> util = new ExcelUtil<Product>(Product.class);
        util.exportExcel(response, list, "AI产品数据");
    }

    /**
     * 获取AI产品详细信息
     */
    @PreAuthorize("@ss.hasPermi('system:product:query')")
    @GetMapping(value = "/{id}")
    public AjaxResult getInfo(@PathVariable("id") Long id) {
        return success(productService.selectProductById(id));
    }

    /**
     * 新增AI产品
     */
    @PreAuthorize("@ss.hasPermi('system:product:add')")
    @Log(title = "AI产品", businessType = BusinessType.INSERT)
    @PostMapping
    public AjaxResult add(@RequestBody Product product) {
        return toAjax(productService.insertProduct(product));
    }

    /**
     * 修改AI产品
     */
    @PreAuthorize("@ss.hasPermi('system:product:edit')")
    @Log(title = "AI产品", businessType = BusinessType.UPDATE)
    @PutMapping
    public AjaxResult edit(@RequestBody Product product) {
        return toAjax(productService.updateProduct(product));
    }

    /**
     * 删除AI产品
     */
    @PreAuthorize("@ss.hasPermi('system:product:remove')")
    @Log(title = "AI产品", businessType = BusinessType.DELETE)
    @DeleteMapping("/{ids}")
    public AjaxResult remove(@PathVariable Long[] ids) {
        return toAjax(productService.deleteProductByIds(ids));
    }
    
    /**
     * 根据游戏ID筛选该游戏有的算法
     */
    @GetMapping("/listAlgorithmsByGameId")
    public AjaxResult listAlgorithmsByGameId(@RequestParam Long gameId) {
        List<Map<String, Object>> algorithms = productService.selectAlgorithmsByGameId(gameId);
        return success(algorithms);
    }
}
