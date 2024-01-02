#pragma once
#include<vector>
#include<map>
#include<cstring>
#include<algorithm>
#define ll long long
#define dvectr vector<vector<short>>
#define fi first
#define se second
using namespace std;
const int inf=1e6;//输了/赢了估值
const int TIE=-1;//平局估值
class MinMax:public baseGoBangAi{
private:
    //最终决定
    pii finalDecision;
    //节点计数
    ll countCnt=0,countYe=0;
    //决定Ai的风格偏好,加起来权重为10
    int defendBias=4,attackBias=6;
    //当前游戏玩家(1为Ai默认身份)
    int player=1;
    //最大搜索深度
    int maximumDeep=6;
    //方向向量
    int dx[24]={-1,-1,0,1,1,1,0,-1, -2,-2,-2,-1,0,1,2, 2,2,2,2,1,0,-1,-2,-2};
    int dy[24]={0,1,1,1,0,-1,-1,-1, 0,1,2,2,2,2,2,1,0, -1,-2,-2,-2,-2,-2,-1};
    //当前棋盘
    dvectr nowMap;
    //估值表 _空 O自己 X对手
    map<string,int> scoreChart;
    int stander=0;
    

    void start(dvectr &nowMap);

    //启发式获取可行落子位置
    vector<pii> getUsefulSteps();

    //四个搜索函数-是否在过程中估值/是否带搜索剪枝
    int evalToGo(int deep,int lastScore);
    // int evalToGo(int deep,int lastScore,int alpha, int beta);
    int evalToGo(int deep);
    int evalToGo(int deep,int alpha, int beta);
    
    
    //对view视角下的局面攻击分数估值
    int evalNowSituation(int view);
    //获取在X,Y落子带来的总收益(带风险偏好)
    int evalXYTRoundScore(int x,int y,int player);
    //计算view视角下在X,Y落子获得的攻击分数
    int calculateXY(int x,int y,int view);


    //计算一条String的攻击估值
    int calculateLine(string &s);
    //初始化评分表
    void initScoreChart();
    //判断棋子place在view视角下对应什么字符
    char getViewChar(int &place,int &view);
    //判断是不是平局了
    bool isTie();
    //判断是不是这个人赢了
    bool isHeWin(int view);
    //判断是否结束,没结束返回0,player输了返回-inf,赢了返回inf
    int isEnd();
    //测试函数,打印二维数组
    void show(dvectr q);

};
void MinMax::start(dvectr &nowMap){
    this->nowMap=nowMap;
    initScoreChart();
    //以前的局面已经无法改变，只考虑后续操作带来的收益
    // evalToGo(1,0);
    // evalToGo(1,0,-inf,inf);
    // evalToGo(2);
    evalToGo(4,-inf,inf);
}
\
//启发式获取可行落子位置
vector<pii> MinMax::getUsefulSteps(){
    //最终结果
    vector<pii> q;
    //优先级表并记录是否合法
    dvectr st=nowMap;
    for(int i=1;i<nowMap.size();i++)
        for(int j=1;j<nowMap[i].size();j++)
            if(nowMap[i][j])st[i][j]=-inf;
    
    //获取合法点位并加权
    for(int i=1;i<nowMap.size();i++)
        for(int j=1;j<nowMap[i].size();j++){
            //没下棋,忽略
            if(!nowMap[i][j])continue;
            //添加权重
            for(int k=0;k<24;k++){
                int x=dx[k]+i,y=dy[k]+j;
                if(x<1||y<1||x>15||y>15)continue;
                //内圈8个优先级更高,外圈优先级低一点
                int power=k<8?2:1;
                //周围二十四格有落子过
                st[x][y]+=power;
            }
        }

    //记录所有价值表并按价值排序
    vector<pair<int,pii>> temp;
    for(int i=1;i<nowMap.size();i++)
        for(int j=1;j<nowMap[i].size();j++)
            if(st[i][j]>0)temp.push_back({st[i][j],{i,j}});
    sort(temp.begin(),temp.end());
    reverse(temp.begin(),temp.end());
    //将有价值的节点作为答案返回
    for(auto t:temp) q.push_back(t.se);
    
    //队列为空随机走中间九格之一，且大概率走最中间
    if(q.empty()){
        int a=rand()%4,b=rand()%4,t=rand()%10;
        if(t<80)q.push_back({8,8});
        else q.push_back({8+dx[a],8+dy[b]});
    }
    return q;
}

