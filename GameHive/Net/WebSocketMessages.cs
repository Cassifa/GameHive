/*************************************************************************************
 * 文 件 名:   WebSocketMessages.cs
 * 描    述: 
 * 版    本：  V3.0 引入DRL、MinMax-MCTS混合算法
 * 创 建 者：  Cassifa
 * 创建时间：  2025/5/25 23:32
*************************************************************************************/
using GameHive.Net.Constants;

namespace GameHive.Net {
    //客户端发送给服务器的消息对象
    public class GameMessage {
        //消息类型：START-开始游戏，MOVE-落子，END-结束游戏
        public ClientEventType Event { get; set; }

        //游戏类型，仅在START事件中使用
        public string GameType { get; set; }

        //落子X坐标，仅在MOVE事件中使用
        public int X { get; set; }

        //落子Y坐标，仅在MOVE事件中使用
        public int Y { get; set; }

        //转换为JSON字符串时使用
        public string EventString {
            get => Event.GetTypeString();
            set => Event = ClientEventTypeExtensions.FromType(value);
        }
    }

    //服务器返回给客户端的消息对象
    public class GameResponse {
        //消息类型：START-游戏开始，MOVE-对手落子，RESULT-游戏结束
        public ServerEventType Event { get; set; }

        //对手ID，仅在START事件中使用
        public long OpponentId { get; set; }

        //对手名称，仅在START事件中使用
        public string OpponentName { get; set; }

        //是否先手，仅在START事件中使用
        public bool First { get; set; }

        //落子X坐标，仅在MOVE事件中使用
        public int X { get; set; }

        //落子Y坐标，仅在MOVE事件中使用
        public int Y { get; set; }

        //游戏结果，仅在RESULT事件中使用
        public string WinStatus { get; set; }

        //转换为JSON字符串时使用
        public string EventString {
            get => Event.GetTypeString();
            set => Event = ServerEventTypeExtensions.FromType(value);
        }
    }
}

