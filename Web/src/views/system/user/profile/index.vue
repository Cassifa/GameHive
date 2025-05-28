<template>
  <div class="app-container">
    <el-row :gutter="20">
      <el-col :span="6" :xs="24">
        <el-card class="box-card">
          <div>
            <div class="text-center">
              <userAvatar />
            </div>
            <ul class="list-group list-group-striped">
              <li class="list-group-item">
                <div class="item-left">
                  <svg-icon icon-class="user" />
                  <span>用户名称</span>
                </div>
                <div class="item-right">{{ user.userName }}</div>
              </li>
              <li class="list-group-item">
                <div class="item-left">
                  <svg-icon icon-class="peoples" />
                  <span>所属角色</span>
                </div>
                <div class="item-right">{{ roleGroup }}</div>
              </li>
              <li class="list-group-item">
                <div class="item-left">
                  <svg-icon icon-class="date" />
                  <span>创建日期</span>
                </div>
                <div class="item-right">{{ user.createTime }}</div>
              </li>
            </ul>
          </div>
        </el-card>
      </el-col>
      <el-col :span="18" :xs="24">
        <el-card>
          <div class="statistics-container">
            <!-- 总体统计概览 -->
            <div class="overview-section">
              <el-row :gutter="20">
                <el-col :span="6">
                  <div class="stat-card total-games">
                    <div class="stat-icon">
                      <i class="el-icon-trophy"></i>
                    </div>
                    <div class="stat-content">
                      <div class="stat-number">{{ overallStats.totalGames }}</div>
                      <div class="stat-label">总对局</div>
                    </div>
                  </div>
                </el-col>
                <el-col :span="6">
                  <div class="stat-card wins">
                    <div class="stat-icon">
                      <i class="el-icon-medal"></i>
                    </div>
                    <div class="stat-content">
                      <div class="stat-number">{{ overallStats.totalWins }}</div>
                      <div class="stat-label">胜利</div>
                    </div>
                  </div>
                </el-col>
                <el-col :span="6">
                  <div class="stat-card losses">
                    <div class="stat-icon">
                      <i class="el-icon-close"></i>
                    </div>
                    <div class="stat-content">
                      <div class="stat-number">{{ overallStats.totalLosses }}</div>
                      <div class="stat-label">失败</div>
                    </div>
                  </div>
                </el-col>
                <el-col :span="6">
                  <div class="stat-card winrate">
                    <div class="stat-icon">
                      <i class="el-icon-star-on"></i>
                    </div>
                    <div class="stat-content">
                      <div class="stat-number">{{ calculateWinRate() }}%</div>
                      <div class="stat-label">胜率</div>
                    </div>
                  </div>
                </el-col>
              </el-row>
            </div>

            <!-- 图表展示区域 -->
            <div class="charts-section">
              <el-row :gutter="20">
                <!-- 游戏模式分布饼图 -->
                <el-col :span="12">
                  <div class="chart-container">
                    <h3 class="chart-title">游戏模式分布</h3>
                    <div ref="gameModeChart" class="chart"></div>
                  </div>
                </el-col>
                
                <!-- 游戏类型统计柱状图 -->
                <el-col :span="12">
                  <div class="chart-container">
                    <h3 class="chart-title">游戏类型统计</h3>
                    <div ref="gameTypeChart" class="chart"></div>
                  </div>
                </el-col>
              </el-row>
              
              <el-row :gutter="20" style="margin-top: 20px;">
                <!-- 算法类型分布饼图 -->
                <el-col :span="12">
                  <div class="chart-container">
                    <h3 class="chart-title">挑战算法分布</h3>
                    <div ref="algorithmChart" class="chart"></div>
                  </div>
                </el-col>
                
                <!-- 胜负平分布饼图 -->
                <el-col :span="12">
                  <div class="chart-container">
                    <h3 class="chart-title">对局结果分布</h3>
                    <div ref="resultChart" class="chart"></div>
                  </div>
                </el-col>
              </el-row>
            </div>
          </div>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script>
import userAvatar from "./userAvatar";
import { getUserProfile } from "@/api/system/user";
import { getPlayerStatistics } from "@/api/PlayerStatistics/PlayerStatistics";
import * as echarts from 'echarts';

