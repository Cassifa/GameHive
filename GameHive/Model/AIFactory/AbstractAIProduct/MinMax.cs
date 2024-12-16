/*************************************************************************************
 * 文 件 名:   MinMax.cs
 * 描    述: 博弈树抽象产品，实现博弈树基本流程
 *          实现方法：1.游戏开始-初始化棋盘并重置已落子数量 2.获取下一步决策 3.运行博弈树计算决策
 *          定义抽象方法:
 *                    1.GetAvailableMoves  获取所有可下棋点位
 *                    2.GetAvailableMovesByNewPieces 使用上次局面、上次可落子点、本次落子点获取本次可落子点
 *                    3.PlayChess          下棋，更新本地保存的棋盘和缓存
 *                    4.GetCurrentBoard    获取当前棋盘
 *                    5.EvalNowSituation   AI视角下局面估值
 *          虚函数:
 *                    1.VCT                计算是否可连续冲三杀棋
 *                    2.HeuristicSort      对获取的可下棋节点启发式排序
 *          工具函数:
 *                    1.DeepeningKillBoard 对获取的可下棋节点启发式排序
 *                    2.DeepeningMinMax    对获取的可下棋节点启发式排序
 *          实现抽象策略:
 *                    2.GetNextAIMove      对获取的可下棋节点启发式排序
 *                    2.EvalToGo           对获取的可下棋节点启发式排序
 *                    3.GameStart          启动游戏，初始化参数，重置缓存
 *                    4.GameForcedEnd      终止游戏
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:11
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIUtils;
using GameHive.Model.AIUtils.AlgorithmUtils;

namespace GameHive.Model.AIFactory.AbstractAIProduct {
    internal abstract class MinMax : AbstractAIStrategy {
        /**********成员定义**********/

        //最大搜索深度；
        protected int maxDeep, killingMaxDeep;
        protected bool RunKillBoard = false;
        //是否需要迭代加深计算
        protected bool DeepeningKillingActivated = false;
        //游戏是否结束
        public bool GameOver;
        //当前已经落子数量
        protected int PlayedPiecesCnt;
        protected int TotalPiecesCnt;
        //MinMax缓存表、局面估值缓存表、算杀估值表
        protected ZobristHashingCache<int> MinMaxCache;
        private Tuple<int, int> FinalDecide = new Tuple<int, int>(0, 0);

        /**********1.抽象方法**********/
        //初始化棋盘
        protected abstract void InitGame();

        //获取可下棋点位
        protected abstract List<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board);

        //使用历史可用与最新落子获取最新可用
        //从历史可落子点位中移除本次落子点(若存在)
        //添加新可用点至表头，老可用点引用传入，返回深拷贝列表(老可用点为浅拷贝)
        protected abstract List<Tuple<int, int>> GetAvailableMovesByNewPieces(
            List<List<Role>> currentBoard, List<Tuple<int, int>> lastAvailableMoves,
            int lastX, int lastY);

        //估值函数
        protected abstract int EvalNowSituation(List<List<Role>> currentBoard, Role role);

        //在算法备份的棋盘上落子
        protected abstract void PlayChess(int x, int y, Role role);

        //获取当前棋盘
        protected abstract List<List<Role>> GetCurrentBoard();

        /**********2.默认实现方法**********/
        //启发式搜索
        protected virtual void HeuristicSort(ref List<Tuple<int, int>> moves, int lastX, int lastY) { }
        //计算VCT杀棋
        protected virtual Tuple<int, int> VCT(Role type, int leftDepth) {
            return null;
        }


        /**********2.迭代加深博弈树**********/
        //迭代加深算杀
        private Tuple<int, int> DeepeningKillBoard(int maxDepth, int lastX, int lastY) {
            List<Tuple<int, int>> lastAvailableMoves = GetAvailableMoves(GetCurrentBoard());
            Tuple<int, int>? result = Tuple.Create(-1, -1);
            for (int i = 2; i <= maxDepth; i += 2) {
                result = VCT(Role.AI, i);
                if (result.Item1 != -1)
                    break;
            }
            return result;
        }
        //迭代加深下棋-对于有算杀的无效因为此处能找到解一定被算杀找到过了
        private void DeepeningMinMax(int maxDepth, int lastX, int lastY) {
            List<Tuple<int, int>> lastAvailableMoves = GetAvailableMoves(GetCurrentBoard());
            //逐步下棋，直到在最大层搜或者得分表示已经胜利
            for (int i = DeepeningKillingActivated ? 2 : maxDepth; i <= maxDepth; i += 2) {
                int value = EvalToGo(i, int.MinValue, int.MaxValue, lastAvailableMoves, lastX, lastY);
                if (value >= 1_000_000 - i)
                    return;
            }
        }


        /**********3.实现策略模式方法**********/

        //获取下一步移动
        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            if (RecordSimulateUtil.ActiveSimulate)
                //如果在模拟杀棋管辖部署内执行模拟杀棋逻辑,修改了上次玩家落子和决策
                if (RecordSimulateUtil.SimulateKillBoard(ref lastX, ref lastY, ref FinalDecide)) {
                    //如果是AI先手传来的上一步是-1，-1
                    if (lastX != -1)
                        PlayChess(lastX, lastY, Role.Player);
                    //下模拟步
                    PlayChess(FinalDecide.Item1, FinalDecide.Item2, Role.AI);
                    return FinalDecide;
                }
            //收到玩家移动，更新棋盘
            if (lastX != -1)
                PlayChess(lastX, lastY, Role.Player);
            //总共已经落子的大于5则考虑杀棋
            if (RunKillBoard && PlayedPiecesCnt > 5) {
                Tuple<int, int> KillingMove = DeepeningKillBoard(killingMaxDeep, lastX, lastY);
                //杀棋命中直接返回杀棋
                if (KillingMove.Item1 != -1) {
                    PlayChess(KillingMove.Item1, KillingMove.Item2, Role.AI);
                    return KillingMove;
                }
            }
            DeepeningMinMax(maxDeep, lastX, lastY);

            ////计算最优值
            //EvalToGo(maxDeep, int.MinValue, int.MaxValue, GetAvailableMoves(currentBoard), lastX, lastY);

            //AI下棋
            PlayChess(FinalDecide.Item1, FinalDecide.Item2, Role.AI);
            //计算出AI移动，跟新棋盘
            return FinalDecide;
        }

        //执行博弈树搜索
        private int EvalToGo(int depth, int alpha, int beta, //List<Tuple<int, int>> lastUsedMoves,
                List<Tuple<int, int>> lastAvailableMoves, int lastX, int lastY) {
            if (GameOver) return 1_000_000;
            int nowScore = 0; Tuple<int, int> nowDec = new Tuple<int, int>(0, 0);
            //不是顶层则查缓存-顶层需要搜决策节点价值
            if (depth != maxDeep && MinMaxCache.GetValue(ref nowScore) >= depth)
                return nowScore;

            // 检查当前局面的胜负情况
            Role winner = CheckGameOverByPiece(GetCurrentBoard(), lastX, lastY);
            if (winner == Role.Draw) return 0;
            else if (winner == Role.AI) return 1_000_000;//- depth;//防止AI调戏玩家
            else if (winner == Role.Player) return -1_000_000;
            if (depth == 0) return EvalNowSituation(GetCurrentBoard(), Role.AI);
            bool IsAi = ((depth % 2) == 0);

            //根据上一步操作获取下一步可行点位
            var availableMoves = GetAvailableMovesByNewPieces(GetCurrentBoard(), lastAvailableMoves, lastX, lastY);
            HeuristicSort(ref availableMoves, lastX, lastY);
            if (IsAi) {
                //AI Max层，寻找Min层返回的价值最大的
                nowScore = int.MinValue;
                foreach (var move in availableMoves) {
                    PlayChess(move.Item1, move.Item2, Role.AI);
                    int nowRoundScore = EvalToGo(depth - 1, alpha, beta, availableMoves, move.Item1, move.Item2);
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
                //人类 Min层，寻找价值最小的局面
                nowScore = int.MaxValue;
                foreach (var move in availableMoves) {
                    PlayChess(move.Item1, move.Item2, Role.Player);
                    int nowRoundScore = EvalToGo(depth - 1, alpha, beta, availableMoves, move.Item1, move.Item2);
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
            //只有顶层更新FinalDecide有意义
            FinalDecide = nowDec;
            //更新缓存
            MinMaxCache.Log(nowScore, depth);
            return nowScore;
        }


        //游戏开始
        public override void GameStart(bool IsAIFirst) {
            GameOver = false;
            PlayedPiecesCnt = 0;
            InitGame();
            MinMaxCache.RefreshLog();
        }

        //游戏结束
        public override void GameForcedEnd() {
            GameOver = true;
        }


        /**********4.博弈树不需要**********/
        //用户下棋
        public override void UserPlayPiece(int lastX, int lastY) {
        }

        ////获取在X,Y落子带来的总收益(带风险偏好)
        //protected abstract int evalXYTRoundScore(List<List<Role>> currentBoard, int x,int y, Role role);
        ////计算 role 视角下在X,Y落子获得的攻击分数
        //protected abstract int calculateXY(List<List<Role>> currentBoard, int x,int y, Role role);


    }
}
