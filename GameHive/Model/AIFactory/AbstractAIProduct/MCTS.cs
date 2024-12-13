/*************************************************************************************
 * 文 件 名:   MTCS.cs
 * 描    述: 蒙特卡洛搜索抽象产品
 *          选择->模拟/拓展->反向传播
 *          负责：选择、模拟、拓展三个步骤
 *          提供方法：1.获取AI下一步（实现抽象方法） 2.调用一定次数蒙特卡洛计算决策 3.执行一次蒙特卡洛过程
 *                  4.根据根节点选择节点 5.从当前节点开始模拟 6.拓展当前节点 7.获取所有Empty点，保证换根成功 8.更新模拟次数
 *             多线程：1.开启游戏，构建根节点，开启搜索线程
 *                    2.估值一次，会执行 MinSearchCount次，然后检查是否有要求过输入
 *                    3.获取AI下一步 收到玩家决策/AI做出决策会进行换根并返回当前根节点最优决策
 *          定义抽象方法：拓展获取所有可行决策且保证不为空
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:11
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIUtils.MonteCarloTreeSearch;

namespace GameHive.Model.AIFactory.AbstractAIProduct {
    internal abstract class MCTS : AbstractAIStrategy {
        //游戏搜索轮数
        protected int SearchCount, baseCount;
        //是否更新搜索次数(小规模棋盘无需更新)
        protected bool NeedUpdateSearchCount = false;
        //单次线程最小搜索次数，达到后释放一次锁
        private int MinSearchCount = 1000;
        //互斥期间搜索次数
        private int AIMoveSearchCount, PlayedPiecesCnt;
        //互斥锁
        Mutex mutex = new Mutex();
        //游戏结束信号
        private volatile bool end = false;
        //已经落子的数量
        protected int TotalPiecesCnt;
        //根节点
        protected MCTSNode RootNode;

        //获取可行落子
        protected abstract List<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board);

        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            //争夺锁，换根
            lock (mutex) {
                Role winner;
                //AI先手会根据(-1,-1)构造初始棋盘
                if (lastX == -1) winner = Role.Empty;
                else winner = CheckGameOverByPiece(currentBoard, lastX, lastY);
                //根据玩家决策换根
                if (lastX != -1) {
                    PlayedPiecesCnt++;
                    RootNode = RootNode.MoveRoot(lastX, lastY, NodeExpansion);
                }
                AIMoveSearchCount = 0;

                //释放锁并等待搜索线程通知-收到通知后判断是否达标
                while (AIMoveSearchCount < SearchCount)
                    Monitor.Wait(mutex);
                //根据AI决策换根
                Tuple<int, int> AIDecision = RootNode.GetGreatestUCB().PieceSelectedCompareToFather;
                RootNode = RootNode.MoveRoot(AIDecision.Item1, AIDecision.Item2, NodeExpansion);
                PlayedPiecesCnt++;
                if (NeedUpdateSearchCount)
                    UpdateSearchCount(baseCount, TotalPiecesCnt * TotalPiecesCnt, PlayedPiecesCnt, ref SearchCount);
                return AIDecision;
            }
        }

        //执行一次搜索任务
        private void EvalToGo() {
            //一直执行直到结束
            while (!end) {
                lock (mutex) {
                    //搜最小单元后释放一次锁
                    for (int i = 0; i < MinSearchCount; i++)
                        SimulationOnce();
                    AIMoveSearchCount += MinSearchCount;
                    Monitor.Pulse(mutex);
                }
            }
        }

        //游戏开始-初始化根节点，启动搜索线程
        public override void GameStart(bool IsAIFirst) {
            end = false; PlayedPiecesCnt = 0; SearchCount = baseCount;
            //创建根节点
            List<List<Role>> board = new List<List<Role>>(TotalPiecesCnt);
            List<Tuple<int, int>> moves = new List<Tuple<int, int>>();
            for (int i = 0; i < TotalPiecesCnt; i++) {
                List<Role> row = new List<Role>(TotalPiecesCnt);
                for (int j = 0; j < TotalPiecesCnt; j++) {
                    moves.Add(new Tuple<int, int>(i, j));
                    row.Add(Role.Empty);
                }
                board.Add(row);
            }
            Role WhoLeadTo = Role.AI;
            if (IsAIFirst) WhoLeadTo = Role.Player;
            RootNode = new MCTSNode(board, null, -1, -1, WhoLeadTo, Role.Empty, moves);
            //启动搜索任务
            Task.Run(() => EvalToGo());
        }


        //强制游戏结束 停止需要多线程的AI 更新在内部保存过状态的AI
        public override void GameForcedEnd() {
            end = true;
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
        private void NodeExpansion(MCTSNode father, bool Force = false) {
            //搜到了无解点，强制加入所有点保证换根成功
            if (Force) {
                father.AvailablePiece = GetAllEmptyPlace(father.NodeBoard);
            }
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


        //防止对于AI而言无可行点导致换根失败
        private List<Tuple<int, int>> GetAllEmptyPlace(List<List<Role>> board) {
            List<Tuple<int, int>> moves = new List<Tuple<int, int>>();
            for (int i = 0; i < board.Count; i++)
                for (int j = 0; j < board.Count; j++)
                    if (board[i][j] == Role.Empty)
                        moves.Add(new Tuple<int, int>(i, j));
            return moves;
        }

        //更新模拟次数
        protected void UpdateSearchCount(int BaseCount, int TotalCnt, int NowCount, ref int SearchCount) {
            int LeftCount = TotalCnt - NowCount;
            if (LeftCount == 0) return;
            int t = 0;
            //未下棋数量越少搜索次数越高
            while (TotalCnt / LeftCount >= 1.5) {
                t++;
                TotalCnt /= LeftCount;
            }
            t = Math.Min(t, 5);
            SearchCount = BaseCount * (int)Math.Pow(10, t);
        }
        //用户下棋
        public override void UserPlayPiece(int lastX, int lastY) { }
    }
}
