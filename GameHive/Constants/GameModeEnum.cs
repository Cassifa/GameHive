/*************************************************************************************
 * 文 件 名:   GameModeEnum.cs
 * 描    述: 
 * 版    本：  V3.0 引入DRL、MinMax-MCTS混合算法
 * 创 建 者：  Cassifa
 * 创建时间：  2025/5/25 22:14
*************************************************************************************/
namespace GameHive.Constants.GameModeEnum {
    public enum GameMode {
        LocalGame,           // 本地对战
        LMMGame,             // 与大模型对战
        OnlineGame,         // 联机对战
    }

    public static class GameModeExtensions {
        public static string GetChineseName(this GameMode mode) => mode switch {
            GameMode.LocalGame => "本地对战",
            GameMode.LMMGame => "与大模型对战",
            GameMode.OnlineGame => "联机对战",
            _ => "未知模式"
        };

        public static string GetEnglishName(this GameMode mode) => mode switch {
            GameMode.LocalGame => "Local Game",
            GameMode.LMMGame => "AI Game",
            GameMode.OnlineGame => "Online Game",
            _ => "Unknown Mode"
        };
    }
}