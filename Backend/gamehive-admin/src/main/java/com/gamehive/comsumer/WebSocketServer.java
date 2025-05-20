package com.gamehive.comsumer;

import com.alibaba.fastjson2.JSONObject;
import com.gamehive.comsumer.utils.GamePlayer;
import com.gamehive.comsumer.utils.WebSocketMessageObj;
import com.gamehive.comsumer.utils.Game;
import com.gamehive.comsumer.utils.JwtAuthentication;
import com.gamehive.constants.GameTypeEnum;
import com.gamehive.mapper.BotMapper;
import com.gamehive.mapper.RecordMapper;
import com.gamehive.mapper.PlayerMapper;
import com.gamehive.pojo.Bot;
import com.gamehive.pojo.Player;
import com.gamehive.pojo.User;
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
    public static ConcurrentHashMap<Long,WebSocketServer> users=new ConcurrentHashMap<>();
    //匹配池
    private Session session =null;
    private Player user;

    //非单例模式
    public static PlayerMapper userMapper;
    public static RecordMapper recordMapper;

    public static RestTemplate restTemplate;//spring boot间通信
    private final static String addPlayerUrl="http://127.0.0.1:3001/player/add/";
    private final static String removePlayerUrl="http://127.0.0.1:3001/player/remove/";

    public Game game=null;
    @Autowired
    public void setUserMapper(PlayerMapper userMapper){
        WebSocketServer.userMapper=userMapper;
    }
    @Autowired
    public void setRecordMapper(RecordMapper recordMapper){
        WebSocketServer.recordMapper=recordMapper;
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
        Long userId= JwtAuthentication.getUserId();
        this.user=userMapper.selectById(userId);
        if(this.user!=null){
            users.put(userId,this);
        }else{
            this.session.close();
        }
    }

    public static void startGame(Integer aId, Integer bId, GameTypeEnum gameTypeEnum){
        Player a=userMapper.selectById(aId),b=userMapper.selectById(bId);
        Game game=new Game(gameTypeEnum,
                a.getUserId(),
                b.getUserId()
        );
        game.createMap();

        //一方断开链接则无视其操作
        if(users.get(a.getUserId())!=null) users.get(a.getUserId()).game=game;
        if(users.get(b.getUserId())!=null) users.get(b.getUserId()).game=game;

        game.start();

        JSONObject respGame=new JSONObject();
        respGame.put("a_id",game.getPlayerA().getUserId());
        respGame.put("a_sx",game.getPlayerA().getSx());
        respGame.put("a_sy",game.getPlayerA().getSy());
        respGame.put("b_id",game.getPlayerB().getUserId());
        respGame.put("b_sx",game.getPlayerB().getSx());
        respGame.put("b_sy",game.getPlayerB().getSy());
        respGame.put("game_map",game.getG());

        JSONObject respA= new JSONObject(),respB= new JSONObject();
        respA.put("event","start-matching");
        respA.put("opponent_username",b.getUsername());
        respA.put("game",respGame);
        //一方断开链接则无视其操作
        if(users.get(a.getUserId())!=null)
            users.get(a.getUserId()).sendMessage(respA.toJSONString());

        respB.put("event","start-matching");
        respB.put("opponent_username",a.getUsername());
        respB.put("game",respGame);
        if(users.get(b.getUserId())!=null)
            users.get(b.getUserId()).sendMessage(respB.toJSONString());

    }

    @OnClose
    public void onClose() {
        // 关闭链接
        System.out.println("closed");
        if(this.user!=null){
            users.remove(this.user.getUserId() );
        }
    }

    //根据开始游戏请求进行匹配
    private void startMatching(WebSocketMessageObj matchData){
//        System.out.println("startMatching");
        MultiValueMap<String,String> data=new LinkedMultiValueMap<>();
        data.add("user_id",this.user.getUserId().toString());
        data.add("rating",this.user.getRaking().toString());
        data.add("bot_id",botId.toString());
        //spring boot通信，接收方地址 数据 返回值class(反射机制)
        restTemplate.postForObject(addPlayerUrl,data,String.class);
    }

    private void stopMatching(){
//        System.out.println("stopMatching");
        MultiValueMap<String,String> data=new LinkedMultiValueMap<>();
        data.add("user_id",this.user.getUserId().toString());
        data.add("rating",this.user.getRaking().toString());
        restTemplate.postForObject(removePlayerUrl,data,String.class);

    }

    private void move(int x,int y){
        if(game.getPlayerA().getUserId()==user.getUserId()){
            if (game.getPlayerA().getBotId().equals(-1))
                game.setNextStepA(d);
        }else if(game.getPlayerB().getUserId()==user.getUserId()){
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
        if (event != null) {
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
