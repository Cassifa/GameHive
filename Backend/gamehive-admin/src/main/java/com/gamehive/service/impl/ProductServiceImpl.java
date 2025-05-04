package com.gamehive.service.impl;

import java.util.List;
import java.util.Map;
import java.util.ArrayList;
import java.util.HashMap;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import com.gamehive.mapper.ProductMapper;
import com.gamehive.pojo.Product;
import com.gamehive.service.IProductService;
import com.baomidou.mybatisplus.core.conditions.query.LambdaQueryWrapper;

/**
 * Algorithm-Game具体产品Service业务层处理
 * 
 * @author Cassifa
 * @date 2025-05-05
 */
@Service
public class ProductServiceImpl implements IProductService 
{
    @Autowired
    private ProductMapper productMapper;

    /**
     * 查询Algorithm-Game具体产品
     * 
     * @param id Algorithm-Game具体产品主键
     * @return Algorithm-Game具体产品
     */
    @Override
    public Product selectProductById(Long id)
    {
        return productMapper.selectProductById(id);
    }

    /**
     * 查询Algorithm-Game具体产品列表
     * 
     * @param product Algorithm-Game具体产品
     * @return Algorithm-Game具体产品
     */
    @Override
    public List<Product> selectProductList(Product product)
    {
        return productMapper.selectProductList(product);
    }

    /**
     * 新增Algorithm-Game具体产品
     * 
     * @param product Algorithm-Game具体产品
     * @return 结果
     */
    @Override
    public int insertProduct(Product product)
    {
        return productMapper.insertProduct(product);
    }

    /**
     * 修改Algorithm-Game具体产品
     * 
     * @param product Algorithm-Game具体产品
     * @return 结果
     */
    @Override
    public int updateProduct(Product product)
    {
        return productMapper.updateProduct(product);
    }

    /**
     * 批量删除Algorithm-Game具体产品
     * 
     * @param ids 需要删除的Algorithm-Game具体产品主键
     * @return 结果
     */
    @Override
    public int deleteProductByIds(Long[] ids)
    {
        return productMapper.deleteProductByIds(ids);
    }

    /**
     * 删除Algorithm-Game具体产品信息
     * 
     * @param id Algorithm-Game具体产品主键
     * @return 结果
     */
    @Override
    public int deleteProductById(Long id)
    {
        return productMapper.deleteProductById(id);
    }

    /**
     * 根据游戏ID查询该游戏关联的算法列表
     * 
     * @param gameId 游戏ID
     * @return 算法ID和名称的列表
     */
    @Override
    public List<Map<String, Object>> selectAlgorithmsByGameId(Long gameId) {
        // 使用LambdaQueryWrapper查询
        LambdaQueryWrapper<Product> queryWrapper = new LambdaQueryWrapper<>();
        queryWrapper.eq(Product::getGameTypeId, gameId);
        queryWrapper.select(Product::getAlgorithmTypeId, Product::getAlgorithmTypeName);
        
        // 使用BaseMapper查询
        List<Product> productList = productMapper.selectList(queryWrapper);
        
        // 整理结果
        List<Map<String, Object>> resultList = new ArrayList<>();
        for (Product product : productList) {
            Map<String, Object> algorithmInfo = new HashMap<>();
            algorithmInfo.put("algorithmId", product.getAlgorithmTypeId());
            algorithmInfo.put("algorithmName", product.getAlgorithmTypeName());
            resultList.add(algorithmInfo);
        }
        
        return resultList;
    }
}
