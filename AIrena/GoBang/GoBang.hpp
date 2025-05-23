#pragma once
#include"../BaseAi.hpp"
#include "./eval/baseGoBangAi.hpp"
#include "./eval/MCTS/MonteCarlo.cpp"
#include "./eval/MinMax/MinMax.cpp"
class GoBang :public BaseAi{
private:
    bool isHeWinner(int nowChecking)override;
    //检查玩家位置合法性
    bool checkPlace(int x,int y)override;
    //AI决定怎么走⭐
    int evalToDo(vector<vector<short>> &nowMap,int nowVision)override;
    //调用不同的Ai
    baseGoBangAi *usingAi;
public:
    //调用构造函数,新建地图
    GoBang():BaseAi(15,15){};
    ~GoBang(){
        delete usingAi;
    };
    //玩家选择位置
    bool selectPlace(int x,int y)override;

    //绘制当前地图状态
    void showMap()override;

    //判断是否结束
    bool isEnd()override;

    void startGame();
    void choiceAiType(string s);
};
//向玩家展示当前地图
void GoBang::showMap(){
	int i=0,j=0,row=15,col=15;
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
        printf(" %2d ",i);
    printf("\n");
	
}
//检查落子后游戏是不是结束了
bool GoBang::isEnd(){
    if(isHeWinner(1)) this->setWinner(1);
    else if(isHeWinner(2)) this->setWinner(2);
    else if(isHeWinner(3)) this->setWinner(3);
    else return false;
    usingAi->end();
    return true;
}
//检查他是不是赢了
bool GoBang::isHeWinner(int view){//0-Ai 1-玩家
    if(view==3){
        for(int i=0;i<map.size();i++)
            for(int j=0;j<map[i].size();j++)
                if(map[i][j]==0)
                    return false;
        return true;
    }
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
//玩家选择位置
bool GoBang::selectPlace(int x,int y){
    swap(x,y);x=16-x;
    if(checkPlace(x,y)){
        playerMove(x,y);
        //选好了向Ai发送移动信息
        usingAi->sendPlayerMoveMessage(x,y);
        return true;
    }
    return false;
}
//检查这个位置是否合法
bool GoBang::checkPlace(int x,int y){
    if(x>15||x<1)return false;
    if(y>15||y<1)return false;
    return map[x][y]==0;
}
//在决策树上Min-Max ɑß搜索剪枝 决定Ai下棋位置,返回当前局面当前视角下能得到的最大分数⭐
int GoBang::evalToDo(vector<vector<short>> &nowMap,int deep){
    //需要Ai移动了,此时一定是玩家刚走过了
    pii ans=usingAi->evalToGo();
    finalDecide=ans;
    // aiMove(finD)
    return 0;
}

void GoBang::startGame(){
    cout<<"开始游戏!"<<endl;
    cout<<"X表示你的棋子,O表示Ai的棋子"<<endl;
    //Ai先手直接走8,8
    if(isAiFirst){
        aiMove(8,8);
        usingAi->setBeginningState(this->map);
    }
    //否则初始Ai类的地图为空地图
    else {
        showMap();
        usingAi->setBeginningState(this->map);
    }

}
void GoBang::choiceAiType(string type){
    if(type=="MinMax")usingAi=new MinMax();
    else usingAi=new MonteCarlo();
}