package com.gamehive.mapper;

import java.util.List;
import com.gamehive.pojo.Product;
import com.baomidou.mybatisplus.core.mapper.BaseMapper;
import org.apache.ibatis.annotations.Mapper;

/**
 * AI产品Mapper接口
 *
 * @author Cassifa
 * @date 2025-05-05
 */
@Mapper
public interface ProductMapper extends BaseMapper<Product> {

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
     * 删除AI产品
     *
     * @param id AI产品主键
     * @return 结果
     */
    public int deleteProductById(Long id);

    /**
     * 批量删除AI产品
     *
     * @param ids 需要删除的数据主键集合
     * @return 结果
     */
    public int deleteProductByIds(Long[] ids);
}
