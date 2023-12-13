#pragma once
#include<vector>
#include<map>
#include<cstring>
#include<algorithm>
#include<windows.h>
#define ll long long
#define dvectr vector<vector<int>>
#define fi first
#define se second
using namespace std;
const int inf=1e6;//输了/赢了估值
const int TIE=-1;//平局估值
struct MinMax{

    pii finalDecision;
    ll countCnt=0;
    ll countYe=0;
    int player=1;
    int dx[24]={-1,-1,0,1,1,1,0,-1, -2,-2,-2,-1,0,1,2, 2,2,2,2,1,0,-1,-2,-2};
    int dy[24]={0,1,1,1,0,-1,-1,-1, 0,1,2,2,2,2,2,1,0, -1,-2,-2,-2,-2,-2,-1};
    dvectr nowMap;
    //估值表 _空 O自己 X对手
    map<string,int> scoreChart;
    int stander=0;
    
    void start(dvectr &nowMap);
    //获取当前深度下最优决策 传入深度,父亲局面分数 传入时保证还有空位
    int evalToGo(int deep,int lastScore);
    
    //判断是否结束,没结束返回0,player输了返回-inf,赢了返回inf
    int isEnd();
    //获取所有有效落子位置,不考虑周围九个格都没落子的点位，减少搜索空间
    //对周围子多的点优先搜索,便于剪枝
    vector<pii> getUsefulSteps();
    //评估当前局面Ai视角下的局面
    int evalNowSituation(int view);

    //判断是不是平局了
    bool isTie();
    //判断是不是这个人赢了
    bool isHeWin(int view);
    //这个位置要不要考虑放子(周围二十四格有落子过)
    bool putable(int x,int y);

    //计算x,y落子对view带来的增益
    int calculate(int x,int y,int view);
    //计算字符串价值
    int calculateLine(string &s);
    //计算player在x,y落子后收益
    int calculateRoundScore(int x,int y,int player);
    //初始化评分表
    void initScoreChart();
    void show(vector<pii> q);
    //获取place在view眼里转化为模板串后怎么填
    char getViewChar(int &place,int &view);

