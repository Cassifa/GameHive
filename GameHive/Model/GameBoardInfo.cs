/*************************************************************************************
 * 文 件 名:   GameBoardInfo.cs
 * 描    述: 某种棋盘的特质，包括行列数、是否在网格落子、可用AI列表，不随具体游戏而变化的属性
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/25 22:26
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;

namespace GameHive.Model {
    internal class GameBoardInfo {
        public GameBoardInfo(int column, bool isCenter, List<AIAlgorithmType> allAIType) {
            this.AllAIType = allAIType;
            this.Column = column;
            this.IsCenter = isCenter;
            CaclateCenter();
        }
        //行列数
        public int Column { get; set; }
        //标号是否居中
        public bool IsCenter { get; set; }
        //可落子半径
        public double R {  get; set; }
        //每个棋子可落子中心点
        public List<List<Tuple<double,double>>> ChessCenter { get; set; }
        //可用的AI列表
        public List<AIAlgorithmType> AllAIType { get; set; }
        private void CaclateCenter() { }
    }
}
