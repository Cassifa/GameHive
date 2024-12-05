/*************************************************************************************
 * 文 件 名:   GoBang88DRL.cs
 * 描    述: 深度强化学习8*8五子棋产品实例
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:36
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;

namespace GameHive.Model.AIFactory.ConcreteProduct {
    internal class GoBang88DRL : DRL {
        /*****实现两个策略*****/
        public override Role CheckGameOver(List<List<Role>> currentBoard) {
            if (currentBoard[0][0] != Role.Empty) {
                return currentBoard[0][0];
            }
            return Role.Empty;
        }
        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard) {
            Random rand = new Random();
            // 获取棋盘大小
            int rowCount = currentBoard.Count;
            int colCount = currentBoard[0].Count;
            // 收集所有不为 Empty 的点
            List<Tuple<int, int>> availableMoves = new List<Tuple<int, int>>();

            for (int i = 0; i < rowCount; i++) {
                for (int j = 0; j < colCount; j++) {
                    if (currentBoard[i][j] != Role.Empty) {
                        availableMoves.Add(new Tuple<int, int>(i, j));
                    }
                }
            }
            int randomIndex = rand.Next(availableMoves.Count);
            return availableMoves[randomIndex];
        }
    }
}
