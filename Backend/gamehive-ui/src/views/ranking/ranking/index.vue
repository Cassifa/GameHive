<template>
  <div class="app-container">
    <el-table v-loading="loading" :data="rankingList">
      <el-table-column label="用户编号" align="center" prop="userId" />
      <el-table-column label="天梯积分" align="center" prop="raking" />
      <el-table-column label="与AI对战记录" align="center" prop="recordWithAi" />
      <el-table-column label="与玩家对战记录" align="center" prop="recordWithPlayer" />
    </el-table>
    
    <pagination
      v-show="total>0"
      :total="total"
      :page.sync="queryParams.pageNum"
      :limit.sync="queryParams.pageSize"
      @pagination="getList"
    />
  </div>
</template>

<script>
import { listRanking } from "@/api/ranking/ranking";

export default {
  name: "Ranking",
  data() {
    return {
      // 遮罩层
      loading: true,
      // 总条数
      total: 0,
      // 天梯排行表格数据
      rankingList: [],
      // 查询参数
      queryParams: {
        pageNum: 1,
        pageSize: 10,
      }
    };
  },
  created() {
    this.getList();
  },
  methods: {
    /** 查询天梯排行列表 */
    getList() {
      this.loading = true;
      listRanking(this.queryParams).then(response => {
        this.rankingList = response.rows;
        this.total = response.total;
        this.loading = false;
      });
    }
  }
};
</script>
