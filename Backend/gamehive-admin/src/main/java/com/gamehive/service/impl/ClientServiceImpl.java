package com.gamehive.service.impl;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.gamehive.common.core.domain.entity.SysUser;
import com.gamehive.common.core.domain.model.LoginUser;
import com.gamehive.pojo.Record;
import com.gamehive.service.IClientService;
import com.gamehive.service.IRecordService;
import com.gamehive.system.service.ISysUserService;
import java.util.ArrayList;
import java.util.Date;
import java.util.HashMap;
import java.util.List;
import java.util.Map;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.security.authentication.AuthenticationManager;
import org.springframework.security.authentication.UsernamePasswordAuthenticationToken;
import org.springframework.security.core.Authentication;
import org.springframework.stereotype.Service;

/**
 * 客户端服务实现
 */
@Service
public class ClientServiceImpl implements IClientService {

    @Autowired
    private AuthenticationManager authenticationManager;

    @Autowired
    private IRecordService recordService;

    @Autowired
    private ISysUserService userService;

    @Autowired
    private ObjectMapper objectMapper;

    /**
     * 客户端登录
     *
     * @param username 用户名
     * @param password 密码
     * @return 包含用户ID和基本信息的Map
     */
    @Override
    public Object login(String username, String password) {
        Authentication authentication = authenticationManager.authenticate(
                new UsernamePasswordAuthenticationToken(username, password)
        );

        // 获取登录用户信息
        LoginUser loginUser = (LoginUser) authentication.getPrincipal();
        SysUser user = loginUser.getUser();

        // 返回包含用户ID和用户名的信息
        Map<String, Object> userInfo = new HashMap<>();
        userInfo.put("userId", user.getUserId());
        userInfo.put("userName", user.getUserName());
        userInfo.put("nickName", user.getNickName());

        return userInfo;
    }

    /**
     * 上传对局结果
     *
     * @param gameData 游戏数据
     * @return 是否上传成功
     */
    @Override
    public boolean uploadGameResult(Map<String, Object> gameData) {
        try {
            // 验证必填参数
            if (!validateRequiredParams(gameData)) {
                return false;
            }

            Record record = new Record();

            // 1. 设置基本信息
            record.setIsPkAi(true); // 必须是与AI对局
            record.setAlgorithmName((String) gameData.get("algorithmName"));
            record.setGameTypeName((String) gameData.get("gameTypeName"));
            record.setRecordTime(new Date()); // 设置当前时间为对局时间

            // 2. 获取玩家ID并设置玩家信息
            Long playerId = Long.valueOf(gameData.get("userId").toString());

            // 验证用户是否存在
            SysUser user = userService.selectUserById(playerId);
            if (user == null) {
                return false; // 用户不存在，上传失败
            }

            // 设置玩家ID和名称
            boolean playerFirst = (Boolean) gameData.get("playerFirst");
            if (playerFirst) {
                record.setFirstPlayerId(playerId);
                record.setFirstPlayer(user.getNickName() != null ? user.getNickName() : user.getUserName());
                record.setSecondPlayerId(-1L);
                record.setSecondPlayerName(record.getAlgorithmName());
            } else {
                record.setFirstPlayerId(-1L);
                record.setFirstPlayer(record.getAlgorithmName());
                record.setSecondPlayerId(playerId);
                record.setSecondPlayerName(user.getNickName() != null ? user.getNickName() : user.getUserName());
            }

            // 3. 直接使用客户端传入的整数赢家标识 (0-先手赢，1-后手赢，2-平局)
            Long winnerObj = Long.valueOf(gameData.get("winner").toString());
            record.setWinner(winnerObj);

            // 4. 处理操作序列，分别保存先后手的操作
            @SuppressWarnings("unchecked")
            List<Map<String, Object>> moves = (List<Map<String, Object>>) gameData.get("moves");

            // 分离先手和后手的操作
            List<Map<String, Object>> firstPlayerMoves = new ArrayList<>();
            List<Map<String, Object>> secondPlayerMoves = new ArrayList<>();

            for (Map<String, Object> move : moves) {
                String role = (String) move.get("role");

                // 根据role和playerFirst确定该步骤是属于先手还是后手
                boolean isFirstPlayer = (playerFirst && "Player".equals(role)) || (!playerFirst && "AI".equals(role));
                
                // 构造坐标对
                Map<String, Object> positionPair = new HashMap<>();
                positionPair.put("x", move.get("x"));
                positionPair.put("y", move.get("y"));

                // 添加到对应的列表
                if (isFirstPlayer) {
                    firstPlayerMoves.add(positionPair);
                } else {
                    secondPlayerMoves.add(positionPair);
                }
            }

            // 将两个列表分别序列化
            record.setFirstPlayerPieces(objectMapper.writeValueAsString(firstPlayerMoves));
            record.setPlayerBPieces(objectMapper.writeValueAsString(secondPlayerMoves));

            // 5. 保存记录
            int result = recordService.insertRecord(record);
            return result > 0;
        } catch (Exception e) {
            // 记录日志
            e.printStackTrace();
            return false;
        }
    }

    /**
     * 验证上传对局必填参数
     *
     * @param gameData 游戏数据
     * @return 是否包含所有必填参数
     */
    private boolean validateRequiredParams(Map<String, Object> gameData) {
        // 检查必填字段
        if (!gameData.containsKey("userId") || gameData.get("userId") == null ||
                !gameData.containsKey("algorithmName") || gameData.get("algorithmName") == null ||
                !gameData.containsKey("gameTypeName") || gameData.get("gameTypeName") == null ||
                !gameData.containsKey("playerFirst") || gameData.get("playerFirst") == null ||
                !gameData.containsKey("winner") || gameData.get("winner") == null ||
                !gameData.containsKey("moves") || gameData.get("moves") == null) {
            return false;
        }
        return true;
    }
} 