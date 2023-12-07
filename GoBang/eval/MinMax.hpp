#pragma once
#include<vector>
#include<map>
#include<cstring>
#include<algorithm>
#define ll long long
#define dvectr vector<vector<int>>
#define fi first
#define se second
using namespace std;
const int inf=1e6;//输了/赢了估值
const int TIE=-10;//平局估值
struct MinMax{

    pii finalDecision;
    int player=1;
    int dx[24]={-1,-1,0,1,1,1,0,-1, -2,-2,-2,-1,0,1,2, 2,2,2,2,1,0,-1,-2,-2}
    int dy[24]={0,1,1,1,0,-1,-1,-1, 0,1,2,2,2,2,2,1,0, -1,-2,-2,-2,-2,-2,-1};
    dvectr nowMap;
    map<string,int> scoreChart;
    
    void start(dvectr &nowMap);
    //获取当前深度下最优决策 传入深度,父亲局面分数 传入时保证还有空位
    int evalToGo(int deep,int lastScore);
    
    //判断是否结束,没结束返回0,player输了返回-inf,赢了返回inf
    int isEnd();
    //获取所有有效落子位置,不考虑周围九个格都没落子的点位，减少搜索空间
    vector<pii> getUsefulSteps();
    //评估当前局面Ai视角下的局面
    int evalNowSituation();

    //判断是不是平局了
    bool isTie();
    //判断是不是这个人赢了
    bool isHeWin(int view);
    //这个位置要不要考虑放子(周围二十四格有落子过)
    bool putable(int x,int y);

    //计算x,y落子带来的增益
    int calculate(int x,int y,int view);
    //计算字符串价值
    int calculateLine(String &s);
    //初始化评分表
    void initScoreChart();
};

void MinMax::start(dvectr &nowMap){
    this->nowMap=nowMap;
    initScoreChart();
    evalToGo(1,0);
}
bool MinMax::putable(int a,int b){
    for(int i=0;i<24;i++){
        int x=dx[i]+a,y=dy[i]+b;
        if(x<0||y<0||x>14||y>14)continue;
        //周围二十四格有落子过
        if(nowMap[x][y])return true;
    }
    return false;
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
        int a=rand()%4,b=rand()%4,t=rand()%10;
        if(t<7)q.push({8,8});
        else q.push_back({8+dx[a],8+dy[i]});
    }
    return q;
}
int MinMax::evalToGo(int deep,int lastScore){
    //此局面游戏已经结束
    int end=isEnd(nowMap);
    if(end)return end;
    //达到最大深度
    if(deep==5)return lastScore;
    //是否是自己在挑选局面(Max层)
    bool isMe=deep&1;
    int nowScore;
    vector<pii> q=getUsefulSteps();
    if(isMe){
        nowScore=-inf;
        for(auto t:q){
            int x=t.fi,y=t.se;
            //此位置原始效益
            int oldScore=calculate(x,y,player);
            nowMap[x][y]=4-player;
            //此位置防御增益
            int defend=calculate(x,y,4-player)-oldScore;
            nowMap[x][y]=player;
            //此位置攻击增益
            int attack=calculate(x,y,player)-oldScore;
            //此局面收益
            int nowRoundScore=lastScore+defend+attack;
            if(nowRoundScore>nowScore){
                nowScore=nowRoundScore;
                finalDecision=t;
            }
        }
    }
    else{
        nowScore=inf;

    }
    return nowScore;
}
int MinMax::calculate(int x,int y,int view){
    int ans=0;
    string s;
    //按行获取
    for(int i=0;i<nowMap.size();i++){
        int t=nowMap[x][i];
        //没填
        if(!t)s+='_';
        //当前估分人填了
        else if(t==view)s+='O';
        //对方填了
        else s+='X';

    }
    ans+=calculateLine(s);
    s="";
    //按列获取
    for(int i=0;i<nowMap.size();i++){
        int t=nowMap[i][y];
        if(!t)s+='_';
        else if(t==view)s+='O';
        else s+='X';
    }
    ans+=calculateLine(s);
    s="";
    //按主对角线获取
    for(int i=0;i<nowMap.size();i++)
        for(int j=0;j<nowMap.size();j++){
            int a=x-i,b=y-j;
            if(a<0||b<0||a!=b)continue;
            int t=nowMap[a][b];
            if(!t)s+='_';
            else if(t==view)s+='O';
            else s+='X';
            break;
        }
    ans+=calculateLine(s);
    s="";
    //按照副对角线获取
    for(int i=0;i<nowMap.size();i++)
        for(int j=0;j<nowMap.size();j++){
            if(i+j!=x+y)continue;
            int t=nowMap[i][y];
            if(!t)s+='_';
            else if(t==view)s+='O';
            else s+='X';
            break;
        }
    ans+=calculateLine(s);
    return ans;
}
int MinMax::calculateLine(String &s){
    int ans=0,size=s.size();
    for(auto t:scoreChart){
        int cnt=0,len=t.size();
        for(int i=0;i<size-len;i++)
            if(s.substr(i,len)==t.fi)cnt++;
        //估值=出现次数*价值
        ans+=cnt*t.se;
    }
    return ans;
}
void MinMax::initScoreChart(){

}