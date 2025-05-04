package com.gamehive.service;

import java.util.List;
import java.util.Map;
import com.gamehive.pojo.Product;

/**
 * AI产品Service接口
 * 
 * @author Cassifa
 * @date 2025-05-05
 */
public interface IProductService 
{
    /**
     * 查询AI产品
     * 
     * @param id AI产品主键
     * @return AI产品
     */
    public Product selectProductById(Long id);

    /**
     * 查询AI产品列表
     * 
     * @param product AI产品
     * @return AI产品集合
     */
    public List<Product> selectProductList(Product product);

    /**
     * 新增AI产品
     * 
     * @param product AI产品
     * @return 结果
     */
    public int insertProduct(Product product);

    /**
     * 修改AI产品
     * 
     * @param product AI产品
     * @return 结果
     */
    public int updateProduct(Product product);

    /**
     * 批量删除AI产品
     * 
     * @param ids 需要删除的AI产品主键集合
     * @return 结果
     */
    public int deleteProductByIds(Long[] ids);

    /**
     * 删除AI产品信息
     * 
     * @param id AI产品主键
     * @return 结果
     */
    public int deleteProductById(Long id);

    /**
     * 根据游戏ID查询该游戏关联的算法列表
     * 
     * @param gameId 游戏ID
     * @return 算法ID和名称的列表
     */
    public List<Map<String, Object>> selectAlgorithmsByGameId(Long gameId);
}
