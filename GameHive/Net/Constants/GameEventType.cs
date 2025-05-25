/*************************************************************************************
 * 文 件 名:   GameEventType.cs
 * 描    述: 游戏事件类型枚举
 * 版    本：  V3.0 引入DRL、MinMax-MCTS混合算法
 * 创 建 者：  Cassifa
 * 创建时间：  2025/5/25 23:33
*************************************************************************************/
namespace GameHive.Net.Constants {
    //客户端发送给服务器的事件类型
    public enum ClientEventType {
        //开始匹配
        Start = 0,
        //停止匹配
        Stop = 1,
        //落子
        Move = 2
    }

    //服务器返回给客户端的事件类型
    public enum ServerEventType {
        //对手落子
        Move = 0,
        //游戏结果
        Result = 1,
        //游戏开始
        Start = 2
    }

    //客户端事件类型扩展方法
    public static class ClientEventTypeExtensions {
        //获取事件类型字符串
        public static string GetTypeString(this ClientEventType type) {
            return type switch {
                ClientEventType.Start => "start-matching",
                ClientEventType.Stop => "stop-matching",
                ClientEventType.Move => "move",
                _ => string.Empty
            };
        }

        //从代码获取枚举值
        public static ClientEventType FromCode(int code) {
            return code switch {
                0 => ClientEventType.Start,
                1 => ClientEventType.Stop,
                2 => ClientEventType.Move,
                _ => throw new ArgumentException($"Invalid code: {code}")
            };
        }

        //从类型字符串获取枚举值
        public static ClientEventType FromType(string type) {
            return type switch {
                "start-matching" => ClientEventType.Start,
                "stop-matching" => ClientEventType.Stop,
                "move" => ClientEventType.Move,
                _ => throw new ArgumentException($"Invalid type: {type}")
            };
        }
    }

    //服务器事件类型扩展方法
    public static class ServerEventTypeExtensions {
        //获取事件类型字符串
        public static string GetTypeString(this ServerEventType type) {
            return type switch {
                ServerEventType.Move => "move",
                ServerEventType.Result => "result",
                ServerEventType.Start => "start",
                _ => string.Empty
            };
        }

        //从代码获取枚举值
        public static ServerEventType FromCode(int code) {
            return code switch {
                0 => ServerEventType.Move,
                1 => ServerEventType.Result,
                2 => ServerEventType.Start,
                _ => throw new ArgumentException($"Invalid code: {code}")
            };
        }

        //从类型字符串获取枚举值
        public static ServerEventType FromType(string type) {
            return type switch {
                "move" => ServerEventType.Move,
                "result" => ServerEventType.Result,
                "start" => ServerEventType.Start,
                _ => throw new ArgumentException($"Invalid type: {type}")
            };
        }
    }
}
