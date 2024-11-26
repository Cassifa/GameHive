/*************************************************************************************
 * 文 件 名:   BoardManagerReceiver.cs
 * 描    述: 棋盘管理类-接收信息
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 20:20
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Constants.GameTypeEnum;
using GameHive.Constants.RoleTypeEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameHive.Model.GameManager {
    internal partial class BoardManager {
        //玩家终止游戏
        public void EndGame() { }
        //玩家开始游戏
        public void StartGame() { }
        //检查此处落子是否有效
        public void CheckValid(int x, int y) { }
        //用户设置先后手
        public void SetFirst(Role first) { }
        //用户在x,y下棋
        public void UserPalyChess(int x, int y) { }
        //切换算法
        public void SwitchGame(GameType gameType) { }
        public void SwitchAIType(AIAlgorithmType type) { }
    }
}
