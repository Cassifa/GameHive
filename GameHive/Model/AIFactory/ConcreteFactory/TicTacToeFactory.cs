/*************************************************************************************
 * 文 件 名:   TicTacToeFactory.cs
 * 描    述: 井字棋工厂
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 20:35
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Constants.DifficultyLevelEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.AIFactory.ConcreteProduct;
using GameHive.Model.GameInfo;

namespace GameHive.Model.AIFactory {
    internal class TicTacToeFactory : AbstractFactory {
        public override MCTS GetMCTSProduct(DifficultyLevel level) {
            SetConcreteProductInfo(TicTacToeMCTS.concreteProductInfo);
            return new TicTacToeMCTS(boardInfo.Column, level);
        }

        public override MinMax GetMinMaxProduct(DifficultyLevel level) {
            SetConcreteProductInfo(TicTacToeMinMax.concreteProductInfo);
            return new TicTacToeMinMax(boardInfo.Column, level);
        }

        public override Negamax GetNegamaxProduct(DifficultyLevel level) {
            SetConcreteProductInfo(TicTacToeNegamax.concreteProductInfo);
            return new TicTacToeNegamax(boardInfo.Column, level);
        }

        public override DeepRL GetDeepRLProduct(DifficultyLevel level) {
            SetConcreteProductInfo(TicTacToeDRL.concreteProductInfo);
            return new TicTacToeDRL(boardInfo.Column, level);
        }

        //单例模式
        private static TicTacToeFactory _instance;
        private TicTacToeFactory() {
            List<AIAlgorithmType> aiTypes = new List<AIAlgorithmType> {
                AIAlgorithmType.DeepRL,
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
