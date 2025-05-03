/*************************************************************************************
 * 文 件 名:   ConcreteProductInfo.cs
 * 描    述:  具体游戏产品（算法-游戏类型）信息
 * 版    本：  V3.0 引入DRL、MinMax-MCTS混合算法
 * 创 建 者：  Cassifa
 * 创建时间：  2025/5/3 16:00
*************************************************************************************/
using GameHive.Constants.DifficultyLevelEnum;

namespace GameHive.Model.GameInfo {
    internal class ConcreteProductInfo {
        //当前产品的难度列表
        public List<DifficultyLevel> DifficultyLevels { get; set; }


        // 构造函数初始化难度列表
        public ConcreteProductInfo(int levelCnt = 1) {
            DifficultyLevels = DifficultyLevelExtensions.GetLevelRange(levelCnt);
        }
    }
}