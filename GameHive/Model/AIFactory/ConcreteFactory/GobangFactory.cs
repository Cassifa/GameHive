/*************************************************************************************
 * 文 件 名:   GobangFactory.cs
 * 描    述: 五子棋工厂
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 20:35
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Constants.DifficultyLevelEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.AIFactory.ConcreteProduct;
using GameHive.Model.AIFactory.ConcreteProduct.Gobang;
using GameHive.Model.AIUtils.AlphaBetaPruning;
using GameHive.Model.GameInfo;

namespace GameHive.Model.AIFactory {
    internal class GobangFactory : AbstractFactory {

        public override MinMax GetMinMaxProduct(DifficultyLevel level) {
            SetConcreteProductInfo(GoBangMinMax.concreteProductInfo);
            return new GoBangMinMax(boardInfo.Column, level, RewardTableUtil.GetGOBangRewardTable(), RewardTableUtil.GetGOBangKillingTable());
        }

        public override DeepRL GetDeepRLProduct(DifficultyLevel level) {
            SetConcreteProductInfo(GoBangDRL.concreteProductInfo);
            return new GoBangDRL(boardInfo.Column, level);
        }

        public override MCTS GetMCTSProduct(DifficultyLevel level) {
            throw new NotImplementedException();
        }
        public override Negamax GetNegamaxProduct(DifficultyLevel level) {
            throw new NotImplementedException();
        }

        //单例模式
        private static GobangFactory _instance;
        private GobangFactory() {
            List<AIAlgorithmType> aiTypes = new List<AIAlgorithmType> {
                AIAlgorithmType.DeepRL,
                AIAlgorithmType.Minimax
            };
            boardInfo = new GameBoardInfo(15, false, aiTypes);
        }
        // 公共静态属性，提供实例访问
        public static GobangFactory Instance {
            get {
                // 如果实例尚未创建，则创建实例
                if (_instance == null) {
                    // 使用锁确保线程安全
                    lock (typeof(GobangFactory)) {
                        if (_instance == null) {
                            _instance = new GobangFactory();
                        }
                    }
                }
                return _instance;
            }
        }
    }
}
