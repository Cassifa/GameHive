/*************************************************************************************
* 文 件 名:   MTCS.cs
* 描    述: 蒙特卡洛搜索抽象产品
*          选择->模拟/拓展->反向传播
*          负责：选择、模拟、拓展三个步骤
*          提供方法：1.获取AI下一步（实现抽象方法） 
*                  2.调用一定次数蒙特卡洛计算决策 
*                  3.执行一次蒙特卡洛过程
*                  4.根据根节点选择节点 
*                  5.从当前节点开始模拟 
*                  6.拓展当前节点 
*                  7.获取所有Empty点，保证换根成功 
*                  8.更新模拟次数
*                  9.PlayChess：用于更新缓存当前的局面哈希值
*          多线程： 1.开启游戏，构建根节点，开启搜索线程
*                  2.估值一次，会执行 MinSearchCount次，然后检查是否有要求过做出决策
*                  3.获取AI下一步 收到玩家决策/AI做出决策会进行换根并返回当前根节点最优决策
*          抽象方法：
*                  1.拓展获取所有可行决策且保证不为空
*                  2.通过缓存获取结果
* 版    本：  V2.0 .NET客户端初版
* 创 建 者：  Cassifa
* 创建时间：  2024/11/26 18:11
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIUtils.AlgorithmUtils;
using GameHive.Model.AIUtils.MonteCarloTreeSearch;

namespace GameHive.Model.AIFactory.AbstractAIProduct {
    internal abstract class MCTS : AbstractAIStrategy {
        //*****MCTS抽象类内变量*****//
        //根节点
        private MCTSNode RootNode;
        //互斥锁
        Mutex mutex = new Mutex();
        //游戏结束信号
        private volatile bool end = false;
        //游戏当前搜索轮数
        private int SearchCount;
        //互斥期间搜索次数-用于判断是否达到SearchCount
        private int AIMoveSearchCount;
        //是否玩家正在思考
        private bool PlayerPlaying;

        //*****游戏构造参数*****//
        //棋盘一行落子的数
        protected int TotalPiecesCnt;
        //是否启用多线程搜索
        protected bool MultiThreadExecutionEnabled=true;
        //是否运行反向传播MinMax
        protected bool RunBackPropagateMinMax = false;
        //初始搜索轮数 多线程内部最小搜索次数（达到后释放锁）
        protected int baseCount = 1000, MinSearchCount = 1000;
        //是否更新搜索次数(小规模棋盘无需更新)
        protected bool NeedUpdateSearchCount = false;

        //*****MCTS类共享给子类参数*****//
        //缓存表
        protected ZobristHashingCache<Role> GameOverStatusCache;
        //总下棋数目
        protected int PlayedPiecesCnt;

        //获取可行落子
        protected abstract List<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board);
        //使用Cache获取结果
        protected abstract Role CheckGameOverByPieceWithCache(List<List<Role>> currentBoard, int x, int y);

        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            //争夺锁，换根
            lock (mutex) {
                PlayerPlaying = false;
                //根据玩家决策换根
                if (lastX != -1) {
                    PlayedPiecesCnt++;
                    RootNode = RootNode.MoveRoot(lastX, lastY, NodeExpansion);
                    PlayChess(Role.Player, lastX, lastY);
                }
                AIMoveSearchCount = 0;

                //释放锁并等待搜索线程通知-收到通知后判断是否达标
                while (AIMoveSearchCount <= SearchCount)
                    Monitor.Wait(mutex);
                //根据AI决策换根
                Tuple<int, int> AIDecision = RootNode.GetGreatestUCB().PieceSelectedCompareToFather;
                RootNode = RootNode.MoveRoot(AIDecision.Item1, AIDecision.Item2, NodeExpansion);
                //AI在此下棋，记录日志。不需要更新缓存字典，因为换根前的拓展已经记录过此状态。但需要更新缓存局面值
                PlayChess(Role.AI, AIDecision.Item1, AIDecision.Item2);

                PlayedPiecesCnt++;
                //更新搜索次数
                if (NeedUpdateSearchCount)
                    UpdateSearchCount(baseCount, TotalPiecesCnt * TotalPiecesCnt, PlayedPiecesCnt, ref SearchCount);
                //轮到玩家
                PlayerPlaying = true;
                return AIDecision;
            }
        }

        //执行一次搜索任务
        private void EvalToGo() {
            //一直执行直到结束
            while (!end) {
                lock (mutex) {
                    //禁用多线程搜索标记
                    if (!MultiThreadExecutionEnabled && PlayerPlaying)
                        continue;
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
            end = false;
            PlayedPiecesCnt = 0;
            SearchCount = baseCount;
            //创建根节点与初始可移动序列
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
            //非先手者作为虚拟节点（根节点的父节点）所有人
            Role WhoLeadTo;
            if (IsAIFirst) {
                WhoLeadTo = Role.Player;
                PlayerPlaying = false;
            } else {
                WhoLeadTo = Role.AI;
                PlayerPlaying = true;
            }
            RootNode = new MCTSNode(board, null, -1, -1, WhoLeadTo, Role.Empty, moves);

            //初始化缓存表
            GameOverStatusCache = new ZobristHashingCache<Role>(TotalPiecesCnt, TotalPiecesCnt);
            //启动搜索任务
            Task.Run(() => EvalToGo());
        }

        //维护当前追踪的缓存哈希值
        private void PlayChess(Role role, int x, int y) {
            GameOverStatusCache.UpdateCurrentBoardHash(x, y, role);
        }

        //强制游戏结束 停止需要多线程的AI 更新在内部保存过状态的AI
        public override void GameForcedEnd() {
            end = true;
        }

        //运行一次蒙特卡洛过程
        private void SimulationOnce() {
            //选择前打上忽略标记
            GameOverStatusCache.ActiveMoveDiscard();
            MCTSNode SimulationAim = Selection(RootNode);
            if (SimulationAim.IsNewLeaf())
                //RollOut模拟过程也会打一次标记
                SimulationAim.BackPropagation(RollOut(SimulationAim));
            else
                NodeExpansion(SimulationAim);
            //蒙特卡洛运行结束后恢复
            GameOverStatusCache.WithDrawMoves();
        }

        //选择 从Root开始仅选择叶子节点（可能为终止节点）
        private MCTSNode Selection(MCTSNode root) {
            MCTSNode currentSelected = root;
            while (true) {
                if (currentSelected.IsLeaf)
                    break;
                currentSelected = currentSelected.GetGreatestUCB();
                //更新缓存哈希值
                PlayChess(currentSelected.LeadToThisStatus, currentSelected.PieceSelectedCompareToFather.Item1,
                    currentSelected.PieceSelectedCompareToFather.Item2);
            }
            return currentSelected;
        }

        //从当前节点开始模拟，返回赢家-如果节点是全新的
        private Role RollOut(MCTSNode node) {
            //如果这个节点本身就是终止节点，直接返回 
            if (node.getWinner() != Role.Empty)
                return node.getWinner();
            List<List<Role>> currentBoard = node.NodeBoard.Select(row => new List<Role>(row)).ToList();
            Random rand = new Random();
            //对导致当前局面的角色取反为当前玩家
            Role WhoPlaying = node.LeadToThisStatus == Role.Player ? Role.AI : Role.Player;
            Role lastChessWinnerResult;
            //启用缓存回溯功能
            GameOverStatusCache.ActiveMoveDiscard();
            List<Tuple<int, int>> movesLsit = new List<Tuple<int, int>>();
            List<Role> roleList = new List<Role>();
            List<List<List<Role>>> boardStates = new List<List<List<Role>>>(); // 新增棋盘状态记录
            while (true) {
                //获取可行点并模拟落子
                List<Tuple<int, int>> moves = GetAvailableMoves(currentBoard);
                Tuple<int, int> move = moves[rand.Next(moves.Count)];
                movesLsit.Add(move);
                currentBoard[move.Item1][move.Item2] = WhoPlaying;

                //结果录入缓存
                PlayChess(WhoPlaying, move.Item1, move.Item2);
                lastChessWinnerResult = CheckGameOverByPieceWithCache(currentBoard, move.Item1, move.Item2);
                roleList.Add(lastChessWinnerResult);
                List<List<Role>> copiedBoard = currentBoard.Select(row => new List<Role>(row)).ToList();
                boardStates.Add(copiedBoard);
                //已经结束直接跳出
                if (lastChessWinnerResult != Role.Empty)
                    break;
                if (WhoPlaying == Role.AI)
                    WhoPlaying = Role.Player;
                else
                    WhoPlaying = Role.AI;
            }
            //回溯本次模拟结果
            GameOverStatusCache.WithDrawMoves();
            return lastChessWinnerResult;
        }

        //拓展节点-如果此节点不是全新的
        private void NodeExpansion(MCTSNode father, bool Force = false) {
            //搜到了无解点，强制加入所有点保证换根成功
            if (Force) {
                father.AvailablePiece = GetAllEmptyPlace(father.NodeBoard);
            }
            List<Tuple<int, int>> moves = father.AvailablePiece;

            Role sonPlayerView;
            if (father.LeadToThisStatus == Role.AI)
                sonPlayerView = Role.Player;
            else
                sonPlayerView = Role.AI;

            foreach (var move in moves) {
                //深拷贝新棋盘
                List<List<Role>> currentBoard = father.NodeBoard.Select(row => new List<Role>(row)).ToList();
                currentBoard[move.Item1][move.Item2] = sonPlayerView;

                //结果录入缓存
                PlayChess(sonPlayerView, move.Item1, move.Item2);
                Role lastChessWinnerResult = CheckGameOverByPieceWithCache(currentBoard, move.Item1, move.Item2);
                //撤销本次记录（同一个点同一人下两次等价撤销）
                PlayChess(sonPlayerView, move.Item1, move.Item2);

                //拓展节点
                MCTSNode nowSon = new MCTSNode(currentBoard, father, move.Item1, move.Item2,
                    sonPlayerView, lastChessWinnerResult,
                    GetAvailableMoves(currentBoard));
                father.AddSon(nowSon, move.Item1, move.Item2);

            }
            father.IsLeaf = false;
            //反向传播胜利信息
            if (RunBackPropagateMinMax)
                father.RunBackPropagateMinMax();
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
            if (LeftCount == 0)
                return;
            int t = 0;
            //未下棋数量越少搜索次数越高
            while (TotalCnt / LeftCount >= 1.5) {
                t++;
                TotalCnt /= LeftCount;
            }
            t = Math.Min(t, 3);
            SearchCount = BaseCount * (int)Math.Pow(10, t);
        }

        //用户下棋
        public override void UserPlayPiece(int lastX, int lastY) { }
    }
}
