///*************************************************************************************
// * 文 件 名:   GoBang88DRL.cs
// * 描    述: 深度强化学习8*8五子棋产品实例
// * 版    本：  V2.0 .NET客户端初版
// * 创 建 者：  Cassifa
// * 创建时间：  2024/11/26 18:36
//*************************************************************************************/
//using GameHive.Constants.RoleTypeEnum;
//using GameHive.Model.AIFactory.AbstractAIProduct;

//namespace GameHive.Model.AIFactory.ConcreteProduct {
//    internal class GoBang88DRL : MinMaxMCTS {
//        private int TotalPiecesCnt;
//        public GoBang88DRL() {
//            TotalPiecesCnt = 8;
//            ModelPath = "GoBang88DRLModel.onnx";
//        }

//        public override Role CheckGameOverByPiece(List<List<Role>> currentBoard, int x, int y) {
//            Role currentPlayer = currentBoard[x][y];
//            //水平、垂直、主对角线、副对角线
//            int[] dx = { 1, 0, 1, 1 };
//            int[] dy = { 0, 1, 1, -1 };
//            for (int direction = 0; direction < 4; direction++) {
//                int count = 1;
//                // 检查当前方向上的连续棋子，向正方向（dx[direction], dy[direction]）和反方向（-dx[direction], -dy[direction]）扩展
//                for (int step = 1; step <= 4; step++) {
//                    int nx = x + dx[direction] * step;
//                    int ny = y + dy[direction] * step;
//                    if (nx < 0 || ny < 0 || nx >= TotalPiecesCnt || ny >= TotalPiecesCnt || currentBoard[nx][ny] != currentPlayer)
//                        break;
//                    count++;
//                }
//                for (int step = 1; step <= 4; step++) {
//                    int nx = x - dx[direction] * step;
//                    int ny = y - dy[direction] * step;
//                    if (nx < 0 || ny < 0 || nx >= TotalPiecesCnt || ny >= TotalPiecesCnt || currentBoard[nx][ny] != currentPlayer)
//                        break;
//                    count++;
//                }
//                if (count >= 5) return currentPlayer;
//            }
//            return Role.Empty;
//        }

//        //暂时重载这个方法
//        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard, int lastX, int lastY) {
//            List<Tuple<int, int>> emptyPoints = new List<Tuple<int, int>>();
//            for (int i = 0; i < currentBoard.Count; i++)
//                for (int j = 0; j < currentBoard[i].Count; j++)
//                    if (currentBoard[i][j] == Role.Empty)
//                        emptyPoints.Add(Tuple.Create(i, j));
//            Random random = new Random();
//            int randomIndex = random.Next(emptyPoints.Count);
//            return emptyPoints[randomIndex];
//        }

//    }
//}
