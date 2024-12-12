/*************************************************************************************
 * 文 件 名:   MinMaxMCTS.cs
 * 描    述: 蒙特卡洛博弈树抽象产品
 *          使用蒙特卡洛完成估值函数
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:11
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIUtils.MonteCarloTreeSearch;
namespace GameHive.Model.AIFactory.AbstractAIProduct {
    internal abstract class MinMaxMCTS : AbstractAIStrategy {
        //最大搜索深度
        protected int maxDeep;
        //搜索轮数
        protected int SearchCount;
        protected MCTSNode? RootNode;
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
                return EvalNowSituation(GetCurrentBoard(), lastX, lastY, Role.AI, GetAvailableMovesByNewPieces(GetCurrentBoard(), lastAvailableMoves, lastX, lastY));
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

        //通过蒙特卡洛对博弈树节点估值
        double EvalNowSituation(List<List<Role>> Board, int lastX, int lastY, Role role, List<Tuple<int, int>> availablePiece) {
            RootNode = new MCTSNode(GetCurrentBoard(), null, lastX, lastY, Role.Player,
                Role.Empty, availablePiece);

            for (int i = 0; i < SearchCount; i++)
                SimulationOnce();
            SimulationOnce();
            MCTSNode aim = RootNode.GetGreatestUCB();
            return aim.GetUCB();
        }

        //运行一次蒙特卡洛过程
        private void SimulationOnce() {
            MCTSNode SimulationAim = Selection(RootNode);
            if (SimulationAim.IsNewLeaf())
                SimulationAim.BackPropagation(RollOut(SimulationAim));
            else NodeExpansion(SimulationAim);
        }

        //选择 从Root开始仅选择叶子节点（可能为终止节点）
        private MCTSNode Selection(MCTSNode root) {
            MCTSNode currentSelected = root;
            while (true) {
                if (currentSelected.IsLeaf) break;
                currentSelected = currentSelected.GetGreatestUCB();
            }
            return currentSelected;
        }

        //从当前节点开始模拟，返回赢家-如果节点是全新的
        private Role RollOut(MCTSNode node) {
            //如果这个节点本身就是终止节点，直接返回
            if (node.Winner != Role.Empty) return node.Winner;
            List<List<Role>> currentBoard = node.NodeBoard.Select(row => new List<Role>(row)).ToList();
            Random rand = new Random();
            Role WhoPlaying = node.LeadToThisStatus;
            Role winner;
            while (true) {
                //获取可行点并模拟落子
                List<Tuple<int, int>> moves = GetAvailableMoves(currentBoard);
                Tuple<int, int> move = moves[rand.Next(moves.Count)];
                currentBoard[move.Item1][move.Item2] = WhoPlaying;
                winner = CheckGameOverByPiece(currentBoard, move.Item1, move.Item2);
                //已经结束直接跳出
                if (winner != Role.Empty) break;
                if (WhoPlaying == Role.AI) WhoPlaying = Role.Player;
                else WhoPlaying = Role.AI;
            }
            return winner;
        }

        //拓展节点-如果此节点不是全新的
        private void NodeExpansion(MCTSNode father) {
            List<Tuple<int, int>> moves = father.AvailablePiece;
            Role sonPlayerView;
            if (father.LeadToThisStatus == Role.AI) sonPlayerView = Role.Player;
            else sonPlayerView = Role.AI;
            foreach (var move in moves) {
                List<List<Role>> currentBoard = father.NodeBoard.Select(row => new List<Role>(row)).ToList();
                currentBoard[move.Item1][move.Item2] = sonPlayerView;
                MCTSNode nowSon = new MCTSNode(currentBoard, father, move.Item1, move.Item2,
                    sonPlayerView, CheckGameOverByPiece(currentBoard, move.Item1, move.Item2),
                    GetAvailableMoves(currentBoard));
                father.AddSon(nowSon, move.Item1, move.Item2);
            }
            father.IsLeaf = false;
        }




        //用户下棋
        public override void UserPlayPiece(int lastX, int lastY) { }
        //强制游戏结束 停止需要多线程的AI 更新在内部保存过状态的AI
        public override void GameForcedEnd() { }
    }
}
