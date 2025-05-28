<template>
  <div class="app-container">
    <!-- 游戏记录热力图 -->
    <el-card class="box-card">
      <div slot="header" class="clearfix">
        <game-record-heatmap ref="heatmap" :query-params="heatmapParams"></game-record-heatmap>
      </div>
    </el-card>
    
    <el-form :model="queryParams" ref="queryForm" size="small" :inline="true" v-show="showSearch" label-width="100px">
      <el-form-item label="游戏类别" prop="gameTypeId">
        <el-select v-model="queryParams.gameTypeId" placeholder="请选择游戏类别" clearable @change="handleGameTypeChange">
          <el-option
            v-for="item in gameTypeOptions"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
      </el-form-item>
      <el-form-item label="对局类型" prop="gameMode">
        <el-select v-model="queryParams.gameMode" placeholder="请选择对局类型" clearable @change="handleGameModeChange" style="width: 180px">
          <el-option label="本地对战" :value="0" />
          <el-option label="与大模型对战" :value="1" />
          <el-option label="联机对战" :value="2" />
        </el-select>
      </el-form-item>
      <el-form-item label="算法类别" prop="algorithmId">
        <el-select v-model="queryParams.algorithmId" placeholder="请选择算法类别" clearable :disabled="!isLocalGameSelected">
          <el-option
            v-for="item in algorithmOptions"
            :key="item.value"
            :label="item.label"
            :value="item.value"
          />
        </el-select>
      </el-form-item>
      <el-form-item label="玩家" prop="playerName">
        <el-input
          v-model="queryParams.playerName"
          placeholder="请输入玩家名称"
          clearable
          @keyup.enter.native="handleQuery"
        />
      </el-form-item>
      <el-form-item label="赢家" prop="winner">
        <el-select v-model="queryParams.winner" placeholder="请选择赢家" clearable>
          <el-option
            v-for="dict in dict.type.win_status"
            :key="dict.value"
            :label="dict.label"
            :value="dict.value"
          />
        </el-select>
      </el-form-item>
      <el-form-item>
        <el-button type="primary" icon="el-icon-search" size="mini" @click="handleQuery">搜索</el-button>
        <el-button icon="el-icon-refresh" size="mini" @click="resetQuery">重置</el-button>
      </el-form-item>
    </el-form>

    <el-row :gutter="10" class="mb8">
      <el-col :span="1.5">
        <el-button
          type="warning"
          plain
          icon="el-icon-download"
          size="mini"
          @click="handleExport"
        >导出</el-button>
      </el-col>
      <right-toolbar :showSearch.sync="showSearch" @queryTable="getList"></right-toolbar>
    </el-row>

    <el-table v-loading="loading" :data="RecordList" @selection-change="handleSelectionChange" @row-click="handleRowClick">
      <el-table-column type="selection" width="55" align="center" />
      <el-table-column label="对局ID" align="center" prop="recordId" />
      <el-table-column label="游戏类别" align="center" prop="gameTypeName" />
      <el-table-column label="对局结束时间" align="center" prop="recordTime" width="180">
        <template slot-scope="scope">
          <span>{{ parseTime(scope.row.recordTime, '{y}-{m}-{d}') }}</span>
        </template>
      </el-table-column>
      <el-table-column label="对局类型" align="center" prop="gameMode">
        <template slot-scope="scope">
          <span>{{ getGameModeText(scope.row.gameMode) }}</span>
        </template>
      </el-table-column>
      <el-table-column label="算法名称" align="center" prop="algorithmName" />
      <el-table-column label="赢家" align="center" prop="winner">
        <template slot-scope="scope">
          <dict-tag :options="dict.type.win_status" :value="scope.row.winner"/>
        </template>
      </el-table-column>
      <el-table-column label="先手玩家" align="center" prop="firstPlayerName" />
      <el-table-column label="后手玩家" align="center" prop="secondPlayerName" />
    </el-table>
    
    <pagination
      v-show="total>0"
      :total="total"
      :page.sync="queryParams.pageNum"
      :limit.sync="queryParams.pageSize"
      @pagination="getList"
    />

    <!-- 添加或修改对局记录对话框 -->
    <el-dialog :title="title" :visible.sync="open" width="500px" append-to-body>
      <el-form ref="form" :model="form" :rules="rules" label-width="80px">
      </el-form>
      <div slot="footer" class="dialog-footer">
        <el-button type="primary" @click="submitForm">确 定</el-button>
        <el-button @click="cancel">取 消</el-button>
      </div>
    </el-dialog>
  </div>
</template>

