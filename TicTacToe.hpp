#include<queue>
#include<vector>
#include<algorithm>
#include"BaseAi.hpp"
#define pii pair<int,int>
class TicTacToe :public BaseAi{
private:
    bool isHeWinner(int nowChecking)override;
    //检查玩家位置合法性
    bool checkPlace(int x,int y)override;
    //AI决定怎么走⭐
    pii evalToDo(vector<vector<int>> nowMap)override;
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
    for(int i=1;i<=3;i++){
        for(int j=1;j<=3;j++){
            char now;
            if(map[i][j]==0)now='*';
            else if(map[i][j]==2)now='X';
            else now='O';
            cout<<now;
        }
        cout<<endl;
    }
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
//使用决策树决定Ai下棋位置⭐
pii TicTacToe::evalToDo(vector<vector<int>> nowMap){
    for(int i=1;i<=3;i++)   
        for(int j=1;j<=3;j++)
            if(map[i][j]==0) return pii({i,j});
    return pii({1,1});
}