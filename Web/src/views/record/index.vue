<template>
  <div class="app-container">
    <el-card class="box-card">
      <div slot="header" class="clearfix">
        <span>对局记录热力图</span>
        <el-button style="float: right; padding: 3px 0" type="text" @click="refreshHeatmap">刷新</el-button>
      </div>
      <game-record-heatmap :useMockData="true" ref="heatmap" />
    </el-card>
    
    <!-- 原有内容 -->
    <el-form :model="queryParams" ref="queryForm" size="small" :inline="true" v-show="showSearch" label-width="68px">
      <!-- 表单内容 -->
    </el-form>
    
    <!-- 其他内容 -->
  </div>
</template>

<script>
import GameRecordHeatmap from "@/components/GameRecordHeatmap/index";

export default {
  name: "Record",
  components: {
    GameRecordHeatmap
  },
  methods: {
    refreshHeatmap() {
      this.$refs.heatmap.refresh();
    }
  },
  // 保留原有内容
}
</script> 