﻿/*************************************************************************************
 * 文 件 名:   ZobristHashingCache.cs
 * 描    述: 缓存表 可用于MinMax函数(需要深度) 估值函数(命中直接返回估值) 判别局面类型函数(命中直接返回类型)
 * 版    本：  V2.0 .NET客户端初版
 * 创 建 者：  Cassifa
 * 创建时间：  2024/12/16 1:46
*************************************************************************************/
using GameHive.Constants.RoleTypeEnum;
using System.Security.Cryptography;
namespace GameHive.Model.AIUtils.AlgorithmUtils {
    public class ZobristHashingCache<T> {
        private int LengthX, lengthY;
        // 已记录的估值与深度映射表，合并为一个字典
        private Dictionary<long, (T value, int deep)> Cache = new Dictionary<long, (T, int)>();
        // 当前记录棋盘状态
        private long CurrentBoardHash;

        // 忽略变化栈
        private Stack<long> DiscardMoveStack;

        // 随机数表
        private List<List<long>> AICache;
        private List<List<long>> PlayerCache;

        // 根据传入大小初始化 Cache 表
        public ZobristHashingCache(int x, int y) {
            CurrentBoardHash = 0;
            LengthX = x;
            lengthY = y;
            DiscardMoveStack = new Stack<long>();
            PlayerCache = new List<List<long>>(LengthX);
            AICache = new List<List<long>>(LengthX);
            InitBoard();
        }

        // 落子-只能接受 Role.AI 或 Role.Player 同一个点下两次相当于撤销
        public void UpdateCurrentBoardHash(int x, int y, Role role) {
            if (role == Role.Empty)
                throw new KeyNotFoundException("不接受Empty类型的传入！");
            if (role == Role.AI)
                CurrentBoardHash ^= AICache[x][y];
            if (role == Role.Player)
                CurrentBoardHash ^= PlayerCache[x][y];
        }

        // 记录当前 Hash 为 Value
        public void Log(T value, int deep = 0) {
            if (Cache.TryGetValue(CurrentBoardHash, out var existingEntry)) {
                if (existingEntry.deep > deep)
                    return;
            }
            Cache[CurrentBoardHash] = (value, deep);
        }

        // 尝试查缓存，查到了改变接受的参数，返回深度，没查到返回 -1
        public int GetValue(ref T valueHolder) {
            if (Cache.TryGetValue(CurrentBoardHash, out var entry)) {
                valueHolder = entry.value;
                return entry.deep;
            }
            return -1;
        }

        // 刷新缓存 - 重置当前局面
        public void RefreshLog() {
            CurrentBoardHash = 0;
            //InitBoard();//随机数表可以继续用
        }

        // 记录一个忽略变更标记点
        public void ActiveMoveDiscard() {
            DiscardMoveStack.Push(CurrentBoardHash);
        }

        // 回溯局面至上次记录节点
        public void WithDrawMoves() {
            if (DiscardMoveStack.Count == 0)
                throw new KeyNotFoundException("未启用回溯!");
            CurrentBoardHash = DiscardMoveStack.Pop();
        }

        //初始化棋盘
        private void InitBoard() {
            PlayerCache.Clear();
            AICache.Clear();
            for (int i = 0; i < LengthX; i++) {
                List<long> row = new List<long>(lengthY);
                for (int j = 0; j < lengthY; j++) {
                    row.Add(GenerateHighQualityRandomLong());
                }
                PlayerCache.Add(row);
            }
            for (int i = 0; i < LengthX; i++) {
                List<long> row = new List<long>(lengthY);
                for (int j = 0; j < lengthY; j++) {
                    row.Add(GenerateHighQualityRandomLong());
                }
                AICache.Add(row);
            }
        }

        // 生成高质量随机 long 类型数
        private long GenerateHighQualityRandomLong() {
            byte[] buffer = new byte[8]; // 8字节表示一个 long
            RandomNumberGenerator.Fill(buffer); // 填充高质量随机数
            return BitConverter.ToInt64(buffer, 0); // 转换为 long
        }
    }
}