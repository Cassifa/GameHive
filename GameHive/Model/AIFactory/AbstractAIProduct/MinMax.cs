/*************************************************************************************
 * 文 件 名:   MinMax.cs
 * 描    述: 博弈树抽象产品
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:11
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;

namespace GameHive.Model.AIFactory.AbstractAIProduct {
    internal abstract class MinMax : AbstractAIStrategy {
        protected int maxDeep;
        private Tuple<int, int> FinalDecide;
        //决定Ai的风格偏好,加起来权重为10
        //protected int DefendBias = 4, AttackBias = 6;
        //获取可下棋点位
        protected abstract List<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board);
        protected abstract int EvalNowSituation(List<List<Role>> currentBoard, Role role);
        ////获取在X,Y落子带来的总收益(带风险偏好)
        //protected abstract int evalXYTRoundScore(List<List<Role>> currentBoard, int x,int y, Role role);
        ////计算 role 视角下在X,Y落子获得的攻击分数
        //protected abstract int calculateXY(List<List<Role>> currentBoard, int x,int y, Role role);

        //获取下一步移动
        public override Tuple<int, int> GetNextAIMove(List<List<Role>> currentBoard) {
            //计算最优值
            EvalToGo(currentBoard, 1, int.MinValue, int.MaxValue);
            return FinalDecide;
        }
        //执行博弈树搜索
        private int EvalToGo(List<List<Role>> currentBoard, int depth, int alpha, int bata) {

        }
        //计算一条的估值
        private int CalculateLine() {

        }
    }
}
