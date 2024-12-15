/*************************************************************************************
 * 文 件 名:   ZobristHashingCache.cs
 * 描    述: 缓存表 可用于MinMax函数(需要深度) 估值函数(命中直接返回估值) 判别局面类型函数(命中直接返回类型)
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/12/16 1:46
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using System.Security.Cryptography;

public class ZobristHashingCache<T> {
    //已经记录的估值映射表
    private Dictionary<long, T> ValueCache = new Dictionary<long, T>();
    //记录分值映射的深度，对于博弈树有用，深度越大越可信
    private Dictionary<long, int> ValueDeep = new Dictionary<long, int>();
    //当前记录棋盘状态
    private long CurrentBoardHash;
    //随机数表
    private List<List<long>> AICache;
    private List<List<long>> PlayerCache;

    //根据传入大小初始化 Cache 表
    public ZobristHashingCache(int x, int y) {
        CurrentBoardHash = 0;
        PlayerCache = new List<List<long>>(x);
        AICache = new List<List<long>>(x);
        for (int i = 0; i < x; i++) {
            List<long> row = new List<long>(y);
            for (int j = 0; j < y; j++) {
                row.Add(GenerateHighQualityRandomLong());
            }
            PlayerCache.Add(row);
        }
        for (int i = 0; i < x; i++) {
            List<long> row = new List<long>(y);
            for (int j = 0; j < y; j++) {
                row.Add(GenerateHighQualityRandomLong());
            }
            AICache.Add(row);
        }
    }
    //落子-只能接受Role.AI Role.Player
    public void UpdateCurrentBoardHash(int x, int y, Role role) {
        if (role == Role.Empty)
            throw new KeyNotFoundException("不接受Empty类型的传入！");
        if (role == Role.AI) CurrentBoardHash ^= AICache[x][y];
        if (role == Role.Player) CurrentBoardHash ^= PlayerCache[x][y];
    }

    //记录当前Hash为Value
    public void Log(T value, int deep = 0) {
        //之前存过的深度大于当前深度则拒绝更新
        if (ValueDeep.TryGetValue(CurrentBoardHash, out int lastDeep)) {
            if (lastDeep > deep) return;
        }
        ValueCache[CurrentBoardHash] = value;
        ValueDeep[CurrentBoardHash] = deep;
    }

    //尝试查缓存,查到了改变接受的参数，返回深度，没查到返回-1
    public int GetValue(ref T valueHolder) {
        if (ValueCache.TryGetValue(CurrentBoardHash, out T value)) {
            valueHolder = value;
            return ValueDeep[CurrentBoardHash];
        }
        return -1;
    }

    // 生成高质量随机 long 类型数
    private long GenerateHighQualityRandomLong() {
        byte[] buffer = new byte[8]; // 8字节表示一个 long
        RandomNumberGenerator.Fill(buffer); // 填充高质量随机数
        return BitConverter.ToInt64(buffer, 0); // 转换为 long
    }
}
