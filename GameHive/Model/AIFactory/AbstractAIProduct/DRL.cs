/*************************************************************************************
 * 文 件 名:   DRL.cs
 * 描    述: 深度强化学习抽象产品
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:11
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;

namespace GameHive.Model.AIFactory.AbstractAIProduct {
    internal abstract class DRL : AbstractAIStrategy {

        //用户下棋
        public override void UserPlayPiece(int lastX, int lastY) { }
        //强制游戏结束 停止需要多线程的AI 更新在内部保存过状态的AI
        public override void GameForcedEnd() { }

        //游戏开始
        public override void GameStart(bool IsAIFirst) { }
    }
}
