package com.gamehive.controller.pk;

import com.gamehive.service.pk.ReceiveLMMoveService;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.util.MultiValueMap;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;

import java.util.Objects;

@RestController
public class ReceiveLMMMoveController {

    @Autowired
    private ReceiveLMMoveService receiveLMMoveService;

    /**
     * 处理语言模型(LMM)的移动请求
     * <p>
     * 从请求参数中解析出用户ID、移动坐标、模型名称和移动原因，
     * 并调用服务层处理该移动请求
     *
     * @param data 请求参数，包含以下字段：
     *             - user_id: 用户ID
     *             - x: 移动目标的x坐标
     *             - y: 移动目标的y坐标
     *             - model_name: 使用的语言模型名称
     *             - reason: 模型给出的移动原因说明
     * @return 执行结果字符串，表示操作是否成功
     */
    @PostMapping("/api/pk/receive/bot/move/")
    public String receiveBotMove(@RequestParam MultiValueMap<String, String> data) {
        //解析出人类玩家ID
        Long userId = Long.parseLong(Objects.requireNonNull(data.getFirst("user_id")));
        //解析坐标x和y
        Integer x = Integer.parseInt(Objects.requireNonNull(data.getFirst("x")));
        Integer y = Integer.parseInt(Objects.requireNonNull(data.getFirst("y")));
        //解析模型名称和原因
        String modelName = Objects.requireNonNull(data.getFirst("model_name"));
        String reason = Objects.requireNonNull(data.getFirst("reason"));

        //调用服务处理LMM移动
        return receiveLMMoveService.receiveBotMove(userId, x, y, modelName, reason);
    }
}