<script>
import { listRecord, getRecord, delRecord, addRecord, updateRecord, exportRecord, getRecordDetail } from "@/api/record/record";
import { listGameTypeOptions } from "@/api/GameType/GameType";
import { listAlgorithmOptions } from "@/api/Algorithm/Algorithm";
import { listAlgorithmsByGameId } from "@/api/Product/Product";
import { getPlayerStatistics } from "@/api/PlayerStatistics/PlayerStatistics";
import GameRecordHeatmap from "@/components/GameRecordHeatmap";

export default {
  name: "Record",
  components: {
    GameRecordHeatmap
  },
  dicts: ['win_status'],
  data() {
    return {
      // 遮罩层
      loading: true,
      // 选中数组
      ids: [],
      // 非单个禁用
      single: true,
      // 非多个禁用
      multiple: true,
      // 显示搜索条件
      showSearch: true,
      // 总条数
      total: 0,
      // 对局记录表格数据
      RecordList: [],
      // 弹出层标题
      title: "",
      // 是否显示弹出层
      open: false,
      // 游戏类型选项
      gameTypeOptions: [],
      // 算法类型选项
      algorithmOptions: [],
      // 是否已选择本地对战
      isLocalGameSelected: false,
      // 查询参数
      queryParams: {
        pageNum: 1,
        pageSize: 10,
        gameTypeId: null,
        gameMode: null,
        algorithmId: null,
        winner: null,
        playerName: null
      },
      // 表单参数
      form: {},
      // 表单校验
      rules: {
      },
      // 热力图参数
      heatmapParams: {
        gameTypeId: null,        // 游戏类型ID
        gameMode: null,          // 对局类型
        algorithmId: null,       // 算法ID
        winner: null,            // 赢家
        playerName: null         // 玩家名称，用于模糊匹配
      }
    };
  },
  created() {
    this.getList();
    this.getGameTypeOptions();
    // 初始化检查是否选择了本地对战
    this.isLocalGameSelected = this.queryParams.gameMode === 0;
    // 如果已选择游戏类型且是本地对战，则获取算法选项
    if (this.queryParams.gameTypeId && this.isLocalGameSelected) {
      this.getAlgorithmOptions();
    }
    // 获取玩家统计信息（示例用户ID为102）
    this.getPlayerStatisticsData();
  },
  methods: {
    /** 查询对局记录列表 */
    getList() {
      this.loading = true;
      listRecord(this.queryParams).then(response => {
        this.RecordList = response.rows;
        this.total = response.total;
        this.loading = false;
      }).catch(error => {
        console.error('查询失败:', error);
        this.loading = false;
      });
    },
    /** 获取游戏类型选项 */
    getGameTypeOptions() {
      listGameTypeOptions().then(response => {
        this.gameTypeOptions = response.data;
      });
    },
    /** 获取算法类型选项 */
    getAlgorithmOptions() {
      // 如果选择了游戏类别，则根据游戏类别获取算法
      if (this.queryParams.gameTypeId) {
        listAlgorithmsByGameId(this.queryParams.gameTypeId).then(response => {
          // 转换数据格式，将后端的算法信息转为下拉框需要的格式
          this.algorithmOptions = response.data.map(item => {
            return {
              value: item.algorithmId,
              label: item.algorithmName
            };
          });
        });
      } else {
        // 否则获取所有算法
        listAlgorithmOptions().then(response => {
          this.algorithmOptions = response.data;
        });
      }
    },
    /** 游戏类型变更 */
    handleGameTypeChange() {
      this.queryParams.algorithmId = null;
      if (this.isLocalGameSelected) {
        this.getAlgorithmOptions();
      }
    },
    /** 对局类型变更 */
    handleGameModeChange() {
      this.queryParams.algorithmId = null;
      // 检查是否选择了本地对战(0)
      this.isLocalGameSelected = this.queryParams.gameMode === 0;
      if (this.isLocalGameSelected) {
        this.getAlgorithmOptions();
      }
    },
    /** 获取对局类型文本 */
    getGameModeText(gameMode) {
      const gameModeMap = {
        0: '本地对战',
        1: '与大模型对战', 
        2: '联机对战'
      };
      return gameModeMap[gameMode] || '未知';
    },
    // 取消按钮
    cancel() {
      this.open = false;
      this.reset();
    },
    // 表单重置
    reset() {
      this.form = {
        recordId: null,
        gameTypeId: null,
        gameTypeName: null,
        recordTime: null,
        gameMode: null,
        algorithmId: null,
        algorithmName: null,
        winner: null,
        firstPlayerId: null,
        firstPlayer: null,
        secondPlayerId: null,
        secondPlayerName: null,
        firstPlayerPieces: null,
        playerBPieces: null
      };
      this.resetForm("form");
    },
    /** 搜索按钮操作 */
    handleQuery() {
      this.queryParams.pageNum = 1;
      this.getList();
      
      // 同步查询参数到热力图
      this.heatmapParams = {
        gameTypeId: this.queryParams.gameTypeId,
        gameMode: this.queryParams.gameMode,
        algorithmId: this.queryParams.algorithmId,
        winner: this.queryParams.winner,
        playerName: this.queryParams.playerName
      };
      // 刷新热力图
      if (this.$refs.heatmap) {
        this.$refs.heatmap.refresh();
      }
    },
    /** 重置按钮操作 */
    resetQuery() {
      this.resetForm("queryForm");
      this.isLocalGameSelected = false;
      this.algorithmOptions = [];
      
      // 重置热力图参数
      this.heatmapParams = {
        gameTypeId: null,
        gameMode: null,
        algorithmId: null,
        winner: null,
        playerName: null
      };
      
      // 刷新热力图
      if (this.$refs.heatmap) {
        this.$refs.heatmap.refresh();
      }
      
      this.handleQuery();
    },
    // 多选框选中数据
    handleSelectionChange(selection) {
      this.ids = selection.map(item => item.recordId)
      this.single = selection.length!==1
      this.multiple = !selection.length
    },
    /** 新增按钮操作 */
    handleAdd() {
      this.reset();
      this.open = true;
      this.title = "添加对局记录";
    },
    /** 修改按钮操作 */
    handleUpdate(row) {
      this.reset();
      const recordId = row.recordId || this.ids
      getRecord(recordId).then(response => {
        this.form = response.data;
        this.open = true;
        this.title = "修改对局记录";
      });
    },
    /** 提交按钮 */
    submitForm() {
      this.$refs["form"].validate(valid => {
        if (valid) {
          if (this.form.recordId != null) {
            updateRecord(this.form).then(response => {
              this.$modal.msgSuccess("修改成功");
              this.open = false;
              this.getList();
            });
          } else {
            addRecord(this.form).then(response => {
              this.$modal.msgSuccess("新增成功");
              this.open = false;
              this.getList();
            });
          }
        }
      });
    },
    /** 删除按钮操作 */
    handleDelete(row) {
      const recordIds = row.recordId || this.ids;
      this.$modal.confirm('是否确认删除对局记录编号为"' + recordIds + '"的数据项？').then(function() {
        return delRecord(recordIds);
      }).then(() => {
        this.getList();
        this.$modal.msgSuccess("删除成功");
      }).catch(() => {});
    },
    /** 导出按钮操作 */
    handleExport() {
      this.$confirm('是否确认导出所有对局记录数据项?', "警告", {
        confirmButtonText: "确定",
        cancelButtonText: "取消",
        type: "warning"
      }).then(() => {
        this.$modal.loading("正在导出数据，请稍候...");
        return exportRecord(this.queryParams);
      }).then(response => {
        // 创建a标签下载文件
        const blob = new Blob([response], { type: 'application/vnd.ms-excel' });
        const link = document.createElement('a');
        link.href = window.URL.createObjectURL(blob);
        link.download = '对局记录.xlsx';
        link.click();
        window.URL.revokeObjectURL(link.href);
        this.$modal.closeLoading();
        this.$modal.msgSuccess("导出成功");
      }).catch(() => {
        this.$modal.closeLoading();
      });
    },
    /** 行点击事件 */
    handleRowClick(row) {
      // 获取对局详情
      getRecordDetail(row.recordId).then(response => {
        console.log('对局详情数据:', response.data);
        // 跳转到空白页面
        this.$router.push({
          path: '/record/detail',
          query: { recordId: row.recordId }
        });
      }).catch(error => {
        console.error('获取对局详情失败:', error);
        this.$modal.msgError("获取对局详情失败");
      });
    },
    // 获取玩家统计信息（示例用户ID为102）
    getPlayerStatisticsData() {
      // 获取当前登录用户的ID
      const currentUserId = this.$store.state.user.id;
      if (!currentUserId) {
        console.warn('当前用户未登录，无法获取统计信息');
        return;
      }
      
      getPlayerStatistics(currentUserId).then(response => {
        console.log('玩家统计信息:', response.data);
      }).catch(error => {
        console.error('获取玩家统计信息失败:', error);
        this.$modal.msgError("获取玩家统计信息失败");
      });
    }
  }
};
</script>

<style scoped>
.box-card {
  margin-bottom: 20px;
}

.el-card__body {
  overflow-x: auto; /* 添加横向滚动 */
  padding: 20px;
}
</style>
