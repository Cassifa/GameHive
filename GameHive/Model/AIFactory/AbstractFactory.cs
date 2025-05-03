/*************************************************************************************
 * 文 件 名:   AbstractFactory.cs
 * 描    述: 抽象工厂，规定了四种类型的AI
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:05
*************************************************************************************/
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.GameInfo;

namespace GameHive.Model.AIFactory {
    internal abstract class AbstractFactory {
        protected GameBoardInfo boardInfo;
        private ConcreteProductInfo currentConcreteProductInfo;
        public abstract MCTS GetMCTSProduct();
        public abstract MinMax GetMinMaxProduct();
        public abstract Negamax GetNegamaxProduct();
        public abstract DeepRL GetDeepRLProduct();
        public GameBoardInfo GetBoardInfoProduct() {
            return boardInfo;
        }
        public ConcreteProductInfo GetConcreteProductInfo() {
            return currentConcreteProductInfo;
        }
        protected void SetConcreteProductInfo(ConcreteProductInfo info) {
            currentConcreteProductInfo=info;
        }
    }
}
