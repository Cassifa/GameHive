-- 修改 record 表结构
-- 1. 将 is_pk_ai 改为 game_mode (整数表示游戏模式)
ALTER TABLE record CHANGE COLUMN is_pk_ai game_mode int NOT NULL COMMENT '游戏模式（0-本地对战 1-与大模型对战 2-联机对战）';

-- 2. 将 first_player 改名为 first_player_name
ALTER TABLE record CHANGE COLUMN first_player first_player_name varchar(30) NULL COMMENT '先手玩家名称';

-- 3. 将 player_b_pieces 改名为 second_player_pieces
ALTER TABLE record CHANGE COLUMN player_b_pieces second_player_pieces varchar(500) NOT NULL COMMENT '后手玩家操作序列，json格式';

-- 更新后的完整表结构
/*
CREATE TABLE record (
    record_id           int auto_increment comment '对局id' primary key,
    game_type_id        int                                   not null comment '本局对局对应的game_id',
    game_type_name      varchar(30)                           not null comment '本局游戏名称',
    record_time         timestamp   default CURRENT_TIMESTAMP not null comment '对局结束时间',
    game_mode           int                                   not null comment '游戏模式（0-本地对战 1-与大模型对战 2-联机对战）',
    algorithm_id        int         default -1                not null comment '对战的算法编号,如果为匹配对战则为-1',
    algorithm_name      varchar(50) default ' '               not null comment '算法名称',
    winner              int                                   not null comment '赢家（0-先手 1-后手 2-平局）',
    first_player_id     int                                   not null comment '先手id',
    first_player_name   varchar(30)                           null comment '先手玩家名称',
    second_player_id    int         default -1                not null comment '后手',
    second_player_name  varchar(30)                           null comment '后手名称',
    first_player_pieces varchar(500)                          not null comment '先手玩家操作序列，json格式',
    second_player_pieces varchar(500)                         not null comment '后手玩家操作序列，json格式'
) comment '对局记录';
*/ 