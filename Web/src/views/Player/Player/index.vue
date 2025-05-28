<template>
  <div class="app-container">
    <!-- 搜索表单 -->
    <div class="search-container">
      <el-form :model="queryParams" ref="queryForm" size="small" :inline="true" v-show="showSearch" label-width="68px">
        <el-form-item label="用户昵称" prop="userName">
          <el-input
            v-model="queryParams.userName"
            placeholder="请输入用户昵称"
            clearable
            @keyup.enter.native="handleQuery"
            prefix-icon="el-icon-search"
          />
        </el-form-item>
        <el-form-item>
          <el-button type="primary" icon="el-icon-search" size="mini" @click="handleQuery">搜索</el-button>
          <el-button
            type="warning"
            plain
            icon="el-icon-download"
            size="mini"
            @click="handleExport"
            v-hasPermi="['Player:Player:export']"
          >导出</el-button>
        </el-form-item>
      </el-form>
    </div>

    <!-- 排行榜表格 -->
    <div class="leaderboard-container">
      <el-table 
        v-loading="loading" 
        :data="PlayerList" 
        class="leaderboard-table"
        :row-class-name="getRowClassName"
        element-loading-text="正在加载天梯排行数据..."
        element-loading-spinner="el-icon-loading"
        element-loading-background="rgba(0, 0, 0, 0.8)"
      >
        <el-table-column label="排名" align="center" width="100">
          <template slot-scope="scope">
            <div class="rank-cell">
              <div class="rank-number" :class="getRankClass(scope.$index + 1)">
                <i v-if="scope.$index + 1 <= 3" 
                   :class="getRankIcon(scope.$index + 1)"></i>
                <span v-else>{{ scope.$index + 1 }}</span>
              </div>
            </div>
          </template>
        </el-table-column>
        
        <el-table-column label="用户昵称" align="center" prop="userName" min-width="120">
          <template slot-scope="scope">
            <div class="player-name">
              <i class="el-icon-user-solid"></i>
              <span>{{ scope.row.userName }}</span>
            </div>
          </template>
        </el-table-column>
        
        <el-table-column label="天梯积分" align="center" prop="raking" min-width="120">
          <template slot-scope="scope">
            <div class="score-cell">
              <el-tag 
                :type="getScoreTagType(scope.row.raking)" 
                effect="dark" 
                size="medium"
                class="score-tag"
              >
                <i class="el-icon-star-on"></i>
                {{ scope.row.raking || 1000 }}
              </el-tag>
            </div>
          </template>
        </el-table-column>
        
        <el-table-column label="总场次" align="center" min-width="100">
          <template slot-scope="scope">
            <div class="games-cell">
              <span class="games-number">{{ scope.row.totalGames !== undefined ? scope.row.totalGames : 0 }}</span>
              <span class="games-label">场</span>
            </div>
          </template>
        </el-table-column>
        
        <el-table-column label="胜率" align="center" width="120">
          <template slot-scope="scope">
            <div class="winrate-cell">
              <el-progress 
                :percentage="parseFloat(calculateWinRate(scope.row))" 
                :color="getWinRateColor(parseFloat(calculateWinRate(scope.row)))"
                :stroke-width="8"
                :show-text="false"
                class="winrate-progress"
              ></el-progress>
              <span class="winrate-text">{{ calculateWinRate(scope.row) }}%</span>
            </div>
          </template>
        </el-table-column>
        
        <!-- 空状态 -->
        <template slot="empty">
          <div class="empty-state">
            <i class="el-icon-trophy empty-icon"></i>
            <p class="empty-text">暂无排行数据</p>
            <p class="empty-subtext">快去开始你的第一场对局吧！</p>
          </div>
        </template>
      </el-table>
    </div>
  </div>
</template>

<script>
import { listPlayer, getPlayer } from "@/api/Player/Player";
import { getPlayerStatistics } from "@/api/PlayerStatistics/PlayerStatistics";

