<template>
  <div class="app-container">
    <el-form :model="queryParams" ref="queryForm" size="small" :inline="true" v-show="showSearch" label-width="100px">
      <el-form-item label="算法名称" prop="algorithmTypeId">
        <el-select v-model="queryParams.algorithmTypeId" placeholder="请选择算法名称" clearable>
          <el-option
            v-for="item in algorithmOptions"
            :key="item.value"
            :label="item.label"
            :value="item.value">
          </el-option>
        </el-select>
      </el-form-item>
      <el-form-item label="游戏类型" prop="gameTypeId">
        <el-select v-model="queryParams.gameTypeId" placeholder="请选择游戏类型" clearable>
          <el-option
            v-for="item in gameTypeOptions"
            :key="item.value"
            :label="item.label"
            :value="item.value">
          </el-option>
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
          v-hasPermi="['Product:Product:add']"
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
          v-hasPermi="['Product:Product:edit']"
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
          v-hasPermi="['Product:Product:remove']"
        >删除</el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button
          type="warning"
          plain
          icon="el-icon-download"
          size="mini"
          @click="handleExport"
          v-hasPermi="['Product:Product:export']"
        >导出</el-button>
      </el-col>
      <right-toolbar :showSearch.sync="showSearch" @queryTable="getList"></right-toolbar>
    </el-row>

    <el-table v-loading="loading" :data="ProductList" @selection-change="handleSelectionChange">
      <el-table-column type="selection" width="55" align="center" />
      <el-table-column label="产品ID" align="center" prop="id" />
      <el-table-column label="算法名称" align="center" prop="algorithmTypeName" />
      <el-table-column label="游戏类型中文名" align="center" prop="gameTypeName" />
      <el-table-column label="最高难度等级" align="center" prop="maximumLevel" />
      <el-table-column label="被玩家挑战次数" align="center" prop="challengedCount" />
      <el-table-column label="胜利次数" align="center" prop="winCount" />
      <el-table-column label="操作" align="center" class-name="small-padding fixed-width">
        <template slot-scope="scope">
          <el-button
            size="mini"
            type="text"
            icon="el-icon-edit"
            @click="handleUpdate(scope.row)"
            v-hasPermi="['Product:Product:edit']"
          >修改</el-button>
          <el-button
            size="mini"
            type="text"
            icon="el-icon-delete"
            @click="handleDelete(scope.row)"
            v-hasPermi="['Product:Product:remove']"
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

    <!-- 添加或修改AI产品对话框 -->
    <el-dialog :title="title" :visible.sync="open" width="500px" append-to-body>
      <el-form ref="form" :model="form" :rules="rules" label-width="120px">
        <el-form-item label="算法名称" prop="algorithmTypeName">
          <el-select v-model="form.algorithmTypeId" placeholder="请选择算法名称" @change="handleAlgorithmChange">
            <el-option
              v-for="item in algorithmOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value">
            </el-option>
          </el-select>
        </el-form-item>
        <el-form-item label="游戏类型中文名" prop="gameTypeName">
          <el-select v-model="form.gameTypeId" placeholder="请选择游戏类型" @change="handleGameTypeChange">
            <el-option
              v-for="item in gameTypeOptions"
              :key="item.value"
              :label="item.label"
              :value="item.value">
            </el-option>
          </el-select>
        </el-form-item>
        <el-form-item label="最高难度等级" prop="maximumLevel">
          <el-input v-model="form.maximumLevel" placeholder="请输入最高难度等级" />
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
import { listProduct, getProduct, delProduct, addProduct, updateProduct } from "@/api/Product/Product";
import request from "@/utils/request";

export default {
  name: "Product",
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
      // AI产品表格数据
      ProductList: [],
      // 弹出层标题
      title: "",
      // 是否显示弹出层
      open: false,
      // 算法类型选项
      algorithmOptions: [],
      // 游戏类型选项
      gameTypeOptions: [],
      // 查询参数
      queryParams: {
        pageNum: 1,
        pageSize: 10,
        algorithmTypeId: null,
        gameTypeId: null,
      },
      // 表单参数
      form: {},
      // 表单校验
      rules: {
        algorithmTypeId: [
          { required: true, message: "算法名称不能为空", trigger: "change" }
        ],
        gameTypeId: [
          { required: true, message: "游戏类型中文名不能为空", trigger: "change" }
        ],
        maximumLevel: [
          { required: true, message: "最高难度等级不能为空", trigger: "blur" }
        ],
      }
    };
  },
  created() {
    this.getList();
    this.getAlgorithmOptions();
    this.getGameTypeOptions();
  },
  methods: {
    /** 查询AI产品列表 */
    getList() {
      this.loading = true;
      // 构建查询参数
      const queryParams = {
        ...this.queryParams
      };
      
      listProduct(queryParams).then(response => {
        this.ProductList = response.rows;
        this.total = response.total;
        this.loading = false;
      });
    },
    /** 获取算法类型下拉框选项 */
    getAlgorithmOptions() {
      request({
        url: '/Algorithm/Algorithm/options',
        method: 'get'
      }).then(res => {
        this.algorithmOptions = res.data;
      });
    },
    /** 获取游戏类型下拉框选项 */
    getGameTypeOptions() {
      request({
        url: '/GameType/GameType/options',
        method: 'get'
      }).then(res => {
        this.gameTypeOptions = res.data;
      });
    },
    /** 算法选择变更处理 */
    handleAlgorithmChange(value) {
      const selectedAlgorithm = this.algorithmOptions.find(item => item.value === value);
      if (selectedAlgorithm) {
        this.form.algorithmTypeName = selectedAlgorithm.label;
      }
    },
    /** 游戏类型选择变更处理 */
    handleGameTypeChange(value) {
      const selectedGameType = this.gameTypeOptions.find(item => item.value === value);
      if (selectedGameType) {
        this.form.gameTypeName = selectedGameType.label;
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
        id: null,
        algorithmTypeId: null,
        algorithmTypeName: null,
        gameTypeId: null,
        gameTypeName: null,
        maximumLevel: null,
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
      this.title = "添加AI产品";
    },
    /** 修改按钮操作 */
    handleUpdate(row) {
      this.reset();
      const id = row.id || this.ids
      getProduct(id).then(response => {
        this.form = response.data;
        this.open = true;
        this.title = "修改AI产品";
      });
    },
    /** 提交按钮 */
    submitForm() {
      this.$refs["form"].validate(valid => {
        if (valid) {
          if (this.form.id != null) {
            updateProduct(this.form).then(response => {
              this.$modal.msgSuccess("修改成功");
              this.open = false;
              this.getList();
            });
          } else {
            addProduct(this.form).then(response => {
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
      this.$modal.confirm('是否确认删除AI产品编号为"' + ids + '"的数据项？').then(function() {
        return delProduct(ids);
      }).then(() => {
        this.getList();
        this.$modal.msgSuccess("删除成功");
      }).catch(() => {});
    },
    /** 导出按钮操作 */
    handleExport() {
      this.download('Product/Product/export', {
        ...this.queryParams
      }, `Product_${new Date().getTime()}.xlsx`)
    }
  }
};
</script>
