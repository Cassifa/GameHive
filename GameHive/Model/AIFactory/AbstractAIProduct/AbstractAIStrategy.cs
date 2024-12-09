/*************************************************************************************
 * 文 件 名:   AbstractAIStrategy.cs
 * 描    述: 根据策略模式：此类制定抽象策略，所有抽象产品实现抽象策略，供棋盘管理类调用
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:49
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
namespace GameHive.Model.AIFactory.AbstractAIProduct {
    internal abstract class AbstractAIStrategy {
        //获取下一步移动 棋盘 玩家上一次落子的X,上一次落子的Y
        public abstract Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard, int lastX, int lastY);
        //检查游戏是否结束
        public abstract Role CheckGameOverByPiece(List<List<Role>> currentBoard,int x,int y);
        //用户下棋
        public abstract void UserPlayPiece(int lastX, int lastY);
        //游戏开始
        public abstract void GameStart(bool IsAIFirst);
        //游戏结束 停止需要多线程的AI 更新在内部保存过状态的AI
        public abstract void GameForcedEnd();

    }
}
