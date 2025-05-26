/*************************************************************************************
 * 文 件 名:   GameSession.cs
 * 描    述: 游戏会话管理类，负责处理在线游戏的所有网络相关操作
 * 版    本：  V3.0 引入DRL、MinMax-MCTS混合算法
 * 创 建 者：  Cassifa
 * 创建时间：  2025/5/25 23:39
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Net.Constants;

namespace GameHive.Net {
    public class GameSession {
        private readonly WebSocketClient webSocketClient;
        private bool isMyTurn;

        //游戏开始事件
        public event EventHandler<GameStartEventArgs> OnGameStart;
        //对手落子事件
        public event EventHandler<OpponentMoveEventArgs> OnOpponentMove;
        //游戏结束事件
        public event EventHandler<GameResultEventArgs> OnGameResult;
        //错误事件
        public event EventHandler<Exception> OnError;

        //是否轮到自己落子
        public bool IsMyTurn => isMyTurn;

        //构造函数
        public GameSession(string baseUrl, string userId) {
            webSocketClient = new WebSocketClient(baseUrl, userId);
            webSocketClient.OnMessageReceived += HandleWebSocketMessage;
            webSocketClient.OnError += (s, e) => OnError?.Invoke(this, e);
        }

        //开始游戏会话
        public async Task StartSessionAsync(string gameType, bool playerWittLMM) {
            try {
                await webSocketClient.ConnectAsync();
                var startMessage = new GameMessage {
                    Event = ClientEventType.Start,
                    GameType = gameType,
                    PlayWithLMM = playerWittLMM
                };
                await webSocketClient.SendMessageAsync(startMessage);
            } catch (Exception ex) {
                throw;
            }
        }

        //结束游戏会话
        public async Task EndSessionAsync() {
            try {
                var endMessage = new GameMessage {
                    Event = ClientEventType.Stop
                };
                await webSocketClient.SendMessageAsync(endMessage);
                await webSocketClient.DisconnectAsync();
            } catch (Exception ex) {
                throw;
            }
        }

        //发送落子消息
        public async Task SendMoveAsync(int x, int y) {
            try {
                var moveMessage = new GameMessage {
                    Event = ClientEventType.Move,
                    X = x,
                    Y = y
                };
                await webSocketClient.SendMessageAsync(moveMessage);
                isMyTurn = false;
            } catch (Exception ex) {
                throw;
            }
        }

        //处理WebSocket消息
        private void HandleWebSocketMessage(object sender, GameResponse response) {
            // 将字符串类型的事件转换为枚举
            ServerEventType eventType = ServerEventTypeExtensions.FromType(response.Event);

            switch (eventType) {
                case ServerEventType.Start:
                    HandleGameStart(response);
                    break;
                case ServerEventType.Move:
                    if (response.X.HasValue && response.Y.HasValue) {
                        HandleOpponentMove(response);
                    }
                    break;
                case ServerEventType.Result:
                    if (!string.IsNullOrEmpty(response.WinStatus)) {
                        HandleGameResult(response);
                    }
                    break;
            }
        }

        //处理游戏开始消息
        private void HandleGameStart(GameResponse response) {
            if (response.IsFirst != null)
                isMyTurn = (bool)response.IsFirst;
            OnGameStart?.Invoke(this, new GameStartEventArgs {
                IsFirst = isMyTurn
            });
        }

        //处理对手落子消息
        private void HandleOpponentMove(GameResponse response) {
            OnOpponentMove?.Invoke(this, new OpponentMoveEventArgs {
                X = response.X.Value,
                Y = response.Y.Value
            });
            isMyTurn = true;
        }

        //处理游戏结束消息
        private void HandleGameResult(GameResponse response) {
            GameStatus status = GameStatusExtensions.FromName(response.WinStatus);
            Role winner = status switch {
                GameStatus.PlayerAWin => Role.Player,
                GameStatus.PlayerBWin => Role.AI,
                GameStatus.Draw => Role.Empty,
                _ => Role.Empty
            };

            OnGameResult?.Invoke(this, new GameResultEventArgs {
                Winner = winner
            });
        }
    }

    //游戏开始事件参数
    public class GameStartEventArgs : EventArgs {
        //是否先手
        public bool IsFirst { get; set; }
    }

    //对手落子事件参数
    public class OpponentMoveEventArgs : EventArgs {
        //落子X坐标
        public int X { get; set; }
        //落子Y坐标
        public int Y { get; set; }
    }

    //游戏结束事件参数
    public class GameResultEventArgs : EventArgs {
        //获胜方
        public Role Winner { get; set; }
    }
}

