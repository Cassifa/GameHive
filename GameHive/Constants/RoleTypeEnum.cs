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
    }
}
