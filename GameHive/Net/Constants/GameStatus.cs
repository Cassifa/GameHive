/*************************************************************************************
 * 文 件 名:   GameStatus.cs
 * 描    述: 游戏胜利状态枚举
 * 版    本：  V3.0 引入DRL、MinMax-MCTS混合算法
 * 创 建 者：  Cassifa
 * 创建时间：  2025/5/25 23:34
*************************************************************************************/
namespace GameHive.Net.Constants {
    //游戏胜利状态
    public enum GameStatus {
        //玩家A获胜
        PlayerAWin = 0,
        //玩家B获胜
        PlayerBWin = 1,
        //游戏平局
        Draw = 2,
        //游戏未结束
        Unfinished = 3
    }

    //游戏状态扩展方法
    public static class GameStatusExtensions {
        //获取状态名称
        public static string GetName(this GameStatus status) {
            return status switch {
                GameStatus.PlayerAWin => "aWin",
                GameStatus.PlayerBWin => "bWin",
                GameStatus.Draw => "draw",
                GameStatus.Unfinished => "unfinished",
                _ => string.Empty
            };
        }

        //从代码获取枚举值
        public static GameStatus FromCode(int code) {
            return code switch {
                0 => GameStatus.PlayerAWin,
                1 => GameStatus.PlayerBWin,
                2 => GameStatus.Draw,
                3 => GameStatus.Unfinished,
                _ => throw new ArgumentException($"Invalid code: {code}")
            };
        }

        //从名称获取枚举值
        public static GameStatus FromName(string name) {
            return name switch {
                "aWin" => GameStatus.PlayerAWin,
                "bWin" => GameStatus.PlayerBWin,
                "draw" => GameStatus.Draw,
                "unfinished" => GameStatus.Unfinished,
                _ => throw new ArgumentException($"Invalid name: {name}")
            };
        }
    }
}