export default {
  name: "Player",
  data() {
    return {
      // 遮罩层
      loading: true,
      // 显示搜索条件
      showSearch: true,
      // 总条数
      total: 0,
      // 玩家表格数据
      PlayerList: [],
      // 查询参数
      queryParams: {
        userName: null,
      }
    };
  },
  created() {
    this.getList();
  },
  methods: {
    /** 查询玩家列表 */
    async getList() {
      this.loading = true;
      try {
        const response = await listPlayer(this.queryParams);
        
        if (response && response.rows) {
          this.PlayerList = response.rows;
          this.total = response.total;
          
          // 为每个玩家获取统计信息
          await this.loadPlayerStatistics();
          
          // 按天梯积分降序排序
          this.PlayerList.sort((a, b) => {
            const aRaking = a.raking || 1000;
            const bRaking = b.raking || 1000;
            return bRaking - aRaking;
          });
          
          // 强制更新表格
          this.$nextTick(() => {
            this.$forceUpdate();
          });
        } else {
          console.error('响应数据格式错误:', response);
          this.PlayerList = [];
          this.total = 0;
        }
        
      } catch (error) {
        console.error('获取玩家列表失败:', error);
        this.PlayerList = [];
        this.total = 0;
      } finally {
        this.loading = false;
      }
    },
    
    /** 加载玩家统计信息 */
    async loadPlayerStatistics() {
      if (!this.PlayerList || this.PlayerList.length === 0) {
        return;
      }
      
      const promises = this.PlayerList.map(async (player, index) => {
        try {
          const statsResponse = await getPlayerStatistics(player.userId);
          
          if (statsResponse && statsResponse.code === 200 && statsResponse.data && statsResponse.data.overallStats) {
            const stats = statsResponse.data.overallStats;
            // 有效对局数（不包括终止的对局）- 这是要显示的总场次
            const validGames = (stats.totalWins || 0) + (stats.totalLosses || 0) + (stats.totalDraws || 0);
            
            // 创建新的玩家对象
            const updatedPlayer = {
              ...player,
              totalGames: validGames,
              wins: stats.totalWins || 0,
              losses: stats.totalLosses || 0,
              draws: stats.totalDraws || 0,
              aborts: stats.totalAborts || 0
            };
            
            // 直接替换数组中的元素
            this.$set(this.PlayerList, index, updatedPlayer);
          } else {
            // 如果没有统计信息，设置默认值
            const updatedPlayer = {
              ...player,
              totalGames: 0,
              wins: 0,
              losses: 0,
              draws: 0,
              aborts: 0
            };
            this.$set(this.PlayerList, index, updatedPlayer);
          }
        } catch (error) {
          console.error(`获取玩家${player.userId}统计信息失败:`, error);
          // 设置默认值
          const updatedPlayer = {
            ...player,
            totalGames: 0,
            wins: 0,
            losses: 0,
            draws: 0,
            aborts: 0
          };
          this.$set(this.PlayerList, index, updatedPlayer);
        }
      });
      
      await Promise.all(promises);
      
      // 强制更新组件
      this.$forceUpdate();
    },
    
    /** 搜索按钮操作 */
    handleQuery() {
      this.getList();
    },
    
    /** 导出按钮操作 */
    handleExport() {
      this.download('Player/Player/export', {
        ...this.queryParams
      }, `Player_${new Date().getTime()}.xlsx`)
    },
    
    /** 计算胜率 */
    calculateWinRate(row) {
      if (!row.totalGames || row.totalGames === 0) return '0.00';
      return ((row.wins / row.totalGames) * 100).toFixed(2);
    },
    
    /** 获取排名样式类 */
    getRankClass(rank) {
      if (rank === 1) return 'rank-first';
      if (rank === 2) return 'rank-second';
      if (rank === 3) return 'rank-third';
      return 'rank-normal';
    },
    
    /** 获取排名图标 */
    getRankIcon(rank) {
      if (rank === 1) return 'el-icon-trophy';
      if (rank === 2) return 'el-icon-trophy';
      if (rank === 3) return 'el-icon-trophy';
      return '';
    },
    
    /** 获取积分标签类型 */
    getScoreTagType(score) {
      if (score >= 1500) return 'danger';
      if (score >= 1300) return 'warning';
      if (score >= 1100) return 'success';
      return 'info';
    },
    
    /** 获取胜率颜色 */
    getWinRateColor(percentage) {
      if (percentage >= 80) return '#f56c6c';
      if (percentage >= 60) return '#e6a23c';
      if (percentage >= 40) return '#67c23a';
      return '#909399';
    },
    
    /** 获取行样式类名 */
    getRowClassName({row, rowIndex}) {
      const rank = rowIndex + 1;
      if (rank <= 3) return 'top-rank-row';
      return '';
    },
  }
};
</script>

