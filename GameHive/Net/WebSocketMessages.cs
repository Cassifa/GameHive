/*************************************************************************************
 * 文 件 名:   WebSocketMessages.cs
 * 描    述: 
 * 版    本：  V3.0 引入DRL、MinMax-MCTS混合算法
 * 创 建 者：  Cassifa
 * 创建时间：  2025/5/26 2:32
*************************************************************************************/
using GameHive.Net.Constants;
using System.Text.Json.Serialization;

namespace GameHive.Net {
    //客户端发送给服务器的消息对象
    public class GameMessage {
        [JsonPropertyName("event")]
        public ClientEventType Event { get; set; }

        [JsonPropertyName("gameType")]
        public string GameType { get; set; }

        [JsonPropertyName("playWithLMM")]
        public bool PlayWithLMM { get; set; }

        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("userId")]
        public int UserId { get; set; }

        [JsonPropertyName("userName")]
        public string UserName { get; set; }

        //用于JSON序列化
        [JsonIgnore]
        public string EventString {
            get => Event.GetTypeString();
            set => Event = ClientEventTypeExtensions.FromType(value);
        }
    }

    //服务器返回给客户端的消息对象
    public class GameResponse {
        [JsonPropertyName("event")]
        public ServerEventType Event { get; set; }

        [JsonPropertyName("isFirst")]
        public bool IsFirst { get; set; }

        [JsonPropertyName("winStatus")]
        public string WinStatus { get; set; }

        [JsonPropertyName("x")]
        public int X { get; set; }

        [JsonPropertyName("y")]
        public int Y { get; set; }

        [JsonPropertyName("opponentId")]
        public long OpponentId { get; set; }

        [JsonPropertyName("opponentName")]
        public string OpponentName { get; set; }

        [JsonPropertyName("gameStatus")]
        public string GameStatus { get; set; }

        //用于JSON序列化
        [JsonIgnore]
        public string EventString {
            get => Event.GetTypeString();
            set => Event = ServerEventTypeExtensions.FromType(value);
        }
    }
}

