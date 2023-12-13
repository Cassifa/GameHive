#pragma once
#include <iostream>
#include<vector>
#include<ctime>
#include"MinMax.hpp"
#include"MonteCarlo.hpp"
using namespace std;
#define pii pair<int,int>
//输入地图，需要的决策类型
pii evalToGo(vector<vector<int>> &nowMap,int type){
    pii ans;
    //用MinMaxAi
    if(type==1){
        MinMax Ai;
        Ai.start(nowMap);
        ans=Ai.finalDecision;
        cout<<"非叶子节点数"<<" "<<Ai.countCnt<<endl;
        cout<<"叶子节点数"<<" "<<Ai.countYe<<endl;
    }
    //用蒙特卡洛Ai
    else if(type==2){
        // MonteCarlo Ai;
    }
    return ans;
}