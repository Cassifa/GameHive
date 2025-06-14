﻿/*************************************************************************************
* 文 件 名:   MisereTicTacToeMCTS.cs
* 描    述: 蒙塔卡洛搜索不围棋产品实例
* 版    本：  V2.0 .NET客户端初版
* 创 建 者：  Cassifa
* 创建时间：  2024/11/26 18:38
*************************************************************************************/
using GameHive.Constants.DifficultyLevelEnum;
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.GameInfo;
namespace GameHive.Model.AIFactory.ConcreteProduct.AntiGo {
    internal class AntiGoMCTS : MCTS {
        //具体产品信息 包含难度
        public static ConcreteProductInfo concreteProductInfo = new ConcreteProductInfo(4);
        public AntiGoMCTS(int Column, DifficultyLevel level) {
            TotalPiecesCnt = Column;
            switch (level) {
                case DifficultyLevel.LEVEL_1:
                    MultiThreadExecutionEnabled = false;
                    RunBackPropagateMinMax = false;
                    NeedUpdateSearchCount = false;
                    baseCount = 3_000;
                    break;
                case DifficultyLevel.LEVEL_2://开启多线程搜索与动态搜索次数
                    MultiThreadExecutionEnabled = true;
                    RunBackPropagateMinMax = false;
                    NeedUpdateSearchCount = true;
                    baseCount = 5_000;
                    break;
                case DifficultyLevel.LEVEL_3://开启反向传播MinMax
                    MultiThreadExecutionEnabled = true;
                    RunBackPropagateMinMax = true;
                    NeedUpdateSearchCount = true;
                    baseCount = 10_000;
                    break;
                case DifficultyLevel.LEVEL_4://提高搜索次数
                    MultiThreadExecutionEnabled = true;
                    RunBackPropagateMinMax = true;
                    NeedUpdateSearchCount = true;
                    baseCount = 15_000;
                    break;
            }
        }
        private int TotalAttempts = 0;       // 总数
        private int SuccessCount = 0;         // 命中次数
        private double SuccessRate = 0;       // 命中比例
        //private List<int> TotalAttemptsHistory = new List<int>();  // 每次的尝试次数
        //private List<int> SuccessCountHistory = new List<int>();    // 每次的命中次数
        //private List<double> SuccessRateHistory = new List<double>(); // 每次的命中率
        /*****实现三个策略*****/
        //启用Cache判断胜负
        protected override Role CheckGameOverByPieceWithCache(List<List<Role>> currentBoard, int x, int y) {
            TotalAttempts++;
            Role result = Role.Empty;
            //先查缓存
            if (GameOverStatusCache.GetValue(ref result) != -1) {
                SuccessCount++;
                return result;
            }
            SuccessRate = SuccessCount / (double)TotalAttempts;
            //记录缓存
            result = CheckGameOverByPiece(currentBoard, x, y);
            GameOverStatusCache.Log(result);
            return result;
        }

        //根据某次落子查看游戏是否结束，导致出现无气局面的人判负，对手判胜
        public override Role CheckGameOverByPiece(List<List<Role>> currentBoard, int x, int y) {
            int[] dx = { 0, -1, 1, 0, 0 };
            int[] dy = { 0, 0, 0, -1, 1 };
            List<List<bool>> visitStatusBoard = new List<List<bool>>();
            for (int i = 0; i < currentBoard.Count; i++)
                visitStatusBoard.Add(new List<bool>(new bool[currentBoard[i].Count]));
            for (int i = 0; i < 5; i++) {
                int newX = x + dx[i];
                int newY = y + dy[i];
                if (newX == TotalPiecesCnt || newY == TotalPiecesCnt || newX < 0 || newY < 0
                    || currentBoard[newX][newY] == Role.Empty)
                    continue;
                //要清除访问状态 否则会被访问过的堵住
                Clear(ref visitStatusBoard);
                //如果导致某个位置无气说明此步非法
                if (!IsAlive(currentBoard, visitStatusBoard, newX, newY)) {
                    return currentBoard[x][y] == Role.AI ? Role.Player : Role.AI;
                }
            }
            return Role.Empty;
        }
        //重置状态
        private void Clear(ref List<List<bool>> bools) {
            for (int i = 0; i < bools.Count; i++)
                for (int j = 0; j < bools[i].Count; j++)
                    bools[i][j] = false;
        }

        //检测一个点所在连通块是否有气 搜过一块后会把所在连通块标记，不需要重复搜索
        private bool IsAlive(List<List<Role>> rawBoard, List<List<bool>> visitStatusBoard, int x, int y) {
            //标记这个节点已经搜过
            visitStatusBoard[x][y] = true;
            // 上 下 左 右
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };
            for (int i = 0; i < 4; i++) {
                int newX = x + dx[i];
                int newY = y + dy[i];
                if (newX == TotalPiecesCnt || newY == TotalPiecesCnt || newX < 0 || newY < 0)
                    continue;
                //当前字旁边有气
                if (rawBoard[newX][newY] == Role.Empty)
                    return true;
                //搜到同种且未被搜过节点 如果这个节点能搜到气则本连通块有气
                if (rawBoard[newX][newY] == rawBoard[x][y] && !visitStatusBoard[newX][newY]
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
