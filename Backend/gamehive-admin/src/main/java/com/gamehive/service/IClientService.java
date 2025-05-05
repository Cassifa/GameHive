package com.gamehive.service;

import com.gamehive.common.core.domain.AjaxResult;

import java.util.Map;

/**
 * 客户端服务接口
 * 处理客户端不鉴权接口的业务逻辑
 */
public interface IClientService {

    /**
     * 客户端登录
     * 
     * @param username 用户名
     * @param password 密码
     * @return 用户信息，包含userId, userName, nickName等字段
     */
    AjaxResult login(String username, String password);

    /**
     * 上传对局结果
     * 
     * @param gameData 游戏数据，包含:
     *                 - userId: 玩家ID (必填)
     *                 - algorithmName: 算法名称 (必填)
     *                 - gameTypeName: 游戏类别名称 (必填)
     *                 - playerFirst: 是否玩家先手 (必填，true/false)
     *                 - winner: 赢家 (必填，整数：0-先手赢，1-后手赢，2-平局)
     *                 - moves: 操作序列 (必填，数组，每个元素包含id、role("Player"/"AI")、x、y)
     * @return 是否上传成功
     */
    boolean uploadGameResult(Map<String, Object> gameData);
} 