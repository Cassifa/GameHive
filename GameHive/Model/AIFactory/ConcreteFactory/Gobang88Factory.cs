/*************************************************************************************
 * 文 件 名:   Gobang88Factory.cs
 * 描    述: 8*8五子棋工厂
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 20:35
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.AIFactory.ConcreteProduct;

namespace GameHive.Model.AIFactory {
    internal class Gobang88Factory : AbstractFactory {

        public override DRL GetDRLProduct() {
            return new GoBang88DRL();
        }

        public override MinMax GetMinMaxProduct() {
            return new GoBang88MinMax(initRewardTable());
        }

        /*——————————不可用———————————*/
        public override Negamax GetNegamaxProduct() {
            throw new NotImplementedException();
        }

        public override MCTS GetMCTSProduct() {
            throw new NotImplementedException();
        }

        private Dictionary<string, int> initRewardTable() {
            var rewardTable = new Dictionary<string, int>();
            // 四
            // 活四 _OOOO_
            rewardTable["EAAAAE"] = 5000;
            // 冲四 O_OOO _OOOOX OO_OO
            rewardTable["A_EAAA"] = 700;
            rewardTable["EAAAAP"] = 1000;
            rewardTable["AAEAA"] = 700;

            // 三
            // 活三（可成活四）_OOO_ O_OO
            rewardTable["EAAAE"] = 800;
            rewardTable["AEAA"] = 150;
            // 眠三 __OOOX _O_OOX _OO_OX O__OO O_O_O X_OOO_X
            rewardTable["EEAAAP"] = 100;
            rewardTable["EA_EAAP"] = 80;
            rewardTable["EAA_EAP"] = 60;
            rewardTable["AEEAA"] = 60;
            rewardTable["AEAEA"] = 60;
            rewardTable["PEAAAEAP"] = 60;

            // 二
            // 活二 __OO__ _O_O_ O__O
            rewardTable["EEAAEE"] = 50;
            rewardTable["EA_EA"] = 20;
            rewardTable["AEEA"] = 20;
            // 眠二 ___OOX __O_OX _O__OX O___O
            rewardTable["EEEAAP"] = 10;
            rewardTable["EEA_EAP"] = 10;
            rewardTable["EAEEAP"] = 10;
            rewardTable["AEEEA"] = 10;

            // 添加翻转情况
            var reversedTable = new Dictionary<string, int>();
            foreach (var entry in rewardTable) {
                var reversed = new string(entry.Key.Reverse().ToArray());
                reversedTable[reversed] = entry.Value;
            }

            foreach (var entry in reversedTable) {
                rewardTable[entry.Key] = entry.Value;
            }

            return rewardTable;
        }


        //单例模式
        private static Gobang88Factory _instance;
        private Gobang88Factory() {
            List<AIAlgorithmType> aiTypes = new List<AIAlgorithmType> {
                AIAlgorithmType.AlphaBetaPruning,
                AIAlgorithmType.DRL
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
