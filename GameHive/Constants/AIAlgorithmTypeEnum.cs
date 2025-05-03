/*************************************************************************************
 *
 * 文 件 名:   AIAlgorithmTypeEnum.cs
 * 描    述: 用于定义人工智能算法种类，并提供中英文名转化
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/25 22:27
*************************************************************************************/

namespace GameHive.Constants.AIAlgorithmTypeEnum {
    public enum AIAlgorithmType {
        Minimax,            // Minimax-αβ剪枝
        MCTS,               // 蒙特卡洛树搜索
        Negamax,            // 负极大值算法
        DeepRL,             // 深度强化学习
    }

    public static class AIAlgorithmTypeExtensions {
        public static string GetChineseName(this AIAlgorithmType algorithm) => algorithm switch {
            AIAlgorithmType.Minimax => "Minimax-αβ剪枝",
            AIAlgorithmType.MCTS => "蒙特卡洛树搜索",
            AIAlgorithmType.Negamax => "负极大值搜索",
            AIAlgorithmType.DeepRL => "深度强化学习",
            _ => "未知算法"
        };

        public static string GetEnglishName(this AIAlgorithmType algorithm) => algorithm switch {
            AIAlgorithmType.Minimax => "Minimax with Alpha-Beta Pruning",
            AIAlgorithmType.MCTS => "Monte Carlo Tree Search",
            AIAlgorithmType.Negamax => "Negamax Algorithm",
            AIAlgorithmType.DeepRL => "Deep Reinforcement Learning",
            _ => "Unknown Algorithm"
        };
    }
}