<style scoped>
.app-container {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  min-height: 100vh;
  padding: 20px;
}

.search-container {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 15px;
  padding: 15px 20px;
  margin-bottom: 5px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  animation: slideInLeft 0.6s ease-out 0.2s both;
}

.leaderboard-container {
  background: rgba(255, 255, 255, 0.05);
  border-radius: 15px;
  padding: 20px;
  margin-bottom: 20px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  backdrop-filter: blur(10px);
  border: 1px solid rgba(255, 255, 255, 0.1);
  animation: slideInUp 0.6s ease-out 0.4s both;
}

.leaderboard-table {
  border-radius: 10px;
  overflow: visible;
  background: transparent;
}

.rank-cell {
  display: flex;
  justify-content: center;
  align-items: center;
  position: relative;
  z-index: 1;
}

.rank-number {
  width: 40px;
  height: 40px;
  border-radius: 50%;
  display: flex;
  align-items: center;
  justify-content: center;
  font-weight: bold;
  font-size: 16px;
  transition: all 0.3s ease;
  cursor: pointer;
  position: relative;
  z-index: 10;
}

.rank-number:hover {
  transform: scale(1.1);
  box-shadow: 0 6px 20px rgba(0, 0, 0, 0.2);
  z-index: 20;
}

.rank-first {
  background: linear-gradient(45deg, #ffd700, #ffed4e);
  color: #fff;
  box-shadow: 0 4px 15px rgba(255, 215, 0, 0.4);
}

.rank-first:hover {
  box-shadow: 0 8px 25px rgba(255, 215, 0, 0.6);
}

.rank-second {
  background: linear-gradient(45deg, #c0c0c0, #e8e8e8);
  color: #fff;
  box-shadow: 0 4px 15px rgba(192, 192, 192, 0.4);
}

.rank-second:hover {
  box-shadow: 0 8px 25px rgba(192, 192, 192, 0.6);
}

.rank-third {
  background: linear-gradient(45deg, #cd7f32, #daa520);
  color: #fff;
  box-shadow: 0 4px 15px rgba(205, 127, 50, 0.4);
}

.rank-third:hover {
  box-shadow: 0 8px 25px rgba(205, 127, 50, 0.6);
}

.rank-normal {
  background: linear-gradient(45deg, #667eea, #764ba2);
  color: #fff;
}

.rank-normal:hover {
  background: linear-gradient(45deg, #5a6fd8, #6a4190);
}

.player-name {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 8px;
  font-weight: bold;
  font-size: 18px;
  font-style: italic;
  color: white !important;
  transition: all 0.3s ease;
}

.player-name:hover {
  transform: translateY(-2px);
  color: #ffd700 !important;
}

.player-name i {
  color: white !important;
  font-size: 20px;
  font-weight: bold;
  transition: all 0.3s ease;
}

.player-name span {
  color: white !important;
  font-weight: bold;
  font-size: 18px;
  font-style: italic;
}

.player-name:hover i {
  transform: scale(1.2);
  color: #ffd700 !important;
}

.player-name:hover span {
  color: #ffd700 !important;
}

.score-cell {
  display: flex;
  justify-content: center;
}

.score-tag {
  font-size: 14px;
  font-weight: bold;
  padding: 8px 16px;
  border-radius: 20px;
  transition: all 0.3s ease;
  cursor: pointer;
  display: flex;
  align-items: center;
  justify-content: center;
  text-align: center;
  line-height: 1;
}

.score-tag:hover {
  transform: translateY(-2px);
  box-shadow: 0 4px 15px rgba(0, 0, 0, 0.2);
}

.score-tag i {
  margin-right: 5px;
  transition: all 0.3s ease;
}

.score-tag:hover i {
  transform: rotate(360deg);
}

.games-cell {
  display: flex;
  align-items: center;
  justify-content: center;
  gap: 4px;
  transition: all 0.3s ease;
  cursor: pointer;
}

.games-cell:hover {
  transform: scale(1.05);
}

.games-number {
  font-size: 18px;
  font-weight: bold;
  color: #333;
  transition: all 0.3s ease;
}

.games-cell:hover .games-number {
  color: #ffd700;
}

.games-label {
  font-size: 12px;
  color: #999;
  transition: all 0.3s ease;
}

.games-cell:hover .games-label {
  color: #ffd700;
}

.winrate-cell {
  display: flex;
  flex-direction: column;
  align-items: center;
  gap: 5px;
  transition: all 0.3s ease;
  cursor: pointer;
}

.winrate-cell:hover {
  transform: translateY(-2px);
}

.winrate-progress {
  width: 80px;
  transition: all 0.3s ease;
}

.winrate-cell:hover .winrate-progress {
  transform: scale(1.1);
}

.winrate-text {
  font-size: 12px;
  font-weight: bold;
  color: #333;
  transition: all 0.3s ease;
}

.winrate-cell:hover .winrate-text {
  color: #ffd700;
  transform: scale(1.1);
}

/* 表格行样式 */
::v-deep .top-rank-row {
  background: linear-gradient(90deg, rgba(255, 215, 0, 0.2), rgba(255, 255, 255, 0.1)) !important;
}

::v-deep .top-rank-row:hover {
  background: linear-gradient(90deg, rgba(255, 215, 0, 0.3), rgba(255, 255, 255, 0.2)) !important;
}

::v-deep .top-rank-row .cell {
  color: white !important;
}

/* 表格头部样式 */
::v-deep .el-table__header {
  background: linear-gradient(45deg, #667eea, #764ba2) !important;
}

::v-deep .el-table__header th {
  background: transparent !important;
  color: white !important;
  font-weight: bold !important;
  border: none !important;
  text-align: center !important;
}

::v-deep .el-table__header .cell {
  color: white !important;
  font-weight: bold !important;
  text-align: center !important;
}

/* 表格主体样式 */
::v-deep .el-table {
  background: transparent !important;
  color: white !important;
  border: none !important;
  overflow: visible !important;
}

::v-deep .el-table .el-table__header-wrapper {
  overflow: visible !important;
}

::v-deep .el-table .el-table__body-wrapper {
  overflow: visible !important;
}

::v-deep .el-table td {
  overflow: visible !important;
}

::v-deep .el-table th {
  overflow: visible !important;
}

::v-deep .el-table::before {
  display: none !important;
}

::v-deep .el-table::after {
  display: none !important;
}

::v-deep .el-table__border {
  display: none !important;
}

::v-deep .el-table__body {
  overflow: visible !important;
  background: transparent !important;
}

::v-deep .el-table tr {
  background: transparent !important;
}

/* 表格单元格样式 */
::v-deep .el-table td {
  border: none !important;
  border-bottom: none !important;
  padding: 15px 0 !important;
  background: transparent !important;
}

::v-deep .el-table th.is-leaf {
  border-bottom: none !important;
}

::v-deep .el-table .cell {
  overflow: visible !important;
  color: white !important;
  text-align: center !important;
}

::v-deep .el-table tr:hover {
  background: rgba(255, 255, 255, 0.1) !important;
}

::v-deep .el-table tr:hover .cell {
  color: white !important;
}

::v-deep .el-table__body-wrapper {
  border: none !important;
}

/* 空状态样式 */
.empty-state {
  padding: 60px 20px;
  text-align: center;
}

.empty-icon {
  font-size: 4em;
  color: rgba(255, 255, 255, 0.6);
  margin-bottom: 20px;
  display: block;
}

.empty-text {
  font-size: 1.2em;
  color: white;
  margin: 0 0 10px 0;
  font-weight: 500;
}

.empty-subtext {
  font-size: 0.9em;
  color: rgba(255, 255, 255, 0.8);
  margin: 0;
}

/* 加载动画优化 */
::v-deep .el-loading-mask {
  border-radius: 15px;
}

::v-deep .el-loading-text {
  color: #667eea !important;
  font-weight: bold;
}

/* 表格滚动条样式 */
::v-deep .el-table__body-wrapper::-webkit-scrollbar {
  width: 8px;
  height: 8px;
}

::v-deep .el-table__body-wrapper::-webkit-scrollbar-track {
  background: rgba(0, 0, 0, 0.1);
  border-radius: 4px;
}

::v-deep .el-table__body-wrapper::-webkit-scrollbar-thumb {
  background: linear-gradient(45deg, #667eea, #764ba2);
  border-radius: 4px;
}

::v-deep .el-table__body-wrapper::-webkit-scrollbar-thumb:hover {
  background: linear-gradient(45deg, #5a6fd8, #6a4190);
}

/* 响应式设计 */
@media (max-width: 768px) {
  .app-container {
    padding: 10px;
  }
  
  .search-container {
    padding: 15px;
    margin-bottom: 15px;
  }
  
  .leaderboard-container {
    padding: 15px;
    margin-bottom: 15px;
  }
  
  .rank-number {
    width: 35px;
    height: 35px;
    font-size: 14px;
  }
  
  .score-tag {
    font-size: 12px;
    padding: 6px 12px;
  }
  
  .games-number {
    font-size: 16px;
  }
  
  .winrate-progress {
    width: 60px;
  }
  
  .winrate-text {
    font-size: 11px;
  }
}

@media (max-width: 480px) {
  .rank-number {
    width: 30px;
    height: 30px;
    font-size: 12px;
  }
  
  .player-name {
    flex-direction: column;
    gap: 4px;
  }
  
  .score-tag {
    font-size: 11px;
    padding: 4px 8px;
  }
  
  .games-number {
    font-size: 14px;
  }
  
  .winrate-progress {
    width: 50px;
  }
}

/* 动画关键帧 */
@keyframes slideInLeft {
  from {
    transform: translateX(-30px);
    opacity: 0;
  }
  to {
    transform: translateX(0);
    opacity: 1;
  }
}

@keyframes slideInUp {
  from {
    transform: translateY(30px);
    opacity: 0;
  }
  to {
    transform: translateY(0);
    opacity: 1;
  }
}

/* 表格行动画 */
::v-deep .el-table__row {
  animation: fadeInRow 0.5s ease-out;
}

@keyframes fadeInRow {
  from {
    opacity: 0;
    transform: translateX(-20px);
  }
  to {
    opacity: 1;
    transform: translateX(0);
  }
}

/* 搜索表单样式 */
::v-deep .search-container .el-form-item__label {
  color: rgba(255, 255, 255, 0.8) !important;
  font-weight: bold;
  text-shadow: 0 1px 2px rgba(0, 0, 0, 0.5);
}

::v-deep .search-container .el-form-item {
  margin-bottom: 0 !important;
}

::v-deep .search-container .el-form--inline .el-form-item {
  margin-right: 15px;
  margin-bottom: 0 !important;
}

::v-deep .search-container .el-input__inner {
  background: rgba(0, 0, 0, 0.3) !important;
  border: 2px solid rgba(255, 255, 255, 0.3) !important;
  color: white !important;
  border-radius: 20px;
}

::v-deep .search-container .el-input__inner::placeholder {
  color: rgba(255, 255, 255, 0.6) !important;
}

::v-deep .search-container .el-input__inner:focus {
  border-color: rgba(255, 255, 255, 0.8) !important;
  box-shadow: 0 0 10px rgba(255, 255, 255, 0.3) !important;
  background: rgba(0, 0, 0, 0.4) !important;
}

::v-deep .search-container .el-input__prefix {
  color: rgba(255, 255, 255, 0.6) !important;
}

/* 按钮样式优化 */
::v-deep .el-button--primary {
  background: linear-gradient(45deg, #667eea, #764ba2) !important;
  border: none !important;
  border-radius: 20px;
  color: white !important;
}

::v-deep .el-button--primary:hover {
  background: linear-gradient(45deg, #5a6fd8, #6a4190) !important;
}

::v-deep .el-button {
  border-radius: 20px;
  font-weight: bold;
}

::v-deep .el-button--warning {
  background: linear-gradient(45deg, #f39c12, #e67e22) !important;
  border: none !important;
  color: white !important;
}

::v-deep .el-button--warning:hover {
  background: linear-gradient(45deg, #e67e22, #d35400) !important;
}
</style>
