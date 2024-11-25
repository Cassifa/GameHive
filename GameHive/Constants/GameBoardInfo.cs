namespace GameHive.Constants {
    internal class GameBoardInfo {
        public GameBoardInfo(int cloumn, bool isCenter, List<GameBoardInfo> info) {
            this.boardInfo = info;
            this.column = column;
            this.isCenter = isCenter;
        }
        private int column { get; set; }
        private bool isCenter { get; set; }
        private List<GameBoardInfo> boardInfo { get; set; }
    }
}
