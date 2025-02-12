<template>
  <div class="app-container">
    <el-form :model="queryParams" ref="queryForm" size="small" :inline="true" v-show="showSearch" label-width="68px">
      <el-form-item label="游戏名" prop="gameName">
        <el-input
          v-model="queryParams.gameName"
          placeholder="请输入游戏名"
          clearable
          @keyup.enter.native="handleQuery"
        />
      </el-form-item>
      <el-form-item label="棋盘格数" prop="boardSize">
        <el-input
          v-model="queryParams.boardSize"
          placeholder="请输入棋盘格数"
          clearable
          @keyup.enter.native="handleQuery"
        />
      </el-form-item>
      <el-form-item label="是否格中落子" prop="isCellCenter">
        <el-input
          v-model="queryParams.isCellCenter"
          placeholder="请输入是否格中落子"
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
          v-hasPermi="['aiSys:gameType:add']"
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
          v-hasPermi="['aiSys:gameType:edit']"
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
          v-hasPermi="['aiSys:gameType:remove']"
        >删除</el-button>
      </el-col>
      <el-col :span="1.5">
        <el-button
          type="warning"
          plain
          icon="el-icon-download"
          size="mini"
          @click="handleExport"
          v-hasPermi="['aiSys:gameType:export']"
        >导出</el-button>
      </el-col>
      <right-toolbar :showSearch.sync="showSearch" @queryTable="getList"></right-toolbar>
    </el-row>

    <el-table v-loading="loading" :data="gameTypeList" @selection-change="handleSelectionChange">
      <el-table-column type="selection" width="55" align="center" />
      <el-table-column label="游戏编号" align="center" prop="gameId" />
      <el-table-column label="游戏名" align="center" prop="gameName" />
      <el-table-column label="规则简介" align="center" prop="gameIntroduction" />
      <el-table-column label="规则" align="center" prop="gameRule" />
      <el-table-column label="棋盘格数" align="center" prop="boardSize" />
      <el-table-column label="最低有效步数" align="center" prop="minValidPieces" />
      <el-table-column label="是否格中落子" align="center" prop="isCellCenter" />
      <el-table-column label="操作" align="center" class-name="small-padding fixed-width">
        <template slot-scope="scope">
          <el-button
            size="mini"
            type="text"
            icon="el-icon-edit"
            @click="handleUpdate(scope.row)"
            v-hasPermi="['aiSys:gameType:edit']"
          >修改</el-button>
          <el-button
            size="mini"
            type="text"
            icon="el-icon-delete"
            @click="handleDelete(scope.row)"
            v-hasPermi="['aiSys:gameType:remove']"
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

    <!-- 添加或修改游戏类型对话框 -->
    <el-dialog :title="title" :visible.sync="open" width="500px" append-to-body>
      <el-form ref="form" :model="form" :rules="rules" label-width="80px">
        <el-form-item label="游戏名" prop="gameName">
          <el-input v-model="form.gameName" placeholder="请输入游戏名" />
        </el-form-item>
        <el-form-item label="规则简介" prop="gameIntroduction">
          <el-input v-model="form.gameIntroduction" type="textarea" placeholder="请输入内容" />
        </el-form-item>
        <el-form-item label="规则" prop="gameRule">
          <el-input v-model="form.gameRule" type="textarea" placeholder="请输入内容" />
        </el-form-item>
        <el-form-item label="棋盘格数" prop="boardSize">
          <el-input v-model="form.boardSize" placeholder="请输入棋盘格数" />
        </el-form-item>
        <el-form-item label="最低有效步数" prop="minValidPieces">
          <el-input v-model="form.minValidPieces" placeholder="请输入最低有效步数" />
        </el-form-item>
        <el-form-item label="是否格中落子" prop="isCellCenter">
          <el-input v-model="form.isCellCenter" placeholder="请输入是否格中落子" />
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
          <el-table-column label="AI类别" prop="aiTypeId" width="150">
            <template slot-scope="scope">
              <el-input v-model="scope.row.aiTypeId" placeholder="请输入AI类别" />
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
import { listGameType, getGameType, delGameType, addGameType, updateGameType } from "@/api/aiSys/gameType";

export default {
  name: "GameType",
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
      // 游戏类型表格数据
      gameTypeList: [],
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
        gameName: null,
        boardSize: null,
        isCellCenter: null
      },
      // 表单参数
      form: {},
      // 表单校验
      rules: {
        gameName: [
          { required: true, message: "游戏名不能为空", trigger: "blur" }
        ],
        boardSize: [
          { required: true, message: "棋盘格数不能为空", trigger: "blur" }
        ],
        minValidPieces: [
          { required: true, message: "最低有效步数不能为空", trigger: "blur" }
        ],
        isCellCenter: [
          { required: true, message: "是否格中落子不能为空", trigger: "blur" }
        ]
      }
    };
  },
  created() {
    this.getList();
  },
  methods: {
    /** 查询游戏类型列表 */
    getList() {
      this.loading = true;
      listGameType(this.queryParams).then(response => {
        this.gameTypeList = response.rows;
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
        gameId: null,
        gameName: null,
        gameIntroduction: null,
        gameRule: null,
        boardSize: null,
        minValidPieces: null,
        isCellCenter: null
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
      this.ids = selection.map(item => item.gameId)
      this.single = selection.length!==1
      this.multiple = !selection.length
    },
    /** 新增按钮操作 */
    handleAdd() {
      this.reset();
      this.open = true;
      this.title = "添加游戏类型";
    },
    /** 修改按钮操作 */
    handleUpdate(row) {
      this.reset();
      const gameId = row.gameId || this.ids
      getGameType(gameId).then(response => {
        this.form = response.data;
        this.aiGameList = response.data.aiGameList;
        this.open = true;
        this.title = "修改游戏类型";
      });
    },
    /** 提交按钮 */
    submitForm() {
      this.$refs["form"].validate(valid => {
        if (valid) {
          this.form.aiGameList = this.aiGameList;
          if (this.form.gameId != null) {
            updateGameType(this.form).then(response => {
              this.$modal.msgSuccess("修改成功");
              this.open = false;
              this.getList();
            });
          } else {
            addGameType(this.form).then(response => {
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
      const gameIds = row.gameId || this.ids;
      this.$modal.confirm('是否确认删除游戏类型编号为"' + gameIds + '"的数据项？').then(function() {
        return delGameType(gameIds);
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
      obj.aiTypeId = "";
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
      this.download('aiSys/gameType/export', {
        ...this.queryParams
      }, `gameType_${new Date().getTime()}.xlsx`)
    }
  }
};
</script>
