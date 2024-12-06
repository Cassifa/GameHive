/*************************************************************************************
 * 文 件 名:   ACAutomaton.cs
 * 描    述: AC自动机工具类，通过得分规则构造自动机，并且提供估值方法
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/12/6 15:15
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;

namespace GameHive.Model.AIUtils.AlgorithmUtils {
    internal class ACAutomaton {
        private Dictionary<List<Role>, int> AITable;
        private Dictionary<List<Role>, int> PlayerTable;
        //通过得分表构造AC自动机
        public ACAutomaton(Dictionary<List<Role>, int> RewardTable) {
            AITable = new Dictionary<List<Role>, int>();
            PlayerTable = new Dictionary<List<Role>, int>();

            foreach (var entry in RewardTable) {
                // 原始规则保存到 AITable
                AITable[entry.Key] = entry.Value;

                // 构造 PlayerTable，反转 AI 和 Player
                var reversedKey = entry.Key.Select(r =>
                    r == Role.AI ? Role.Player : r == Role.Player ? Role.AI : r
                ).ToList();
                PlayerTable[reversedKey] = entry.Value;
            }
        }
        //计算一组序列对对于 role 的价值
        public int CalculateLineValue(List<Role> mode, Role role) {
            int score = 0;
            var table = role == Role.AI ? AITable : PlayerTable;
            foreach (var pattern in table) {
                int index = 0, count = 0;
                while (index + pattern.Key.Count <= mode.Count) {
                    bool match = true;
                    for (int i = 0; i < pattern.Key.Count; i++) {
                        if (mode[index + i] != pattern.Key[i]) {
                            match = false;
                            break;
                        }
                    }
                    if (match) {
                        count++;
                        index += pattern.Key.Count; // 移动到下一个可能的起点
                    } else {
                        index++; // 匹配失败，逐字符移动
                    }
                }
                score += count * pattern.Value; // 出现次数 * 价值
            }

            return score;
        }

    }
}
