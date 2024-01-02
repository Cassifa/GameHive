#pragma once
#include<vector>
#include<iostream>
#define ll long long
#define dvectr vector<vector<short>>
#define fi first
#define se second
using namespace std;
//传入地图要求估值/要求结束游戏
class baseGoBangAi{
    protected:
        pair<short,short> playerLastMove;
    public:
        baseGoBangAi(){};
        // 注意：若析构函数不为虚函数，则子类的析构函数不会被调用！！
        virtual ~baseGoBangAi()=0;
        //初始化地图
        virtual void setBeginningState(dvectr map)=0;
        //收到玩家移动消息
        virtual void sendPlayerMoveMessage(int x,int y);
        //获取下一步 保证当前局面一定是玩家刚走(不是棋局刚开始场面无棋子)
        virtual pair<short,short> evalToGo()=0;
        //结束游戏
        virtual void end()=0;

};
void baseGoBangAi::sendPlayerMoveMessage(int x,int y){
    playerLastMove={x,y};
}
