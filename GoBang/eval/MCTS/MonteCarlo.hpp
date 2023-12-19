#pragma once
#include<MCTSNode.hpp>
class MonteCarlo{
    private:
        int player=1;
        int Maximum=1e5;
        MCTSNode *root;
    public:
        MonteCarlo(){};
        ~MonteCarlo(){};

        //设置玩家角色
        void setPlayer(int player){
            this.player=player;
        }
        
        //换根-是否必须建立新根
        void changeRoot(dvectr *newRoot,bool need);

        //初始化
        void initMCTS();

        //选择
        /*从根节点开始依次比较子节点UCB最大的并选择
          直到达到叶子节点
          1.若未模拟则模拟,否则拓展并选择其叶子节点
        */
        void selection();

        //拓展-当节点是已经被更新过的
        void expansion(MCTSNode *nowNode,int deep);

        //模拟-当节点是全新未更新的
        void simulation(MCTSNode *beginNode,int deep);

        //反向传播
        void rollout(MCTSNode *beginNode,int values);

        //获取当前结果
        pii getCurrentResult();
};

MonteCarlo::~MonteCarlo(){
    //设置假的新根把所有全删了
    MCTSNode *temp=new MCTSNode;
    changeRoot(temp);
    delete temp;
}
