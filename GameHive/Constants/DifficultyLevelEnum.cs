/*************************************************************************************
 * 文 件 名:   DifficultyLevelEnum.cs
 * 描    述:   游戏难度等级
 * 版    本：  V3.0 引入DRL、MinMax-MCTS混合算法
 * 创 建 者：  Cassifa
 * 创建时间：  2025/5/3 15:19
*************************************************************************************/
namespace GameHive.Constants.DifficultyLevelEnum {
    public enum DifficultyLevel {
        LEVEL_1 = 1,
        LEVEL_2 = 2,
        LEVEL_3 = 3,
        LEVEL_4 = 4,
        LEVEL_5 = 5
    }

    public static class DifficultyLevelExtensions {
        // 获取中文难度名称
        public static string GetChineseName(this DifficultyLevel level) => level switch {
            DifficultyLevel.LEVEL_1 => "一级",
            DifficultyLevel.LEVEL_2 => "二级",
            DifficultyLevel.LEVEL_3 => "三级",
            DifficultyLevel.LEVEL_4 => "四级",
            DifficultyLevel.LEVEL_5 => "五级",
            _ => "未知"
        };

        // 获取英文难度名称
        public static string GetEnglishName(this DifficultyLevel level) => level switch {
            DifficultyLevel.LEVEL_1 => "Level 1",
            DifficultyLevel.LEVEL_2 => "Level 2",
            DifficultyLevel.LEVEL_3 => "Level 3",
            DifficultyLevel.LEVEL_4 => "Level 4",
            DifficultyLevel.LEVEL_5 => "Level 5",
            _ => "Unknown"
        };

        public static List<DifficultyLevel> GetLevelRange(int maxLevel) {
            if (maxLevel < 1 || maxLevel > 5)
                throw new ArgumentOutOfRangeException("难度等级必须是1-5");

            return Enumerable.Range(1, maxLevel)
                .Select(x => (DifficultyLevel)x)
                .ToList();
        }
        public static bool IsGreaterThan(this DifficultyLevel current, DifficultyLevel other)
            => (int)current > (int)other;

        public static bool IsLessThan(this DifficultyLevel current, DifficultyLevel other)
            => (int)current < (int)other;

        public static bool IsEqualTo(this DifficultyLevel current, DifficultyLevel other)
            => current == other;
    }
}
