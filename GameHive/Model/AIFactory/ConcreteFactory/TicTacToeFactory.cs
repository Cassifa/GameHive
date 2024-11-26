/*************************************************************************************
 * 文 件 名:   TicTacToeFactory.cs
 * 描    述: 井字棋工厂
 * 版    本：  V1.0
 * 创 建 者：  Cassifa
 * 创建时间：  2024/11/26 20:35
*************************************************************************************/
using GameHive.Model.AIFactory.AbstractAIProduct;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GameHive.Model.AIFactory {
    internal class TicTacToeFactory : AbstractFactory {
        //单例模式
        private static TicTacToeFactory _instance;
        private TicTacToeFactory() { }
        // 公共静态属性，提供实例访问
        public static TicTacToeFactory Instance {
            get {
                // 如果实例尚未创建，则创建实例
                if (_instance == null) {
                    // 使用锁确保线程安全
                    lock (typeof(TicTacToeFactory)) {
                        if (_instance == null) {
                            _instance = new TicTacToeFactory();
                        }
                    }
                }
                return _instance;
            }
        }

        public MCTS GetMCTSProduct() {
            throw new NotImplementedException();
        }

        public MinMax GetMinMaxProduct() {
            throw new NotImplementedException();
        }

        public Negamax GetNegamaxProduct() {
            throw new NotImplementedException();
        }
        private DRL GetDRLProduct() {
            throw new NotImplementedException();
        }
    }
}
