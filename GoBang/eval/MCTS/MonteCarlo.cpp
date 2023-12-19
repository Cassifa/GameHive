#include "MonteCarlo.hpp"
//换根-是否必须建立新根
void MonteCarlo::changeRoot(dvectr *newRoot,bool need){

}

//初始化
void MonteCarlo::initMCTS(){

}

//选择
/*从根节点开始依次比较子节点UCB最大的并选择
    直到达到叶子节点
    1.若未模拟则模拟,否则拓展并选择其叶子节点
*/
void MonteCarlo::selection(){
    MCTSNode *nowLookingNode=root;
    int deep=1;
    //找到叶子节点
    while(true){
        if(nowLookingNode->n!=0)break;
        MCTSNode *willChoiceSon;
        double ma=-1e10;
        for(auto t:nowLookingNode->sons)
            if(t->getUCB(deep)>ma)willChoiceSon=t;
        nowLookingNode=willChoiceSon=;
        deep++;
    }
    //如果未扩展则先扩展再随机选择其一个儿子
    if(nowLookingNode->sons.size()==0){
        //判断当前节点是否已经结束
        int isEnd=nowLookingNode->isEnd(player);
        if(isEnd){
            rollout(nowLookingNode,isEnd);
            return;
        }
        //没有结束则拓展并随机模拟其子节点
        expansion(nowLookingNode,deep);
        nowLookingNode=nowLookingNode->sons[rand()%nowLookingNode->sons.size()];
    }
    simulation(nowLookingNode,deep);
}

//拓展-当节点是已经被更新过的
void MonteCarlo::expansion(MCTSNode *nowNode,int deep){
    //如果是自己则下一层节点自己下棋
    nowNode->getSons((deep&1)?player:3-player);
}

//模拟-当节点是全新未更新的
void MonteCarlo::simulation(MCTSNode *beginNode,int deep){
    int endScore=beginNode->isEnd();
    deep++;
    MCTSNode nowNode;
    nowNode.map=beginNode->map;
    while(!endScore){
        vector<pii> q=nowNode.getUsefulSteps();
        pii nextStep=q[rand()%q.size()];
        nowNode.map[nextStep.fi][nextStep.se]=(deep&1) player:3-player;
        endScore=nowNode.isEnd();
        deep++;
    }
    rollout(beginNode,endScore);
}

//反向传播
void MonteCarlo::rollout(MCTSNode *beginNode,int value){
    while(beginNode!=root){
        beginNode->n++;
        beginNode->value+=value;
    }
    root->n++;
    root->value+=value;
}

//获取当前结果
pii MonteCarlo::getCurrentResult(){
    double ma=-1e10;
    pii descion;
    for(auto t:root->sons){
        if(t->value/t->n>ma)descion=t->cmpDescion(&root);
    }
    return descion;
}