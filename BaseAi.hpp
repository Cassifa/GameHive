#pragma once
#include <iostream>
#include<vector>
#include<ctime>
using namespace std;
#define pii pair<int,int>
//只支持双人回合制下棋
class BaseAi{
private:
    //Ai是否先手
    bool isAiFirst;
    //谁赢了
    int Winner=0;
    //当前是第几轮(每走一步是一轮)
    int nowRound=1;
protected:
    //地图 0-没走 1-Ai 2-玩家
    vector<vector<int>> map;
    //当前Ai决策
    pii finalDecide={1,1};

    //检查此位置是否合法
    virtual bool checkPlace(int x,int y)=0;
    //AI决定怎么走⭐
    virtual int evalToDo(vector<vector<int>> &nowMap,int nowVision)=0;
    //判断他此时有没有赢(Ai/玩家)
    virtual bool isHeWinner(int nowChecking)=0;

    //玩家移动
    void playerMove(int x,int y);
    //Ai移动
    void aiMove(int x,int y);
    
    void setWinner(int Winner);
    //轮数自增
    void increaseRound();
public:
    BaseAi(int x,int y): map(x + 1, std::vector<int>(y + 1, 0)) {};
    ~BaseAi(){};

    //开始游戏
    void startGame();

    //玩家选择位置
    virtual bool selectPlace(int x,int y)=0;

    //绘制当前地图状态
    virtual void showMap()=0;
    //判断要不要Ai走
    bool letAiMove();

    //判断是否结束
    virtual bool isEnd()=0;
    //打印游戏结果
    void printGameResult();

    //选择谁先走
    void setIsAiFirst(bool choice);
    int getRound();
    
};
//开始游戏并展示地图
void BaseAi::startGame(){
    cout<<"开始游戏!"<<endl;
    showMap();
    cout<<"X表示你的棋子,O表示Ai的棋子"<<endl;
}
//设置先后顺序
void BaseAi::setIsAiFirst(bool choice=false){
    this->isAiFirst=choice;
}
//玩家移动
void BaseAi::playerMove(int x,int y){
    map[x][y]=2;
    //展示的棋盘x轴与实际棋盘x轴方向相反，所以应该矫正为map.size()+1-x,但是数组下标0开始所以不+1
    //再交换x,y次序使之矫正为笛卡尔坐标系
    cout<<"你在"<<y+1<<","<<map.size()-x<<"落子了"<<endl;
    showMap();increaseRound();
}
//Ai移动
void BaseAi::aiMove(int x,int y){
    map[x][y]=1;
    cout<<"Ai在"<<y+1<<","<<map.size()-x<<"落子了"<<endl;
    showMap();increaseRound();
}
//打印游戏结果
void BaseAi::printGameResult(){
    if(Winner==1){
        cout<<"Ai赢了"<<endl;
    }else if(Winner==2){
        cout<<"你赢了"<<endl;
    }
    else cout<<"平局了"<<endl;
}
//判断是Ai不是该Ai走了
bool BaseAi::letAiMove(){
    //是Ai先走：1357
    //不是Ai先走：2468
    int t=isAiFirst+nowRound;
    if(!(t&1)){
        //Ai进行Min-Max决策,地图,深度
        evalToDo(map,1);
        aiMove(finalDecide.first,finalDecide.second);
        return true;
    }
    return false;
}
void BaseAi::setWinner(int Winner){
    this->Winner=Winner;
}
int BaseAi::getRound(){
    return this->nowRound;
}
void BaseAi::increaseRound(){
    this->nowRound++;
}