/*************************************************************************************
 * 文 件 名:   GameBoardInfo.cs
 * 描    述: 
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/25 22:26
*************************************************************************************/
namespace GameHive.Model {
    internal class GameBoardInfo {
        public GameBoardInfo(int cloumn, bool isCenter, List<GameBoardInfo> info) {
            boardInfo = info;
            column = column;
            this.isCenter = isCenter;
        }
        //行列数
        private int column { get; set; }
        //标号是否居中
        private bool isCenter { get; set; }
        //可用的AI列表
        private List<GameBoardInfo> boardInfo { get; set; }
    }
}
