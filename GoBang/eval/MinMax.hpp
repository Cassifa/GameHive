#pragma once
#include<vector>
#define ll long long
#define dvectr vector<vector<int>>
using namespace std;
const int inf=1e9;//输了/赢了估值
const int TIE=-10;//平局估值
struct MinMax{
    pii finalDecision;
    int player=1;
    //获取当前深度下最优决策 传入地图 深度 传入时保证还有空位
    int evalToGo(dvectr &nowMap,int deep);
    //获取所有有效落子位置,不考虑周围九个格都没落子的点位，减少搜索空间
    vector<pii> getUsefulSteps(dvectr &nowMap);
    //评估当前局面Ai视角下的局面
    int evalNowSituation(dvectr &nowMap);
    //判断是否结束,没结束返回0,player输了返回-inf,赢了返回inf
    int isEnd(dvectr &nowMap);
    //判断是不是这个人赢了
    bool isHeWin(dvectr &nowMap,int view);
    //判断是不是平局了
    bool isTie(dvectr &nowMap);
};
bool MinMax::isTie(dvectr &nowMap){
    for (int i = 0; i < nowMap.size(); ++i)
        for (int j = 0; j < nowMap[i].size(); ++j)
            if (nowMap[i][j] == 0)
                return false;
    return true;
}
bool MinMax::isHeWin(dvectr &nowMap, int view) {
    // 检查横向
    for (int i = 0; i < 15; i++) {
        for (int j = 0; j < 11; j++) 
            if (nowMap[i][j] == view && nowMap[i][j + 1] == view && nowMap[i][j + 2] == view &&
                nowMap[i][j + 3] == view && nowMap[i][j + 4] == view) 
                return true;
    }
    // 检查纵向
    for (int i = 0; i < 11; i++) {
        for (int j = 0; j < 15; j++)
            if (nowMap[i][j] == view && nowMap[i + 1][j] == view && nowMap[i + 2][j] == view &&
                nowMap[i + 3][j] == view && nowMap[i + 4][j] == view)
                return true;
    }
    // 检查主对角线
    for (int i = 0; i < 11; i++) {
        for (int j = 0; j < 11; j++) {
            if (nowMap[i][j] == view && nowMap[i + 1][j + 1] == view && nowMap[i + 2][j + 2] == view &&
                nowMap[i + 3][j + 3] == view && nowMap[i + 4][j + 4] == view) {
                return true;
            }
        }
    }
    // 检查副对角线
    for (int i = 4; i < 15; i++) {
        for (int j = 0; j < 11; j++) {
            if (nowMap[i][j] == view && nowMap[i - 1][j + 1] == view && nowMap[i - 2][j + 2] == view &&
                nowMap[i - 3][j + 3] == view && nowMap[i - 4][j + 4] == view) {
                return true;
            }
        }
    }
    return false;  // 未连成五子
}
int MinMax::isEnd(dvectr &nowMap){
    if(isHeWin(nowMap,player)) return inf;
    else if(isHeWin(nowMap,3-player)) return inf;
    else if(isTie(nowMap)) return TIE;
    return 0;
}
vector<pii> MinMax::getUsefulSteps(dvectr &nowMap){

}
int MinMax::evalToGo(dvectr &nowMap,int deep){
    if(deep==5)return evalNowSituation(nowMap);
    if(isEnd(nowMap)!=0){
        return isEnd(nowMap);
    }
    
}
int MinMax::evalNowSituation(dvectr &nowMap){

}