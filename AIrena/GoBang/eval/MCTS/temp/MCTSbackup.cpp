#pragma once
#include "MonteCarlo.hpp"
//本文件是多线程备份，希望实现用户思考时Ai依然在搜索的效果
//换根-将地图调整到在movePlace下棋的状态并删除多余节点
void MonteCarlo::changeRoot(pii movePlace){
    //结束游戏，删掉根
    if(movePlace.fi==-1){
        delete root;
        return;
    }
    MCTSNode *oldRoot=root;
    //删除无用节点
    for(auto t:oldRoot->sons)
        if(t->map[movePlace.fi][movePlace.se]==0)
            delete t;
    //如果分支本身就存在
    if(oldRoot->sons.size()){
        root=oldRoot->sons[0];
        root->father=nullptr;
        delete oldRoot;
    }
    //如果玩家上次落子周围两圈都没子会导致没有分支
    else{
        //新建根,标记为玩家走过
        root=new MCTSNode;
        root->map=oldRoot->map;
        root->map[movePlace.fi][movePlace.se]=2;
        delete oldRoot;
    }
}

void MonteCarlo::Run(){
    //是否继续执行蒙特卡洛搜索
    bool keep=true;
    int search_times;
    while(keep){
        //处理消息队列
        while(true){
            int nowMessage=-1;
            //消息队列 1:玩家移动 2:要求五秒内输出移动 3：结束游戏
            //无消息退出
            if(!(this->qmsg.try_pop(nowMessage)))break;
            //玩家移动了
            if(nowMessage==MOVE){
                //加锁等移动完
				std::lock_guard<std::mutex> lg(this->lock);
                changeRoot(this->playerLastMove);
            }
            //要求输出决策
            else if(nowMessage==ASK){
                //加锁确保玩家移动完成
				std::lock_guard<std::mutex> lg(this->lock);
				need_move = true;
				ready_move = false;
            }
            //结束游戏
            else if(nowMessage==END){
				std::lock_guard<std::mutex> lg(this->lock);
                changeRoot({-1,-1});
                keep=false;
                break;
            }
        }
        if(!keep)break;
        
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

        //如果需要移动达到规定时间
        if(need_move&&(clock()-start_time)>950){
            cout<<"本次用了 "<<clock()-start_time<<" 毫秒，搜了 "<<search_times<<" 次\n";
            search_times=0;
            //得到最大UCB的决策
            MCTSNode* move=root->findMaxiumUCBNode();
            aiMove=move->cmpWhereToGo();
            //同步
            std::lock_guard<std::mutex> lg(this->lock);
			need_move = false;
			ready_move = true;

        }

    }
}
MCTSNode* MonteCarlo::selection(){
    MCTSNode *nowNode=root,*next=nullptr;
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
}

//获取下一步
pii MonteCarlo::evalToGo(){
    qmsg.push(ASK);
    //等待计算输出
    while(!ready_move){}
    ready_move=false;
    return aiMove;
}
//初始化地图
void MonteCarlo::setBeginningState(dvectr map){
    //初始化状态
	start_time = 0;
	pMain = nullptr;
	need_move = false;
	ready_move = false;
	root = nullptr;
    //初始化根节点
    root=new MCTSNode;
    root->map=map;
    root->isAi=true;
    root->father=nullptr;
	srand((unsigned)time(0));
    //开启线程
	pMain = new std::thread(&MonteCarlo::Run, this);
}
//收到玩家移动消息,加入消息队列
void MonteCarlo::sendPlayerMoveMessage(int x,int y){
    playerLastMove={x,y};
    qmsg.push(MOVE);
}
//结束游戏
void MonteCarlo::end(){
    qmsg.push(END);
	pMain->join();
}
MonteCarlo::~MonteCarlo(){
	this->end();
	if (pMain != nullptr)
		delete pMain;
}