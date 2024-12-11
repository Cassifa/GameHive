/*************************************************************************************
 * 文 件 名:   MisereTicTacToeMInMax.cs
 * 描    述: α-β剪枝博弈树不围棋产品实例
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:38
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;

namespace GameHive.Model.AIFactory.ConcreteProduct {
    internal class AntiGoMinMax : MinMax {
        public override Role CheckGameOverByPiece(List<List<Role>> currentBoard, int x, int y) {
            throw new NotImplementedException();
        }

        protected override int EvalNowSituation(List<List<Role>> currentBoard, Role role) {
            throw new NotImplementedException();
        }

        protected override List<Tuple<int, int>> GetAvailableMoves(List<List<Role>> board) {
            throw new NotImplementedException();
        }

        protected override List<Tuple<int, int>> GetAvailableMovesByNewPieces(List<List<Role>> currentBoard, List<Tuple<int, int>> lastAvailableMoves, int lastX, int lastY) {
            throw new NotImplementedException();
        }

        protected override List<List<Role>> GetCurrentBoard() {
            throw new NotImplementedException();
        }

        protected override void InitBoards() {
            throw new NotImplementedException();
        }

        protected override void PlayChess(int x, int y, Role role) {
            throw new NotImplementedException();
        }
    }
}
