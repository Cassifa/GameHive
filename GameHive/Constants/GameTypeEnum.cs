namespace GameHive.Constants.GameTypeEnum {
    public enum GameType {
        Gobang,
        Gobang88,
        TicTacToe,
        MisereTicTacToe,
        Reversi
    }
    public static class BoardGameExtensions {
        public static string GetChineseName(this GameType game) {
            return game switch {
                GameType.Gobang => "五子棋",
                GameType.Gobang88 => "8*8五子棋",
                GameType.TicTacToe => "井字棋",
                GameType.MisereTicTacToe => "反井字棋",
                GameType.Reversi => "黑白棋",
                _ => "未知"
            };
        }

        public static string GetEnglishName(this GameType game) {
            return game switch {
                GameType.Gobang => "Gobang",
                GameType.Gobang88 => "8*8Gobang",
                GameType.TicTacToe => "Tic-Tac-Toe",
                GameType.MisereTicTacToe => "Misère Tic-Tac-Toe",
                GameType.Reversi => "Reversi",
                _ => "Unknown"
            };
        }
    }
}
