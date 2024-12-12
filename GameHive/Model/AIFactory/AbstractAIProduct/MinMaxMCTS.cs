/*************************************************************************************
 * 文 件 名:   DRL.cs
 * 描    述: 深度强化学习抽象产品
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:11
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIUtils.MonteCarloTreeSearch;
namespace GameHive.Model.AIFactory.AbstractAIProduct {
    internal abstract class MinMaxMCTS : AbstractAIStrategy {
        //搜索轮数
        //最大搜索深度
        protected int maxDeep;
        protected int SearchCount;
        protected MCTSNode RootNode;
        protected List<List<Role>> currentBoard;
        private Tuple<int, int>? FinalDecide;
        //当前已经落子数量
        protected int PlayedPiecesCnt;
        protected int TotalPiecesCnt;
        //在算法备份的棋盘上落子
        protected abstract void PlayChess(int x, int y, Role role);
        //获取可下棋点位
        protected abstract List<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board);
        //使用历史可用与最新落子获取最新可用
        //从历史可落子点位中移除本次落子点(若存在)
        protected abstract List<Tuple<int, int>> GetAvailableMovesByNewPieces(
            List<List<Role>> currentBoard, List<Tuple<int, int>> lastAvailableMoves,
            int lastX, int lastY);
        //获取当前棋盘
        protected abstract List<List<Role>> GetCurrentBoard();

        //获取下一步移动
        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            //收到玩家移动，更新棋盘
            if (lastX != -1)
                PlayChess(lastX, lastY, Role.Player);
            List<Tuple<int, int>> lastAvailableMoves = GetAvailableMoves(currentBoard);
            //计算最优值
            EvalToGo(0, double.NegativeInfinity, double.PositiveInfinity, lastAvailableMoves, lastX, lastY);
            //AI下棋
            PlayChess(FinalDecide.Item1, FinalDecide.Item2, Role.AI);
            //计算出AI移动，跟新棋盘
            return FinalDecide;
        }

        //执行博弈树搜索
        private double EvalToGo(int depth, double alpha, double beta,
                List<Tuple<int, int>> lastAvailableMoves, int lastX, int lastY) {
            // 检查当前局面的胜负情况
            Role winner = CheckGameOverByPiece(GetCurrentBoard(), lastX, lastY);
            if (winner == Role.Draw) return 0;
            else if (winner == Role.AI) return 1_000_000;
            else if (winner == Role.Player) return -1_000_000;
            if (depth == maxDeep)
                return EvalNowSituation(GetCurrentBoard(), Role.AI);
            bool IsAi = ((depth % 2) == 0);
            double nowScore; Tuple<int, int>? nowDec = null;
            //根据上一步操作获取下一步可行点位
            var availableMoves = GetAvailableMovesByNewPieces(GetCurrentBoard(), lastAvailableMoves, lastX, lastY);
            if (IsAi) {
                nowScore = double.NegativeInfinity;
                foreach (var move in availableMoves) {
                    PlayChess(move.Item1, move.Item2, Role.AI);
                    double nowRoundScore = EvalToGo(depth + 1, alpha, beta, availableMoves, move.Item1, move.Item2);
                    PlayChess(move.Item1, move.Item2, Role.Empty);
                    if (nowRoundScore > nowScore) {
                        nowScore = nowRoundScore;
                        nowDec = move;
                        alpha = Math.Max(alpha, nowRoundScore);
                    }
                    if (alpha >= beta)
                        break;
                }
            } else {
                nowScore = double.PositiveInfinity;
                foreach (var move in availableMoves) {
                    PlayChess(move.Item1, move.Item2, Role.Player);
                    double nowRoundScore = EvalToGo(depth + 1, alpha, beta, availableMoves, move.Item1, move.Item2);
                    PlayChess(move.Item1, move.Item2, Role.Empty);
                    if (nowRoundScore < nowScore) {
                        nowScore = nowRoundScore;
                        nowDec = move;
                        beta = Math.Min(beta, nowRoundScore);
                    }
                    if (alpha >= beta)
                        break;
                }

            }
            FinalDecide = nowDec;
            return nowScore;
        }

        //double EvalNowSituation(List<List<Role>> Board, Role role) {

        //}
    }
}
