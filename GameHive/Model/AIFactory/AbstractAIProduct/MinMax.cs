/*************************************************************************************
 * 文 件 名:   MinMax.cs
 * 描    述: 博弈树抽象产品
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:11
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;

namespace GameHive.Model.AIFactory.AbstractAIProduct {
    internal abstract class MinMax : AbstractAIStrategy {
        int maxDeep;
        public abstract bool IsHeWin(Role role);
        public abstract bool IsEnd();

    }
}
