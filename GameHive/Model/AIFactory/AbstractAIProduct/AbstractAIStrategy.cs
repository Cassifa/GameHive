/*************************************************************************************
 * 文 件 名:   AbstractAIStrategy.cs
 * 描    述: 根据策略模式：此类制定抽象策略，所有抽象产品实现抽象策略，供棋盘管理类调用
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 18:49
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameHive.Model.AIFactory.AbstractAIProduct {
    internal abstract class AbstractAIStrategy {
        //获取下一步移动
        public abstract Tuple<int,int> GetNextAIMove(List<List<Role>> currentBoard);
        public abstract Role CheckGameOver(List<List<Role>> currentBoard);
        private GameBoardInfo GameBoardInfo;

    }
}