//四个搜索函数
//两个过程中估值
int MinMax::evalToGo(int deep,int lastScore){
    //此局面游戏已经结束
    int end=isEnd();
    if(end)return end;
    //达到最大深度
    if(deep==6)return lastScore;
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
            int nowRoundScore=evalXYTRoundScore(t.fi,t.se,player)+lastScore;
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
            int nowRoundScore=evalXYTRoundScore(t.fi,t.se,3-player)+lastScore;
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

//两个叶子节点估值⭐
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
    if(!deep){
        countCnt--;
        countYe++;
        int a=attackBias*evalNowSituation(player),b=defendBias*evalNowSituation(3-player);
        int ans=a-b;
        // show(nowMap);
        // cout<<endl;
        return ans;
    }
    //是否是player自己在挑选局面(Max层)
    bool isMe=deep+1&1;
    // int nowScore;
    vector<pii> q=getUsefulSteps();//更新alpha/beta且没有被剪枝则采纳为最终结果
    if(isMe){
        for(auto t:q){
            //计算在这里落子后局面收益(player视角:下一层局面估值)maxDepperBeta
            nowMap[t.fi][t.se]=player;
            int nowRoundScore=evalToGo(deep-1,alpha,beta);
            nowMap[t.fi][t.se]=0;
            
            if(nowRoundScore>alpha){
                if(nowRoundScore>beta)return alpha;
                alpha=nowRoundScore;
                finalDecision=t;
            }
        }
        return alpha;
    }
    else{
        int nowScore=inf;
        for(auto t:q){
            //计算在这里落子后局面收益(对手视角:下一层局面估值)
            nowMap[t.fi][t.se]=3-player;
            int nowRoundScore=evalToGo(deep-1,alpha,beta);
            nowMap[t.fi][t.se]=0;
            
            if(nowRoundScore<beta){
                if(nowRoundScore<alpha)return beta;
                beta=nowRoundScore;
                finalDecision=t;
            }
        }
        return beta;
    }
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
    if(!deep){
        countCnt--;
        countYe++;
        int a=4*evalNowSituation(player),b=6*evalNowSituation(3-player);
        int ans=a-b;
        return ans;
    }
    //是否是player自己在挑选局面(Max层)
    bool isMe=deep+1&1;
    int nowScore;
    vector<pii> q=getUsefulSteps();
    if(isMe){
        nowScore=-inf;
        for(auto t:q){
            //计算在这里落子后局面收益(player视角:下一层局面估值)
            nowMap[t.fi][t.se]=player;
            int nowRoundScore=evalToGo(deep-1);
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
            int nowRoundScore=evalToGo(deep-1);
            //恢复现场
            nowMap[t.fi][t.se]=0;
        }
    }
    return nowScore;
}

