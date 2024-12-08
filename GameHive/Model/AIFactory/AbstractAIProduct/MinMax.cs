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
        protected abstract HashSet<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board);
        protected abstract int EvalNowSituation(List<List<Role>> currentBoard, Role role);
        //在算法备份的棋盘上落子
        protected abstract void PlayChess(int x, int y, Role role);
        //获取当前棋盘
        protected abstract List<List<Role>> GetCurrentBoard();
        //使用历史可用与最新落子获取最新可用
        protected abstract HashSet<Tuple<int, int>> GetAvailableMovesByNewPieces(List<List<Role>> currentBoard,
                    HashSet<Tuple<int, int>> lastAvailableMoves, int lastX, int lastY);

        //获取下一步移动
        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            //收到玩家移动，更新棋盘
            PlayChess(lastX, lastY, Role.Player);
            HashSet<Tuple<int, int>> lastAvailableMoves = GetAvailableMoves(currentBoard);
            //计算最优值
            EvalToGo(0, int.MinValue, int.MaxValue, lastAvailableMoves, lastX, lastY);
            PlayChess(FinalDecide.Item1, FinalDecide.Item2, Role.AI);
            //计算出AI移动，跟新棋盘
            return FinalDecide;
        }

        //执行博弈树搜索
        private int EvalToGo(int depth, int alpha, int beta,
                HashSet<Tuple<int, int>> lastAvailableMoves, int lastX, int lastY) {
            // 检查当前局面的胜负情况
            Role winner = CheckGameOver(GetCurrentBoard());
            if (winner == Role.Draw) return 0;
            else if (winner == Role.AI) return 1_000_000;
            else if (winner == Role.Player) return -1_000_000;
            if (depth == maxDeep) {
                int attackScore = AttackBias * EvalNowSituation(GetCurrentBoard(), Role.AI);
                int defendScore = DefendBias * EvalNowSituation(GetCurrentBoard(), Role.Player);
                return attackScore - defendScore;
            }
            bool IsAi = ((depth % 2) == 0);
            int nowScore; Tuple<int, int>? nowDec = null;
            //根据上一步操作获取下一步可行点位
            var availableMoves = GetAvailableMovesByNewPieces(GetCurrentBoard(), lastAvailableMoves, lastX, lastY);
            if (IsAi) {
                nowScore = int.MinValue;
                foreach (var move in availableMoves) {
                    PlayChess(move.Item1, move.Item2, Role.AI);
                    int nowRoundScore = EvalToGo(depth + 1, alpha, beta, availableMoves, move.Item1, move.Item2);
                    PlayChess(move.Item1, move.Item2, Role.Empty);
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
                    PlayChess(move.Item1, move.Item2, Role.Player);
                    int nowRoundScore = EvalToGo(depth + 1, alpha, beta, availableMoves, move.Item1, move.Item2);
                    PlayChess(move.Item1, move.Item2, Role.Empty);
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
