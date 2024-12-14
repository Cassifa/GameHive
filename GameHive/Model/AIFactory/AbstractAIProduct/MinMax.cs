/*************************************************************************************
 * 文 件 名:   MinMax.cs
 * 描    述: 博弈树抽象产品
 *          实现方法：1.游戏开始-初始化棋盘并重置已落子数量 2.获取下一步决策 3.运行博弈树计算决策
 *          定义抽象方法
 *                    1.GetAvailableMoves获取所有可下棋点位
 *                    2.EvalNowSituation根据特定玩家视角对局面估值
 *                    3.PlayChess下棋，更新本地保存的棋盘
 *                    4.GetCurrentBoard获取当前棋盘
 *                    5.GetAvailableMovesByNewPieces使用上次局面、上次可落子点、本次落子点获取本次可落子点
 *                    6.计算是否可杀棋(提供默认空实现)
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:11
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;

namespace GameHive.Model.AIFactory.AbstractAIProduct {
    internal abstract class MinMax : AbstractAIStrategy {
        //最大搜索深度；
        protected int maxDeep, killingMaxDeep;
        //当前已经落子数量
        protected int PlayedPiecesCnt;
        protected int TotalPiecesCnt;
        //决定Ai的风格偏好,加起来权重为10
        protected int DefendBias = 4, AttackBias = 6;
        private Tuple<int, int>? FinalDecide;
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
        protected abstract int EvalNowSituation(List<List<Role>> currentBoard, Role role);
        //在算法备份的棋盘上落子
        protected abstract void PlayChess(int x, int y, Role role);
        //获取当前棋盘
        protected abstract List<List<Role>> GetCurrentBoard();
        //计算VCT杀棋
        protected virtual Tuple<int, int>? VCT(Role type, int depth, List<Tuple<int, int>> lastAvailableMoves, int lastX, int lastY) {
            return null;
        }
        //获取下一步移动
        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            //收到玩家移动，更新棋盘
            if (lastX != -1)
                PlayChess(lastX, lastY, Role.Player);
            List<Tuple<int, int>> lastAvailableMoves = GetAvailableMoves(currentBoard);
            //总共已经落子的大于5则考虑杀棋
            //if (PlayedPiecesCnt > 5) {
            //    Tuple<int, int>? KillingMove = null;
            //    //Tuple<int, int>? KillingMove = VCT(Role.AI, 0, lastAvailableMoves, lastX, lastY);
            //    //杀棋命中直接返回杀棋
            //    if (KillingMove != null) {
            //        PlayChess(KillingMove.Item1, KillingMove.Item2, Role.AI);
            //        return KillingMove;
            //    }
            //}
            lastAvailableMoves = GetAvailableMoves(currentBoard);
            //计算最优值
            EvalToGo(0, int.MinValue, int.MaxValue, lastAvailableMoves, lastX, lastY);
            //AI下棋
            PlayChess(FinalDecide.Item1, FinalDecide.Item2, Role.AI);
            //计算出AI移动，跟新棋盘
            return FinalDecide;
        }

        //执行博弈树搜索
        private int EvalToGo(int depth, int alpha, int beta,
                List<Tuple<int, int>> lastAvailableMoves, int lastX, int lastY) {
            // 检查当前局面的胜负情况
            Role winner = CheckGameOverByPiece(GetCurrentBoard(), lastX, lastY);
            if (winner == Role.Draw) return 0;
            else if (winner == Role.AI) return 1_000_000;
            else if (winner == Role.Player) return -1_000_000;
            if (depth == maxDeep)
                return EvalNowSituation(GetCurrentBoard(), Role.AI);
            bool IsAi = ((depth % 2) == 0);
            int nowScore; Tuple<int, int>? nowDec = null;
            //根据上一步操作获取下一步可行点位
            var availableMoves = GetAvailableMovesByNewPieces(GetCurrentBoard(), lastAvailableMoves, lastX, lastY);
            if (IsAi) {
                //AI Max层，寻找Min层返回的价值最大的
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
                    if (alpha >= beta)
                        break;
                }
            } else {
                //人类 Min层，寻找价值最小的局面
                nowScore = int.MaxValue;
                foreach (var move in availableMoves) {
                    PlayChess(move.Item1, move.Item2, Role.Player);
                    int nowRoundScore = EvalToGo(depth + 1, alpha, beta, availableMoves, move.Item1, move.Item2);
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


        //游戏开始
        public override void GameStart(bool IsAIFirst) {
            PlayedPiecesCnt = 0;
            InitGame();
        }

        /*****博弈树不需要*****/
        //游戏结束
        public override void GameForcedEnd() { }
        //用户下棋
        public override void UserPlayPiece(int lastX, int lastY) {
        }

        ////获取在X,Y落子带来的总收益(带风险偏好)
        //protected abstract int evalXYTRoundScore(List<List<Role>> currentBoard, int x,int y, Role role);
        ////计算 role 视角下在X,Y落子获得的攻击分数
        //protected abstract int calculateXY(List<List<Role>> currentBoard, int x,int y, Role role);


    }
}
