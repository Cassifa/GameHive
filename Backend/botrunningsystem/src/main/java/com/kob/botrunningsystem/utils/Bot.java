package com.kob.botrunningsystem.utils;

import java.io.File;
import java.io.FileNotFoundException;
import java.util.*;

public class Bot implements java.util.function.Supplier<Integer>{

    private final int[][] now_g= new int[13][14];
    private int eval(int x,int y,int left){//这一步走哪里 剩余搜索长度
        if(left==0)return 0;//剩余步数用尽
        if(now_g[x][y]==1)return 0;//不合法
        int[] dx={-1,0,1,0},dy={0,1,0,-1};
        int ans=0;
        now_g[x][y]=1;
        for(int i=0;i<4;i++){
            int a=x+dx[i],b=y+dy[i];
            ans=Math.max(ans,eval(a,b,left-1));
        }
        now_g[x][y]=0;
        return ans+1;
    }
    public Integer nextMove(int[][] g,List<Cell> aCells,List<Cell> bCells) {
        //不增长下一步会空出
        if(!check_tail_increasing(bCells.size()))g[bCells.get(0).x][bCells.get(0).y]=0;
        if(!check_tail_increasing(bCells.size()))g[aCells.get(0).x][aCells.get(0).y]=0;
        for (int i=0;i<13;i++)
            System.arraycopy(g[i], 0, now_g[i], 0, 14);
        int[] dx={-1,0,1,0},dy={0,1,0,-1};
        int ans=0,res=0;
        for(int i=0;i<4;i++){
            int x=aCells.get(aCells.size()-1).x+dx[i];
            int y=aCells.get(aCells.size()-1).y+dy[i];
            //对当前这步进行估值
            int t=eval(x,y,9);//最远搜9步,减小搜索空间
            if(t>=res){
                if(res==t){//估值相同随机决定是否转向
                    int random=(int)(Math.random()*10)&1;
                    if(random==1)ans=i;
                }else ans=i;
                res=t;
            }
        }
        return ans;
    }

    static class  Cell{
        public int x,y;
        public Cell(int x,int y){
            this.x=x;
            this.y=y;
        }
    }

    private  boolean check_tail_increasing(int step){//检验是否增长
        if(step<=10)return true;
        return step%3==1;
    }

    public List<Cell> getCells(int sx,int sy,String steps){
        steps=steps.substring(1,steps.length()-1);
        List<Cell> ans=new ArrayList<>();
        int[] dx={-1,0,1,0},dy={0,1,0,-1};
        int x=sx,y=sy,now=0;
        ans.add(new Cell(x, y));
        for(int i=0;i<steps.length();i++){
            int d=steps.charAt(i)-'0';
            x+=dx[d];
            y+=dy[d];
            ans.add(new Cell(x, y));
            if(!check_tail_increasing(++now))
                ans.remove(0);
        }
        return ans;
    }

    public Integer nextMove(String input){
        String[] stirs=input.split("#");
        //地图#me_sx#me_sy#me_操作#you_sx#you_sy#对手操作
        //0:地图 1:me_sx 2:me_sy 3:me_操作 4:you_sx 5:you_sy 6:you_操作
        int[][] g=new int[13][14];
        //处理地图
        for(int i=0,k=0;i<13;i++){
            for(int j=0;j<14;j++,k++){
                if(stirs[0].charAt(k)=='1')
                    g[i][j]=1;
            }
        }
        int aSx=Integer.parseInt(stirs[1]),aSy=Integer.parseInt(stirs[2]);
        int bSx=Integer.parseInt(stirs[4]),bSy=Integer.parseInt(stirs[5]);

        List<Cell> aCells=getCells(aSx,aSy,stirs[3]);
        List<Cell> bCells=getCells(bSx,bSy,stirs[6]);
        //数组尾是蛇头

        for(Cell c:aCells) g[c.x][c.y]=1;
        for(Cell c:bCells) g[c.x][c.y]=1;

        return nextMove(g ,aCells,bCells);
    }
    @Override
    public Integer get() {
        File file=new File("input.txt");
        try {
            Scanner sc=new Scanner(file);
            return nextMove(sc.next());
        } catch (FileNotFoundException e) {
            throw new RuntimeException(e);
        }
    }
}
