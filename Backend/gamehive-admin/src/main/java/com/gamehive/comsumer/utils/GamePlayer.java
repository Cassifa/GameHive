package com.gamehive.comsumer.utils;

import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

import java.util.ArrayList;
import java.util.List;

@Data
@AllArgsConstructor
@NoArgsConstructor
public class GamePlayer {
    private Integer id,sx,sy;
    private Integer botId;
    private String botCode;//-1人工
    private List<Integer> steps;

    private  boolean check_tail_increasing(int step){//检验是否增长
        if(step<=10)return true;
        return step%3==1;
    }
    public List<Cell> getCells(){
        List<Cell> ans=new ArrayList<>();
        int[] dx={-1,0,1,0},dy={0,1,0,-1};
        int x=sx,y=sy,now=0;
        ans.add(new Cell(x,y));
        for(int d:steps){
            x+=dx[d];
            y+=dy[d];
            ans.add(new Cell(x,y));
            if(!check_tail_increasing(++now))
                ans.remove(0);
        }
        return ans;
    }

    public String getStepsString(){
        StringBuilder ans=new StringBuilder();
        for(int d:steps){
            ans.append(d);
        }
        return ans.toString();
    }
}
