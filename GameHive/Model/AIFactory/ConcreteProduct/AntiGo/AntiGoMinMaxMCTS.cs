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
    internal class AntiGoMinMaxMCTS : MinMaxMCTS {
        public override Role CheckGameOverByPiece(List<List<Role>> currentBoard, int x, int y) {
            throw new NotImplementedException();
        }
    }
}
