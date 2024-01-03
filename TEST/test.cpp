#include<vector>
#include<iostream>
#include "son.cpp"
using namespace std;
int main(){
    father *fa=new son();
    fa->say();
    delete fa;
    return 0;
}