    int evalToGo(int deep);
    int evalToGo(int deep,int alpha, int beta);
};
void MinMax::show(vector<pii> q){
    for(auto t:q)
        nowMap[t.fi][t.se]=4;
    for(int i=1;i<=15;i++){
        for(int j=1;j<=15;j++)
            cout<<nowMap[i][j]<<" ";
        cout<<endl;
    }
    for(auto t:q)
        nowMap[t.fi][t.se]=0;
}
void MinMax::start(dvectr &nowMap){
    this->nowMap=nowMap;
    initScoreChart();
    //以前的局面已经无法改变，只考虑后续操作带来的收益
    evalToGo(1);
}
bool MinMax::putable(int a,int b){
    for(int i=0;i<24;i++){
        int x=dx[i]+a,y=dy[i]+b;
        if(x<1||y<1||x>15||y>15)continue;
        //周围二十四格有落子过
        if(nowMap[x][y])return true;
    }
    return false;
}
bool MinMax::isTie(){
    for(int i=1;i<nowMap.size();i++)
        for(int j=1;j<nowMap.size();j++)
            if(nowMap[i][j]==0)
                return false;
    return true;
}
bool MinMax::isHeWin(int view){
    //检查横向
    for(int i=1;i<nowMap.size();i++){
        for(int j=1;j<=11;j++) 
            if (nowMap[i][j]==view&&nowMap[i][j+1]==view&&nowMap[i][j+2]==view&&
                nowMap[i][j+3]==view&&nowMap[i][j+4]==view)
                return true;
    }
    //检查纵向
    for(int i=1;i<=11;i++){
        for(int j=1;j<nowMap.size();j++)
            if (nowMap[i][j]==view&&nowMap[i+1][j]==view&&nowMap[i+2][j]==view&&
                nowMap[i+3][j]==view&&nowMap[i+4][j]==view)
                return true;
    }
    //检查主对角线
    for(int i=1;i<=11;i++){
        for(int j=1;j<=11;j++){
            if(nowMap[i][j]==view&&nowMap[i+1][j+1]==view&&nowMap[i+2][j+2]==view&&
                nowMap[i+3][j+3]==view&&nowMap[i+4][j+4]==view){
                return true;
            }
        }
    }
    //检查副对角线
    for(int i=1;i<=11;i++){
        for(int j=5;j<=15;j++){
            if(nowMap[i][j]==view&&nowMap[i+1][j-1]==view&&nowMap[i+2][j-2]==view&&
                nowMap[i+3][j-3]==view&&nowMap[i+4][j-4]==view){
                return true;
            }
        }
    }
    //未连成五子
    return false; 
}
int MinMax::isEnd(){
    if(isHeWin(player)) return inf;
    else if(isHeWin(3-player)) return -inf;
    else if(isTie()) return TIE;
    return 0;
}
vector<pii> MinMax::getUsefulSteps(){
    vector<pii> q;
    for(int i=1;i<nowMap.size();i++)
        for(int j=1;j<nowMap[i].size();j++)
            //这一格为空且周围24格有过落子，减少搜索空间
            if(nowMap[i][j]==0&&putable(i,j))q.push_back({i,j});
    //队列为空随机走中间九格之一，且大概率走最中间
    if(q.empty()){
        // int a=rand()%4,b=rand()%4,t=rand()%10;
        // if(t<7)q.push_back({8,8});
        // else q.push_back({8+dx[a],8+dy[b]});
        q.push_back({8,8});
    }
    return q;
}
//四个搜索函数
//过程估值
int MinMax::evalToGo(int deep,int lastScore){
    //此局面游戏已经结束
    int end=isEnd();
    if(end)return end;
    //达到最大深度
    if(deep==5)return lastScore;
    //是否是player自己在挑选局面(Max层)
    bool isMe=deep&1;
    int nowScore;
    vector<pii> q=getUsefulSteps();
    // if(deep<=3){
    //     cout<<"Deep: "<<deep<<endl;
        // show(q);
        // for(auto t:q)
        //     cout<<t.fi<<" "<<t.se<<endl;
    // }
    if(isMe){
        nowScore=-inf;
        for(auto t:q){
            //计算在这里落子后局面收益(player视角:下一层局面估值)
            int nowRoundScore=calculateRoundScore(t.fi,t.se,player)+lastScore;
            int leadToScore=evalToGo(deep+1,nowRoundScore);
            if(leadToScore>nowScore){
                nowScore=leadToScore;
                finalDecision=t;
            }
            //恢复现场
            nowMap[t.fi][t.se]=0;
        }
    }
    else{
        nowScore=inf;
        for(auto t:q){
            //计算在这里落子收益
            int nowRoundScore=calculateRoundScore(t.fi,t.se,3-player)+lastScore;
            int leadToScore=evalToGo(deep+1,nowRoundScore);
            if(leadToScore<nowScore){
                nowScore=leadToScore;
                finalDecision=t;
            }
            //恢复现场
            nowMap[t.fi][t.se]=0;
        }
    }
    return nowScore;
}
//叶子节点估值
int MinMax::evalToGo(int deep,int alpha, int beta){
    countCnt++;
    //此局面游戏已经结束
    int end=isEnd();
    if(end){
        countCnt--;
        countYe++;
        return end;
    }
    //达到最大深度
    if(deep==4){
        countCnt--;
        countYe++;
        return evalNowSituation(player);
    }
    //是否是player自己在挑选局面(Max层)
    bool isMe=deep&1;
    int nowScore;
    vector<pii> q=getUsefulSteps();
    if(isMe){
        nowScore=-inf;
        for(auto t:q){
            //计算在这里落子后局面收益(player视角:下一层局面估值)
            nowMap[t.fi][t.se]=player;
            int nowRoundScore=evalToGo(deep+1,alpha,beta);
            alpha=max(alpha,nowRoundScore);

            //alpha剪枝
            if(nowRoundScore>=beta)break;
            if(nowRoundScore>nowScore){
                nowScore=nowRoundScore;
                finalDecision=t;
            }

            //恢复现场
            nowMap[t.fi][t.se]=0;
        }
    }
    else{
        nowScore=inf;
        for(auto t:q){
            //计算在这里落子后局面收益(player视角:下一层局面估值)
            nowMap[t.fi][t.se]=3-player;
            int nowRoundScore=evalToGo(deep+1,alpha,beta);
            beta=min(beta,nowRoundScore);

            //beta剪枝
            if(nowRoundScore<=alpha)break;
            if(nowRoundScore<nowScore){
                nowScore=nowRoundScore;
                finalDecision=t;
            }
            
            //恢复现场
            nowMap[t.fi][t.se]=0;
        }
    }
    return nowScore;
}
int MinMax::evalToGo(int deep){
    countCnt++;
    //此局面游戏已经结束
    int end=isEnd();
    if(end){
        countCnt--;
        countYe++;
        return end;
    }
    //达到最大深度
    if(deep==4){
        countCnt--;
        countYe++;
        return evalNowSituation(player);
    }
    //是否是player自己在挑选局面(Max层)
    bool isMe=deep&1;
    int nowScore;
    vector<pii> q=getUsefulSteps();
    if(isMe){
        nowScore=-inf;
        for(auto t:q){
            //计算在这里落子后局面收益(player视角:下一层局面估值)
            nowMap[t.fi][t.se]=player;
            int nowRoundScore=evalToGo(deep+1);
            if(nowRoundScore>nowScore){
                nowScore=nowRoundScore;
                finalDecision=t;
            }
            //恢复现场
            nowMap[t.fi][t.se]=0;
        }
    }
    else{
        nowScore=inf;
        for(auto t:q){
            //计算在这里落子后局面收益(player视角:下一层局面估值)
            nowMap[t.fi][t.se]=3-player;
            int nowRoundScore=evalToGo(deep+1);
            if(nowRoundScore<nowScore){
                nowScore=nowRoundScore;
                finalDecision=t;
            }
            //恢复现场
            nowMap[t.fi][t.se]=0;
        }
    }
    return nowScore;
}
int MinMax::calculateRoundScore(int x,int y,int player){
    //决定Ai的风格偏好,加起来权重为10
    int defendBias=4,attackBias=6;

    //此位置原始效益
    int oldScorePlayer=calculate(x,y,player);
    int oldScoreOpponent=calculate(x,y,3-player);

    //此位置防御增益
    nowMap[x][y]=3-player;
    int defend=calculate(x,y,3-player)-oldScoreOpponent;

    //此位置攻击增益
    nowMap[x][y]=player;
    int attack=calculate(x,y,player)-oldScorePlayer;

    // 落子收益:攻击收益+防御收益
    int nowRoundScore=attack*attackBias+defend*defendBias;
    return nowRoundScore;
}
int MinMax::calculate(int x,int y,int view){
    int ans=0;
    string s;
    //按行获取
    for(int i=1;i<nowMap.size();i++){
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
    for(int i=1;i<nowMap.size();i++){
        int t=nowMap[i][y];
        if(!t)s+='_';
        else if(t==view)s+='O';
        else s+='X';
    }
    ans+=calculateLine(s);
    s="";
    //按主对角线获取
    for(int i=1;i<nowMap.size();i++)
        for(int j=1;j<nowMap.size();j++){
            if(x-i!=y-i)continue;
            int t=nowMap[i][j];
            if(!t)s+='_';
            else if(t==view)s+='O';
            else s+='X';
            break;
        }
    ans+=calculateLine(s);
    s="";
    //按照副对角线获取
    for(int i=1;i<nowMap.size();i++)
        for(int j=1;j<nowMap.size();j++){
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
int MinMax::calculateLine(string &s){
    int ans=0;
    for(auto t:scoreChart){
        int index=0,cnt=0;
        //寻找当前评估模式串t.fi在这一串中出现了几次
        while((index=s.find(t.fi,index))!=-1){
            cnt++;index+=t.fi.size();
            if(index>=s.size())break;
        }
        //估值=出现次数*价值
        ans+=cnt*t.se;
        // if(cnt&&(!stander))cout<<s<<" "<<t.fi<<" "<<t.se<<"    "<<cnt<<endl;
    }
    // if(ans){
    //     stander=1;
    // }
    return ans;
}
char MinMax::getViewChar(int &place,int &view){
    //没填
    if(!place)return '_';
    //当前估分人填了
    else if(place==view)return 'O';
    //对方填了
    else return 'X';
}
int MinMax::evalNowSituation(int view){
    return 0;
    int ans=0;
    string s="";
    //估值行
    for(int i=1;i<nowMap.size();i++){
        for(auto t:nowMap[i]){
            s+=getViewChar(t,view);
        }
        ans+=calculateLine(s);
        s="";
    }
    //估值列
    for(int i=1;i<nowMap.size();i++){
        for(int j=1;j<nowMap.size();j++){
            s+=getViewChar(nowMap[j][i],view);
        }
        ans+=calculateLine(s);
        s="";
    }
    //估值主对角线
    //左下半棋盘
    for(int cnt=2;cnt<=16;cnt++){
        if(cnt<6)continue;
        int i=1,j=cnt-i;//1,15~15,1
        while(j){
            s+=getViewChar(nowMap[i][j],view);
            i++;j--;
        }
        ans+=calculateLine(s);
        s="";
    }
    //右上半棋盘
    for(int i,j=15,t=2;t<=11;t++,j=15){
        i=t;
        while(i<16){
            s+=getViewChar(nowMap[i][j],view);
            i++;j--;
        }
        ans+=calculateLine(s);
        s="";
    }
    //估值副对角线
    for(int b=-14;b<=14;b++){//确定b值
        for(int i=1;i<=15;i++)//枚举x
            for(int j=1;j<=15;j++){//枚举y
                if(j==i+b){//y==-x+b则为斜率b下的一个点
                    s+=getViewChar(nowMap[i][j],view);
                    break;
                }
            }
        ans+=calculateLine(s);
        s="";
    }
    return ans;
}
void MinMax::initScoreChart(){
    //棋局参考文章https://blog.csdn.net/weixin_71872462/article/details/128574864?ops_request_misc=&request_id=&biz_id=102&utm_term=%E4%BA%94%E5%AD%90%E6%A3%8BAi%E5%86%B2%E5%9B%9B&utm_medium=distribute.pc_search_result.none-task-blog-2~all~sobaiduweb~default-0-128574864.142^v96^pc_search_result_base1&spm=1018.2226.3001.4187

    //四
    //活四
    scoreChart["_OOOO_"]=5000;
    //冲四
    scoreChart["O_OOO"]=700;
    scoreChart["_OOOOX"]=1000;
    scoreChart["OO_OO"]=700;

    //三
    //活三(可成活四)
    scoreChart["_OOO_"]=500;
    scoreChart["O_OO"]=150;
    //眠三
    scoreChart["__OOOX"]=100;
    scoreChart["_O_OOX"]=80;
    scoreChart["_OO_OX"]=60;
    scoreChart["O__OO"]=60;
    scoreChart["O_O_O"]=60;
    scoreChart["X_OOO_X"]=60;

    //二
    //活二
    scoreChart["__OO__"]=50;
    scoreChart["_O_O_"]=20;
    scoreChart["O__O"]=20;
    //眠二
    scoreChart["___OOX"]=10;
    scoreChart["__O_OX"]=10;
    scoreChart["_O__OX"]=10;
    scoreChart["O___O"]=10;

    //考虑翻过来的情况
    map<string,int> st=scoreChart;
    for(auto t:st){
        string s=t.fi;
        reverse(s.begin(),s.end());
        scoreChart[s]=t.se;
    }
}