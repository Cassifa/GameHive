/*************************************************************************************
 * 文 件 名:   AbstractFactory.cs
 * 描    述: 抽象工厂，规定了四种类型的AI
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:05
*************************************************************************************/
using GameHive.Constants.DifficultyLevelEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.GameInfo;

namespace GameHive.Model.AIFactory {
    internal abstract class AbstractFactory {
        //当前棋盘信息
        protected GameBoardInfo boardInfo;
        //当前产品信息
        private ConcreteProductInfo currentConcreteProductInfo;
        public abstract MCTS GetMCTSProduct(DifficultyLevel level=DifficultyLevel.LEVEL_1);
        public abstract MinMax GetMinMaxProduct(DifficultyLevel level = DifficultyLevel.LEVEL_1);
        public abstract Negamax GetNegamaxProduct(DifficultyLevel level = DifficultyLevel.LEVEL_1);
        public abstract DeepRL GetDeepRLProduct(DifficultyLevel level = DifficultyLevel.LEVEL_1);
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
