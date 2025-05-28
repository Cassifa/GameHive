-- 更新 player 表结构
-- 1. 删除 record_with_ai 字段
ALTER TABLE player DROP COLUMN record_with_ai;

-- 2. 删除 record_with_player 字段  
ALTER TABLE player DROP COLUMN record_with_player;

-- 3. 增加 game_statistics 字段
ALTER TABLE player ADD COLUMN game_statistics TEXT NULL COMMENT '对局统计信息';

-- 更新后的完整表结构
/*
CREATE TABLE player (
    user_id         bigint      NOT NULL COMMENT '用户id' PRIMARY KEY,
    user_name       varchar(30) NULL COMMENT '用户昵称',
    raking          int         NOT NULL DEFAULT 1400 COMMENT '天梯积分',
    game_statistics text        NULL COMMENT '对局统计信息'
) COMMENT '玩家表';
*/ 