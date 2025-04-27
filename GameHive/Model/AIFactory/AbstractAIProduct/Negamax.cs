/*************************************************************************************
 * 文 件 名:   Negamax.cs
 * 描    述: 负极大值博弈树抽象产品
 *          负责完成负极大值搜索逻辑
 *          要求产品实现 1.判断胜负 2.获取可用行动
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:12
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;

namespace GameHive.Model.AIFactory.AbstractAIProduct {
    internal abstract class Negamax : AbstractAIStrategy {
        //获取可下棋点位
        protected abstract List<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board);

        private Tuple<int, int> FinalDecide;
        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard, int lastX, int lastY) {
            EvalToGo(currentBoard, 1);
            return FinalDecide;
        }

        private int EvalToGo(List<List<Role>> currentBoard, int depth) {
            Role currentPlayer = (depth & 1) == 1 ? Role.AI : Role.Player;
            // 检查当前局面的胜负情况
            Role winner = CheckGameOverByPiece(currentBoard, -1, -1);
            if (winner == Role.Draw) return 0;
            if (winner == currentPlayer) return 1_000_000;
            if (winner != Role.Empty) return -1_000_000;

            // 初始化最大分数
            int maxScore = int.MinValue;
            Tuple<int, int>? bestMove = null;

            var availableMoves = GetAvailableMoves(currentBoard);
            foreach (var move in availableMoves) {
                currentBoard[move.Item1][move.Item2] = currentPlayer;
                int score = -EvalToGo(currentBoard, depth + 1);
                if (score > maxScore || (score == maxScore && new Random().Next(2) == 0)) {
                    maxScore = score;
                    bestMove = move;
                }
                currentBoard[move.Item1][move.Item2] = Role.Empty;
            }
            if (bestMove != null) FinalDecide = bestMove;
            return maxScore;
        }



        /*****负极大值博弈树不需要*****/
        //用户下棋
        public override void UserPlayPiece(int lastX, int lastY) {
        }
        //强制游戏结束 停止需要多线程的AI 更新在内部保存过状态的AI
        public override void GameForcedEnd() {
        }
        //游戏开始
        public override void GameStart(bool IsAIFirst) { }
    }
}
