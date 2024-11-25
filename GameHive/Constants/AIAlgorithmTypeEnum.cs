namespace GameHive.Constants.AIAlgorithmTypeEnum {
    public enum AIAlgorithmType {
        AlphaBetaPruning,
        Negamax,
        MCTS,
        DRL
    }
    public static class AIAlgorithmTypeExtensions {
        public static string GetChineseName(this AIAlgorithmType algorithm) {
            return algorithm switch {
                AIAlgorithmType.AlphaBetaPruning => "α-β剪枝博弈树",
                AIAlgorithmType.Negamax => "负极大值搜索",
                AIAlgorithmType.MCTS => "蒙特卡洛搜索",
                AIAlgorithmType.DRL => "深度强化学习",
                _ => "未知"
            };
        }

        public static string GetEnglishName(this AIAlgorithmType algorithm) {
            return algorithm switch {
                AIAlgorithmType.AlphaBetaPruning => "Alpha-Beta Pruning",
                AIAlgorithmType.Negamax => "Negamax",
                AIAlgorithmType.MCTS => "MonteCarloTreeSearch",
                AIAlgorithmType.DRL => "DeepReinforcementLearning",
                _ => "Unknown"
            };
        }
    }
}
