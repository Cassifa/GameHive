/*************************************************************************************
 * 文 件 名:   StatusSwitchRegister.cs
 * 描    述: 
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 1:32
*************************************************************************************/
using GameHive.Constants.GameModeEnum;
using GameHive.Constants.GameStatusEnum;
using GameHive.Constants.RoleTypeEnum;

namespace GameHive.Controller {
    //用于注册游戏状态切换
    internal partial class Controller {
        private void RegisterStatusSwitch() {
            statusSwitchRegister += StatusSwitchClick;
        }

        private void StatusSwitchClick(object sender, EventArgs e) {
            if (CurrentGameMode == GameMode.OnlineGame) {
                switch (CurrentGameStatus) {
                    case GameStatus.NotStarted:
                        StartGame();
                        break;
                    case GameStatus.Matching:
                        try {
                            gameSession.EndSessionAsync().Wait();
                            gameSession = null;
                            CurrentGameStatus = GameStatus.NotStarted;
                            ViewMessageStopMatching();
                        } catch { }
                        break;
                    case GameStatus.Playing:
                        EndGame(Role.Empty);
                        break;
                }
            } else {
                if (CurrentGameStatus == GameStatus.Playing) {
                    EndGame(Role.Empty);
                } else {
                    StartGame();
                }
            }
        }
    }
}
