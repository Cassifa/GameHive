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

/**
 * Algorithm-Game具体产品Controller
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
     * 查询Algorithm-Game具体产品列表
     */
    @PreAuthorize("@ss.hasPermi('system:product:list')")
    @GetMapping("/list")
    public TableDataInfo list(Product product) {
        startPage();
        List<Product> list = productService.selectProductList(product);
        return getDataTable(list);
    }

    /**
     * 导出Algorithm-Game具体产品列表
     */
    @PreAuthorize("@ss.hasPermi('system:product:export')")
    @Log(title = "Algorithm-Game具体产品", businessType = BusinessType.EXPORT)
    @PostMapping("/export")
    public void export(HttpServletResponse response, Product product) {
        List<Product> list = productService.selectProductList(product);
        ExcelUtil<Product> util = new ExcelUtil<Product>(Product.class);
        util.exportExcel(response, list, "Algorithm-Game具体产品数据");
    }

    /**
     * 获取Algorithm-Game具体产品详细信息
     */
    @PreAuthorize("@ss.hasPermi('system:product:query')")
    @GetMapping(value = "/{id}")
    public AjaxResult getInfo(@PathVariable("id") Long id) {
        return success(productService.selectProductById(id));
    }

    /**
     * 新增Algorithm-Game具体产品
     */
    @PreAuthorize("@ss.hasPermi('system:product:add')")
    @Log(title = "Algorithm-Game具体产品", businessType = BusinessType.INSERT)
    @PostMapping
    public AjaxResult add(@RequestBody Product product) {
        return toAjax(productService.insertProduct(product));
    }

    /**
     * 修改Algorithm-Game具体产品
     */
    @PreAuthorize("@ss.hasPermi('system:product:edit')")
    @Log(title = "Algorithm-Game具体产品", businessType = BusinessType.UPDATE)
    @PutMapping
    public AjaxResult edit(@RequestBody Product product) {
        return toAjax(productService.updateProduct(product));
    }

    /**
     * 删除Algorithm-Game具体产品
     */
    @PreAuthorize("@ss.hasPermi('system:product:remove')")
    @Log(title = "Algorithm-Game具体产品", businessType = BusinessType.DELETE)
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
