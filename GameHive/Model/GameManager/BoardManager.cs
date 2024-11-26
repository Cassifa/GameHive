/*************************************************************************************
 * 文 件 名:   BoardManager.cs
 * 描    述: 
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 20:19
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Constants.GameTypeEnum;
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory.AbstractAIProduct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameHive.Model.GameManager {
    internal partial class BoardManager {
        //当前在玩的游戏
        private GameType runningGame;
        //当前在运行的AI算法
        private AbstractAIStrategy runningAI;

    }
}
