package com.gamehive.controller;

import java.util.List;
import javax.servlet.http.HttpServletResponse;
import org.springframework.security.access.prepost.PreAuthorize;
import org.springframework.beans.factory.annotation.Autowired;
import org.springframework.web.bind.annotation.GetMapping;
import org.springframework.web.bind.annotation.PostMapping;
import org.springframework.web.bind.annotation.PutMapping;
import org.springframework.web.bind.annotation.DeleteMapping;
import org.springframework.web.bind.annotation.PathVariable;
import org.springframework.web.bind.annotation.RequestBody;
import org.springframework.web.bind.annotation.RequestMapping;
import org.springframework.web.bind.annotation.RequestParam;
import org.springframework.web.bind.annotation.RestController;
import com.gamehive.common.annotation.Log;
import com.gamehive.common.core.controller.BaseController;
import com.gamehive.common.core.domain.AjaxResult;
import com.gamehive.common.enums.BusinessType;
import com.gamehive.pojo.Record;
import com.gamehive.service.IRecordService;
import com.gamehive.common.utils.poi.ExcelUtil;
import com.gamehive.common.core.page.TableDataInfo;
import com.github.pagehelper.PageHelper;

/**
 * 对局记录Controller
 *
 * @author Cassifa
 * @date 2025-05-05
 */
@RestController
@RequestMapping("/Record/Record")
public class RecordController extends BaseController {

    @Autowired
    private IRecordService recordService;

    /**
     * 查询对局记录列表
     * 支持的查询条件:
     * - pageNum: 当前页码
     * - pageSize: 每页记录数（如果为空则返回所有数据）
     * - gameTypeId: 游戏类型ID
     * - isPkAi: 是否与AI对局（'1'表示是，'0'表示否）
     * - algorithmId: 算法ID
     * - winner: 赢家
     * - playerName: 玩家名称（用于模糊查询先手或后手玩家）
     */
    @PreAuthorize("@ss.hasPermi('system:record:list')")
    @GetMapping("/list")
    public TableDataInfo list(
            @RequestParam(value = "pageNum", required = false) Integer pageNum,
            @RequestParam(value = "pageSize", required = false) Integer pageSize,
            @RequestParam(value = "gameTypeId", required = false) Long gameTypeId,
            @RequestParam(value = "isPkAi", required = false) Boolean isPkAi,
            @RequestParam(value = "algorithmId", required = false) Long algorithmId,
            @RequestParam(value = "winner", required = false) Long winner,
            @RequestParam(value = "playerName", required = false) String playerName,
            Record record) {
        
        // 设置查询参数
        if (gameTypeId != null) {
            record.setGameTypeId(gameTypeId);
        }
        if (isPkAi != null) {
            record.setIsPkAi(isPkAi);
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
        
        // 如果pageSize不为空，使用分页查询
        if (pageSize != null && pageSize > 0) {
            // 使用PageHelper进行分页
            int pageNumValue = (pageNum != null && pageNum > 0) ? pageNum : 1;
            PageHelper.startPage(pageNumValue, pageSize);
        }
        // 否则不分页，返回所有数据
        
        List<Record> list = recordService.selectRecordList(record);
        return getDataTable(list);
    }

    /**
     * 导出对局记录列表
     */
    @PreAuthorize("@ss.hasPermi('system:record:export')")
    @Log(title = "对局记录", businessType = BusinessType.EXPORT)
    @PostMapping("/export")
    public void export(HttpServletResponse response, Record record) {
        List<Record> list = recordService.selectRecordList(record);
        ExcelUtil<Record> util = new ExcelUtil<Record>(Record.class);
        util.exportExcel(response, list, "对局记录数据");
    }

    /**
     * 获取对局记录详细信息
     */
    @PreAuthorize("@ss.hasPermi('system:record:query')")
    @GetMapping(value = "/{recordId}")
    public AjaxResult getInfo(@PathVariable("recordId") Long recordId) {
        return success(recordService.selectRecordByRecordId(recordId));
    }

    /**
     * 新增对局记录
     */
    @PreAuthorize("@ss.hasPermi('system:record:add')")
    @Log(title = "对局记录", businessType = BusinessType.INSERT)
    @PostMapping
    public AjaxResult add(@RequestBody Record record) {
        return toAjax(recordService.insertRecord(record));
    }

    /**
     * 修改对局记录
     */
    @PreAuthorize("@ss.hasPermi('system:record:edit')")
    @Log(title = "对局记录", businessType = BusinessType.UPDATE)
    @PutMapping
    public AjaxResult edit(@RequestBody Record record) {
        return toAjax(recordService.updateRecord(record));
    }

    /**
     * 删除对局记录
     */
    @PreAuthorize("@ss.hasPermi('system:record:remove')")
    @Log(title = "对局记录", businessType = BusinessType.DELETE)
    @DeleteMapping("/{recordIds}")
    public AjaxResult remove(@PathVariable Long[] recordIds) {
        return toAjax(recordService.deleteRecordByRecordIds(recordIds));
    }
}
