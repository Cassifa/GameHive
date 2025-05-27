package com.gamehive.pojo;

import com.fasterxml.jackson.annotation.JsonFormat;
import com.gamehive.common.annotation.Excel;
import com.gamehive.common.core.domain.BaseEntity;
import java.util.Date;
import lombok.AllArgsConstructor;
import lombok.Data;
import lombok.NoArgsConstructor;

/**
 * 对局记录对象 record
 *
 * @author Cassifa
 * @date 2025-05-05
 */
@Data
@AllArgsConstructor
@NoArgsConstructor
public class Record extends BaseEntity {

    private static final long serialVersionUID = 1L;

    /**
     * 对局id
     */
    private Long recordId;

    /**
     * 本局对局对应的game_id
     */
    private Long gameTypeId;

    /**
     * 本局游戏名称
     */
    @Excel(name = "本局游戏名称")
    private String gameTypeName;

    /**
     * 对局结束时间
     */
    @JsonFormat(pattern = "yyyy-MM-dd")
    @Excel(name = "对局结束时间", width = 30, dateFormat = "yyyy-MM-dd")
    private Date recordTime;

    /**
     * 游戏模式（0-本地对战 1-与大模型对战 2-联机对战）
     */
    @Excel(name = "游戏模式", readConverterExp = "0=本地对战,1=与大模型对战,2=联机对战")
    private Integer gameMode;

    /**
     * 对战的算法编号,如果为匹配对战则为-1,与大模型为-2
     */
    private Long algorithmId;

    /**
     * 算法名称
     */
    @Excel(name = "算法名称")
    private String algorithmName;

    /**
     * 赢家（0-先手 1-后手 2-平局 3-无）
     */
    @Excel(name = "赢家", readConverterExp = "0=先手,1=后手,2=平局,3=无")
    private Long winner;

    /**
     * 先手id
     */
    private Long firstPlayerId;

    /**
     * 先手玩家名称
     */
    @Excel(name = "先手玩家")
    private String firstPlayerName;

    /**
     * 后手id
     */
    private Long secondPlayerId;

    /**
     * 后手玩家名称
     */
    @Excel(name = "后手玩家")
    private String secondPlayerName;

    /**
     * 先手玩家操作序列，json格式
     */
    private String firstPlayerPieces;

    /**
     * 后手玩家操作序列，json格式
     */
    private String secondPlayerPieces;
}
