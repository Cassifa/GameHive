#include "BaseAi.hpp"
#include "./TicTacToe/TicTacToe.hpp"
#include "./GoBang/GoBang.hpp"
int f(){
    BaseAi *baseAi;
    srand((unsigned)time(NULL)); 
    //等待输入游戏类别
    // bool wait=true;
    // while(wait){
    //     cout<<"请选择你要玩的游戏\n1:井字棋\n2:五子棋\n";
    //     int mode;cin>>mode;
    //     switch(mode){
    //         case 1:
    //             baseAi=new TicTacToe();
    //             wait=false;
    //             break;
    //         // case 2:
    //         //     baseAi=new GoBang();
    //         //     wait=false;
    //         //     break;
    //         default:
    //             cout<<"输入错误"<<endl;
    //             break;
    //     }
    // }
    
    //输入先后手
    // wait=true;
    // while(wait){
    //     cout<<"请选择你是先手后手\n1:先手\n2:后手\n";
    //     int mode=2;
    //     // cin>>mode;
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
    //             cout<<"输入错误"<<endl;
    //             break;
    //     }
    // }

    //设置五子棋Ai类别
    // wait=true;
    // while(wait){
    //     cout<<"请选择想挑战的Ai\n1:MinMax\n2:MonteCarlo\n";
    //     int mode=2;
    //     // cin>>mode;
    //     switch(mode){
    //         case 1:
    //             baseAi->choniceAiType("MinMax");
    //             wait=false;
    //             break;
    //         case 2:
    //             baseAi->choniceAiType("MonteCarlo");
    //             wait=false;
    //             break;
    //         default:
    //             cout<<"输入错误"<<endl;
    //             break;
    //     }
    // }

    // baseAi->setIsAiFirst(false);
    bool wait;
    // baseAi=new TicTacToe();
    baseAi=new GoBang();
    baseAi->setIsAiFirst(true);
    baseAi->choiceAiType("MonteCarlo");
    //开始游戏
    baseAi->startGame();
    while (true){
        //判断结束了
        if(baseAi->isEnd()){

            baseAi->printGameResult();
            delete baseAi;
            break;
        }
        //输出当前轮数信息(Ai与玩家都走了算一轮)
        if((baseAi->getRound())&1)
            cout<<"\n当前是第"<<((baseAi->getRound())+1)/2<<"轮"<<endl;
        //看Ai该不该走
        if(baseAi->letAiMove())continue;
        wait=true;
        while(wait){
            cout<<"请选择你要选择的位置(列,行,笛卡尔坐标系):\n";
            int x,y;cin>>x>>y;
            if(baseAi->selectPlace(x,y))
                wait=false;
            else cout<<"输入错误!"<<endl;
        }
    }
    return 0;
}
int main(){
    while(true){
        f();
    }
    return 0;
}