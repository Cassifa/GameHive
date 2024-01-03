#pragma once
#include<map>
#include<cstring>
#include<algorithm>
#include"../baseGoBangAi.hpp"
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
        char getViewChar(short &place,int &view);
        //判断是不是平局了
        bool isTie();
        //判断是不是这个人赢了
        bool isHeWin(int view);
        //判断是否结束,没结束返回0,player输了返回-inf,赢了返回inf
        int isEnd();
        //测试函数,打印二维数组
        void show(dvectr q);
    public:
        MinMax(){};
        ~MinMax() {cout<<"子类虚函数执行";};
        //初始化地图
        void setBeginningState(dvectr map)override;
        //收到玩家移动消息
        void sendPlayerMoveMessage(int x,int y)override;
        //获取下一步 保证当前局面一定是玩家刚走(不是棋局刚开始场面无棋子)
        pii evalToGo()override;
        //结束游戏
        void end()override{};

};