#pragma once
#include "MonteCarlo.hpp"
#include <fstream>
//初始化地图
void MonteCarlo::setBeginningState(dvectr map){
    //初始化根节点
    root=new MCTSNode;
    root->map=map;
    root->isAi=true;
    root->father=nullptr;
    root->getSons();
    root->fatherDecision={-1,-1};
	srand((unsigned)time(0));
    // save();
    // if(map[8][8]) init();
}

//收到玩家移动消息,改根
void MonteCarlo::sendPlayerMoveMessage(int x,int y){
    changeRoot({x,y});
}
//结束游戏
void MonteCarlo::end(){
    changeRoot({-1,-1});
}

//获取下一步
pii MonteCarlo::evalToGo(){
    //等待计算输出
    start_time=clock();
    int search_times=0;
    while (true){
        // clock()-start_time<9500
        clock_t t=clock();
        if(t-start_time>10000)break;
        search_times++;
        //选择:得到一个叶子节点
        MCTSNode *nowNode=this->selection();
        //没有模拟过那么模拟
        if(nowNode->n==0){
            //获取模拟得分
            int score=this->simulation(*nowNode);
            rollout(nowNode,score);
        }
        //模拟过那么拓展
        else{
            //拓展节点
            nowNode->getSons();
            //节点分数
            int score=0;
            //已经无棋可下,说明棋盘满了;否则将第一个节点作为下次搜的节点并直接模拟
            if(nowNode->sons.size()!=0){
                nowNode=nowNode->sons[0];
                score=simulation(*nowNode);
            }
            rollout(nowNode,score);
        }
    }
    cout<<"本次用了 "<<clock()-start_time<<" 毫秒，搜了 "<<search_times<<" 次\n";
    search_times=0;
    //得到最大UCB的决策
    MCTSNode* move=root->findMaxiumUCBNode();
    aiMove=move->cmpWhereToGo();
    //换根
    changeRoot(aiMove);
    return aiMove;
}

//换根-将地图调整到在movePlace下棋的状态并删除多余节点
void MonteCarlo::changeRoot(pii movePlace){
    //结束游戏，删掉根
    if(movePlace.fi==-1){
        delete root;
        return;
    }
    MCTSNode *oldRoot=root;
    //删除无用节点
    vector<MCTSNode*> temp;
    for(auto t:oldRoot->sons)
        if(t->map[movePlace.fi][movePlace.se]==0){
            delete t;
        }
        else temp.push_back(t);
    oldRoot->sons=temp;
    //如果分支本身就存在
    if(oldRoot->sons.size()){
        root=oldRoot->sons[0];
        root->father=nullptr;
        oldRoot->sons.clear();
        delete oldRoot;
    }
    //如果玩家上次落子周围两圈都没子会导致没有分支
    else{
        //新建根,标记为玩家走过
        root=new MCTSNode;
        root->map=oldRoot->map;
        root->map[movePlace.fi][movePlace.se]=1+oldRoot->isAi;
        root->isAi=!oldRoot->isAi;
        root->father=nullptr;
        oldRoot->sons.clear();
        delete oldRoot;
    }
    // cout<<"换根到了:\n";
    // dvectr tt=root->map;
    // for(int i=1;i<=tt.size();i++){
    //     for(auto t:tt[i])
    //         if(!t)cout<<"  ";
    //         else cout<<t<<" ";
    //     cout<<endl;
    // }
    cout<<"孩子数量："<<root->sons.size()<<endl;
    cout<<"次数与分值："<<root->n<<" "<<root->win<<endl;
}

//选择叶子节点
MCTSNode* MonteCarlo::selection(){
    MCTSNode *nowNode=root;
    //一直找到没有孩子为止
    while(nowNode->sons.size()){
        nowNode=nowNode->findMaxiumUCBNode();
    }
    return nowNode;
}

//拓展-当节点是已经被更新过的
void MonteCarlo::expansion(MCTSNode *nowNode){
    nowNode->getSons();
}

//模拟-当节点是全新未更新的
int MonteCarlo::simulation(MCTSNode beginNode){
    //判断初始节点是否已经结束
    if(beginNode.isHeWin(1))return 1;
    else if(beginNode.isHeWin(2)||beginNode.isDraw())return 0;
    //记录在树中的点视角,接下来每次反转
    int isAi=beginNode.isAi;
    while(true){
        isAi^=1;
        //随机游走
        auto list=beginNode.getUsefulSteps();
        pii t=list[rand()%list.size()];
        //在此落子
        beginNode.map[t.fi][t.se]=2-isAi;

        //判断是否结束
        if(beginNode.isHeWin(1))return 1;
        else if(beginNode.isHeWin(2)||beginNode.isDraw())return 0;

    }
}

//反向传播
void MonteCarlo::rollout(MCTSNode *beginNode,int score){
    while(beginNode->father!=nullptr){
        beginNode->n++;
        beginNode->win+=score;
        beginNode=beginNode->father;
    }
    beginNode->n++;
    beginNode->win+=score;
}


// void MonteCarlo::dfsInit(MCTSNode *nowNode,ofstream &ofs){}
// void MonteCarlo::init(){
//     //(1)创建流对象
//     ofstream ofs;
//     //(2)指定打开方式
//     ofs.open("./GoBang/eval/MCTS/AiFirst.txt", ios::out);
//     this->dfsInit(root,ofs);
//     ofs.close();
// }

// //预打表
// void MonteCarlo::save(){
//     ll t=0,n=1e5;
//     //构建n次蒙特卡洛
//     while(t++<n){
//         //选择:得到一个叶子节点
//         MCTSNode *nowNode=this->selection();
//         //没有模拟过那么模拟
//         if(nowNode->n==0){
//             //获取模拟得分
//             int score=this->simulation(*nowNode);
//             rollout(nowNode,score);
//         }
//         //模拟过那么拓展
//         else{
//             //拓展节点
//             nowNode->getSons();
//             //节点分数
//             int score=0;
//             //已经无棋可下,说明棋盘满了;否则将第一个节点作为下次搜的节点并直接模拟
//             if(nowNode->sons.size()!=0){
//                 nowNode=nowNode->sons[0];
//                 score=simulation(*nowNode);
//             }
//             rollout(nowNode,score);
//         }
//     }

//     //(1)创建流对象
//     ofstream ofs;
//     //(2)指定打开方式
//     ofs.open("./GoBang/eval/MCTS/AiFirst.txt", ios::out);
//     this->dfsSave(root,ofs);
//     ofs.close();
// }

// void MonteCarlo::dfsSave(MCTSNode *nowNode,ofstream &ofs){
//     ofs<<"n "<<nowNode->n<<" ";
//     ofs<<"w "<<nowNode->win<<" ";
//     ofs<<"f "<<nowNode->fatherDecision.fi<<" "<<nowNode->fatherDecision.se<<" ";
//     ofs<<"s "<<" ";
//     for(auto t:nowNode->sons)
//         dfsSave(t,ofs);
//     ofs<<"t"<<" ";
// }