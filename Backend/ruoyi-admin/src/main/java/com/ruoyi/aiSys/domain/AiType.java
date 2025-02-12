package com.ruoyi.aiSys.domain;

import java.util.List;
import org.apache.commons.lang3.builder.ToStringBuilder;
import org.apache.commons.lang3.builder.ToStringStyle;
import com.ruoyi.common.annotation.Excel;
import com.ruoyi.common.core.domain.BaseEntity;

/**
 * AI类型对象 ai_type
 * 
 * @author Cassifa
 * @date 2025-02-13
 */
public class AiType extends BaseEntity
{
    private static final long serialVersionUID = 1L;

    /** AI编号 */
    @Excel(name = "AI编号")
    private Long aiId;

    /** AI名称 */
    @Excel(name = "AI名称")
    private String aiName;

    /** AI简介 */
    @Excel(name = "AI简介")
    private String aiIntroduction;

    /** AI-Game具体产品信息 */
    private List<AiGame> aiGameList;

    public void setAiId(Long aiId) 
    {
        this.aiId = aiId;
    }

    public Long getAiId() 
    {
        return aiId;
    }
    public void setAiName(String aiName) 
    {
        this.aiName = aiName;
    }

    public String getAiName() 
    {
        return aiName;
    }
    public void setAiIntroduction(String aiIntroduction) 
    {
        this.aiIntroduction = aiIntroduction;
    }

    public String getAiIntroduction() 
    {
        return aiIntroduction;
    }

    public List<AiGame> getAiGameList()
    {
        return aiGameList;
    }

    public void setAiGameList(List<AiGame> aiGameList)
    {
        this.aiGameList = aiGameList;
    }

    @Override
    public String toString() {
        return new ToStringBuilder(this,ToStringStyle.MULTI_LINE_STYLE)
            .append("aiId", getAiId())
            .append("aiName", getAiName())
            .append("aiIntroduction", getAiIntroduction())
            .append("aiGameList", getAiGameList())
            .toString();
    }
}
