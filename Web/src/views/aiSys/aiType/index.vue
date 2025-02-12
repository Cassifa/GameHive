<template>
  <div class="app-container">
    <el-form :model="queryParams" ref="queryForm" size="small" :inline="true" v-show="showSearch" label-width="68px">
      <el-form-item label="AI编号" prop="aiId">
        <el-input
          v-model="queryParams.aiId"
          placeholder="请输入AI编号"
          clearable
          @keyup.enter.native="handleQuery"
        />
      </el-form-item>
      <el-form-item label="AI名称" prop="aiName">
        <el-input
          v-model="queryParams.aiName"
          placeholder="请输入AI名称"
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
          v-hasPermi="['aiSys:aiType:add']"
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
          v-hasPermi="['aiSys:aiType:edit']"
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
          v-hasPermi="['aiSys:aiType:remove']"
        >删除</el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button
          type="warning"
          plain
          icon="el-icon-download"
          size="mini"
          @click="handleExport"
          v-hasPermi="['aiSys:aiType:export']"
        >导出</el-button>
      </el-col>
      <right-toolbar :showSearch.sync="showSearch" @queryTable="getList"></right-toolbar>
    </el-row>

    <el-table v-loading="loading" :data="aiTypeList" @selection-change="handleSelectionChange">
      <el-table-column type="selection" width="55" align="center" />
      <el-table-column label="AI编号" align="center" prop="aiId" />
      <el-table-column label="AI名称" align="center" prop="aiName" />
      <el-table-column label="AI简介" align="center" prop="aiIntroduction" />
      <el-table-column label="操作" align="center" class-name="small-padding fixed-width">
        <template slot-scope="scope">
          <el-button
            size="mini"
            type="text"
            icon="el-icon-edit"
            @click="handleUpdate(scope.row)"
            v-hasPermi="['aiSys:aiType:edit']"
          >修改</el-button>
          <el-button
            size="mini"
            type="text"
            icon="el-icon-delete"
            @click="handleDelete(scope.row)"
            v-hasPermi="['aiSys:aiType:remove']"
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

    <!-- 添加或修改AI类型对话框 -->
    <el-dialog :title="title" :visible.sync="open" width="500px" append-to-body>
      <el-form ref="form" :model="form" :rules="rules" label-width="80px">
        <el-form-item label="AI名称" prop="aiName">
          <el-input v-model="form.aiName" placeholder="请输入AI名称" />
        </el-form-item>
        <el-form-item label="AI简介" prop="aiIntroduction">
          <el-input v-model="form.aiIntroduction" type="textarea" placeholder="请输入内容" />
        </el-form-item>
        <el-divider content-position="center">AI-Game具体产品信息</el-divider>
        <el-row :gutter="10" class="mb8">
          <el-col :span="1.5">
            <el-button type="primary" icon="el-icon-plus" size="mini" @click="handleAddAiGame">添加</el-button>
          </el-col>
          <el-col :span="1.5">
            <el-button type="danger" icon="el-icon-delete" size="mini" @click="handleDeleteAiGame">删除</el-button>
          </el-col>
        </el-row>
        <el-table :data="aiGameList" :row-class-name="rowAiGameIndex" @selection-change="handleAiGameSelectionChange" ref="aiGame">
          <el-table-column type="selection" width="50" align="center" />
          <el-table-column label="序号" align="center" prop="index" width="50"/>
          <el-table-column label="游戏类别" prop="gameTypeId" width="150">
            <template slot-scope="scope">
              <el-input v-model="scope.row.gameTypeId" placeholder="请输入游戏类别" />
            </template>
          </el-table-column>
          <el-table-column label="难度" prop="level" width="150">
            <template slot-scope="scope">
              <el-input v-model="scope.row.level" placeholder="请输入难度" />
            </template>
          </el-table-column>
          <el-table-column label="被挑战次数" prop="challengedCount" width="150">
            <template slot-scope="scope">
              <el-input v-model="scope.row.challengedCount" placeholder="请输入被挑战次数" />
            </template>
          </el-table-column>
          <el-table-column label="胜利次数" prop="winCount" width="150">
            <template slot-scope="scope">
              <el-input v-model="scope.row.winCount" placeholder="请输入胜利次数" />
            </template>
          </el-table-column>
        </el-table>
      </el-form>
      <div slot="footer" class="dialog-footer">
        <el-button type="primary" @click="submitForm">确 定</el-button>
        <el-button @click="cancel">取 消</el-button>
      </div>
    </el-dialog>
  </div>
