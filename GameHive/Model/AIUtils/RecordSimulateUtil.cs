/*************************************************************************************
 * 文 件 名:   RecordSimulateUtil.cs
 * 描    述: 用于模拟对局
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/12/15 17:57
*************************************************************************************/
namespace GameHive.Model.AIUtils {
    public class RecordSimulateUtil {
        //已经运行轮数
        private static int RoundCount = 0;
        //是否启用模拟
        public static bool ActiveSimulate = false;
        //是否为VCF模拟
        public static bool IsVCF = true;
        private static List<Tuple<int, int>> VCFSimulate() {
            List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();
            //第1轮
            tuples.Add(Tuple.Create(-1, -1));
            tuples.Add(Tuple.Create(7, 7));
            //第2轮
            tuples.Add(Tuple.Create(7, 6));
            tuples.Add(Tuple.Create(7, 8));
            //第3轮
            tuples.Add(Tuple.Create(8, 6));
            tuples.Add(Tuple.Create(8, 8));
            //第4轮
            tuples.Add(Tuple.Create(9, 7));
            tuples.Add(Tuple.Create(11, 7));
            //第5轮
            tuples.Add(Tuple.Create(10, 8));
            tuples.Add(Tuple.Create(11, 9));
            //第6轮
            tuples.Add(Tuple.Create(9, 9));
            tuples.Add(Tuple.Create(7, 9));
            //第7轮
            tuples.Add(Tuple.Create(8, 10));
            tuples.Add(Tuple.Create(6, 12));
            //第8轮
            tuples.Add(Tuple.Create(7, 11));
            tuples.Add(Tuple.Create(5, 10));
            //第9轮
            tuples.Add(Tuple.Create(6, 8));
            tuples.Add(Tuple.Create(6, 10));
            //第10轮
            tuples.Add(Tuple.Create(6, 9));
            tuples.Add(Tuple.Create(7, 10));
            //第11轮
            tuples.Add(Tuple.Create(9, 6));
            return tuples;
        }
        private static List<Tuple<int, int>> VCTSimulate() {
            List<Tuple<int, int>> tuples = new List<Tuple<int, int>>();
            //第1轮
            tuples.Add(Tuple.Create(-1, -1));
            tuples.Add(Tuple.Create(8, 5));
            //第2轮
            tuples.Add(Tuple.Create(7, 4));
            tuples.Add(Tuple.Create(8, 7));
            //第3轮
            tuples.Add(Tuple.Create(10, 5));
            tuples.Add(Tuple.Create(9, 7));
            //第4轮
            tuples.Add(Tuple.Create(7, 6));
            tuples.Add(Tuple.Create(10, 7));
            //第5轮
            tuples.Add(Tuple.Create(7, 7));
            tuples.Add(Tuple.Create(10, 8));
            //第6轮
            tuples.Add(Tuple.Create(8, 8));
            tuples.Add(Tuple.Create(7, 8));
            //第7轮
            tuples.Add(Tuple.Create(7, 9));
            tuples.Add(Tuple.Create(9, 9));
            //第8轮
            tuples.Add(Tuple.Create(9, 10));
            tuples.Add(Tuple.Create(6, 9));
            //第9轮
            tuples.Add(Tuple.Create(5, 10));
            tuples.Add(Tuple.Create(7, 10));
            //第10轮
            tuples.Add(Tuple.Create(11, 7));
            return tuples;
        }

        //模拟杀棋对局，必定AI先手，
        public static bool SimulateKillBoard(ref int lastX, ref int lastY, ref Tuple<int, int> FinalDecide) {
            if (IsVCF) {
                //VCF模拟棋盘管前10轮，和第11轮的玩家移动
                if (RoundCount == 11)
                    return false;
                List<Tuple<int, int>> list = VCFSimulate();
                var PlayerMove = list[RoundCount * 2];
                lastX = PlayerMove.Item1;
                lastY = PlayerMove.Item2;
                //此前已经运行了10轮，当前为第11轮
                if (RoundCount == 10) {
                    RoundCount++;
                    return false;
                }
                FinalDecide = list[RoundCount * 2 + 1];

            } else {
                //VCT模拟棋盘管九轮和第10轮的玩家移动
                if (RoundCount == 10)
                    return false;
                List<Tuple<int, int>> list = VCTSimulate();
                var PlayerMove = list[RoundCount * 2];
                lastX = PlayerMove.Item1;
                lastY = PlayerMove.Item2;
                //此前已经运行了9轮，当前为第10轮
                if (RoundCount == 9) {
                    RoundCount++;
                    return false;
                }
                FinalDecide = list[RoundCount * 2 + 1];
            }
            RoundCount++;
            return true;
        }

        //通知控制层模拟对局
        public static void SimulateKillBoardController(ref int lastX, ref int lastY) {
            if (IsVCF) {
                //VCF模拟棋盘管前10轮，和第11轮的玩家移动
                if (RoundCount == 11)
                    return;
                List<Tuple<int, int>> list = VCFSimulate();
                var PlayerMove = list[RoundCount * 2];
                lastX = PlayerMove.Item1;
                lastY = PlayerMove.Item2;

            } else {
                //VCT模拟棋盘管九轮和第10轮的玩家移动
                if (RoundCount == 10)
                    return;
                List<Tuple<int, int>> list = VCTSimulate();
                var PlayerMove = list[RoundCount * 2];
                lastX = PlayerMove.Item1;
                lastY = PlayerMove.Item2;
            }
        }
    }
}
