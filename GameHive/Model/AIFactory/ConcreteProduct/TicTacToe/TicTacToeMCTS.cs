/*************************************************************************************
 * 文 件 名:   MisereTicTacToeMCTS.cs
 * 描    述: 蒙塔卡洛搜索井字棋产品实例
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:38
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.AIUtils.MonteCarloTreeSearch;

namespace GameHive.Model.AIFactory.ConcreteProduct {
    internal class TicTacToeMCTS : MCTS {
        private int TotalPiecesCnt;
        public TicTacToeMCTS() {
            TotalPiecesCnt = 3;
            currentBoard = new List<List<Role>>(TotalPiecesCnt);
        }
        /*****实现两个策略*****/
        public override Role CheckGameOver(List<List<Role>> currentBoard) {
            int boardSize = currentBoard.Count;
            // 检查行
            for (int i = 0; i < boardSize; i++) {
                if (currentBoard[i][0] != Role.Empty &&
                    currentBoard[i][0] == currentBoard[i][1] &&
                    currentBoard[i][1] == currentBoard[i][2]) {
                    return currentBoard[i][0];
                }
            }
            // 检查列
            for (int j = 0; j < boardSize; j++) {
                if (currentBoard[0][j] != Role.Empty &&
                    currentBoard[0][j] == currentBoard[1][j] &&
                    currentBoard[1][j] == currentBoard[2][j]) {
                    return currentBoard[0][j];
                }
            }
            // 检查主对角线
            if (currentBoard[0][0] != Role.Empty &&
                currentBoard[0][0] == currentBoard[1][1] &&
                currentBoard[1][1] == currentBoard[2][2]) {
                return currentBoard[0][0];
            }
            // 检查副对角线
            if (currentBoard[0][2] != Role.Empty &&
                currentBoard[0][2] == currentBoard[1][1] &&
                currentBoard[1][1] == currentBoard[2][0]) {
                return currentBoard[0][2];
            }
            //return PlayedPiecesCnt == TotalPiecesCnt * TotalPiecesCnt ? Role.Draw : Role.Empty;
            for (int i = 0; i < boardSize; i++)
                for (int j = 0; j < boardSize; j++)
                    if (currentBoard[i][j] == Role.Empty)
                        return Role.Empty;
            return Role.Draw;
        }

        //根据某次落子查看游戏是否结束
        public override Role CheckGameOverByPiece(List<List<Role>> currentBoard, int x, int y) {
            if (currentBoard[x][0] != Role.Empty &&
                currentBoard[x][0] == currentBoard[x][1] &&
                currentBoard[x][1] == currentBoard[x][2]) {
                return currentBoard[x][0];
            }
            if (currentBoard[0][y] != Role.Empty &&
                currentBoard[0][y] == currentBoard[1][y] &&
                currentBoard[1][y] == currentBoard[2][y]) {
                return currentBoard[0][y];
            }
            // 检查主对角线
            if (x == y && currentBoard[0][0] != Role.Empty &&
                currentBoard[0][0] == currentBoard[1][1] &&
                currentBoard[1][1] == currentBoard[2][2]) {
                return currentBoard[0][0];
            }
            // 检查副对角线
            if (x + y == TotalPiecesCnt-1 && currentBoard[0][2] != Role.Empty &&
                currentBoard[0][2] == currentBoard[1][1] &&
                currentBoard[1][1] == currentBoard[2][0]) {
                return currentBoard[0][2];
            }
            for (int i = 0; i < TotalPiecesCnt; i++)
                for (int j = 0; j < TotalPiecesCnt; j++)
                    if (currentBoard[i][j] == Role.Empty)
                        return Role.Empty;
            return Role.Draw;
        }

        //开始游戏，启动蒙特卡洛搜索线程，后台搜索
        public override void GameStart(bool IsAIFirst) {
            //初始化棋盘
            for (int i = 0; i < TotalPiecesCnt; i++)
                currentBoard.Add(new List<Role>(new Role[TotalPiecesCnt]));
            for (int i = 0; i < TotalPiecesCnt; i++)
                for (int j = 0; j < TotalPiecesCnt; j++)
                    currentBoard[i][j] = Role.Empty;
            //构造根节点
            Role role;
            if (IsAIFirst) role = Role.Player;
            else role = Role.AI;
            RootNode = new MCTSNode(currentBoard, null, -1, -1, role, Role.Empty,GetAvailableMoves(currentBoard));
        }

        //获取可行落子点位
        protected override List<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board) {
            List<Tuple<int, int>> ans = new List<Tuple<int, int>>();
            for (int i = 0; i < board.Count; i++)
                for (int j = 0; j < board[i].Count; j++)
                    if (board[i][j] == Role.Empty) ans.Add(new Tuple<int, int>(i, j));
            return ans;
        }
    }
}
