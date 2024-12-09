/*************************************************************************************
 * 文 件 名:   MisereTicTacToeMCTS.cs
 * 描    述: 蒙塔卡洛搜索不围棋产品实例
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:38
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;

namespace GameHive.Model.AIFactory.ConcreteProduct {
    internal class AntiGoMCTS : MCTS {
        private int TotalPiecesCnt;
        public AntiGoMCTS() {
            TotalPiecesCnt = 7;
            SearchCount = 1000;
            currentBoard = new List<List<Role>>(TotalPiecesCnt);
        }
        /*****实现三个策略*****/
        //根据某次落子查看游戏是否结束
        public override Role CheckGameOverByPiece(List<List<Role>> currentBoard, int x, int y) {
            //原棋盘备份，对其做修改表示已经搜过了
            var boardCopy = currentBoard.Select(row => new List<Role>(row)).ToList();
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };
            for (int i = 0; i < 4; i++) {
                int newX = x + dx[i];
                int newY = y + dy[i];
                if (newX == TotalPiecesCnt || newY == TotalPiecesCnt || newX < 0 || newY < 0)
                    continue;
                //这个节点不为空（搜过的认为等价空） 并且搜索发现对应连通块没有气
                if (boardCopy[newX][newY] != Role.Empty && !IsAlive(currentBoard, boardCopy, newX, newY))
                    return currentBoard[x][y] == Role.AI ? Role.Player : Role.AI;
            }
            return Role.Empty;
        }

        //检测一个点所在连通块是否有气 搜过一块后会把所在连通块值为空，不需要重复搜索
        private bool IsAlive(List<List<Role>> rawBoard, List<List<Role>> visitStatusBoard, int x, int y) {
            //标记这个节点已经搜过
            visitStatusBoard[x][y] = Role.Empty;
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };
            for (int i = 0; i < 4; i++) {
                int newX = x + dx[i];
                int newY = y + dy[i];
                if (newX == TotalPiecesCnt || newY == TotalPiecesCnt || newX < 0 || newY < 0)
                    continue;
                //搜到一个有气的
                if (rawBoard[newX][newY] == Role.Empty) return true;
                //搜到同种且未被搜过节点 如果这个节点能搜到气则本连通块有气
                if (visitStatusBoard[newX][newY] == rawBoard[x][y]
                    && IsAlive(rawBoard, visitStatusBoard, newX, newY))
                    return true;
            }
            return false;
        }

        //获取所有可行移动点位，如果没有可行那么返回一个会导致失败的点位
        protected override List<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board) {
            List<Tuple<int, int>> emptyMoves = new List<Tuple<int, int>>();
            List<Tuple<int, int>> availableMoves = new List<Tuple<int, int>>();
            for (int i = 0; i < TotalPiecesCnt; i++)
                for (int j = 0; j < TotalPiecesCnt; j++) {
                    if (board[i][j] == Role.Empty) {
                        emptyMoves.Add(new Tuple<int, int>(i, j));
                    }
                }
            foreach (var t in emptyMoves)
                if (CheckGameOverByPiece(board, t.Item1, t.Item2) == Role.Empty)
                    availableMoves.Add(t);
            if (availableMoves.Count == 0)
                availableMoves.Add(emptyMoves[0]);
            return availableMoves;
        }

    }
}