//收益计算函数
//对view视角下的局面攻击分数估值
int MinMax::evalNowSituation(int view){
    // return 0;
    int ans=0;
    string s="";
    //估值行
    for(int i=1;i<nowMap.size();i++){
        bool can=false;
        for(auto t:nowMap[i]){
            char now=getViewChar(t,view);
            s+=now;
            if(now!='_')can=true;
        }
        if(can)ans+=calculateLine(s);
        s="";
    }
    //估值列
    for(int i=1;i<nowMap.size();i++){
        bool can=false;
        for(int j=1;j<nowMap.size();j++){
            char now=getViewChar(nowMap[j][i],view);
            s+=now;
            if(now!='_')can=true;
        }
        if(can)ans+=calculateLine(s);
        s="";
    }
    //估值主对角线
    //左下半棋盘
    for(int cnt=6;cnt<=16;cnt++){
        bool can=false;
        int i=1,j=cnt-i;//1,15~15,1
        while(j){
            char now=getViewChar(nowMap[i][j],view);
            s+=now;
            if(now!='_')can=true;
            i++;j--;
        }
        if(can)ans+=calculateLine(s);
        s="";
    }
    //右上半棋盘
    for(int i,j=15,t=2;t<=11;t++,j=15){
        bool can=false;
        i=t;
        while(i<16){
            char now=getViewChar(nowMap[i][j],view);
            s+=now;
            if(now!='_')can=true;
            i++;j--;
        }
        if(can)ans+=calculateLine(s);
        s="";
    }
    //估值副对角线
    for(int b=-14;b<=14;b++){//确定b值
        bool can=false;
        for(int i=1;i<=15;i++)//枚举x
            for(int j=1;j<=15;j++){//枚举y
                if(j==i+b){//y==-x+b则为斜率b下的一个点
                    char now=getViewChar(nowMap[i][j],view);
                    s+=now;
                    if(now!='_')can=true;
                    break;
                }
            }
        if(can)ans+=calculateLine(s);
        s="";
    }
    return ans;
}

//获取在X,Y落子带来的总收益(带风险偏好)
int MinMax::evalXYTRoundScore(int x,int y,int player){

    //此位置原始效益
    int oldScorePlayer=calculateXY(x,y,player);
    int oldScoreOpponent=calculateXY(x,y,3-player);

    //此位置防御增益
    nowMap[x][y]=3-player;
    int defend=calculateXY(x,y,3-player)-oldScoreOpponent;

    //此位置攻击增益
    nowMap[x][y]=player;
    int attack=calculateXY(x,y,player)-oldScorePlayer;

    // 落子收益:攻击收益+防御收益
    int nowRoundScore=attack*attackBias+defend*defendBias;
    return nowRoundScore;
}

//计算view视角下在X,Y落子获得的攻击分数
int MinMax::calculateXY(int x,int y,int view){
    int ans=0;
    string s;
    bool can=false;
    //按行获取
    for(int i=1;i<nowMap.size();i++){
        int t=nowMap[x][i];
        //有子才去估值
        char now=getViewChar(t,view);
        s+=now;
        if(now!='_')can=true;
    }
    if(can)ans+=calculateLine(s);
    s="";
    //按列获取
    can=false;
    for(int i=1;i<nowMap.size();i++){
        int t=nowMap[i][y];
        char now=getViewChar(t,view);
        s+=now;
        if(now!='_')can=true;
    }
    if(can)ans+=calculateLine(s);
    s="";
    //按主对角线获取
    can=false;
    for(int i=1;i<nowMap.size();i++)
        for(int j=1;j<nowMap.size();j++){
            if(x-i!=y-i)continue;
            int t=nowMap[i][j];
            char now=getViewChar(t,view);
            s+=now;
            if(now!='_')can=true;
            break;
        }
    if(can)ans+=calculateLine(s);
    s="";
    //按照副对角线获取
    can=false;
    for(int i=1;i<nowMap.size();i++)
        for(int j=1;j<nowMap.size();j++){
            if(i+j!=x+y)continue;
            int t=nowMap[i][y];
            char now=getViewChar(t,view);
            s+=now;
            if(now!='_')can=true;
            break;
        }
    ans+=calculateLine(s);
    return ans;
}


//辅助函数
//计算一条String的攻击估值
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
    }
    return ans;
}

//初始化攻击估值表
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
    scoreChart["_OOO_"]=800;
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

//判断棋子place在view视角下对应什么字符
char MinMax::getViewChar(int &place,int &view){
    //没填
    if(!place)return '_';
    //当前估分人填了
    else if(place==view)return 'O';
    //对方填了
    else return 'X';
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

//测试函数,打印二维数组
void MinMax::show(dvectr nowMap){
    for(int i=1;i<=15;i++){
        for(int j=1;j<=15;j++)
            cout<<nowMap[i][j]<<" ";
        cout<<endl;
    }
}