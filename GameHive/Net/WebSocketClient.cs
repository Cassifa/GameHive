/*************************************************************************************
 * 文 件 名:   WebSocketClient.cs
 * 描    述: 
 * 版    本：  V3.0 引入DRL、MinMax-MCTS混合算法
 * 创 建 者：  Cassifa
 * 创建时间：  2025/5/25 23:31
*************************************************************************************/
using System.Diagnostics;
using System.Net.WebSockets;
using System.Text;
using System.Text.Json;

namespace GameHive.Net {
    //WebSocket客户端，负责与服务器的WebSocket通信
    public class WebSocketClient {
        private ClientWebSocket webSocket;
        private bool isConnected = false;
        private CancellationTokenSource webSocketCts;
        private readonly string baseUrl;
        private readonly string userId;

        //收到服务器消息时触发
        public event EventHandler<GameResponse> OnMessageReceived;
        //连接成功时触发
        public event EventHandler OnConnected;
        //连接断开时触发
        public event EventHandler OnDisconnected;
        //发生错误时触发
        public event EventHandler<Exception> OnError;

        //是否已连接
        public bool IsConnected => isConnected;

        //构造函数
        //@param baseUrl WebSocket服务器基础URL
        //@param userId 用户ID
        public WebSocketClient(string baseUrl, string userId) {
            this.baseUrl = baseUrl;
            this.userId = userId;
        }

        //连接到WebSocket服务器
        public async Task ConnectAsync() {
            if (isConnected) {
                return;
            }

            try {
                webSocket = new ClientWebSocket();
                webSocketCts = new CancellationTokenSource();

                string wsUrl = $"{baseUrl.TrimEnd('/')}/websocket/{userId}";
                await webSocket.ConnectAsync(new Uri(wsUrl), webSocketCts.Token);

                isConnected = true;
                OnConnected?.Invoke(this, EventArgs.Empty);

                _ = ReceiveMessagesAsync();
            } catch (WebSocketException wsEx) {
                isConnected = false;
                OnError?.Invoke(this, wsEx);
                throw;
            } catch (UriFormatException uriEx) {
                isConnected = false;
                OnError?.Invoke(this, uriEx);
                throw;
            } catch (Exception ex) {
                isConnected = false;
                OnError?.Invoke(this, ex);
                throw;
            }
        }

        //断开WebSocket连接
        public async Task DisconnectAsync() {
            if (!isConnected) {
                return;
            }

            try {
                webSocketCts?.Cancel();
                if (webSocket?.State == WebSocketState.Open) {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Game ended", CancellationToken.None);
                }
            } catch (Exception ex) {
                OnError?.Invoke(this, ex);
            } finally {
                isConnected = false;
                webSocket?.Dispose();
                webSocket = null;
                webSocketCts?.Dispose();
                webSocketCts = null;
                OnDisconnected?.Invoke(this, EventArgs.Empty);
            }
        }

        //接收WebSocket消息的循环
        private async Task ReceiveMessagesAsync() {
            var buffer = new byte[4096];
            try {
                while (isConnected && webSocket?.State == WebSocketState.Open) {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), webSocketCts.Token);
                    if (result.MessageType == WebSocketMessageType.Close) {
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    HandleMessage(message);
                }
            } catch (OperationCanceledException) {
                // 忽略取消操作
            } catch (Exception ex) {
                OnError?.Invoke(this, ex);
            }
        }

        //处理接收到的WebSocket消息
        private void HandleMessage(string message) {
            try {
                var response = JsonSerializer.Deserialize<GameResponse>(message);
                if (response != null) {
                    OnMessageReceived?.Invoke(this, response);
                }
            } catch (JsonException jsonEx) {
                OnError?.Invoke(this, jsonEx);
            } catch (Exception ex) {
                OnError?.Invoke(this, ex);
            }
        }

        //发送消息到WebSocket服务器
        public async Task SendMessageAsync(GameMessage message) {
            if (!isConnected || webSocket?.State != WebSocketState.Open) {
                return;
            }

            try {
                var json = JsonSerializer.Serialize(message);
                var bytes = Encoding.UTF8.GetBytes(json);
                await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, webSocketCts.Token);
            } catch (Exception ex) {
                OnError?.Invoke(this, ex);
            }
        }
    }
}
