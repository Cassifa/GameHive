/*************************************************************************************
 * 文 件 名:   MinMax.cs
 * 描    述: 博弈树抽象产品
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:11
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIUtils.AlgorithmUtils;

namespace GameHive.Model.AIFactory.AbstractAIProduct {
    internal abstract class MinMax : AbstractAIStrategy {
        protected int maxDeep { set; get; }
        //决定Ai的风格偏好,加起来权重为10
        protected int DefendBias = 4, AttackBias = 6;
        private Tuple<int, int> FinalDecide;
        protected ACAutomaton ACautomaton;
        //获取可下棋点位
        protected abstract List<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board);
        protected abstract int EvalNowSituation(List<List<Role>> currentBoard, Role role);
        //初始化AC自动机
        protected abstract void InitACAutomaton();

        //获取下一步移动
        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard) {
            //计算最优值
            EvalToGo(currentBoard, maxDeep, int.MinValue, int.MaxValue);
            return FinalDecide;
        }

        //执行博弈树搜索
        private int EvalToGo(List<List<Role>> currentBoard, int depth, int alpha, int beta) {
            // 检查当前局面的胜负情况
            Role winner = CheckGameOver(currentBoard);
            if (winner == Role.Draw) return 0;
            else if (winner == Role.AI) return int.MaxValue;
            else if (winner == Role.Player) return int.MinValue;
            if (depth == 0) {
                int attackScore = AttackBias * EvalNowSituation(currentBoard, Role.AI);
                int defendScore = DefendBias * EvalNowSituation(currentBoard, Role.Player);
                return attackScore - defendScore;
            }
            bool IsAi = ((depth % 2) == 0);
            int nowScore; Tuple<int, int>? nowDec = null;
            var availableMoves = GetAvailableMoves(currentBoard);
            if (IsAi) {
                nowScore = int.MinValue;
                foreach (var move in availableMoves) {
                    currentBoard[move.Item1][move.Item2] = Role.AI;
                    int nowRoundScore = EvalToGo(currentBoard, depth - 1, alpha, beta);
                    currentBoard[move.Item1][move.Item2] = Role.Empty;
                    if (nowRoundScore > nowScore) {
                        nowScore = nowRoundScore;
                        nowDec = move;
                        alpha = Math.Max(alpha, nowRoundScore);
                    }
                    if (alpha >= beta) break;
                }
            } else {
                nowScore = int.MaxValue;
                foreach (var move in availableMoves) {
                    currentBoard[move.Item1][move.Item2] = Role.Player;
                    int nowRoundScore = EvalToGo(currentBoard, depth - 1, alpha, beta);
                    currentBoard[move.Item1][move.Item2] = Role.Empty;
                    if (nowRoundScore < nowScore) {
                        nowScore = nowRoundScore;
                        nowDec = move;
                        alpha = Math.Min(beta, nowRoundScore);
                    }
                    if (alpha >= beta) break;
                }

            }
            FinalDecide = nowDec;
            return nowScore;
        }



        ////获取在X,Y落子带来的总收益(带风险偏好)
        //protected abstract int evalXYTRoundScore(List<List<Role>> currentBoard, int x,int y, Role role);
        ////计算 role 视角下在X,Y落子获得的攻击分数
        //protected abstract int calculateXY(List<List<Role>> currentBoard, int x,int y, Role role);
    }
}
