/*************************************************************************************
 * 文 件 名:   MTCS.cs
 * 描    述: 蒙特卡洛搜索抽象产品
 *          选择->模拟/拓展->反向传播
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:11
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIUtils.MonteCarloTreeSearch;

namespace GameHive.Model.AIFactory.AbstractAIProduct {
    internal abstract class MCTS : AbstractAIStrategy {
        //搜索轮数
        protected int SearchCount;
        protected MCTSNode RootNode;
        protected List<List<Role>> currentBoard;
        //获取可行落子
        protected abstract List<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board);
        //根据落子事件检查游戏是否结束
        //public override Role CheckGameOverByPiece(List<List<Role>> currentBoard, int x, int y);

        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            RootNode = new MCTSNode(currentBoard, null, lastX, lastY, Role.Player,
                CheckGameOverByPiece(currentBoard, lastX, lastY), GetAvailableMoves(currentBoard));
            return EvalToGo();
        }

        //模拟一次
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


        //调用蒙特卡洛获取下一步
        private Tuple<int, int> EvalToGo() {
            //模拟两百轮
            for (int i = 0; i < SearchCount; i++)
                SimulationOnce();
            SimulationOnce();
            MCTSNode aim = RootNode.GetGreatestUCB();
            return aim.PieceSelectedCompareToFather;
        }
        //用户下棋
        public override void UserPlayPiece(int lastX, int lastY) { }
        //强制游戏结束 停止需要多线程的AI 更新在内部保存过状态的AI
        public override void GameForcedEnd() { }
        //游戏开始
        public override void GameStart(bool IsAIFirst) { }
    }
}
