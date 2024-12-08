/*************************************************************************************
 * 文 件 名:   ACAutomaton.cs
 * 描    述: AC自动机工具类，通过得分规则构造自动机，并且提供估值方法
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/12/6 15:15
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using static System.Formats.Asn1.AsnWriter;

namespace GameHive.Model.AIUtils.AlgorithmUtils {
    internal class ACAutomaton {
        //得分表
        private Dictionary<List<Role>, int> AIRewardTable = new Dictionary<List<Role>, int>(new ListRoleComparer());
        private Dictionary<List<Role>, int> PlayerRewardTable = new Dictionary<List<Role>, int>(new ListRoleComparer());
        //已记录匹配串
        private Dictionary<List<Role>, int> AIScoreTable = new Dictionary<List<Role>, int>(new ListRoleComparer());
        private Dictionary<List<Role>, int> PlayerScoreTable = new Dictionary<List<Role>, int>(new ListRoleComparer());
        //通过得分表构造AC自动机
        public ACAutomaton(Dictionary<List<Role>, int> RewardTable) {
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
            int score = 0;
            if (role == Role.AI) {
                if (AIScoreTable.TryGetValue(mode, out score))
                    return score;
            } else {
                if (PlayerScoreTable.TryGetValue(mode, out score))
                    return score;
            }
            return ACAutomatonCalculateLineValue(mode, role);
        }


        //AC自动机计算未出现过的序列对于 role 的价值,并返回
        private int ACAutomatonCalculateLineValue(List<Role> mode, Role role) {
            int score = 0;
            var table = role == Role.AI ? AIRewardTable : PlayerRewardTable;
            foreach (var pattern in table) {
                int count = 0;
                var t = pattern.Key;
                if (mode.Count < t.Count) continue;
                for (int i = 0; i <= mode.Count - t.Count; i++) {
                    bool match = true;
                    // 内部循环: 比较 `mode[i + j]` 和 `t[j]`
                    for (int j = 0; j < t.Count; j++) {
                        if (mode[i + j] != t[j]) {
                            match = false;
                            break; // 如果不匹配，直接跳出内层循环
                        }
                    }
                    if (match) {
                        count++;
                        i += t.Count - 1;
                    }
                }
                score += count * pattern.Value; // 出现次数 * 价值
            }
            var scoreTable = role == Role.AI ? AIScoreTable : PlayerScoreTable;
            scoreTable[mode] = score;
            return score;
        }
    }
    public class ListRoleComparer : IEqualityComparer<List<Role>> {
        public bool Equals(List<Role> x, List<Role> y) {
            if (x.Count != y.Count) return false;
            for (int i = 0; i < x.Count; i++) {
                if (x[i] != y[i]) {
                    return false;
                }
            }
            return true;
        }
        public int GetHashCode(List<Role> obj) {
            int hash = 17;
            foreach (var item in obj) {
                hash = hash * 31 + (int)item;
            }
            return hash;
        }
    }

}
