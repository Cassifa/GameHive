/*************************************************************************************
 * 文 件 名:   RoleTypeEnum.cs
 * 描    述: 定义角色枚举，有且仅有玩家与AI两类
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 3:41
*************************************************************************************/
namespace GameHive.Constants.RoleTypeEnum {
    public enum Role {
        AI,
        Player,
        Empty,
        Draw,
    }
    public static class RoleExtensions {
        public static string GetChineseName(this Role game) {
            return game switch {
                Role.Empty => "空",
                Role.AI => "AI",
                Role.Player => "玩家",
                Role.Draw => "平局",
                _ => "未知"
            };
        }

        public static string GetEnglishName(this Role game) {
            return game switch {
                Role.Empty => "Empty",
                Role.AI => "AI",
                Role.Player => "Player",
                Role.Draw => "Draw",
                _ => "Unknown"
            };
        }
        public static bool IsVictory(this Role role) {
            return role == Role.AI || role == Role.Player;
        }

        /// <summary>
        /// 将角色转换为后端需要的胜者参数
        /// </summary>
        /// <param name="winner">胜者角色</param>
        /// <param name="first">先手角色</param>
        /// <returns>0-先手赢，1-后手赢，2-平局，3-终止游戏</returns>
        public static int ToWinnerCode(this Role winner, Role first) {
            if (winner == Role.Empty)
                return 3;  // 终止游戏
            if (winner == Role.Draw)
                return 2;   // 平局

            // 判断是否是先手赢
            bool isFirstWin = (first == Role.Player && winner == Role.Player) ||
                            (first == Role.AI && winner == Role.AI);
            return isFirstWin ? 0 : 1;
        }
    }
}
