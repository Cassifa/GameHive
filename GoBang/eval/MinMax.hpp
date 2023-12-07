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
    int dx[8]={-1,-1,0,1,1,1,0,-1},dy[8]={0,1,1,1,0,-1,-1,-1};
    dvectr nowMap;
    void start(dvectr &nowMap);
    //获取当前深度下最优决策 传入地图 深度 传入时保证还有空位
    int evalToGo(int deep);
    //获取所有有效落子位置,不考虑周围九个格都没落子的点位，减少搜索空间
    vector<pii> getUsefulSteps();
    //评估当前局面Ai视角下的局面
    int evalNowSituation();
    //判断是否结束,没结束返回0,player输了返回-inf,赢了返回inf
    int isEnd();
    //判断是不是这个人赢了
    bool isHeWin(int view);
    //判断是不是平局了
    bool isTie();
    //这个位置要不要考虑放子(周围九格有落子过)
    bool putable(int x,int y);
};
bool MinMax::putable(int a,int b){
    for(int i=0;i<3;i++)
        for(int j=0;j<3;j++){
            int x=dx[i]+a,y=dy[i]+b;
            if(x<0||y<0||x>14||y>14)continue;
            //周围九格有落子过
            if(nowMap[x][y])return true;
        }
    return false;
}
void MinMax::start(dvectr &nowMap){
    this->nowMap=nowMap;
    evalToGo(1);
}
bool MinMax::isTie(){
    for (int i = 0; i < nowMap.size(); ++i)
        for (int j = 0; j < nowMap[i].size(); ++j)
            if (nowMap[i][j] == 0)
                return false;
    return true;
}
bool MinMax::isHeWin( int view) {
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
int MinMax::isEnd(){
    if(isHeWin(nowMap,player)) return inf;
    else if(isHeWin(nowMap,3-player)) return inf;
    else if(isTie(nowMap)) return TIE;
    return 0;
}
vector<pii> MinMax::getUsefulSteps(){
    vector<pii> q;
    for(int i=0;i<nowMap.size();i++)
        for(int j=0;j<nowMap[i].size();i++)
            //这一格为空且周围九格有过落子，减少搜索空间
            if(nowMap[i][j]==0&&putable(i,j))q.push_back({i,j});
    //队列为空随机走中间九格之一，且大概率走最中间
    if(q.empty()){
        int a=rand()%4,b=rand()%4,t=rand()&1;
        if(t)q.push({8,8});
        else q.push_back({8+dx[a],8+dy[i]});
    }
    return q;
}
int MinMax::evalToGo(int deep){
    if(deep==5)return evalNowSituation(nowMap);
    int end=isEnd(nowMap);
    if(end)return end;
    
}
int MinMax::evalNowSituation(){

}