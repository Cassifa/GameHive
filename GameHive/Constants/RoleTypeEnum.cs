namespace GameHive.Constants.RoleTypeEnum {
    public enum Role { 
        AI,
        Player
    }
    public static class RoleExtensions {
        public static string GetChineseName(this Role game) {
            return game switch {
                Role.AI => "AI",
                Role.Player => "玩家",
                _ => "未知"
            };
        }

        public static string GetEnglishName(this Role game) {
            return game switch {
                Role.AI => "AI",
                Role.Player => "Player",
                _ => "Unknown"
            };
        }
    }
}
