package com.gamehive.service.impl;

import com.fasterxml.jackson.databind.ObjectMapper;
import com.gamehive.common.core.domain.AjaxResult;
import com.gamehive.common.core.domain.entity.SysUser;
import com.gamehive.constants.GameModeEnum;
import com.gamehive.constants.SpecialPlayerEnum;
import com.gamehive.framework.web.service.SysLoginService;
import com.gamehive.mapper.ProductMapper;
import com.gamehive.pojo.AlgorithmType;
import com.gamehive.pojo.GameType;
import com.gamehive.pojo.Product;
import com.gamehive.pojo.Record;
import com.gamehive.service.*;
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

    @Autowired
    private SysLoginService sysLoginService;

    @Autowired
    private ProductMapper productMapper;

    @Autowired
    private IPlayerStatisticsService playerStatisticsService;

    /**
     * 客户端登录
     *
     * @param username 用户名
     * @param password 密码
     * @return AjaxResult 包含登录结果
     */
    @Override
    public AjaxResult login(String username, String password) {
        try {
            // 使用SysLoginService的login方法进行登录验证
            // 注意：客户端登录不需要验证码，所以传入null
            String token = sysLoginService.login(username, password, null, null);

            // 获取用户信息
            SysUser user = userService.selectUserByUserName(username);

            // 返回用户信息和token
            Map<String, Object> userInfo = new HashMap<>();
            userInfo.put("userId", user.getUserId());
            userInfo.put("userName", user.getUserName());
            userInfo.put("nickName", user.getNickName());
            userInfo.put("token", token);

            return AjaxResult.success(userInfo);
        } catch (Exception e) {
            return AjaxResult.error(e.getMessage());
        }
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
            record.setGameMode(GameModeEnum.LOCAL_GAME.getCode()); // 0-本地对战
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
                record.setFirstPlayerName(playerName);
                record.setSecondPlayerId((long) SpecialPlayerEnum.AI.getCode());
                record.setSecondPlayerName(SpecialPlayerEnum.AI.getChineseName());
            } else {
                record.setFirstPlayerId((long) SpecialPlayerEnum.AI.getCode());
                record.setFirstPlayerName(SpecialPlayerEnum.AI.getChineseName());
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
            record.setSecondPlayerPieces(objectMapper.writeValueAsString(secondPlayerMoves));

            // 5. 保存记录
            int result = recordService.insertRecord(record);

            if (result > 0) {
                // 6. 更新玩家统计信息（只有非游客玩家才更新）
                if (playerId != 0) {
                    updatePlayerStatistics(record);
                }

                // 7. 更新Product统计信息
                updateProductStatistics(record);

                return true;
            }

            return false;
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

    /**
     * 更新玩家统计信息
     *
     * @param record 对局记录
     */
    private void updatePlayerStatistics(Record record) {
        try {
            if (playerStatisticsService != null) {
                playerStatisticsService.updatePlayerStatistics(record);
            }
        } catch (Exception e) {
            e.printStackTrace();
            // 统计更新失败不影响主流程
        }
    }

    /**
     * 更新Product统计信息
     *
     * @param record 对局记录
     */
    private void updateProductStatistics(Record record) {
        try {
            if (productMapper != null && record.getAlgorithmId() != null && record.getGameTypeId() != null) {
                // 使用LambdaQuery查找对应的Product记录
                Product product = productMapper.selectOne(
                        new com.baomidou.mybatisplus.core.conditions.query.LambdaQueryWrapper<Product>()
                                .eq(Product::getAlgorithmTypeId, record.getAlgorithmId())
                                .eq(Product::getGameTypeId, record.getGameTypeId())
                );

                if (product != null) {
                    // 更新挑战次数
                    product.setChallengedCount(product.getChallengedCount() + 1);

                    // 更新胜利次数（AI胜利的情况）
                    Long winner = record.getWinner();
                    boolean aiWin = false;

                    // 判断AI是否胜利
                    if (winner != null && winner != 2 && winner != 3) { // 不是平局或无效
                        // 在客户端上传的本地对战中，需要根据playerFirst判断AI的位置
                        // 如果玩家先手，AI是后手，winner==1表示AI胜利
                        // 如果玩家后手，AI是先手，winner==0表示AI胜利
                        boolean playerFirst = record.getFirstPlayerId() != null &&
                                !record.getFirstPlayerId().equals((long) SpecialPlayerEnum.AI.getCode());

                        if (playerFirst) {
                            aiWin = (winner == 1); // AI是后手
                        } else {
                            aiWin = (winner == 0); // AI是先手
                        }
                    }

                    if (aiWin) {
                        product.setWinCount(product.getWinCount() + 1);
                    }

                    // 更新Product记录
                    productMapper.updateById(product);
                }
            }
        } catch (Exception e) {
            e.printStackTrace();
            // Product统计更新失败不影响主流程
        }
    }
} 