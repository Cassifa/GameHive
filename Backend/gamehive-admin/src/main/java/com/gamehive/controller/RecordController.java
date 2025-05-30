package com.gamehive.controller;

import java.util.List;
import java.util.Map;
import javax.servlet.http.HttpServletResponse;

import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;
import com.gamehive.common.annotation.Log;
import com.gamehive.common.core.controller.BaseController;
import com.gamehive.common.core.domain.AjaxResult;
import com.gamehive.common.enums.BusinessType;
import com.gamehive.common.utils.SecurityUtils;
import com.gamehive.constants.GameModeEnum;
import com.gamehive.pojo.Record;
import com.gamehive.service.IRecordService;
import com.gamehive.common.utils.poi.ExcelUtil;
import com.gamehive.common.core.page.TableDataInfo;
import com.github.pagehelper.PageHelper;
import org.slf4j.Logger;
import org.slf4j.LoggerFactory;

/**
 * 对局记录Controller
 *
 * @author Cassifa
 * @date 2025-05-05
 */
@RestController
@RequestMapping("/Record/Record")
public class RecordController extends BaseController {

    private static final Logger logger = LoggerFactory.getLogger(RecordController.class);

    @Autowired
    private IRecordService recordService;

    /**
     * 查询对局记录列表
     * 支持的查询条件:
     * - pageNum: 当前页码
     * - pageSize: 每页记录数（如果为空则返回所有数据）
     * - gameTypeId: 游戏类型ID
     * - gameMode: 游戏模式（0-本地对战，1-与大模型对战，2-联机对战）
     * - algorithmId: 算法ID
     * - winner: 赢家
     * - playerName: 玩家名称（用于模糊查询先手或后手玩家）
     * <p>
     * 权限说明:
     * - 管理员(userId=1)：可以查看所有玩家的对局记录
     * - 普通用户：只能查看自己的对局记录
     */
    @GetMapping("/list")
    public TableDataInfo list(
            @RequestParam(value = "gameTypeId", required = false) Long gameTypeId,
            @RequestParam(value = "gameMode", required = false) Integer gameMode,
            @RequestParam(value = "algorithmId", required = false) Long algorithmId,
            @RequestParam(value = "winner", required = false) Long winner,
            @RequestParam(value = "playerName", required = false) String playerName,
            Record record) {

        // 获取当前登录用户ID
        Long userId = getUserId();
        if (userId == null) {
            return getDataTableByException("用户未登录");
        }

        // 设置查询参数
        if (gameTypeId != null) {
            record.setGameTypeId(gameTypeId);
        }
        if (gameMode != null) {
            record.setGameMode(gameMode);
        }
        if (algorithmId != null) {
            record.setAlgorithmId(algorithmId);
        }
        if (winner != null) {
            record.setWinner(winner);
        }
        if (playerName != null && !playerName.isEmpty()) {
            record.getParams().put("playerName", playerName);
        }

        // 判断是否为管理员
        boolean isAdmin = SecurityUtils.isAdmin(userId);

        // 如果不是管理员，只能查看自己的记录
        if (!isAdmin) {
            record.getParams().put("currentUserId", userId);
        }
        // 管理员可以查看所有记录，不设置currentUserId限制

        // 获取分页参数
        int pageNum = com.gamehive.common.utils.ServletUtils.getParameterToInt("pageNum", 1);
        int pageSize = com.gamehive.common.utils.ServletUtils.getParameterToInt("pageSize", 10);

        // 计算offset
        int offset = (pageNum - 1) * pageSize;
        record.getParams().put("pageSize", pageSize);
        record.getParams().put("offset", offset);

        // 查询数据和总数
        List<Record> list = recordService.selectRecordList(record);
        long total = recordService.selectRecordCount(record);

        if (list == null) {
            return getDataTableByException("无权限查看记录");
        }

        // 手动构建分页结果
        TableDataInfo rspData = new TableDataInfo();
        rspData.setCode(200);
        rspData.setMsg("查询成功");
        rspData.setRows(list);
        rspData.setTotal(total);
        return rspData;
    }

