#pragma once
#include<ctime>
#include<queue>
#include<cmath>
#define ll long long
#define dvectr vector<vector<short>>
#define pii pair<short,short>
#define fi first
#define se second
using namespace std;
const int Bais=2;
int dx[24]={-1,-1,0,1,1,1,0,-1, -2,-2,-2,-1,0,1,2, 2,2,2,2,1,0,-1,-2,-2};
int dy[24]={0,1,1,1,0,-1,-1,-1, 0,1,2,2,2,2,2,1,0, -1,-2,-2,-2,-2,-2,-1};
struct MCTSNode{
    //节点途径次数
    ll n=0;
    //总价值
    double value=0;
    //父节点
    MCTSNode *father;
    //节点地图
    dvectr map;
    //子节点
    vector<MCTSNode*> sons;
    
    
    //父节点为第deep层
    double getUCB(int deep){
        if(n==0) return 1e100;
        int fatherN=father->n;
        double valuePart=value/n;
        if((deep&1)==0)valuePart*=-1;
        return valuePart+sqrt(Bais*(fatherN)/n);
    }

    //player赢了返回1,输了/平局返回-1,没结束0
    int isEnd(int player){
        if(isHeWin(player))return 1;
        if(isHeWin(3-player))return -1;
        for(int i=1;i<map.size();i++)
            for(int j=1;j<map.size();j++)
                if(!map[i][j])return 0;
        return -1;
    }

    //拓展自己的儿子
    void getSons(int player){
        vector<pii> q=getUsefulSteps();
        for(auto t:q){
            addSon(t.fi,t.se,player);
        }
    }

    //将在x,y放player的节点作为儿子
    void addSon(int x,int y,int player){
        MCTSNode *nowSon=new MCTSNode;
        nowSon->father=this;
        dvectr temp=map;
        temp[x][y]=player;
        nowSon->map=temp;
        sons.push_back(nowSon);
    }

    vector<pii> getUsefulSteps(){
        vector<pii> ans;
        for(int i=1;i<map.size();i++){
            for(int j=1;j<map[i].size();j++){
                if(!map[i][j]){
                    //看周围24格子有没有落子
                    for(int now=0;now<24;now++){
                        int x=dx[now]+i,y=dy[now]+j;
                        if(x<1||y<1||x>15||y>15)continue;
                        if(map[x][y]){
                            ans.push_back({x,y});
                            break;
                        }
                    }
                }
            }
        }

    }

    //判断这个局面view赢了没
    bool isHeWin(int view){
        //检查横向
        for(int i=1;i<map.size();i++){
            for(int j=1;j<=11;j++) 
                if (map[i][j]==view&&map[i][j+1]==view&&map[i][j+2]==view&&
                    map[i][j+3]==view&&map[i][j+4]==view)
                    return true;
        }
        //检查纵向
        for(int i=1;i<=11;i++){
            for(int j=1;j<map.size();j++)
                if (map[i][j]==view&&map[i+1][j]==view&&map[i+2][j]==view&&
                    map[i+3][j]==view&&map[i+4][j]==view)
                    return true;
        }
        //检查主对角线
        for(int i=1;i<=11;i++){
            for(int j=1;j<=11;j++){
                if(map[i][j]==view&&map[i+1][j+1]==view&&map[i+2][j+2]==view&&
                    map[i+3][j+3]==view&&map[i+4][j+4]==view){
                    return true;
                }
            }
        }
        //检查副对角线
        for(int i=1;i<=11;i++){
            for(int j=5;j<=15;j++){
                if(map[i][j]==view&&map[i+1][j-1]==view&&map[i+2][j-2]==view&&
                    map[i+3][j-3]==view&&map[i+4][j-4]==view){
                    return true;
                }
            }
        }
        //未连成五子
        return false; 
    }

    //根父节点比较看在哪里下棋了
    pii cmpDescion(MCTSNode &pre){
        for(int i=1;i<map.size();i++)
            for(int j=1;j<map.size();j++)
                if(map[i][j]!=pre.map[i][j])return{i,j};
        return {0,0};
    }

};
