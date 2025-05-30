﻿/*************************************************************************************
 * 文 件 名:   MisereTicTacToeMCTS.cs
 * 描    述: 蒙塔卡洛搜索反井字棋产品实例
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:37
*************************************************************************************/
using GameHive.Constants.DifficultyLevelEnum;
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.GameInfo;

namespace GameHive.Model.AIFactory.ConcreteProduct {
    internal class MisereTicTacToeMCTS : MCTS {
        //具体产品信息 包含难度
        public static ConcreteProductInfo concreteProductInfo = new ConcreteProductInfo(2);
        public MisereTicTacToeMCTS(int column, DifficultyLevel level) {
            TotalPiecesCnt = column;
            switch (level) {
                case DifficultyLevel.LEVEL_1:
                    baseCount = 3;
                    MinSearchCount = 3;
                    RunBackPropagateMinMax = false;
                    NeedUpdateSearchCount = false;
                    MultiThreadExecutionEnabled = false;
                    break;
                case DifficultyLevel.LEVEL_2:
                    baseCount = 200;
                    RunBackPropagateMinMax = false;
                    NeedUpdateSearchCount = true;
                    MultiThreadExecutionEnabled = true;
                    break;
            }
        }
        /*****实现两个策略*****/
        //根据某次落子查看游戏是否结束
        public override Role CheckGameOverByPiece(List<List<Role>> currentBoard, int x, int y) {
            if (currentBoard[x][0] != Role.Empty &&
                currentBoard[x][0] == currentBoard[x][1] &&
                currentBoard[x][1] == currentBoard[x][2]) {
                return currentBoard[x][0] == Role.AI ? Role.Player : Role.AI;
            }
            if (currentBoard[0][y] != Role.Empty &&
                currentBoard[0][y] == currentBoard[1][y] &&
                currentBoard[1][y] == currentBoard[2][y]) {
                return currentBoard[0][y] == Role.AI ? Role.Player : Role.AI;
            }
            // 检查主对角线
            if (x == y && currentBoard[0][0] != Role.Empty &&
                currentBoard[0][0] == currentBoard[1][1] &&
                currentBoard[1][1] == currentBoard[2][2]) {
                return currentBoard[0][0] == Role.AI ? Role.Player : Role.AI;
            }
            // 检查副对角线
            if (x + y == TotalPiecesCnt - 1 && currentBoard[0][2] != Role.Empty &&
                currentBoard[0][2] == currentBoard[1][1] &&
                currentBoard[1][1] == currentBoard[2][0]) {
                return currentBoard[0][2] == Role.AI ? Role.Player : Role.AI;
            }
            for (int i = 0; i < TotalPiecesCnt; i++)
                for (int j = 0; j < TotalPiecesCnt; j++)
                    if (currentBoard[i][j] == Role.Empty)
                        return Role.Empty;
            return Role.Draw;
        }


        protected override Role CheckGameOverByPieceWithCache(List<List<Role>> currentBoard, int x, int y) {
            return CheckGameOverByPiece(currentBoard, x, y);
        }
        //获取所有可落子点
        protected override List<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board) {
            List<Tuple<int, int>> ans = new List<Tuple<int, int>>();
            for (int i = 0; i < board.Count; i++)
                for (int j = 0; j < board[i].Count; j++)
                    if (board[i][j] == Role.Empty) ans.Add(new Tuple<int, int>(i, j));
            return ans;
        }

    }
}
