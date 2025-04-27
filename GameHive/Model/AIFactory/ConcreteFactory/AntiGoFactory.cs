/*************************************************************************************
 * 文 件 名:   ReversiFactory.cs
 * 描    述: 不围棋工厂
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 20:36
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.AIFactory.ConcreteProduct;
using GameHive.Model.AIFactory.ConcreteProduct.AntiGo;

namespace GameHive.Model.AIFactory {
    internal class AntiGoFactory : AbstractFactory {
        public override MCTS GetMCTSProduct() {
            return new AntiGoMCTS();
        }

        public override HybridMinimaxMCTS GetHybridMinimaxMCTSProduct() {
            return new AntiGoMinMaxMCTS();
        }

        /*——————————不可用———————————*/

        public override Negamax GetNegamaxProduct() {
            throw new NotImplementedException();
        }
        public override MinMax GetMinMaxProduct() {
            return new AntiGoMinMax();
        }

        //单例模式
        private static AntiGoFactory _instance;
        private AntiGoFactory() {
            List<AIAlgorithmType> aiTypes = new List<AIAlgorithmType> {
                AIAlgorithmType.MCTS,
                AIAlgorithmType.HybridMinimaxMCTS,
            };
            boardInfo = new GameBoardInfo(7, true, aiTypes);
        }
        // 公共静态属性，提供实例访问
        public static AntiGoFactory Instance {
            get {
                // 如果实例尚未创建，则创建实例
                if (_instance == null) {
                    // 使用锁确保线程安全
                    lock (typeof(AntiGoFactory)) {
                        if (_instance == null) {
                            _instance = new AntiGoFactory();
                        }
                    }
                }
                return _instance;
            }
        }
    }
}
