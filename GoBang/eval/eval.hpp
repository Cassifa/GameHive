#pragma once
#include <iostream>
#include<vector>
#include<ctime>
#include"MinMax.hpp"
#include"./MCTS/MonteCarlo.hpp"
using namespace std;
#define pii pair<short,short>
#define dvectr vector<vector<short>>
bool notFirstFlag=false;
bool firstStep(dvectr nowMap);
//输入地图，需要的决策类型
pii evalToGo(dvectr &nowMap,MonteCarlo *monteCarlo,MinMax *minMax,string type){
    pii ans;
    if(notFirstFlag||firstStep(nowMap)){
        //启动蒙特卡洛
        // if(type=="MonteCarlo")
        return {8,8};
    }
    //用MinMaxAi
    if(type=="MinMax"){
        minMax.start(nowMap);
        ans=minMax.finalDecision;
        cout<<"非叶子节点数"<<" "<<Ai.countCnt<<endl;
        cout<<"叶子节点数"<<" "<<Ai.countYe<<endl;
    }
    //用蒙特卡洛Ai
    else if(type=="MonteCarlo"){
        // MonteCarlo Ai;
    }
    return ans;
}
bool firstStep(dvectr nowMap){
    notFirstFlag=true;
    for(int i=1;i<nowMap.size();i++)
        for(int j=1;j<nowMap[i].size();j++)
            if(nowMap[i][j])return false;
    return true;
}