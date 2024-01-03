#pragma once
#include<iostream>
using namespace std;
class father{
private:
public:
    father();
    virtual ~father()=0;
    // ~father();
    virtual void say(){
        cout<<"AAA\n";
    };
};
father::father(){
    cout<<"父类构造了\n";
}
father::~father(){
    cout<<"父析构\n";
}