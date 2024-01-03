#pragma once
#include "father.hpp"
class son :public father{
private:
public:
    son();
    ~son();
    void say() override;
};
// son::~son(){
//     cout<<"在本文件写的析构\n";
// }
// son::son(){
//     cout<<"子类构造了\n";
// }
// son::~son(){
//     cout<<"子析构\n";
// }
// void son::say(){
//     cout<<"SAY\n";
// }