export default {
  name: "Profile",
  components: { userAvatar },
  data() {
    return {
      user: {},
      roleGroup: {},
      postGroup: {},
      statistics: null,
      overallStats: {
        totalGames: 0,
        totalWins: 0,
        totalLosses: 0,
        totalDraws: 0,
        totalAborts: 0
      },
      charts: {
        gameModeChart: null,
        gameTypeChart: null,
        algorithmChart: null,
        resultChart: null
      }
    };
  },
  created() {
    this.getUser();
  },
  mounted() {
    this.$nextTick(() => {
      this.initCharts();
    });
  },
  beforeDestroy() {
    // 销毁图表实例
    Object.values(this.charts).forEach(chart => {
      if (chart) {
        chart.dispose();
      }
    });
  },
  methods: {
    getUser() {
      getUserProfile().then(response => {
        this.user = response.data;
        this.roleGroup = response.roleGroup;
        this.postGroup = response.postGroup;
        
        // 获取统计信息
        this.getStatistics();
      });
    },
    
    async getStatistics() {
      try {
        const response = await getPlayerStatistics(this.user.userId);
        if (response && response.code === 200 && response.data) {
          this.statistics = response.data;
          this.overallStats = response.data.overallStats || this.overallStats;
          
          // 更新图表
          this.$nextTick(() => {
            this.updateCharts();
          });
        }
      } catch (error) {
        console.error('获取统计信息失败:', error);
      }
    },
    
    calculateWinRate() {
      const validGames = this.overallStats.totalWins + this.overallStats.totalLosses + this.overallStats.totalDraws;
      if (validGames === 0) return '0.00';
      return ((this.overallStats.totalWins / validGames) * 100).toFixed(2);
    },
    
    initCharts() {
      // 初始化所有图表
      this.charts.gameModeChart = echarts.init(this.$refs.gameModeChart);
      this.charts.gameTypeChart = echarts.init(this.$refs.gameTypeChart);
      this.charts.algorithmChart = echarts.init(this.$refs.algorithmChart);
      this.charts.resultChart = echarts.init(this.$refs.resultChart);
      
      // 监听窗口大小变化
      window.addEventListener('resize', this.handleResize);
    },
    
    handleResize() {
      Object.values(this.charts).forEach(chart => {
        if (chart) {
          chart.resize();
        }
      });
    },
    
    updateCharts() {
      if (!this.statistics) return;
      
      this.updateGameModeChart();
      this.updateGameTypeChart();
      this.updateAlgorithmChart();
      this.updateResultChart();
    },
    
    updateGameModeChart() {
      const data = [];
      const detailData = {}; // 存储详细统计信息
      
      // 本地对战统计
      if (this.statistics.localGameStats && this.statistics.localGameStats.gameTypeStats && Object.keys(this.statistics.localGameStats.gameTypeStats).length > 0) {
        let localTotal = 0;
        const localDetails = {};
        
        Object.entries(this.statistics.localGameStats.gameTypeStats).forEach(([gameType, gameTypeData]) => {
          if (gameTypeData && gameTypeData.algorithmStats && typeof gameTypeData.algorithmStats === 'object') {
            let gameTotal = 0;
            let gameWins = 0;
            let gameLosses = 0;
            let gameDraws = 0;
            let gameAborts = 0;
            
            Object.values(gameTypeData.algorithmStats).forEach(algorithm => {
              if (algorithm && algorithm.stats && typeof algorithm.stats === 'object') {
                gameWins += (algorithm.stats.wins || 0);
                gameLosses += (algorithm.stats.losses || 0);
                gameDraws += (algorithm.stats.draws || 0);
                gameAborts += (algorithm.stats.aborts || 0);
                gameTotal += (algorithm.stats.wins || 0) + (algorithm.stats.losses || 0) + (algorithm.stats.draws || 0);
              }
            });
            
            if (gameTotal > 0) {
              localDetails[gameType] = {
                total: gameTotal,
                wins: gameWins,
                losses: gameLosses,
                draws: gameDraws,
                aborts: gameAborts,
                winRate: gameTotal > 0 ? ((gameWins / (gameWins + gameLosses + gameDraws)) * 100).toFixed(1) : '0.0'
              };
              localTotal += gameTotal;
            }
          }
        });
        
        if (localTotal > 0) {
          data.push({ name: '本地对战', value: localTotal });
          detailData['本地对战'] = localDetails;
        }
      }
      
      // LMM对战统计
      if (this.statistics.lmmGameStats && this.statistics.lmmGameStats.gameTypeStats && Object.keys(this.statistics.lmmGameStats.gameTypeStats).length > 0) {
        let lmmTotal = 0;
        const lmmDetails = {};
        
        Object.entries(this.statistics.lmmGameStats.gameTypeStats).forEach(([gameType, gameTypeData]) => {
          if (gameTypeData && typeof gameTypeData === 'object') {
            const gameTotal = (gameTypeData.wins || 0) + (gameTypeData.losses || 0) + (gameTypeData.draws || 0);
            if (gameTotal > 0) {
              lmmDetails[gameType] = {
                total: gameTotal,
                wins: gameTypeData.wins || 0,
                losses: gameTypeData.losses || 0,
                draws: gameTypeData.draws || 0,
                aborts: gameTypeData.aborts || 0,
                winRate: gameTotal > 0 ? (((gameTypeData.wins || 0) / gameTotal) * 100).toFixed(1) : '0.0'
              };
              lmmTotal += gameTotal;
            }
          }
        });
        
        if (lmmTotal > 0) {
          data.push({ name: '与大模型对战', value: lmmTotal });
          detailData['与大模型对战'] = lmmDetails;
        }
      }
      
      // 联机对战统计
      if (this.statistics.onlineGameStats && this.statistics.onlineGameStats.gameTypeStats && Object.keys(this.statistics.onlineGameStats.gameTypeStats).length > 0) {
        let onlineTotal = 0;
        const onlineDetails = {};
        
        Object.entries(this.statistics.onlineGameStats.gameTypeStats).forEach(([gameType, gameTypeData]) => {
          if (gameTypeData && typeof gameTypeData === 'object') {
            const gameTotal = (gameTypeData.wins || 0) + (gameTypeData.losses || 0) + (gameTypeData.draws || 0);
            if (gameTotal > 0) {
              onlineDetails[gameType] = {
                total: gameTotal,
                wins: gameTypeData.wins || 0,
                losses: gameTypeData.losses || 0,
                draws: gameTypeData.draws || 0,
                aborts: gameTypeData.aborts || 0,
                winRate: gameTotal > 0 ? (((gameTypeData.wins || 0) / gameTotal) * 100).toFixed(1) : '0.0'
              };
              onlineTotal += gameTotal;
            }
          }
        });
        
        if (onlineTotal > 0) {
          data.push({ name: '联机对战', value: onlineTotal });
          detailData['联机对战'] = onlineDetails;
        }
      }
      
      if (data.length === 0) {
        // 如果没有数据，显示空状态
        const option = {
          backgroundColor: 'transparent',
          title: {
            text: '暂无数据',
            left: 'center',
            top: 'middle',
            textStyle: {
              color: '#999',
              fontSize: 16
            }
          }
        };
        this.charts.gameModeChart.setOption(option);
        return;
      }
      
      const option = {
        backgroundColor: 'transparent',
        tooltip: {
          trigger: 'item',
          formatter: (params) => {
            const modeName = params.name;
            const modeDetails = detailData[modeName];
            
            if (!modeDetails || Object.keys(modeDetails).length === 0) {
              return `${modeName}<br/>总对局: ${params.value} (${params.percent}%)`;
            }
            
            let tooltipContent = `<div style="font-weight: bold; margin-bottom: 8px; color: #333;">${modeName}</div>`;
            tooltipContent += `<div style="margin-bottom: 8px; color: #666;">总对局: ${params.value} (${params.percent}%)</div>`;
            tooltipContent += `<div style="border-top: 1px solid #eee; padding-top: 8px;">`;
            
            Object.entries(modeDetails).forEach(([gameType, stats]) => {
              tooltipContent += `
                <div style="margin-bottom: 6px; padding: 4px; background: rgba(102, 126, 234, 0.05); border-radius: 4px;">
                  <div style="font-weight: 500; color: #333; margin-bottom: 2px;">${gameType}</div>
                  <div style="font-size: 12px; color: #666;">
                    <span style="color: #67c23a;">胜: ${stats.wins}</span> | 
                    <span style="color: #f56c6c;">负: ${stats.losses}</span> | 
                    <span style="color: #e6a23c;">平: ${stats.draws}</span>
                    ${stats.aborts > 0 ? ` | <span style="color: #909399;">终止: ${stats.aborts}</span>` : ''}
                  </div>
                  <div style="font-size: 12px; color: #333; margin-top: 2px;">
                    胜率: <span style="font-weight: bold; color: #667eea;">${stats.winRate}%</span>
                  </div>
                </div>
              `;
            });
            
            tooltipContent += `</div>`;
            return tooltipContent;
          },
          backgroundColor: 'rgba(255, 255, 255, 0.95)',
          borderColor: '#667eea',
          borderWidth: 1,
          textStyle: {
            color: '#333'
          },
          extraCssText: 'box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15); border-radius: 8px; max-width: 300px; z-index: 9999 !important;'
        },
        legend: {
          orient: 'vertical',
          left: 'left',
          textStyle: {
            color: '#333'
          }
        },
        series: [
          {
            name: '游戏模式',
            type: 'pie',
            radius: ['40%', '70%'],
            center: ['60%', '50%'],
            avoidLabelOverlap: false,
            itemStyle: {
              borderRadius: 10,
              borderColor: '#fff',
              borderWidth: 2
            },
            label: {
              show: false,
              position: 'center'
            },
            emphasis: {
              label: {
                show: true,
                fontSize: '18',
                fontWeight: 'bold'
              },
              itemStyle: {
                shadowBlur: 10,
                shadowOffsetX: 0,
                shadowColor: 'rgba(0, 0, 0, 0.5)'
              }
            },
            labelLine: {
              show: false
            },
            data: data,
            color: ['#667eea', '#764ba2', '#f093fb']
          }
        ]
      };
      
      this.charts.gameModeChart.setOption(option);
    },
    
    updateGameTypeChart() {
      const categories = [];
      const winsData = [];
      const lossesData = [];
      const drawsData = [];
      
      // 合并所有游戏模式的游戏类型统计
      const gameTypeStats = {};
      
      // 本地对战
      if (this.statistics.localGameStats && this.statistics.localGameStats.gameTypeStats && Object.keys(this.statistics.localGameStats.gameTypeStats).length > 0) {
        Object.entries(this.statistics.localGameStats.gameTypeStats).forEach(([gameType, gameTypeData]) => {
          if (!gameTypeStats[gameType]) {
            gameTypeStats[gameType] = { wins: 0, losses: 0, draws: 0 };
          }
          if (gameTypeData && gameTypeData.algorithmStats && typeof gameTypeData.algorithmStats === 'object') {
            Object.values(gameTypeData.algorithmStats).forEach(algorithm => {
              if (algorithm && algorithm.stats && typeof algorithm.stats === 'object') {
                gameTypeStats[gameType].wins += (algorithm.stats.wins || 0);
                gameTypeStats[gameType].losses += (algorithm.stats.losses || 0);
                gameTypeStats[gameType].draws += (algorithm.stats.draws || 0);
              }
            });
          }
        });
      }
      
      // LMM对战
      if (this.statistics.lmmGameStats && this.statistics.lmmGameStats.gameTypeStats && Object.keys(this.statistics.lmmGameStats.gameTypeStats).length > 0) {
        Object.entries(this.statistics.lmmGameStats.gameTypeStats).forEach(([gameType, stats]) => {
          if (!gameTypeStats[gameType]) {
            gameTypeStats[gameType] = { wins: 0, losses: 0, draws: 0 };
          }
          if (stats && typeof stats === 'object') {
            gameTypeStats[gameType].wins += (stats.wins || 0);
            gameTypeStats[gameType].losses += (stats.losses || 0);
            gameTypeStats[gameType].draws += (stats.draws || 0);
          }
        });
      }
      
      // 联机对战
      if (this.statistics.onlineGameStats && this.statistics.onlineGameStats.gameTypeStats && Object.keys(this.statistics.onlineGameStats.gameTypeStats).length > 0) {
        Object.entries(this.statistics.onlineGameStats.gameTypeStats).forEach(([gameType, stats]) => {
          if (!gameTypeStats[gameType]) {
            gameTypeStats[gameType] = { wins: 0, losses: 0, draws: 0 };
          }
          if (stats && typeof stats === 'object') {
            gameTypeStats[gameType].wins += (stats.wins || 0);
            gameTypeStats[gameType].losses += (stats.losses || 0);
            gameTypeStats[gameType].draws += (stats.draws || 0);
          }
        });
      }
      
      Object.entries(gameTypeStats).forEach(([gameType, stats]) => {
        categories.push(gameType);
        winsData.push(stats.wins);
        lossesData.push(stats.losses);
        drawsData.push(stats.draws);
      });
      
      if (categories.length === 0) {
        // 如果没有数据，显示空状态
        const option = {
          backgroundColor: 'transparent',
          title: {
            text: '暂无数据',
            left: 'center',
            top: 'middle',
            textStyle: {
              color: '#999',
              fontSize: 16
            }
          }
        };
        this.charts.gameTypeChart.setOption(option);
        return;
      }
      
      const option = {
        backgroundColor: 'transparent',
        tooltip: {
          trigger: 'axis',
          axisPointer: {
            type: 'shadow'
          },
          backgroundColor: 'rgba(0, 0, 0, 0.8)',
          borderColor: '#667eea',
          borderWidth: 1,
          textStyle: {
            color: '#fff'
          }
        },
        legend: {
          data: ['胜利', '失败', '平局'],
          textStyle: {
            color: '#333'
          }
        },
        grid: {
          left: '3%',
          right: '4%',
          bottom: '3%',
          containLabel: true
        },
        xAxis: {
          type: 'category',
          data: categories,
          axisLabel: {
            color: '#333'
          }
        },
        yAxis: {
          type: 'value',
          axisLabel: {
            color: '#333'
          }
        },
        series: [
          {
            name: '胜利',
            type: 'bar',
            stack: '总量',
            data: winsData,
            itemStyle: {
              color: '#67c23a'
            }
          },
          {
            name: '失败',
            type: 'bar',
            stack: '总量',
            data: lossesData,
            itemStyle: {
              color: '#f56c6c'
            }
          },
          {
            name: '平局',
            type: 'bar',
            stack: '总量',
            data: drawsData,
            itemStyle: {
              color: '#e6a23c'
            }
          }
        ]
      };
      
      this.charts.gameTypeChart.setOption(option);
    },
    
    updateAlgorithmChart() {
      const data = [];
      
      if (this.statistics.localGameStats && this.statistics.localGameStats.gameTypeStats && Object.keys(this.statistics.localGameStats.gameTypeStats).length > 0) {
        const algorithmStats = {};
        Object.values(this.statistics.localGameStats.gameTypeStats).forEach(gameType => {
          if (gameType && gameType.algorithmStats && typeof gameType.algorithmStats === 'object') {
            Object.entries(gameType.algorithmStats).forEach(([algorithmName, algorithmData]) => {
              if (!algorithmStats[algorithmName]) {
                algorithmStats[algorithmName] = 0;
              }
              if (algorithmData && algorithmData.stats && typeof algorithmData.stats === 'object') {
                algorithmStats[algorithmName] += (algorithmData.stats.wins || 0) + (algorithmData.stats.losses || 0) + (algorithmData.stats.draws || 0);
              }
            });
          }
        });
        
        Object.entries(algorithmStats).forEach(([algorithm, total]) => {
          if (total > 0) {
            data.push({ name: algorithm, value: total });
          }
        });
      }
      
      if (data.length === 0) {
        // 如果没有数据，显示空状态
        const option = {
          backgroundColor: 'transparent',
          title: {
            text: '暂无数据',
            left: 'center',
            top: 'middle',
            textStyle: {
              color: '#999',
              fontSize: 16
            }
          }
        };
        this.charts.algorithmChart.setOption(option);
        return;
      }
      
      const option = {
        backgroundColor: 'transparent',
        tooltip: {
          trigger: 'item',
          formatter: '{a} <br/>{b}: {c} ({d}%)',
          backgroundColor: 'rgba(0, 0, 0, 0.8)',
          borderColor: '#667eea',
          borderWidth: 1,
          textStyle: {
            color: '#fff'
          }
        },
        legend: {
          orient: 'vertical',
          left: 'left',
          textStyle: {
            color: '#333'
          }
        },
        series: [
          {
            name: '算法类型',
            type: 'pie',
            radius: '70%',
            center: ['60%', '50%'],
            data: data,
            emphasis: {
              itemStyle: {
                shadowBlur: 10,
                shadowOffsetX: 0,
                shadowColor: 'rgba(0, 0, 0, 0.5)'
              }
            },
            color: ['#ff6b6b', '#4ecdc4', '#45b7d1', '#96ceb4', '#feca57', '#ff9ff3']
          }
        ]
      };
      
      this.charts.algorithmChart.setOption(option);
    },
    
    updateResultChart() {
      const data = [
        { name: '胜利', value: this.overallStats.totalWins },
        { name: '失败', value: this.overallStats.totalLosses },
        { name: '平局', value: this.overallStats.totalDraws },
        { name: '终止', value: this.overallStats.totalAborts }
      ].filter(item => item.value > 0);
      
      if (data.length === 0) {
        // 如果没有数据，显示空状态
        const option = {
          backgroundColor: 'transparent',
          title: {
            text: '暂无数据',
            left: 'center',
            top: 'middle',
            textStyle: {
              color: '#999',
              fontSize: 16
            }
          }
        };
        this.charts.resultChart.setOption(option);
        return;
      }
      
      const option = {
        backgroundColor: 'transparent',
        tooltip: {
          trigger: 'item',
          formatter: '{a} <br/>{b}: {c} ({d}%)',
          backgroundColor: 'rgba(0, 0, 0, 0.8)',
          borderColor: '#667eea',
          borderWidth: 1,
          textStyle: {
            color: '#fff'
          }
        },
        legend: {
          orient: 'vertical',
          left: 'left',
          textStyle: {
            color: '#333'
          }
        },
        series: [
          {
            name: '对局结果',
            type: 'pie',
            radius: ['30%', '70%'],
            center: ['60%', '50%'],
            roseType: 'area',
            itemStyle: {
              borderRadius: 8
            },
            data: data,
            color: ['#67c23a', '#f56c6c', '#e6a23c', '#909399']
          }
        ]
      };
      
      this.charts.resultChart.setOption(option);
    }
  }
};
</script>

