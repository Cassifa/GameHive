package com.gamehive.comsumer;

import com.alibaba.fastjson2.JSONObject;
import com.gamehive.comsumer.utils.WebSocketMessageObj;
import com.kob.backend.comsumer.utils.Game;
import com.kob.backend.comsumer.utils.JwtAuthentication;
import com.kob.backend.mapper.BotMapper;
import com.kob.backend.mapper.RecordMapper;
import com.kob.backend.mapper.UserMapper;
import com.kob.backend.pojo.Bot;
import com.kob.backend.pojo.User;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.stereotype.Component;
import org.springframework.util.LinkedMultiValueMap;
import org.springframework.util.MultiValueMap;
import org.springframework.web.client.RestTemplate;

import javax.websocket.*;
import javax.websocket.server.PathParam;
import javax.websocket.server.ServerEndpoint;
import java.io.IOException;
import java.util.concurrent.ConcurrentHashMap;

@Component
@ServerEndpoint("/websocket/{token}")  // 注意不要以'/'结尾
public class WebSocketServer {
    public static ConcurrentHashMap<Integer,WebSocketServer> users=new ConcurrentHashMap<>();
    //匹配池
    private Session session =null;
    private User user;

    //非单例模式
    public static UserMapper userMapper;
    private static BotMapper botMapper;
    public static RecordMapper recordMapper;

    public static RestTemplate restTemplate;//spring boot间通信
    private final static String addPlayerUrl="http://127.0.0.1:3001/player/add/";
    private final static String removePlayerUrl="http://127.0.0.1:3001/player/remove/";

    public Game game=null;
    @Autowired
    public void setUserMapper(UserMapper userMapper){
        WebSocketServer.userMapper=userMapper;
    }
    @Autowired
    public void setRecordMapper(RecordMapper recordMapper){
        WebSocketServer.recordMapper=recordMapper;
    }
    @Autowired
    public void setBotMapper(BotMapper botMapper){
        WebSocketServer.botMapper=botMapper;
    }
    @Autowired
    public void setRestTemplate(RestTemplate restTemplate){
        //在两个spring boot 间通信
        WebSocketServer.restTemplate=restTemplate;
    }

    @OnOpen
    public void onOpen(Session session, @PathParam("token") String token) throws IOException {
        // 建立连接
        this.session=session;
        System.out.println("connected");
        Integer userId= JwtAuthentication.getUserId(token);
        this.user=userMapper.selectById(userId);
        if(this.user!=null){
            users.put(userId,this);
        }else{
            this.session.close();
        }
    }

    public static void startGame(Integer aId,Integer aBotId, Integer bId,Integer bBotId){
        User a=userMapper.selectById(aId),b=userMapper.selectById(bId);
        Bot botA=botMapper.selectById(aBotId),botB=botMapper.selectById(bBotId);
        Game game=new Game(13,14,20,
                a.getId(),botA,
                b.getId(),botB
        );
        game.createMap();

        //一方断开链接则无视其操作
        if(users.get(a.getId())!=null) users.get(a.getId()).game=game;
        if(users.get(b.getId())!=null) users.get(b.getId()).game=game;

        game.start();

        JSONObject respGame=new JSONObject();
        respGame.put("a_id",game.getPlayerA().getId());
        respGame.put("a_sx",game.getPlayerA().getSx());
        respGame.put("a_sy",game.getPlayerA().getSy());
        respGame.put("b_id",game.getPlayerB().getId());
        respGame.put("b_sx",game.getPlayerB().getSx());
        respGame.put("b_sy",game.getPlayerB().getSy());
        respGame.put("game_map",game.getG());

        JSONObject respA= new JSONObject(),respB= new JSONObject();
        respA.put("event","start-matching");
        respA.put("opponent_username",b.getUsername());
        respA.put("opponent_photo",b.getPhoto());
        respA.put("game",respGame);
        //一方断开链接则无视其操作
        if(users.get(a.getId())!=null)
            users.get(a.getId()).sendMessage(respA.toJSONString());

        respB.put("event","start-matching");
        respB.put("opponent_username",a.getUsername());
        respB.put("opponent_photo",a.getPhoto());
        respB.put("game",respGame);
        if(users.get(b.getId())!=null)
            users.get(b.getId()).sendMessage(respB.toJSONString());

    }

    @OnClose
    public void onClose() {
        // 关闭链接
        System.out.println("closed");
        if(this.user!=null){
            users.remove(this.user.getId() );
        }
    }

    //根据开始游戏请求进行匹配
    private void startMatching(WebSocketMessageObj matchData){
//        System.out.println("startMatching");
        MultiValueMap<String,String> data=new LinkedMultiValueMap<>();
        data.add("user_id",this.user.getId().toString());
        data.add("rating",this.user.getRating().toString());
        data.add("bot_id",botId.toString());
        //spring boot通信，接收方地址 数据 返回值class(反射机制)
        restTemplate.postForObject(addPlayerUrl,data,String.class);
    }

    private void stopMatching(){
//        System.out.println("stopMatching");
        MultiValueMap<String,String> data=new LinkedMultiValueMap<>();
        data.add("user_id",this.user.getId().toString());
        data.add("rating",this.user.getRating().toString());
        restTemplate.postForObject(removePlayerUrl,data,String.class);

    }

    private void move(int x,int y){
        if(game.getPlayerA().getId()==user.getId()){
            if (game.getPlayerA().getBotId().equals(-1))
                game.setNextStepA(d);
        }else if(game.getPlayerB().getId()==user.getId()){
            if (game.getPlayerB().getBotId().equals(-1))
                game.setNextStepB(d);
        }
    }
    @OnMessage
    public void onMessage(String message, Session session) {
        // 从Client接收消息
        System.out.println("recived");
        WebSocketMessageObj data=JSONObject.parseObject(message, WebSocketMessageObj.class);
        EventTypeEnum event=EventTypeEnum.fromType(data.getEvent());
        switch (event){
            case MOVE:
                move(data.getX(),data.getY());
                break;
            case STOP:
                stopMatching();
                break;
            case START:
                startMatching(data);
                break;
        }
    }

    @OnError
    public void onError(Session session, Throwable error) {
        System.out.println("错误");
        error.printStackTrace();
    }


    public void sendMessage(String message){//发送信息
        synchronized (this.session){
            try{
                this.session.getBasicRemote().sendText(message);
            }catch(IOException e){
                e.printStackTrace();
            }
        }

    }
    public void setSession(String message) {
        synchronized (this.session){
            try {
                this.session.getBasicRemote().sendText(message);
            }catch (IOException e){
                e.printStackTrace();
            }
        }
    }
}
