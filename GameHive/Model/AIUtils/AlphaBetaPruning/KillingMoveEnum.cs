/*************************************************************************************
 * 文 件 名:   RiskLevelEnum.cs
 * 描    述: 
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/12/12 20:12
*************************************************************************************/
namespace GameHive.Model.AIUtils.AlphaBetaPruning {
    public enum KillingRiskEnum {
        High = 80_000,
        Middle = 50_000,
        Low = 10_000,
    }
    public enum KillTypeEnum {
        Five = 1_000_000,
        FourAlive = 100_000,
        ThreeAlive = 1_000,
        FourBlocked = 900,
    }
    public class KillingBoard {
        public int score = 0;
        public KillingRiskEnum type = KillingRiskEnum.Low;
        public Dictionary<KillTypeEnum, int> typeRecord = new Dictionary<KillTypeEnum, int>();
        public KillingBoard() {
            typeRecord = new Dictionary<KillTypeEnum, int>{
            { KillTypeEnum.Five, 0 },
            { KillTypeEnum.FourAlive, 0 },
            { KillTypeEnum.ThreeAlive, 0 },
            { KillTypeEnum.FourBlocked, 0 }};
        }
    }
}
