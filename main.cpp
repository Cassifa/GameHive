#include"TicTacToe.hpp"
// #include "GoBang.hpp"
int main(){
    BaseAi *baseAi;
    //等待合法输入
    bool wait=true;
    // while(wait){
    //     cout<<"请选择要玩的模式\n1.井字棋\n2.五子棋\n";
    //     int mode;
    //     cin>>mode;
    //     switch(mode){
    //     case 1:
    //         baseAi=new TicTacToe();
    //         wait=false;
    //         break;
    //     case 2:
    //         // baseAi=new GoBang(15,15);
    //         wait=false;
    //         break;
    //     default:
    //         cout<<"输入错误\n";
    //         break;
    //     }
    // }
    baseAi=new TicTacToe();
    wait=true;
    // while(wait){
    //     cout<<"请选择你是先手后手\n1:先手\n2:后手\n";
    //     int mode;cin>>mode;
    //     switch(mode){
    //         case 1:
    //             baseAi->setIsAiFirst(false);
    //             wait=false;
    //             break;
    //         case 2:
    //             baseAi->setIsAiFirst(true);
    //             wait=false;
    //             break;
    //         default:
    //             cout<<"输入错误\n";
    //             break;
    //     }
    // }
    baseAi->setIsAiFirst(true);
    baseAi->startGame();
    while (true){
        cout<<"当前是第"<<((baseAi->getRound())+1)/2<<"轮"<<endl;
        if(baseAi->letAiMove())continue;
        if(baseAi->isEnd()){
            baseAi->printGameResult();
            delete baseAi;
            break;
        }
        wait=true;
        while(wait){
            cout<<"请选择你要选择的位置(行,列,数组坐标系):\n";
            int x,y;cin>>x>>y;
            if(baseAi->selectPlace(x,y))
                wait=false;
            else cout<<"输入错误!\n";
        }
    }
    return 0;
}