<style scoped>
.statistics-container {
  padding: 20px 0;
  position: relative;
  z-index: 1;
}

.overview-section {
  margin-bottom: 30px;
}

.stat-card {
  background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
  border-radius: 15px;
  padding: 20px;
  color: white;
  display: flex;
  align-items: center;
  box-shadow: 0 8px 32px rgba(102, 126, 234, 0.3);
  transition: all 0.3s ease;
  cursor: pointer;
  position: relative;
  overflow: hidden;
}

.stat-card::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(45deg, rgba(255, 255, 255, 0.1), transparent);
  opacity: 0;
  transition: opacity 0.3s ease;
}

.stat-card:hover {
  transform: translateY(-5px);
  box-shadow: 0 15px 40px rgba(102, 126, 234, 0.4);
}

.stat-card:hover::before {
  opacity: 1;
}

.stat-card.wins {
  background: linear-gradient(135deg, #67c23a 0%, #85ce61 100%);
  box-shadow: 0 8px 32px rgba(103, 194, 58, 0.3);
}

.stat-card.wins:hover {
  box-shadow: 0 15px 40px rgba(103, 194, 58, 0.4);
}

.stat-card.losses {
  background: linear-gradient(135deg, #f56c6c 0%, #f78989 100%);
  box-shadow: 0 8px 32px rgba(245, 108, 108, 0.3);
}

.stat-card.losses:hover {
  box-shadow: 0 15px 40px rgba(245, 108, 108, 0.4);
}

.stat-card.winrate {
  background: linear-gradient(135deg, #e6a23c 0%, #eebe77 100%);
  box-shadow: 0 8px 32px rgba(230, 162, 60, 0.3);
}

.stat-card.winrate:hover {
  box-shadow: 0 15px 40px rgba(230, 162, 60, 0.4);
}

.stat-icon {
  font-size: 2.5em;
  margin-right: 15px;
  opacity: 0.8;
  transition: all 0.3s ease;
}

.stat-card:hover .stat-icon {
  transform: scale(1.1);
  opacity: 1;
}

.stat-content {
  flex: 1;
}

.stat-number {
  font-size: 2.2em;
  font-weight: bold;
  line-height: 1;
  margin-bottom: 5px;
  text-shadow: 0 2px 4px rgba(0, 0, 0, 0.3);
}

.stat-label {
  font-size: 0.9em;
  opacity: 0.9;
  text-transform: uppercase;
  letter-spacing: 1px;
}

.charts-section {
  margin-top: 30px;
}

.chart-container {
  background: white;
  border-radius: 15px;
  padding: 20px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;
  position: relative;
  overflow: visible;
  z-index: 1;
}

.chart-container::before {
  content: '';
  position: absolute;
  top: 0;
  left: 0;
  right: 0;
  bottom: 0;
  background: linear-gradient(45deg, rgba(102, 126, 234, 0.05), transparent);
  opacity: 0;
  transition: opacity 0.3s ease;
}

.chart-container:hover {
  transform: translateY(-3px);
  box-shadow: 0 15px 40px rgba(0, 0, 0, 0.15);
}

.chart-container:hover::before {
  opacity: 1;
}

.chart-title {
  margin: 0 0 20px 0;
  font-size: 1.2em;
  font-weight: bold;
  color: #333;
  text-align: center;
  position: relative;
  padding-bottom: 10px;
}

.chart-title::after {
  content: '';
  position: absolute;
  bottom: 0;
  left: 50%;
  transform: translateX(-50%);
  width: 50px;
  height: 3px;
  background: linear-gradient(90deg, #667eea, #764ba2);
  border-radius: 2px;
}

.chart {
  height: 300px;
  width: 100%;
  position: relative;
  z-index: 1;
}

/* 个人信息卡片样式优化 */
.box-card {
  border-radius: 15px;
  box-shadow: 0 8px 32px rgba(0, 0, 0, 0.1);
  transition: all 0.3s ease;
}

.box-card:hover {
  transform: translateY(-3px);
  box-shadow: 0 15px 40px rgba(0, 0, 0, 0.15);
}

.list-group-striped {
  list-style: none;
  padding: 0;
  margin: 20px 0 0 0;
}

.list-group-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 12px 0;
  border-bottom: 1px solid #f0f0f0;
  transition: all 0.3s ease;
}

.list-group-item:hover {
  background: rgba(102, 126, 234, 0.05);
  padding-left: 10px;
  border-radius: 8px;
}

.list-group-item:last-child {
  border-bottom: none;
}

.item-left {
  display: flex;
  align-items: center;
  gap: 8px;
}

.item-left svg {
  color: #667eea;
}

.item-left span {
  font-weight: 500;
  color: #333;
}

.item-right {
  font-weight: bold;
  color: #333;
  text-align: center;
  flex: 1;
  margin-left: 20px;
}

.list-group-item svg {
  margin-right: 0;
  color: #667eea;
}

.pull-right {
  font-weight: bold;
  color: #333;
}

/* 响应式设计 */
@media (max-width: 768px) {
  .stat-card {
    margin-bottom: 15px;
  }
  
  .stat-number {
    font-size: 1.8em;
  }
  
  .stat-icon {
    font-size: 2em;
    margin-right: 10px;
  }
  
  .chart {
    height: 250px;
  }
  
  .chart-container {
    margin-bottom: 20px;
  }
}

/* 动画效果 */
@keyframes fadeInUp {
  from {
    opacity: 0;
    transform: translateY(30px);
  }
  to {
    opacity: 1;
    transform: translateY(0);
  }
}

.stat-card {
  animation: fadeInUp 0.6s ease-out;
}

.chart-container {
  animation: fadeInUp 0.8s ease-out;
}

/* 为不同的统计卡片添加延迟动画 */
.stat-card:nth-child(1) { animation-delay: 0.1s; }
.stat-card:nth-child(2) { animation-delay: 0.2s; }
.stat-card:nth-child(3) { animation-delay: 0.3s; }
.stat-card:nth-child(4) { animation-delay: 0.4s; }

.chart-container:nth-child(1) { animation-delay: 0.5s; }
.chart-container:nth-child(2) { animation-delay: 0.6s; }

/* ECharts tooltip 样式优化 */
::v-deep .echarts-tooltip {
  z-index: 9999 !important;
}

/* 全局确保ECharts tooltip不被遮挡 */
::v-deep .el-tooltip__popper {
  z-index: 9999 !important;
}

::v-deep .echarts-tooltip-content {
  z-index: 9999 !important;
}

/* 确保ECharts容器的tooltip能正确显示 */
.app-container {
  position: relative;
  z-index: 1;
}
</style>
