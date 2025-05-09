#pragma once
#include<vector>
#include"../BaseAi.hpp"
class TicTacToe :public BaseAi{
private:
    bool isHeWinner(int nowChecking)override;
    //检查玩家位置合法性
    bool checkPlace(int x,int y)override;
    //AI决定怎么走⭐
    int evalToDo(vector<vector<short>> &nowMap,int nowVision)override;
public:
    //调用构造函数,新建地图
    TicTacToe():BaseAi(3,3){};
    ~TicTacToe(){};
    //玩家选择位置
    bool selectPlace(int x,int y)override;

    //绘制当前地图状态
    void showMap()override;

    //判断是否结束
    bool isEnd()override;
};
//向玩家展示当前地图
void TicTacToe::showMap(){
	int i=0,j=0,row=3,col=3;
	for (i=1;i<=row;i++){
        cout<<"  ";
		for (j=1;j<=col;j++)
			printf(" ---");
		printf("\n");
		for (j=1;j<=col;j++){
            if(j==1)//打印每一行的数字
                printf("%2d",row-i+1);
            char c=' ';
            if(map[i][j]==1)c='O';
            else if(map[i][j]==2)c='X';
			printf("| %c ", c);
            if (j==col)
                printf("|");
        }
        if(i==row){
            printf("\n  ");
                for(int j=0;j<row;j++)
                printf(" ---");
        }
        printf("\n");
	}
    cout<<"  ";
    for (i =1;i<=row;i++)
        printf("  %d ",i);
    printf("\n");
	
}
//检查落子后游戏是不是结束了
bool TicTacToe::isEnd(){
    if(isHeWinner(1)) this->setWinner(1);
    else if(isHeWinner(2)) this->setWinner(2);
    else if(isHeWinner(3)) this->setWinner(3);
    else return false;
    return true;
}
//检查他是不是赢了
bool TicTacToe::isHeWinner(int now){//0-Ai 1-玩家
    int cnt=0;
    //无棋可下，平局
    if(now==3){
        for(int i=1;i<=3;i++)
            for(int j=1;j<=3;j++)
                if(map[i][j]==0)cnt++;
        if(cnt)cnt=0;
        else return true;
    }
    for(int i=1;i<=3;i++){
        for(int j=1;j<=3;j++)
            cnt+=(now==map[i][j]);
        if(cnt==3)return true;
        else cnt=0;
    }
    for(int j=1;j<=3;j++){
        for(int i=1;i<=3;i++)
            cnt+=(now==map[i][j]);
        if(cnt==3)return true;
        else cnt=0;
    }
    for(int j=1;j<=3;j++){
        for(int i=1;i<=3;i++)
            if(i==j)cnt+=(now==map[i][j]);
    }
    if(cnt==3)return true;
    else cnt=0;
    for(int j=1;j<=3;j++){
        for(int i=1;i<=3;i++)
            if(i+j==4)cnt+=(now==map[i][j]);
    }
    if(cnt==3)return true;
    return false;
}
//玩家选择位置
bool TicTacToe::selectPlace(int x,int y){
    swap(x,y);x=4-x;
    if(checkPlace(x,y)){
        playerMove(x,y);
        return true;
    }
    return false;
}
//检查这个位置是否合法
bool TicTacToe::checkPlace(int x,int y){
    if(x>3||x<1)return false;
    if(y>3||y<1)return false;
    return map[x][y]==0;
}
//在决策树上负极大值搜索 决定Ai下棋位置,返回当前局面当前视角下能得到的最大分数⭐
int TicTacToe::evalToDo(vector<vector<short>> &nowMap,int deep){
    int nowVision=deep&1;
    if(!nowVision)nowVision=2;
    //自己赢了
    if(isHeWinner(nowVision))return 1e8;
    //对方赢了
    else if(isHeWinner(3-nowVision))return -1e8;
    //平局
    else if(isHeWinner(3))return 0;
    //取极大值
    int nowScore=-1e9;
    pii nowDecide;
    vector<pii> useful;
    // 获取当前局面下所有可能的移动
    for(int i=1;i<=3;i++)
        for(int j=1;j<=3;j++)
            if (nowMap[i][j]==0)
                useful.push_back({i, j});
    for(auto t:useful){
        nowMap[t.first][t.second]=nowVision;
        //负极大值
        int score=-1*evalToDo(nowMap,deep+1);
        //引入随机性
        if(score>nowScore||((score==nowScore)&&(rand()&1))){
            nowDecide=t;
            nowScore=score;
        }
        nowMap[t.first][t.second]=0;
    }
    finalDecide=nowDecide;
    return nowScore;
}