/*************************************************************************************
 * 文 件 名:   Gobang88Factory.cs
 * 描    述: 8*8五子棋工厂
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 20:35
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.AIFactory.ConcreteProduct;
using GameHive.Model.AIUtils.AlphaBetaPruning;

namespace GameHive.Model.AIFactory {
    internal class Gobang88Factory : AbstractFactory {

        public override MinMax GetMinMaxProduct() {
            return new GoBang88MinMax(RewardTableUtil.GetGOBangRewardTable(), RewardTableUtil.GetGOBangKillingTable());
        }
        public override DeepRL GetDeepRLProduct() {
            throw new NotImplementedException();
        }

        /*——————————不可用———————————*/
        public override Negamax GetNegamaxProduct() {
            throw new NotImplementedException();
        }

        public override MCTS GetMCTSProduct() {
            throw new NotImplementedException();
        }

        //暂不实现
        public override HybridMinimaxMCTS GetHybridMinimaxMCTSProduct() {
            throw new NotImplementedException();
        }

        //单例模式
        private static Gobang88Factory _instance;
        private Gobang88Factory() {
            List<AIAlgorithmType> aiTypes = new List<AIAlgorithmType> {
                AIAlgorithmType.Minimax,
                AIAlgorithmType.DeepRL,
            };
            boardInfo = new GameBoardInfo(8, false, aiTypes);
        }
        // 公共静态属性，提供实例访问
        public static Gobang88Factory Instance {
            get {
                // 如果实例尚未创建，则创建实例
                if (_instance == null) {
                    // 使用锁确保线程安全
                    lock (typeof(Gobang88Factory)) {
                        if (_instance == null) {
                            _instance = new Gobang88Factory();
                        }
                    }
                }
                return _instance;
            }
        }
    }
}
