/*************************************************************************************
 * 文 件 名:   GameTypeEnum.cs
 * 描    述: 用于定义支持的游戏类型，并提供中英文名转化
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/25 22:04
*************************************************************************************/
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
