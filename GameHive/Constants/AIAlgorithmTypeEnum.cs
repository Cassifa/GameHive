/*************************************************************************************
 *
 * 文 件 名:   AIAlgorithmTypeEnum.cs
 * 描    述: 用于定义人工智能算法种类，并提供中英文名转化
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/25 22:27
*************************************************************************************/

namespace GameHive.Constants.AIAlgorithmTypeEnum {
    public enum AIAlgorithmType {
        AlphaBetaPruning,
        Negamax,
        MCTS,
        MinMaxMCTS,
    }
    public static class AIAlgorithmTypeExtensions {
        public static string GetChineseName(this AIAlgorithmType algorithm) {
            return algorithm switch {
                AIAlgorithmType.AlphaBetaPruning => "α-β剪枝博弈树",
                AIAlgorithmType.Negamax => "负极大值搜索",
                AIAlgorithmType.MCTS => "蒙特卡洛搜索",
                AIAlgorithmType.MinMaxMCTS => "蒙特卡洛博弈树",
                _ => "未知"
            };
        }

        public static string GetEnglishName(this AIAlgorithmType algorithm) {
            return algorithm switch {
                AIAlgorithmType.AlphaBetaPruning => "Alpha-Beta Pruning",
                AIAlgorithmType.Negamax => "Negamax",
                AIAlgorithmType.MCTS => "MonteCarloTreeSearch",
                AIAlgorithmType.MinMaxMCTS => "MinMax-MonteCarloTreeSearch",
                _ => "Unknown"
            };
        }
    }
}
