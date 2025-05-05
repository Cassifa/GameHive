<template>
  <div class="app-container">
    <!-- 游戏记录热力图 -->
    <el-card class="box-card">
      <div slot="header" class="clearfix">
        <game-record-heatmap ref="heatmap" :query-params="heatmapParams"></game-record-heatmap>
      </div>
    </el-card>
    
    <el-form :model="queryParams" ref="queryForm" size="small" :inline="true" v-show="showSearch" label-width="90px">
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
      <el-form-item label="是否与AI对局" prop="isPkAi">
        <el-select v-model="queryParams.isPkAi" placeholder="请选择是否与AI对局" clearable @change="handlePkAiChange">
          <el-option label="是" value="1" />
          <el-option label="否" value="0" />
        </el-select>
      </el-form-item>
      <el-form-item label="算法类别" prop="algorithmId">
        <el-select v-model="queryParams.algorithmId" placeholder="请选择算法类别" clearable :disabled="!isPkAiSelected">
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
          type="primary"
          plain
          icon="el-icon-plus"
          size="mini"
          @click="handleAdd"
          v-hasPermi="['Record:Record:add']"
        >新增</el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button
          type="success"
          plain
          icon="el-icon-edit"
          size="mini"
          :disabled="single"
          @click="handleUpdate"
          v-hasPermi="['Record:Record:edit']"
        >修改</el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button
          type="danger"
          plain
          icon="el-icon-delete"
          size="mini"
          :disabled="multiple"
          @click="handleDelete"
          v-hasPermi="['Record:Record:remove']"
        >删除</el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button
          type="warning"
          plain
          icon="el-icon-download"
          size="mini"
          @click="handleExport"
          v-hasPermi="['Record:Record:export']"
        >导出</el-button>
      </el-col>
      <right-toolbar :showSearch.sync="showSearch" @queryTable="getList"></right-toolbar>
    </el-row>

    <el-table v-loading="loading" :data="RecordList" @selection-change="handleSelectionChange">
      <el-table-column type="selection" width="55" align="center" />
      <el-table-column label="对局ID" align="center" prop="recordId" />
      <el-table-column label="游戏类别" align="center" prop="gameTypeName" />
      <el-table-column label="对局结束时间" align="center" prop="recordTime" width="180">
        <template slot-scope="scope">
          <span>{{ parseTime(scope.row.recordTime, '{y}-{m}-{d}') }}</span>
        </template>
      </el-table-column>
      <el-table-column label="是否与AI对局" align="center" prop="isPkAi">
        <template slot-scope="scope">
          <span>{{ scope.row.isPkAi == 1 ? '是' : '否' }}</span>
        </template>
      </el-table-column>
      <el-table-column label="算法名称" align="center" prop="algorithmName" />
      <el-table-column label="赢家" align="center" prop="winner">
        <template slot-scope="scope">
          <dict-tag :options="dict.type.win_status" :value="scope.row.winner"/>
        </template>
      </el-table-column>
      <el-table-column label="先手玩家" align="center" prop="firstPlayer" />
      <el-table-column label="后手玩家" align="center" prop="secondPlayerName" />
      <el-table-column label="操作" align="center" class-name="small-padding fixed-width">
        <template slot-scope="scope">
          <el-button
            size="mini"
            type="text"
            icon="el-icon-edit"
            @click="handleUpdate(scope.row)"
            v-hasPermi="['Record:Record:edit']"
          >修改</el-button>
          <el-button
            size="mini"
            type="text"
            icon="el-icon-delete"
            @click="handleDelete(scope.row)"
            v-hasPermi="['Record:Record:remove']"
          >删除</el-button>
        </template>
      </el-table-column>
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
import { listRecord, getRecord, delRecord, addRecord, updateRecord } from "@/api/record/record";
import { listGameTypeOptions } from "@/api/GameType/GameType";
import { listAlgorithmOptions } from "@/api/Algorithm/Algorithm";
import { listAlgorithmsByGameId } from "@/api/Product/Product";
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
      // 是否已选择与AI对局
      isPkAiSelected: false,
      // 查询参数
      queryParams: {
        pageNum: 1,
        pageSize: 10,
        gameTypeId: null,
        isPkAi: null,
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
        isPkAi: null,            // 是否与AI对局
        algorithmId: null,       // 算法ID
        winner: null,            // 赢家
        playerName: null         // 玩家名称，用于模糊匹配
      }
    };
  },
  watch: {
    // 监听查询参数变化，同步到热力图参数
    'queryParams.gameTypeId': function(val) {
      this.heatmapParams.gameTypeId = val;
      this.refreshHeatmap();
    },
    'queryParams.isPkAi': function(val) {
      this.heatmapParams.isPkAi = val;
      this.refreshHeatmap();
    },
    'queryParams.algorithmId': function(val) {
      this.heatmapParams.algorithmId = val;
      this.refreshHeatmap();
    },
    'queryParams.winner': function(val) {
      this.heatmapParams.winner = val;
      this.refreshHeatmap();
    },
    'queryParams.playerName': function(val) {
      this.heatmapParams.playerName = val;
      this.refreshHeatmap();
    }
  },
  created() {
    this.getList();
    this.getGameTypeOptions();
    // 初始化检查是否与AI对局的选择状态
    this.isPkAiSelected = this.queryParams.isPkAi === '1' || this.queryParams.isPkAi === 1;
    // 如果已选择游戏类型且是与AI对局，则获取算法选项
    if (this.queryParams.gameTypeId && this.isPkAiSelected) {
      this.getAlgorithmOptions();
    }
  },
  methods: {
    /** 查询对局记录列表 */
    getList() {
      this.loading = true;
      listRecord(this.queryParams).then(response => {
        this.RecordList = response.rows;
        this.total = response.total;
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
      if (this.isPkAiSelected) {
        this.getAlgorithmOptions();
      }
    },
    /** 是否与AI对局变更 */
    handlePkAiChange() {
      this.queryParams.algorithmId = null;
      // 检查是否选择了"是"(1)
      this.isPkAiSelected = this.queryParams.isPkAi === '1' || this.queryParams.isPkAi === 1;
      if (this.isPkAiSelected) {
        this.getAlgorithmOptions();
      }
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
        isPkAi: null,
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
        isPkAi: this.queryParams.isPkAi,
        algorithmId: this.queryParams.algorithmId,
        winner: this.queryParams.winner,
        playerName: this.queryParams.playerName
      };
      this.refreshHeatmap();
    },
    /** 重置按钮操作 */
    resetQuery() {
      this.resetForm("queryForm");
      this.isPkAiSelected = false;
      this.algorithmOptions = [];
      
      // 重置热力图参数
      this.heatmapParams = {
        gameTypeId: null,
        isPkAi: null,
        algorithmId: null,
        winner: null,
        playerName: null
      };
      
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
      this.download('Record/Record/export', {
        ...this.queryParams
      }, `Record_${new Date().getTime()}.xlsx`)
    },
    // 刷新热力图
    refreshHeatmap() {
      // 延迟执行，避免频繁刷新
      clearTimeout(this.heatmapTimer);
      this.heatmapTimer = setTimeout(() => {
        if (this.$refs.heatmap) {
          this.$refs.heatmap.refresh();
        }
      }, 500);
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
