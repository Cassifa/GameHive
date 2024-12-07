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
        //得分表
        private Dictionary<List<Role>, int> AIRewardTable;
        private Dictionary<List<Role>, int> PlayerRewardTable;
        //已记录匹配串
        private Dictionary<List<Role>, int> AIScoreTable;
        private Dictionary<List<Role>, int> PlayerScoreTable;
        //通过得分表构造AC自动机
        public ACAutomaton(Dictionary<List<Role>, int> RewardTable) {
            //初始化得分表
            AIRewardTable = new Dictionary<List<Role>, int>();
            PlayerRewardTable = new Dictionary<List<Role>, int>();
            //初始化已记录得分表
            AIScoreTable = new Dictionary<List<Role>, int>();
            PlayerScoreTable = new Dictionary<List<Role>, int>();

            foreach (var entry in RewardTable) {
                // 原始规则保存到 AITable
                AIRewardTable[entry.Key] = entry.Value;
                // 构造 PlayerTable，反转 AI 和 Player
                var reversedKey = entry.Key.Select(r =>
                    r == Role.AI ? Role.Player : r == Role.Player ? Role.AI : r
                ).ToList();
                PlayerRewardTable[reversedKey] = entry.Value;
            }
        }
        //计算一组序列对对于 role 的价值
        public int CalculateLineValue(List<Role> mode, Role role) {
            if (role == Role.AI) {
                if (AIScoreTable.ContainsKey(mode))
                    return AIScoreTable[mode];
            } else {
                if (PlayerScoreTable.ContainsKey(mode))
                    return PlayerScoreTable[mode];
            }
            return ACAutomatonCalculateLineValue(mode, role);
        }


        //AC自动机计算未出现过的序列对于 role 的价值,并返回
        private int ACAutomatonCalculateLineValue(List<Role> mode, Role role) {
            int score = 0;
            var table = role == Role.AI ? AIRewardTable : PlayerRewardTable;
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
                        index += pattern.Key.Count;
                    } else {
                        index++;
                    }
                }
                score += count * pattern.Value; // 出现次数 * 价值
            }
            var scoreTable = role == Role.AI ? AIScoreTable : PlayerScoreTable;
            scoreTable[mode] = score;
            return score;
        }
    }
}
