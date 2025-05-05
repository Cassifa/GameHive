package com.gamehive.controller;

import com.gamehive.common.core.domain.AjaxResult;
import com.gamehive.service.IClientService;

import java.util.Map;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

/**
 * 客户端API控制器
 * 不需要鉴权的客户端接口
 */
@RestController
@RequestMapping("/client")
public class ClientController {

    @Autowired
    private IClientService clientService;

    /**
     * 客户端登录接口
     *
     * @param username 用户名
     * @param password 密码
     * @return 登录结果，包含userId、userName、nickName等用户信息
     */
    @PostMapping("/login")
    public AjaxResult login(@RequestParam String username, @RequestParam String password) {
        AjaxResult userInfo = clientService.login(username, password);
        return userInfo;
    }

    /**
     * 上传对局结果接口
     *
     * @param requestMap 包含对局信息的请求体，参数包括：
     *                   - userId: 玩家ID (必填)
     *                   - algorithmName: 算法名称 (必填)
     *                   - gameTypeName: 游戏类别名称 (必填)
     *                   - playerFirst: 是否玩家先手 (必填，true/false)
     *                   - winner: 赢家 (必填，整数：0-先手赢，1-后手赢，2-平局)
     *                   - moves: 操作序列 (必填，数组，每个元素包含id、role("Player"/"AI")、x、y)
     * @return 上传结果
     */
    @PostMapping("/upload")
    public AjaxResult uploadGameResult(@RequestBody Map<String, Object> requestMap) {
        try {
            boolean success = clientService.uploadGameResult(requestMap);
            return success ? AjaxResult.success() : AjaxResult.error("上传失败");
        } catch (Exception e) {
            return AjaxResult.error("上传失败：" + e.getMessage());
        }
    }
} 