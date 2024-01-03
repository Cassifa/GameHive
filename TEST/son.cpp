#pragma once
#include "son.hpp"
son::son(){
    cout<<"子类构造了\n";
}
son::~son(){
    cout<<"子析构\n";
}
void son::say(){
    cout<<"SAY\n";
}