</template>

<script>
import { listAiType, getAiType, delAiType, addAiType, updateAiType } from "@/api/aiSys/aiType";

export default {
  name: "AiType",
  data() {
    return {
      // 遮罩层
      loading: true,
      // 选中数组
      ids: [],
      // 子表选中数据
      checkedAiGame: [],
      // 非单个禁用
      single: true,
      // 非多个禁用
      multiple: true,
      // 显示搜索条件
      showSearch: true,
      // 总条数
      total: 0,
      // AI类型表格数据
      aiTypeList: [],
      // AI-Game具体产品表格数据
      aiGameList: [],
      // 弹出层标题
      title: "",
      // 是否显示弹出层
      open: false,
      // 查询参数
      queryParams: {
        pageNum: 1,
        pageSize: 10,
        aiId: null,
        aiName: null,
        aiIntroduction: null
      },
      // 表单参数
      form: {},
      // 表单校验
      rules: {
        aiName: [
          { required: true, message: "AI名称不能为空", trigger: "blur" }
        ],
      }
    };
  },
  created() {
    this.getList();
  },
  methods: {
    /** 查询AI类型列表 */
    getList() {
      this.loading = true;
      listAiType(this.queryParams).then(response => {
        this.aiTypeList = response.rows;
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
        aiId: null,
        aiName: null,
        aiIntroduction: null
      };
      this.aiGameList = [];
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
      this.ids = selection.map(item => item.aiId)
      this.single = selection.length!==1
      this.multiple = !selection.length
    },
    /** 新增按钮操作 */
    handleAdd() {
      this.reset();
      this.open = true;
      this.title = "添加AI类型";
    },
    /** 修改按钮操作 */
    handleUpdate(row) {
      this.reset();
      const aiId = row.aiId || this.ids
      getAiType(aiId).then(response => {
        this.form = response.data;
        this.aiGameList = response.data.aiGameList;
        this.open = true;
        this.title = "修改AI类型";
      });
    },
    /** 提交按钮 */
    submitForm() {
      this.$refs["form"].validate(valid => {
        if (valid) {
          this.form.aiGameList = this.aiGameList;
          if (this.form.aiId != null) {
            updateAiType(this.form).then(response => {
              this.$modal.msgSuccess("修改成功");
              this.open = false;
              this.getList();
            });
          } else {
            addAiType(this.form).then(response => {
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
      const aiIds = row.aiId || this.ids;
      this.$modal.confirm('是否确认删除AI类型编号为"' + aiIds + '"的数据项？').then(function() {
        return delAiType(aiIds);
      }).then(() => {
        this.getList();
        this.$modal.msgSuccess("删除成功");
      }).catch(() => {});
    },
	/** AI-Game具体产品序号 */
    rowAiGameIndex({ row, rowIndex }) {
      row.index = rowIndex + 1;
    },
    /** AI-Game具体产品添加按钮操作 */
    handleAddAiGame() {
      let obj = {};
      obj.gameTypeId = "";
      obj.level = "";
      obj.challengedCount = "";
      obj.winCount = "";
      this.aiGameList.push(obj);
    },
    /** AI-Game具体产品删除按钮操作 */
    handleDeleteAiGame() {
      if (this.checkedAiGame.length == 0) {
        this.$modal.msgError("请先选择要删除的AI-Game具体产品数据");
      } else {
        const aiGameList = this.aiGameList;
        const checkedAiGame = this.checkedAiGame;
        this.aiGameList = aiGameList.filter(function(item) {
          return checkedAiGame.indexOf(item.index) == -1
        });
      }
    },
    /** 复选框选中数据 */
    handleAiGameSelectionChange(selection) {
      this.checkedAiGame = selection.map(item => item.index)
    },
    /** 导出按钮操作 */
    handleExport() {
      this.download('aiSys/aiType/export', {
        ...this.queryParams
      }, `aiType_${new Date().getTime()}.xlsx`)
    }
  }
};
</script>
