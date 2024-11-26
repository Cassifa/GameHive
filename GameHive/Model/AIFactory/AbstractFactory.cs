/*************************************************************************************
 * 文 件 名:   AbstractFactory.cs
 * 描    述: 抽象工厂，规定了四种类型的AI
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:05
*************************************************************************************/
using GameHive.Model.AIFactory.AbstractAIProduct;

namespace GameHive.Model.AIFactory {
    internal interface AbstractFactory {
        public DRL GetDRLProduct();
        public MCTS GetMCTSProduct();
        public MinMax GetMinMaxProduct();
        public Negamax GetNegamaxProduct();
    }
}
