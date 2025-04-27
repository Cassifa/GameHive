/*************************************************************************************
 * 文 件 名:   MisereTicTacToeFactory.cs
 * 描    述: 反井字棋工厂
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 20:35
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.AIFactory.ConcreteProduct;

namespace GameHive.Model.AIFactory {
    internal class MisereTicTacToeFactory : AbstractFactory {
        public override MCTS GetMCTSProduct() {
            return new MisereTicTacToeMCTS();
        }

        public override MinMax GetMinMaxProduct() {
            return new MisereTicTacToeMinMax();
        }

        public override Negamax GetNegamaxProduct() {
            return new MisereTicTacToeNegamax();
        }

        /*——————————不可用———————————*/
        public override DeepRL GetDeepRLProduct() {
            throw new NotImplementedException();
        }
        public override HybridMinimaxMCTS GetHybridMinimaxMCTSProduct() {
            throw new NotImplementedException();
        }

        //单例模式
        private static MisereTicTacToeFactory _instance;
        private MisereTicTacToeFactory() {
            List<AIAlgorithmType> aiTypes = new List<AIAlgorithmType> {
                AIAlgorithmType.Minimax,
                AIAlgorithmType.MCTS,
                AIAlgorithmType.Negamax,
            };
            boardInfo = new GameBoardInfo(3, true, aiTypes);
        }
        // 公共静态属性，提供实例访问
        public static MisereTicTacToeFactory Instance {
            get {
                // 如果实例尚未创建，则创建实例
                if (_instance == null) {
                    // 使用锁确保线程安全
                    lock (typeof(MisereTicTacToeFactory)) {
                        if (_instance == null) {
                            _instance = new MisereTicTacToeFactory();
                        }
                    }
                }
                return _instance;
            }
        }
    }
}
