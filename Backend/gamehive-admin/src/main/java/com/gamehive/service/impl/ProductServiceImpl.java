package com.gamehive.service.impl;

import java.util.List;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Service;
import com.gamehive.mapper.ProductMapper;
import com.gamehive.pojo.Product;
import com.gamehive.service.IProductService;

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
}
