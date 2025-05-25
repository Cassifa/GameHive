package com.gamehive.service.impl;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.gamehive.common.core.domain.AjaxResult;
import com.gamehive.common.core.domain.entity.SysUser;
import com.gamehive.constants.SpecialPlayerEnum;
import com.gamehive.pojo.AlgorithmType;
import com.gamehive.pojo.GameType;
import com.gamehive.pojo.Record;
import com.gamehive.service.IAlgorithmTypeService;
import com.gamehive.service.IClientService;
import com.gamehive.service.IGameTypeService;
import com.gamehive.service.IRecordService;
import com.gamehive.system.service.ISysUserService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.security.crypto.bcrypt.BCryptPasswordEncoder;
import org.springframework.stereotype.Service;

import java.util.*;

/**
 * 客户端服务实现
 */
@Service
public class ClientServiceImpl implements IClientService {

    @Autowired
    private IRecordService recordService;

    @Autowired
    private ISysUserService userService;

    @Autowired
    private IGameTypeService gameTypeService;

    @Autowired
    private IAlgorithmTypeService algorithmTypeService;

    @Autowired
    private BCryptPasswordEncoder passwordEncoder;

    @Autowired
    private ObjectMapper objectMapper;

    /**
     * 客户端登录
     *
     * @param username 用户名
     * @param password 密码
     * @return AjaxResult 包含登录结果
     */
    @Override
    public AjaxResult login(String username, String password) {
        // 1. 根据用户名查询用户信息
        SysUser user = userService.selectUserByUserName(username);

        // 2. 检查用户是否存在以及状态
        if (user == null) {
            return AjaxResult.error("登录失败：用户不存在");
        }
        // 3. 比对密码
        if (!passwordEncoder.matches(password, user.getPassword())) {
            return AjaxResult.error("登录失败：用户名或密码错误");
        }

        // 4. 认证成功，返回用户信息
        Map<String, Object> userInfo = new HashMap<>();
        userInfo.put("userId", user.getUserId());
        userInfo.put("userName", user.getUserName());
        userInfo.put("nickName", user.getNickName());
        return AjaxResult.success(userInfo);
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
            String gameTypeName = (String) gameData.get("gameTypeName");
            String algorithmName = (String) gameData.get("algorithmName");
            record.setGameTypeName(gameTypeName);
            record.setAlgorithmName(algorithmName);
            record.setRecordTime(new Date()); // 设置当前时间为对局时间

            // 根据游戏名称查询游戏类型ID
            GameType queryGameType = new GameType();
            queryGameType.setGameName(gameTypeName);
            List<GameType> gameTypes = gameTypeService.selectGameTypeList(queryGameType);
            if (gameTypes == null || gameTypes.isEmpty()) {
                return false; // 游戏类型不存在
            }
            record.setGameTypeId(gameTypes.get(0).getGameId());

            // 根据算法名称查询算法ID
            AlgorithmType queryAlgorithm = new AlgorithmType();
            queryAlgorithm.setAlgorithmName(algorithmName);
            List<AlgorithmType> algorithms = algorithmTypeService.selectAlgorithmTypeList(queryAlgorithm);
            if (algorithms == null || algorithms.isEmpty()) {
                return false; // 算法不存在
            }
            record.setAlgorithmId(algorithms.get(0).getAlgorithmId());

            // 2. 获取玩家ID并设置玩家信息
            Long playerId = Long.valueOf(gameData.get("userId").toString());

            // 获取玩家名称
            String playerName;
            if (playerId == 0) {
                playerName = SpecialPlayerEnum.GUEST.getChineseName();
            } else {
                // 验证用户是否存在
                SysUser user = userService.selectUserById(playerId);
                if (user == null) {
                    return false; // 用户不存在，上传失败
                }
                playerName = user.getUserName();
            }

            // 设置玩家ID和名称
            boolean playerFirst = (Boolean) gameData.get("playerFirst");
            if (playerFirst) {
                record.setFirstPlayerId(playerId);
                record.setFirstPlayer(playerName);
                record.setSecondPlayerId((long)SpecialPlayerEnum.AI.getCode());
                record.setSecondPlayerName(SpecialPlayerEnum.AI.getChineseName());
            } else {
                record.setFirstPlayerId((long)SpecialPlayerEnum.AI.getCode());
                record.setFirstPlayer(SpecialPlayerEnum.AI.getChineseName());
                record.setSecondPlayerId(playerId);
                record.setSecondPlayerName(playerName);
            }

            // 3. 直接使用客户端传入的整数赢家标识 (0-先手赢，1-后手赢，2-平局,3-终止游戏)
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