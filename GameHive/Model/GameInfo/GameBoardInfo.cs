/*************************************************************************************
 * 文 件 名:   GameBoardInfo.cs
 * 描    述: 某种棋盘的特质，包括行列数、是否在网格落子、可用AI列表，不随具体游戏而变化的属性
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/25 22:26
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;

namespace GameHive.Model.GameInfo {
    internal class GameBoardInfo {
        public GameBoardInfo(int column, bool isCenter, List<AIAlgorithmType> allAIType) {
            AllAIType = allAIType;
            Column = column;
            IsCenter = isCenter;
            totalSize = 810;
            CalculateMapping();
        }
        //面板大小
        public double totalSize { get; private set; }
        //可接收点击的半径
        public double InputR { get; private set; }
        //展示棋子半径
        public double showR { get; private set; }
        //行列数
        public int Column { get; set; }
        //标号是否居中
        public bool IsCenter { get; set; }
        //可落子半径
        public double R { get; set; }
        public double Bias { get; private set; }
        //棋盘格线长度
        public double BoardLength { get; private set; }
        //每个棋子可落子中心点
        public List<List<Tuple<double, double>>> ChessCenter { get; private set; }
        //画行线位置
        public List<double> Rows { get; private set; }
        //画纵列位置
        public List<double> Columns { get; private set; }
        //可用的AI列表
        public List<AIAlgorithmType> AllAIType { get; set; }

        private void CalculateMapping() {
            ChessCenter = new List<List<Tuple<double, double>>>();
            Columns = new List<double>();
            Rows = new List<double>();

            // **中心落子逻辑**
            if (IsCenter) {
                int gridCount = Column; // 实际棋盘网格数等于输入 Column
                R = totalSize / (gridCount * 2 + 2);
                Bias = R;
                double cellSize = R * 2;
                BoardLength = totalSize - R;

                for (int i = 0; i < Column; i++) {
                    //外层数组x(i)方向，屏幕y方向
                    List<Tuple<double, double>> rowCenters = new List<Tuple<double, double>>();
                    for (int j = 0; j < Column; j++) {
                        //内层数组y(i)方向，屏幕x方向
                        double x = j * cellSize + cellSize / 2 + R; // 中心点 x 坐标
                        double y = i * cellSize + cellSize / 2 + R; // 中心点 y 坐标
                        rowCenters.Add(new Tuple<double, double>((double)x, (double)y));
                        if (i == 0) {
                            //计算 Columns
                            Columns.Add((double)(j * cellSize + R));
                        }
                    }
                    ChessCenter.Add(rowCenters);
                    //计算Rows
                    Rows.Add((double)(i * cellSize + R));
                }
                Rows.Add((double)(Column * cellSize + R));
                Columns.Add((double)(Column * cellSize + R));
            } else {
                // **交点落子逻辑**
                int gridCount = Column - 1; // 在交点落子时网格数量少一行一列
                R = totalSize / (gridCount * 2 + 4);
                Bias = 2 * R;
                double cellSize = R * 2;
                BoardLength = totalSize - 2 * R;

                for (int i = 0; i < Column; i++) {
                    List<Tuple<double, double>> rowCenters = new List<Tuple<double, double>>();
                    for (int j = 0; j < Column; j++) {
                        double x = j * cellSize + R * 2; // 交点 x 坐标，预留边界
                        double y = i * cellSize + R * 2; // 交点 y 坐标，预留边界
                        rowCenters.Add(new Tuple<double, double>((double)x, (double)y));
                        if (i == 0) {
                            //计算 Columns
                            Columns.Add((double)(j * cellSize + R * 2));
                        }
                    }
                    ChessCenter.Add(rowCenters);
                    //计算Rows
                    Rows.Add((double)(i * cellSize + R * 2));
                }
                //Rows.Add((double)(Column * cellSize + R * 2));
                //Columns.Add((double)(Column * cellSize + R*2));
            }
            InputR = R * 3 / 5;
            showR = R * 5 / 6;
        }

    }
}
