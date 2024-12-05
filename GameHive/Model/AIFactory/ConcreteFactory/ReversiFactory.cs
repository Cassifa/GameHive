/*************************************************************************************
 * 文 件 名:   ReversiFactory.cs
 * 描    述: 黑白棋工厂
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 20:36
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.AIFactory.ConcreteProduct;

namespace GameHive.Model.AIFactory {
    internal class ReversiFactory : AbstractFactory {
        public override MCTS GetMCTSProduct() {
            return new ReversiTicTacToeMCTS();
        }

        public override MinMax GetMinMaxProduct() {
            return new ReversiTicTacToeMinMax();
        }

        /*——————————不可用———————————*/

        public override Negamax GetNegamaxProduct() {
            throw new NotImplementedException();
        }
        public override DRL GetDRLProduct() {
            throw new NotImplementedException();
        }

        //单例模式
        private static ReversiFactory _instance;
        private ReversiFactory() {
            List<AIAlgorithmType> aiTypes = new List<AIAlgorithmType> {
                AIAlgorithmType.AlphaBetaPruning,
                AIAlgorithmType.Negamax,
                AIAlgorithmType.MCTS,
            };
            boardInfo = new GameBoardInfo(5, true, aiTypes);
        }
        // 公共静态属性，提供实例访问
        public static ReversiFactory Instance {
            get {
                // 如果实例尚未创建，则创建实例
                if (_instance == null) {
                    // 使用锁确保线程安全
                    lock (typeof(ReversiFactory)) {
                        if (_instance == null) {
                            _instance = new ReversiFactory();
                        }
                    }
                }
                return _instance;
            }
        }
    }
}
