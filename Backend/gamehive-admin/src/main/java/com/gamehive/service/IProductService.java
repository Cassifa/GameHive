package com.gamehive.service;

import java.util.List;
import java.util.Map;
import com.gamehive.pojo.Product;

/**
 * Algorithm-Game具体产品Service接口
 * 
 * @author Cassifa
 * @date 2025-05-05
 */
public interface IProductService 
{
    /**
     * 查询Algorithm-Game具体产品
     * 
     * @param id Algorithm-Game具体产品主键
     * @return Algorithm-Game具体产品
     */
    public Product selectProductById(Long id);

    /**
     * 查询Algorithm-Game具体产品列表
     * 
     * @param product Algorithm-Game具体产品
     * @return Algorithm-Game具体产品集合
     */
    public List<Product> selectProductList(Product product);

    /**
     * 新增Algorithm-Game具体产品
     * 
     * @param product Algorithm-Game具体产品
     * @return 结果
     */
    public int insertProduct(Product product);

    /**
     * 修改Algorithm-Game具体产品
     * 
     * @param product Algorithm-Game具体产品
     * @return 结果
     */
    public int updateProduct(Product product);

    /**
     * 批量删除Algorithm-Game具体产品
     * 
     * @param ids 需要删除的Algorithm-Game具体产品主键集合
     * @return 结果
     */
    public int deleteProductByIds(Long[] ids);

    /**
     * 删除Algorithm-Game具体产品信息
     * 
     * @param id Algorithm-Game具体产品主键
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
