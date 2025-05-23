package com.gamehive.service.pk;

public interface ReceiveLMMoveService {

    /**
     * 处理机LMM移动
     *
     * @param userId 用户ID
     * @param x x坐标
     * @param y y坐标
     * @param modelName 模型名称
     * @param reason 移动原因
     * @return
     */
    String receiveBotMove(Long userId, Integer x, Integer y, String modelName, String reason);
}
