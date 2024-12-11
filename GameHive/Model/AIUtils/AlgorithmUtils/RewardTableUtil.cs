/*************************************************************************************
 * 文 件 名:   RewardTableUtil.cs
 * 描    述: 
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/12/11 14:58
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;

namespace GameHive.Model.AIUtils.AlgorithmUtils {
    class RewardTableUtil {
        public static Dictionary<List<Role>, int> GetGOBangRewardTable() {
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

            Role charToRole(char c) {
                return c switch {
                    'E' => Role.Empty,
                    'A' => Role.AI,
                    'P' => Role.Player,
                    _ => Role.Empty
                };
            }
            var convertedList = new Dictionary<List<Role>, int>();
            foreach (var entry in rewardTable) {
                var roleList = entry.Key.Select(charToRole).ToList();
                convertedList[roleList] = entry.Value;
            }
            return convertedList;
        }
    }
}
