<template>
  <div class="app-container">
    <el-form :model="queryParams" ref="queryForm" size="small" :inline="true" v-show="showSearch" label-width="68px">
      <el-form-item label="游戏类别" prop="gameTypeId">
        <el-input
          v-model="queryParams.gameTypeId"
          placeholder="请输入游戏类别"
          clearable
          @keyup.enter.native="handleQuery"
        />
      </el-form-item>
      <el-form-item label="对局时间" prop="recordTime">
        <el-date-picker clearable
          v-model="queryParams.recordTime"
          type="date"
          value-format="yyyy-MM-dd"
          placeholder="请选择对局时间">
        </el-date-picker>
      </el-form-item>
      <el-form-item label="是否与AI对战" prop="isPkAi">
        <el-input
          v-model="queryParams.isPkAi"
          placeholder="请输入是否与AI对战"
          clearable
          @keyup.enter.native="handleQuery"
        />
      </el-form-item>
      <el-form-item label="对战AI" prop="aiGameId">
        <el-input
          v-model="queryParams.aiGameId"
          placeholder="请输入对战AI"
          clearable
          @keyup.enter.native="handleQuery"
        />
      </el-form-item>
      <el-form-item label="玩家A是否先手" prop="isAFirst">
        <el-input
          v-model="queryParams.isAFirst"
          placeholder="请输入玩家A是否先手"
          clearable
          @keyup.enter.native="handleQuery"
        />
      </el-form-item>
      <el-form-item label="赢家" prop="winner">
        <el-input
          v-model="queryParams.winner"
          placeholder="请输入赢家"
          clearable
          @keyup.enter.native="handleQuery"
        />
      </el-form-item>
      <el-form-item label="A玩家ID" prop="playerAId">
        <el-input
          v-model="queryParams.playerAId"
          placeholder="请输入A玩家ID"
          clearable
          @keyup.enter.native="handleQuery"
        />
      </el-form-item>
      <el-form-item label="B玩家ID" prop="playerBId">
        <el-input
          v-model="queryParams.playerBId"
          placeholder="请输入B玩家ID"
          clearable
          @keyup.enter.native="handleQuery"
        />
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
          v-hasPermi="['record:record:add']"
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
          v-hasPermi="['record:record:edit']"
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
          v-hasPermi="['record:record:remove']"
        >删除</el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button
          type="warning"
          plain
          icon="el-icon-download"
          size="mini"
          @click="handleExport"
          v-hasPermi="['record:record:export']"
        >导出</el-button>
      </el-col>
      <right-toolbar :showSearch.sync="showSearch" @queryTable="getList"></right-toolbar>
    </el-row>

    <el-table v-loading="loading" :data="recordList" @selection-change="handleSelectionChange">
      <el-table-column type="selection" width="55" align="center" />
      <el-table-column label="对局编号" align="center" prop="recordId" />
      <el-table-column label="游戏类别" align="center" prop="gameTypeId">
        <template slot-scope="scope">
          <dict-tag :options="dict.type.game_type" :value="scope.row.gameTypeId"/>
        </template>
      </el-table-column>
      <el-table-column label="对局时间" align="center" prop="recordTime" width="180">
        <template slot-scope="scope">
          <span>{{ parseTime(scope.row.recordTime, '{y}-{m}-{d}') }}</span>
        </template>
      </el-table-column>
      <el-table-column label="是否与AI对战" align="center" prop="isPkAi" />
      <el-table-column label="对战AI" align="center" prop="aiGameId" />
      <el-table-column label="玩家A是否先手" align="center" prop="isAFirst" />
      <el-table-column label="赢家" align="center" prop="winner" />
      <el-table-column label="A玩家ID" align="center" prop="playerAId" />
      <el-table-column label="B玩家ID" align="center" prop="playerBId" />
      <el-table-column label="玩家A操作序列" align="center" prop="playerAPieces" />
      <el-table-column label="玩家B操作序列" align="center" prop="playerBPieces" />
      <el-table-column label="操作" align="center" class-name="small-padding fixed-width">
        <template slot-scope="scope">
          <el-button
            size="mini"
            type="text"
            icon="el-icon-edit"
            @click="handleUpdate(scope.row)"
            v-hasPermi="['record:record:edit']"
          >修改</el-button>
          <el-button
            size="mini"
            type="text"
            icon="el-icon-delete"
            @click="handleDelete(scope.row)"
            v-hasPermi="['record:record:remove']"
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

export default {
  name: "Record",
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
      recordList: [],
      // 弹出层标题
      title: "",
      // 是否显示弹出层
      open: false,
      // 查询参数
      queryParams: {
        pageNum: 1,
        pageSize: 10,
        gameTypeId: null,
        recordTime: null,
        isPkAi: null,
        aiGameId: null,
        isAFirst: null,
        winner: null,
        playerAId: null,
        playerBId: null,
      },
      // 表单参数
      form: {},
      // 表单校验
      rules: {
        gameTypeId: [
          { required: true, message: "游戏类别不能为空", trigger: "blur" }
        ],
        recordTime: [
          { required: true, message: "对局时间不能为空", trigger: "blur" }
        ],
        isPkAi: [
          { required: true, message: "是否与AI对战不能为空", trigger: "blur" }
        ],
        aiGameId: [
          { required: true, message: "对战AI不能为空", trigger: "blur" }
        ],
        isAFirst: [
          { required: true, message: "玩家A是否先手不能为空", trigger: "blur" }
        ],
        winner: [
          { required: true, message: "赢家不能为空", trigger: "blur" }
        ],
        playerAId: [
          { required: true, message: "A玩家ID不能为空", trigger: "blur" }
        ],
        playerBId: [
          { required: true, message: "B玩家ID不能为空", trigger: "blur" }
        ],
        playerAPieces: [
          { required: true, message: "玩家A操作序列不能为空", trigger: "blur" }
        ],
        playerBPieces: [
          { required: true, message: "玩家B操作序列不能为空", trigger: "blur" }
        ]
      }
    };
  },
  created() {
    this.getList();
  },
  methods: {
    /** 查询对局记录列表 */
    getList() {
      this.loading = true;
      listRecord(this.queryParams).then(response => {
        this.recordList = response.rows;
        this.total = response.total;
        this.loading = false;
      });
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
        recordTime: null,
        isPkAi: null,
        aiGameId: null,
        isAFirst: null,
        winner: null,
        playerAId: null,
        playerBId: null,
        playerAPieces: null,
        playerBPieces: null
      };
      this.resetForm("form");
    },
    /** 搜索按钮操作 */
    handleQuery() {
      this.queryParams.pageNum = 1;
      this.getList();
    },
    /** 重置按钮操作 */
    resetQuery() {
      this.resetForm("queryForm");
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
      this.download('record/record/export', {
        ...this.queryParams
      }, `record_${new Date().getTime()}.xlsx`)
    }
  }
};
</script>
