package com.gamehive.lmmrunningsystem.dto;

import com.gamehive.lmmrunningsystem.constants.ValidationResultEnum;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

/**
 * 大模型决策结果实体类
 * 封装了大模型返回的落子决策信息，包括坐标位置和决策理由
 *
 * @author Cassifa
 * @since 1.0.0
 */
@Data
@NoArgsConstructor
@AllArgsConstructor
public class LMMDecisionResult {

    // X坐标（行坐标）
    private Integer x;

    // Y坐标（列坐标）
    private Integer y;

    // 决策理由或深度思考内容
    private String reason;

    public boolean isValid(LMMRequestDTO lmmRequest) {
        return validate(lmmRequest) == ValidationResultEnum.VALID;
    }

    public ValidationResultEnum validate(LMMRequestDTO lmmRequest) {
        return validate(lmmRequest.getCurrentMap(), lmmRequest.getGridSize());
    }

    public ValidationResultEnum validate(String currentMap, String gridSize) {
        if (x == null || y == null) {
            return ValidationResultEnum.INVALID_FORMAT;
        }
        int size = Integer.parseInt(gridSize);
        if (x < 0 || x >= size || y < 0 || y >= size) {
            return ValidationResultEnum.OUT_OF_BOUNDS;
        }
        String[] rows = currentMap.split("\n");
        if (x < rows.length && y < rows[x].length()) {
            char position = rows[x].charAt(y);
            if (position != '0') {
                return ValidationResultEnum.POSITION_OCCUPIED;
            }
        }
        return ValidationResultEnum.VALID;
    }
} 
