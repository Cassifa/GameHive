<template>
  <div class="app-container">
    <el-form :model="queryParams" ref="queryForm" size="small" :inline="true" v-show="showSearch" label-width="68px">
      <el-form-item label="AI类别" prop="aiTypeId">
        <el-input
          v-model="queryParams.aiTypeId"
          placeholder="请输入AI类别"
          clearable
          @keyup.enter.native="handleQuery"
        />
      </el-form-item>
      <el-form-item label="游戏类别" prop="gameTypeId">
        <el-input
          v-model="queryParams.gameTypeId"
          placeholder="请输入游戏类别"
          clearable
          @keyup.enter.native="handleQuery"
        />
      </el-form-item>
      <el-form-item label="难度" prop="level">
        <el-input
          v-model="queryParams.level"
          placeholder="请输入难度"
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
          v-hasPermi="['aiSys:aiProduct:add']"
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
          v-hasPermi="['aiSys:aiProduct:edit']"
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
          v-hasPermi="['aiSys:aiProduct:remove']"
        >删除</el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button
          type="warning"
          plain
          icon="el-icon-download"
          size="mini"
          @click="handleExport"
          v-hasPermi="['aiSys:aiProduct:export']"
        >导出</el-button>
      </el-col>
      <right-toolbar :showSearch.sync="showSearch" @queryTable="getList"></right-toolbar>
    </el-row>

    <el-table v-loading="loading" :data="aiProductList" @selection-change="handleSelectionChange">
      <el-table-column type="selection" width="55" align="center" />
      <el-table-column label="编号" align="center" prop="id" />
      <el-table-column label="AI类别" align="center" prop="aiTypeId">
        <template slot-scope="scope">
          <dict-tag :options="dict.type.ai_type" :value="scope.row.aiTypeId"/>
        </template>
      </el-table-column>
      <el-table-column label="游戏类别" align="center" prop="gameTypeId">
        <template slot-scope="scope">
          <dict-tag :options="dict.type.game_type" :value="scope.row.gameTypeId"/>
        </template>
      </el-table-column>
      <el-table-column label="难度" align="center" prop="level" />
      <el-table-column label="被挑战次数" align="center" prop="challengedCount" />
      <el-table-column label="胜利次数" align="center" prop="winCount" />
      <el-table-column label="操作" align="center" class-name="small-padding fixed-width">
        <template slot-scope="scope">
          <el-button
            size="mini"
            type="text"
            icon="el-icon-edit"
            @click="handleUpdate(scope.row)"
            v-hasPermi="['aiSys:aiProduct:edit']"
          >修改</el-button>
          <el-button
            size="mini"
            type="text"
            icon="el-icon-delete"
            @click="handleDelete(scope.row)"
            v-hasPermi="['aiSys:aiProduct:remove']"
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

    <!-- 添加或修改AI-Game具体产品对话框 -->
    <el-dialog :title="title" :visible.sync="open" width="500px" append-to-body>
      <el-form ref="form" :model="form" :rules="rules" label-width="80px">
        <el-form-item label="AI类别" prop="aiTypeId">
          <el-input v-model="form.aiTypeId" placeholder="请输入AI类别" />
        </el-form-item>
        <el-form-item label="游戏类别" prop="gameTypeId">
          <el-input v-model="form.gameTypeId" placeholder="请输入游戏类别" />
        </el-form-item>
        <el-form-item label="难度" prop="level">
          <el-input v-model="form.level" placeholder="请输入难度" />
        </el-form-item>
      </el-form>
      <div slot="footer" class="dialog-footer">
        <el-button type="primary" @click="submitForm">确 定</el-button>
        <el-button @click="cancel">取 消</el-button>
      </div>
    </el-dialog>
  </div>
</template>

<script>
import { listAiProduct, getAiProduct, delAiProduct, addAiProduct, updateAiProduct } from "@/api/aiSys/aiProduct";

export default {
  name: "AiProduct",
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
      // AI-Game具体产品表格数据
      aiProductList: [],
      // 弹出层标题
      title: "",
      // 是否显示弹出层
      open: false,
      // 查询参数
      queryParams: {
        pageNum: 1,
        pageSize: 10,
        aiTypeId: null,
        gameTypeId: null,
        level: null,
      },
      // 表单参数
      form: {},
      // 表单校验
      rules: {
        aiTypeId: [
          { required: true, message: "AI类别不能为空", trigger: "blur" }
        ],
        gameTypeId: [
          { required: true, message: "游戏类别不能为空", trigger: "blur" }
        ],
      }
    };
  },
  created() {
    this.getList();
  },
  methods: {
    /** 查询AI-Game具体产品列表 */
    getList() {
      this.loading = true;
      listAiProduct(this.queryParams).then(response => {
        this.aiProductList = response.rows;
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
        id: null,
        aiTypeId: null,
        gameTypeId: null,
        level: null,
        challengedCount: null,
        winCount: null
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
      this.ids = selection.map(item => item.id)
      this.single = selection.length!==1
      this.multiple = !selection.length
    },
    /** 新增按钮操作 */
    handleAdd() {
      this.reset();
      this.open = true;
      this.title = "添加AI-Game具体产品";
    },
    /** 修改按钮操作 */
    handleUpdate(row) {
      this.reset();
      const id = row.id || this.ids
      getAiProduct(id).then(response => {
        this.form = response.data;
        this.open = true;
        this.title = "修改AI-Game具体产品";
      });
    },
    /** 提交按钮 */
    submitForm() {
      this.$refs["form"].validate(valid => {
        if (valid) {
          if (this.form.id != null) {
            updateAiProduct(this.form).then(response => {
              this.$modal.msgSuccess("修改成功");
              this.open = false;
              this.getList();
            });
          } else {
            addAiProduct(this.form).then(response => {
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
      const ids = row.id || this.ids;
      this.$modal.confirm('是否确认删除AI-Game具体产品编号为"' + ids + '"的数据项？').then(function() {
        return delAiProduct(ids);
      }).then(() => {
        this.getList();
        this.$modal.msgSuccess("删除成功");
      }).catch(() => {});
    },
    /** 导出按钮操作 */
    handleExport() {
      this.download('aiSys/aiProduct/export', {
        ...this.queryParams
      }, `aiProduct_${new Date().getTime()}.xlsx`)
    }
  }
};
</script>
