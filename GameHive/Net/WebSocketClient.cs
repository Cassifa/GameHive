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
            Debug.WriteLine($"[WebSocket] 初始化客户端 - URL: {baseUrl}, 用户ID: {userId}");
        }

        //连接到WebSocket服务器
        public async Task ConnectAsync() {
            if (isConnected) {
                Debug.WriteLine("[WebSocket] 已经连接，跳过连接");
                return;
            }

            try {
                webSocket = new ClientWebSocket();
                webSocketCts = new CancellationTokenSource();

                // 构建完整的 WebSocket URL，添加 /websocket 路径
                string wsUrl = $"{baseUrl.TrimEnd('/')}/websocket/{userId}";
                Debug.WriteLine($"[WebSocket] 尝试连接到: {wsUrl}");

                await webSocket.ConnectAsync(new Uri(wsUrl), webSocketCts.Token);

                isConnected = true;
                Debug.WriteLine("[WebSocket] 连接成功");
                OnConnected?.Invoke(this, EventArgs.Empty);

                //启动消息接收循环
                _ = ReceiveMessagesAsync();
            } catch (WebSocketException wsEx) {
                Debug.WriteLine($"[WebSocket] WebSocket连接失败: {wsEx.Message}, 错误代码: {wsEx.ErrorCode}");
                isConnected = false;
                OnError?.Invoke(this, wsEx);
                throw;
            } catch (UriFormatException uriEx) {
                Debug.WriteLine($"[WebSocket] URL格式错误: {uriEx.Message}");
                isConnected = false;
                OnError?.Invoke(this, uriEx);
                throw;
            } catch (Exception ex) {
                Debug.WriteLine($"[WebSocket] 连接失败: {ex.Message}, 类型: {ex.GetType().Name}");
                isConnected = false;
                OnError?.Invoke(this, ex);
                throw;
            }
        }

        //断开WebSocket连接
        public async Task DisconnectAsync() {
            if (!isConnected) {
                Debug.WriteLine("[WebSocket] 未连接，跳过断开");
                return;
            }

            try {
                webSocketCts?.Cancel();
                if (webSocket?.State == WebSocketState.Open) {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Game ended", CancellationToken.None);
                }
            } catch (Exception ex) {
                Debug.WriteLine($"[WebSocket] 断开连接时发生错误: {ex.Message}, 类型: {ex.GetType().Name}");
                OnError?.Invoke(this, ex);
            } finally {
                isConnected = false;
                webSocket?.Dispose();
                webSocket = null;
                webSocketCts?.Dispose();
                webSocketCts = null;
                OnDisconnected?.Invoke(this, EventArgs.Empty);
                Debug.WriteLine("[WebSocket] 已断开连接");
            }
        }

        //接收WebSocket消息的循环
        private async Task ReceiveMessagesAsync() {
            var buffer = new byte[4096];
            try {
                while (isConnected && webSocket?.State == WebSocketState.Open) {
                    var result = await webSocket.ReceiveAsync(new ArraySegment<byte>(buffer), webSocketCts.Token);
                    if (result.MessageType == WebSocketMessageType.Close) {
                        Debug.WriteLine("[WebSocket] 收到关闭消息");
                        break;
                    }

                    var message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    Debug.WriteLine($"[WebSocket] 收到消息: {message}");
                    HandleMessage(message);
                }
            } catch (OperationCanceledException) {
                Debug.WriteLine("[WebSocket] 消息接收已取消");
            } catch (Exception ex) {
                Debug.WriteLine($"[WebSocket] 接收消息时发生错误: {ex.Message}, 类型: {ex.GetType().Name}");
                OnError?.Invoke(this, ex);
            }
        }

        //处理接收到的WebSocket消息
        private void HandleMessage(string message) {
            try {
                var response = JsonSerializer.Deserialize<GameResponse>(message);
                if (response != null) {
                    Debug.WriteLine($"[WebSocket] 解析消息成功: {response.Event}");
                    OnMessageReceived?.Invoke(this, response);
                } else {
                    Debug.WriteLine("[WebSocket] 解析消息失败: 返回对象为空");
                }
            } catch (JsonException jsonEx) {
                Debug.WriteLine($"[WebSocket] JSON解析错误: {jsonEx.Message}, 原始消息: {message}");
                OnError?.Invoke(this, jsonEx);
            } catch (Exception ex) {
                Debug.WriteLine($"[WebSocket] 处理消息时发生错误: {ex.Message}, 类型: {ex.GetType().Name}");
                OnError?.Invoke(this, ex);
            }
        }

        //发送消息到WebSocket服务器
        public async Task SendMessageAsync(GameMessage message) {
            if (!isConnected || webSocket?.State != WebSocketState.Open) {
                Debug.WriteLine("[WebSocket] 未连接或连接已关闭，无法发送消息");
                return;
            }

            try {
                var json = JsonSerializer.Serialize(message);
                Debug.WriteLine($"[WebSocket] 发送消息: {json}");
                var bytes = Encoding.UTF8.GetBytes(json);
                await webSocket.SendAsync(new ArraySegment<byte>(bytes), WebSocketMessageType.Text, true, webSocketCts.Token);
            } catch (Exception ex) {
                Debug.WriteLine($"[WebSocket] 发送消息时发生错误: {ex.Message}, 类型: {ex.GetType().Name}");
                OnError?.Invoke(this, ex);
            }
        }
    }
}
