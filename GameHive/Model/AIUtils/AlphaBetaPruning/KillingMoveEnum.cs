/*************************************************************************************
 * 文 件 名:   RiskLevelEnum.cs
 * 描    述: 
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/12/12 20:12
*************************************************************************************/
namespace GameHive.Model.AIUtils.AlphaBetaPruning {
    public enum KillingRiskEnum {
        //多冲四
        High = 40_000,//小与活四1/5
        //冲四加活三
        Middle = 25_000,//活四一半，大于活三
        //多活三
        Low = 8_000,//大于活三10小于活四
        None=0,
    }
    public enum KillTypeEnum {
        Five = 1_000_000,
        FourAlive = 50_000,
        ThreeAlive = 800,
        FourBlocked = 700,
        Empty
    }
    public class KillingBoard {
        public int score = 0;
        public KillingRiskEnum type = KillingRiskEnum.None;
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