    /**
     * 导出对局记录列表
     * 管理员可以导出所有记录，普通用户只能导出自己的记录
     */
    @Log(title = "对局记录", businessType = BusinessType.EXPORT)
    @PostMapping("/export")
    public void export(HttpServletResponse response, Record record) {
        // 获取当前登录用户ID
        Long userId = getUserId();
        if (userId != null) {
            // 判断是否为管理员
            boolean isAdmin = SecurityUtils.isAdmin(userId);

            // 如果不是管理员，只能导出自己的记录
            if (!isAdmin) {
                record.getParams().put("currentUserId", userId);
            }
            // 管理员可以导出所有记录，不设置currentUserId限制

            List<Record> list = recordService.selectRecordList(record);
            if (list != null) {
                ExcelUtil<Record> util = new ExcelUtil<Record>(Record.class);
                util.exportExcel(response, list, "对局记录数据");
            }
        }
    }

    /**
     * 获取对局记录详细信息
     */
    @GetMapping(value = "/{recordId}")
    public AjaxResult getInfo(@PathVariable("recordId") Long recordId) {
        Record record = recordService.selectRecordByRecordId(recordId);
        if (record == null) {
            return error("记录不存在");
        }
        return success(record);
    }

    /**
     * 获取对局详情（包含对局记录和游戏类型信息）
     */
    @GetMapping(value = "/detail/{recordId}")
    public AjaxResult getRecordDetail(@PathVariable("recordId") Long recordId) {
        // 获取对局详情（包含游戏类型信息）
        Map<String, Object> recordDetail = recordService.selectRecordDetailByRecordId(recordId);
        if (recordDetail == null) {
            return error("记录不存在");
        }

        return success(recordDetail);
    }

    /**
     * 获取对局记录热力图数据
     * 需要的参数:
     * - gameTypeId: 游戏类型ID (可选)
     * - gameMode: 游戏模式（0-本地对战，1-与大模型对战，2-联机对战） (可选)
     * - algorithmId: 算法ID (可选)
     * - winner: 赢家 (可选)
     * - playerName: 玩家名称，用于模糊匹配 (可选)
     * <p>
     * 管理员可以查看所有玩家的热力图数据，普通用户只能查看自己的数据
     * <p>
     * 返回格式:
     * {
     * "code": 200,
     * "msg": "操作成功",
     * "data": [
     * { "date": "2023-01-01", "count": 5 },
     * { "date": "2023-01-02", "count": 3 },
     * ...
     * ]
     * }
     */
    @GetMapping("/heatmap")
    public AjaxResult getHeatmapData(
            @RequestParam(required = false) Long gameTypeId,
            @RequestParam(required = false) Integer gameMode,
            @RequestParam(required = false) Long algorithmId,
            @RequestParam(required = false) Long winner,
            @RequestParam(required = false) String playerName) {

        // 获取当前登录用户ID
        Long userId = getUserId();
        if (userId == null) {
            return error("用户未登录");
        }

        // 判断是否为管理员
        boolean isAdmin = SecurityUtils.isAdmin(userId);

        // 获取热力图数据
        // 如果是管理员，传递null作为userId参数，表示查看所有玩家数据
        // 如果是普通用户，传递当前用户ID，只查看自己的数据
        List<Map<String, Object>> heatmapData = recordService.getHeatmapData(
                isAdmin ? null : userId,
                gameTypeId,
                gameMode,
                algorithmId,
                winner,
                playerName
        );

        if (heatmapData == null) {
            return error("获取热力图数据失败");
        }

        return success(heatmapData);
    }

    private TableDataInfo getDataTableByException(String message) {
        TableDataInfo rspData = new TableDataInfo();
        rspData.setCode(500);
        rspData.setMsg(message);
        return rspData;
    }
}
