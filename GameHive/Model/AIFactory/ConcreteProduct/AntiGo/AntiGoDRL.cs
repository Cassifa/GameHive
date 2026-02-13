/*************************************************************************************
 * 文 件 名:   AntiGoDRL.cs
 * 描    述: 
 * 版    本：  V3.0 重构DRL模块
 * 创 建 者：  Cassifa
 * 创建时间：  2026/2/2 11:16
*************************************************************************************/
using GameHive.Constants.DifficultyLevelEnum;
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.GameInfo;

namespace GameHive.Model.AIFactory.ConcreteProduct.AntiGo {
    internal class AntiGoDRL : DeepRL {
        //具体产品信息 包含难度
        public static ConcreteProductInfo concreteProductInfo = new ConcreteProductInfo(4);

        public AntiGoDRL(int boardSize, DifficultyLevel level) {
            this.boardSize = boardSize;  // 7x7
            concreteProductInfo.TotalPiecesCnt = boardSize;
            exploreFactor = 5.0;

            byte[]? modelBytes=null;

            switch (level) {
                case DifficultyLevel.LEVEL_1: // 纯神经网络评估
                    modelBytes = Properties.Resources.antigo_41000;
                    useMonteCarlo = false;
                    SearchCount = 0;
                    reuseSearchTree = false;
                    break;
                case DifficultyLevel.LEVEL_2: // 基础 MCTS 搜索
                    modelBytes = Properties.Resources.antigo_40000;
                    useMonteCarlo = true;
                    SearchCount = 30000;
                    reuseSearchTree = false;
                    break;
                case DifficultyLevel.LEVEL_3: // 高强度 MCTS 搜索
                    modelBytes = Properties.Resources.antigo_41000;
                    useMonteCarlo = true;
                    SearchCount = 30000;
                    reuseSearchTree = false;
                    break;
                case DifficultyLevel.LEVEL_4: // 超高强度 MCTS 搜索
                    modelBytes = Properties.Resources.antigo_41000;
                    useMonteCarlo = true;
                    SearchCount = 40000;
                    reuseSearchTree = true;
                    break;
                default:
                    useMonteCarlo = false;
                    SearchCount = 0;
                    reuseSearchTree = false;
                    break;
            }

            if (modelBytes == null || modelBytes.Length == 0) {
                throw new InvalidOperationException($"难度级别 {level} 对应的模型加载失败");
            }

            //加载模型
            LoadModel(modelBytes);
        }

        // 判断游戏是否结束（不围棋规则：导致出现无气局面的人判负）
        public override Role CheckGameOverByPiece(List<List<Role>> currentBoard, int x, int y) {
            if (x == -1) return Role.Empty;

            int[] dx = { 0, -1, 1, 0, 0 };
            int[] dy = { 0, 0, 0, -1, 1 };
            List<List<bool>> visitStatusBoard = new List<List<bool>>();
            for (int i = 0; i < currentBoard.Count; i++)
                visitStatusBoard.Add(new List<bool>(new bool[currentBoard[i].Count]));

            for (int i = 0; i < 5; i++) {
                int newX = x + dx[i];
                int newY = y + dy[i];
                if (newX == boardSize || newY == boardSize || newX < 0 || newY < 0
                    || currentBoard[newX][newY] == Role.Empty)
                    continue;

                // 清除访问状态
                Clear(ref visitStatusBoard);

                // 如果导致某个位置无气说明此步非法，下棋者判负
                if (!IsAlive(currentBoard, visitStatusBoard, newX, newY)) {
                    return currentBoard[x][y] == Role.AI ? Role.Player : Role.AI;
                }
            }
            return Role.Empty;
        }

        // 重置访问状态
        private void Clear(ref List<List<bool>> bools) {
            for (int i = 0; i < bools.Count; i++)
                for (int j = 0; j < bools[i].Count; j++)
                    bools[i][j] = false;
        }

        // 检测一个点所在连通块是否有气
        private bool IsAlive(List<List<Role>> rawBoard, List<List<bool>> visitStatusBoard, int x, int y) {
            visitStatusBoard[x][y] = true;
            int[] dx = { -1, 1, 0, 0 };
            int[] dy = { 0, 0, -1, 1 };
            for (int i = 0; i < 4; i++) {
                int newX = x + dx[i];
                int newY = y + dy[i];
                if (newX == boardSize || newY == boardSize || newX < 0 || newY < 0)
                    continue;
                // 当前子旁边有气
                if (rawBoard[newX][newY] == Role.Empty)
                    return true;
                // 搜到同种且未被搜过节点
                if (rawBoard[newX][newY] == rawBoard[x][y] && !visitStatusBoard[newX][newY]
                    && IsAlive(rawBoard, visitStatusBoard, newX, newY))
                    return true;
            }
            return false;
        }

        // 获取可行落子点（排除会导致自己输的位置）
        protected override List<Tuple<int, int>> GetAvailablePositions(List<List<Role>> currentBoard) {
            var emptyPositions = new List<Tuple<int, int>>();
            var availablePositions = new List<Tuple<int, int>>();

            for (int i = 0; i < boardSize; i++) {
                for (int j = 0; j < boardSize; j++) {
                    if (currentBoard[i][j] == Role.Empty) {
                        emptyPositions.Add(new Tuple<int, int>(i, j));
                    }
                }
            }

            // 过滤掉会导致自己输的位置
            foreach (var pos in emptyPositions) {
                if (CheckGameOverByPiece(currentBoard, pos.Item1, pos.Item2) == Role.Empty) {
                    availablePositions.Add(pos);
                }
            }

            // 如果没有安全位置，返回所有空位（被迫选择）
            if (availablePositions.Count == 0 && emptyPositions.Count > 0) {
                availablePositions.Add(emptyPositions[0]);
            }

            return availablePositions;
        }
    }
}
