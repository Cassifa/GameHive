#pragma once
#include "../baseGoBangAi.hpp"
#include "MCTSNode.hpp"
#include "safe_queue.h"
#include <thread>
#include <mutex>
#include <atomic>
#define MOVE 1
#define ASK 2
#define END 3
class MonteCarlo :public baseGoBangAi{
    private:
        //消息队列 1:玩家移动 2:要求五秒内输出移动 3：结束游戏
        threadsafe_queue<int> qmsg;
        //当前进程
        thread* pMain;
        //锁进程
        mutex lock;
        //当前搜索根
        MCTSNode *root=nullptr;
        //需要移动 正要移动
        atomic_bool need_move, ready_move;
        pii aiMove;//playerLastMove
        //开始时间
        clock_t start_time;
        
        //选择
        /*从根节点开始依次比较子节点UCB最大的并选择
          直到达到叶子节点
          若叶子节点模拟次数不够则模拟,否则拓展并选择其叶子节点
        */
        MCTSNode* selection();

        //拓展-当节点是已经被更新过的
        void expansion(MCTSNode *nowNode);
        //模拟-当节点是全新未更新的
        int simulation(MCTSNode beginNode);
        //反向传播
        void rollout(MCTSNode *beginNode,int score);
        //执行线程
        void Run();
        
        //换根-是否必须建立新根 若传入空指针则为删除整棵树
        void changeRoot(pii playerLastMove);
    public:
        MonteCarlo(){};
        ~MonteCarlo();

        //获取下一步
        pii evalToGo();
        //初始化地图
        void setBeginningState(dvectr map);
        //收到玩家移动消息
        void sendPlayerMoveMessage(int x,int y);
        //结束游戏
        void end();
};


