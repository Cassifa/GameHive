/*************************************************************************************
 * 文 件 名:   LogView.cs
 * 描    述: 
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/25 19:04
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Constants.GameTypeEnum;
using GameHive.Constants.RoleTypeEnum;
using GameHive.Net;

namespace GameHive.View {
    // 操作记录类
    public class MoveRecord {
        public int Id { get; set; }                 // 操作编号
        public Role Role { get; set; }              // 玩家名称
        public int X { get; set; }                  // 数组X坐标
        public int Y { get; set; }                  // 数组Y坐标

        public MoveRecord(int id, Role role, int x, int y) {
            Id = id;
            Role = role;
            X = x;
            Y = y;
        }
    }

    //控制历史记录条目
    internal partial class View {
        private List<MoveRecord> moveRecords = new List<MoveRecord>();
        private int currentMoveId = 0;
        private string opponentName = "AI"; // 默认对手名称

        public void AddLog(Color color, string logContext) {
            logContext = " " + logContext;
            if (mainForm.LogListBox.InvokeRequired) {
                // 跨线程更新控件
                mainForm.LogListBox.Invoke(new Action(() => {
                    mainForm.LogListBox.Items.Add(new ColoredListItem(logContext, color));
                    mainForm.LogListBox.TopIndex = mainForm.LogListBox.Items.Count - 1;
                }));
            } else {
                mainForm.LogListBox.Items.Add(new ColoredListItem(logContext, color));
                mainForm.LogListBox.TopIndex = mainForm.LogListBox.Items.Count - 1;
            }
        }
        //下棋
        public void LogMove(Role role, int x, int y) {
            //添加记录
            // 根据测试结果修正坐标映射
            // 实际的坐标传入似乎有问题，需要重新映射
            char colChar = (char)('A' + y); // 使用y作为列
            int displayY = boardInfo.Column - x-1; // 使用x作为行

            // 根据角色确定显示名称，并保证格式对齐
            string name;
            if (role == Role.Player) {
                name = "玩家";
            } else if (role == Role.AI) {
                name = opponentName; // 直接使用传入的对手名称
            } else {
                name = role.GetChineseName();
            }

            // 格式对齐：确保"在"字的位置固定
            string paddedName = name.PadRight(8);
            string logMessage = $"{currentMoveId.ToString().PadLeft(2)} {paddedName} 在 ({colChar}, {displayY}) 下棋了";
            AddLog(Color.Black, logMessage);

            //维护操作序列
            currentMoveId++;
            var record = new MoveRecord(currentMoveId, role, x, y);
            moveRecords.Add(record);
        }

        // 获取所有操作记录
        public List<MoveRecord> GetMoveRecords() {
            return moveRecords;
        }

        // 清空操作记录
        private void ClearMoveRecords() {
            moveRecords.Clear();
            currentMoveId = 0;
        }

        // 设置对手名称
        public void SetOpponentName(string name) {
            opponentName = name ?? "AI";
        }

        //游戏结束
        private void LogWin(Role role) {
            if (role == Role.Empty) {
                string message = $"游戏终止";
                AddLog(Color.Gray, message);
                return;
            }
            string logMessage = $"{role.GetChineseName()}获胜！";
            if (role == Role.Draw)
                logMessage = $"{role.GetChineseName()}！";
            AddLog(Color.Green, logMessage);
        }
        //切换游戏
        public void LogSwitchGame(GameType game) {
            string logMessage = $"切换至{game.GetChineseName()}游戏";
            AddLog(Color.Gray, logMessage);
        }
        //切换算法
        public void LogSwitchAlgorithm(AIAlgorithmType algorithmType) {
            string logMessage = $"切换至{algorithmType.GetChineseName()} AI";
            AddLog(Color.Gray, logMessage);
        }

        // 上传对局结果到服务器
        public async void UploadGameResult(Role winner, Role first, AIAlgorithmType algorithmType, GameType gameType) {
            try {
                var client = new GameHiveClient();
                bool playerFirst = first == Role.Player;

                // 构造请求数据
                var requestData = new Dictionary<string, object> {
                    ["userId"] = UserInfo.Instance.IsLoggedIn ? UserInfo.Instance.UserId : 0,
                    ["algorithmName"] = algorithmType.GetChineseName(),
                    ["gameTypeName"] = gameType.GetChineseName(),
                    ["playerFirst"] = playerFirst,
                    ["winner"] = winner.ToWinnerCode(first),
                    ["moves"] = moveRecords.Select(r => new Dictionary<string, object> {
                        ["id"] = r.Id,
                        ["role"] = r.Role == Role.Player ? "Player" : "AI",
                        ["x"] = r.X,
                        ["y"] = r.Y
                    }).ToList()
                };

                // 发送请求
                var result = await client.UploadGameResultAsync(requestData);
                if (result.Code == 200) {
                    AddLog(Color.Gray, "对局记录已上传");
                } else {
                    AddLog(Color.Red, $"上传对局记录失败：{result.Msg}");
                }
            } catch (Exception ex) {
                AddLog(Color.Red, $"上传对局记录失败：{ex.Message}");
            }
        }

    }
}
