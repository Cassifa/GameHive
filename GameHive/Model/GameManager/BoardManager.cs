/*************************************************************************************
 * 文 件 名:   BoardManager.cs
 * 描    述:  棋盘管理类，单例，用于屏蔽所有交互，根据策略模式调取AI下一步操作
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 20:19
*************************************************************************************/
using GameHive.Constants.AIAlgorithmTypeEnum;
using GameHive.Constants.GameTypeEnum;
using GameHive.Constants.RoleTypeEnum;
using GameHive.Model.AIFactory;
using GameHive.Model.AIFactory.AbstractAIProduct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameHive.Model.GameManager {
    internal partial class BoardManager {


        //当前在玩的游戏
        private GameType gameType;
        //当前在使用的AI算法
        private AIAlgorithmType aIAlgorithmType;
        private bool gameRunning;
        private Role first;

        //当前在运行的AI产品实例
        private AbstractAIStrategy runningAI;
        //游戏情况
        public GameBoardInfo BoardInfo { get; private set; }
        //当前棋盘
        private List<List<int>> board;


        //单例模式
#pragma warning disable CS8618 
        private static BoardManager _instance;
#pragma warning restore CS8618 
        private BoardManager() { }
        public static BoardManager Instance {
            get {
                if (_instance == null) {
                    lock (typeof(BoardManager)) {
                        if (_instance == null) {
                            _instance = new BoardManager();
                        }
                    }
                }
                return _instance;
            }
        }

    }
}
