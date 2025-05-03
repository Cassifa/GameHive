/*************************************************************************************
 * 文 件 名:   AntiGoMinMax.cs
 * 描    述: 
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/12/13 4:08
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using GameHive.Model.GameInfo;

namespace GameHive.Model.AIFactory.ConcreteProduct {
    class AntiGoMinMax : MinMax {
        //具体产品信息 包含难度
        public static ConcreteProductInfo concreteProductInfo = new ConcreteProductInfo(1);
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

        protected override void InitGame() {
            throw new NotImplementedException();
        }

        protected override void PlayChess(int x, int y, Role role) {
            throw new NotImplementedException();
        }
    }
}
