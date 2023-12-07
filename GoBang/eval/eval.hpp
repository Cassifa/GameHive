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
    if(type==1){
        MinMax now;
        now.evalToGo(nowMap,1);
        ans=now.finalDecision;
    }
    else if(type==2){
        // MonteCarlo now=new MonteCarlo();
        // delete now;
    }
    return ans;
}