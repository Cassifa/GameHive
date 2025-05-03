/*************************************************************************************
 * 文 件 名:   TicTacToeFactory.cs
 * 描    述: 井字棋工厂
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 20:35
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.AIFactory.ConcreteProduct;
using GameHive.Model.GameInfo;

namespace GameHive.Model.AIFactory {
    internal class TicTacToeFactory : AbstractFactory {
        public override MCTS GetMCTSProduct() {
            SetConcreteProductInfo(TicTacToeMCTS.concreteProductInfo);
            return new TicTacToeMCTS();
        }

        public override MinMax GetMinMaxProduct() {
            SetConcreteProductInfo(TicTacToeMinMax.concreteProductInfo);
            return new TicTacToeMinMax();
        }

        public override Negamax GetNegamaxProduct() {
            SetConcreteProductInfo(TicTacToeNegamax.concreteProductInfo);
            return new TicTacToeNegamax();
        }

        /*——————————不可用———————————*/
        public override DeepRL GetDeepRLProduct() {
            throw new NotImplementedException();
        }

        //单例模式
        private static TicTacToeFactory _instance;
        private TicTacToeFactory() {
            List<AIAlgorithmType> aiTypes = new List<AIAlgorithmType> {
                AIAlgorithmType.Minimax,
                AIAlgorithmType.MCTS,
                AIAlgorithmType.Negamax,
            };
            boardInfo = new GameBoardInfo(3, true, aiTypes);
        }
        // 公共静态属性，提供实例访问
        public static TicTacToeFactory Instance {
            get {
                // 如果实例尚未创建，则创建实例
                if (_instance == null) {
                    // 使用锁确保线程安全
                    lock (typeof(TicTacToeFactory)) {
                        if (_instance == null) {
                            _instance = new TicTacToeFactory();
                        }
                    }
                }
                return _instance;
            }
        }
    }
}
