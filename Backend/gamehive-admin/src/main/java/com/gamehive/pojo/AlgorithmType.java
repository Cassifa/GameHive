package com.gamehive.pojo;

import com.gamehive.common.annotation.Excel;
import com.gamehive.common.core.domain.BaseEntity;
import org.apache.commons.lang3.builder.ToStringBuilder;
import org.apache.commons.lang3.builder.ToStringStyle;

/**
 * 算法类型对象 algorithm_type
 *
 * @author Cassifa
 * @date 2025-05-05
 */
public class AlgorithmType extends BaseEntity {
    private static final long serialVersionUID = 1L;

    /**
     * 算法类型ID
     */
    private Long algorithmId;

    /**
     * 算法名称
     */
    @Excel(name = "算法名称")
    private String algorithmName;

    /**
     * 算法思想简介
     */
    @Excel(name = "算法思想简介")
    private String algorithmIntroduction;

    public void setAlgorithmId(Long algorithmId) {
        this.algorithmId = algorithmId;
    }

    public Long getAlgorithmId() {
        return algorithmId;
    }

    public void setAlgorithmName(String algorithmName) {
        this.algorithmName = algorithmName;
    }

    public String getAlgorithmName() {
        return algorithmName;
    }

    public void setAlgorithmIntroduction(String algorithmIntroduction) {
        this.algorithmIntroduction = algorithmIntroduction;
    }

    public String getAlgorithmIntroduction() {
        return algorithmIntroduction;
    }

    @Override
    public String toString() {
        return new ToStringBuilder(this, ToStringStyle.MULTI_LINE_STYLE)
                .append("algorithmId", getAlgorithmId())
                .append("algorithmName", getAlgorithmName())
                .append("algorithmIntroduction", getAlgorithmIntroduction())
                .toString();
    }
}
