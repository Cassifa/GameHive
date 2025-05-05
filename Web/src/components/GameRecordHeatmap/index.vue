<template>
  <div class="record-heatmap-container">
    <div class="heatmap-title">对局记录热力图</div>
    <div v-if="loading" class="loading-container">
      <i class="el-icon-loading"></i>
      <span>加载中...</span>
    </div>
    <template v-else>
      <div class="heatmap-wrapper">
        <!-- 月份标签区域 -->
        <div class="month-labels" :style="{ height: monthLabelHeight + 'px' }">
          <div 
            v-for="(month, index) in calendarData.monthLabels" 
            :key="index"
            class="month-label"
            :style="{
              left: month.position + 'px',
              fontSize: month.fontSize + 'px',
              transform: 'translateX(-50%)'
            }"
          >
            {{ month.name }}
          </div>
        </div>
        
        <!-- 主体内容 -->
        <div class="heatmap-body">
          <!-- 周标签 -->
          <div class="weekdays">
            <div v-for="(day, index) in weekdays" :key="index" class="weekday">
              {{ day }}
            </div>
          </div>
          <!-- 热力图 -->
          <canvas 
            ref="heatmapCanvas" 
            class="heatmap-canvas"
            @mousemove="onCanvasMouseMove"
            @mouseout="hideTooltip"
          ></canvas>
        </div>
      </div>
    </template>
    <!-- 悬浮提示框 -->
    <div class="tooltip" v-show="showTooltipFlag" :style="tooltipStyle">
      <div class="tooltip-date">{{ tooltipData.dateDisplay }}</div>
      <div class="tooltip-count">{{ tooltipData.count }} 场对局</div>
    </div>
  </div>
</template>

<script>
import { getRecordHeatmapData } from "@/api/record/RecordHeatmap";

// 热力图模拟数据服务
class MockHeatmapService {
  constructor() {
    this.today = new Date();
    this.oneYearAgo = new Date();
    this.oneYearAgo.setFullYear(this.today.getFullYear() - 1);
  }

  // 生成模拟数据
  generateMockData() {
    const data = [];
    
    // 模拟一些热点日期 - 最近几个月有较多记录
    const recentMonthsHotDays = this.generateHotDays(90, 0.4, 3, 15);
    
    // 模拟六个月前的活跃周期
    const sixMonthsAgoHotDays = this.generateHotDays(30, 0.7, 180, 10);
    
    // 模拟一年中的节假日活跃
    const holidays = this.generateHolidayActivity();
    
    // 随机生成一些普通日期的数据
    for (let d = new Date(this.oneYearAgo); d <= this.today; d.setDate(d.getDate() + 1)) {
      const dateStr = this.formatDate(d);
      
      // 检查是否是热点日期
      let isHotDay = recentMonthsHotDays.includes(dateStr) || 
                     sixMonthsAgoHotDays.includes(dateStr) ||
                     holidays[dateStr];
      
      // 生成随机数据
      if (isHotDay) {
        const hotValue = holidays[dateStr] || Math.floor(Math.random() * 15) + 3;
        data.push({
          date: dateStr,
          count: hotValue
        });
      } else if (Math.random() > 0.7) { // 30%的普通日期有记录
        const count = Math.floor(Math.random() * 4);
        if (count > 0) {
          data.push({
            date: dateStr,
            count: count
          });
        }
      }
    }
    
    return data;
  }
  
  // 生成特定时间段的热点日期
  generateHotDays(days, probability, daysAgo, maxValue) {
    const hotDays = [];
    const startDate = new Date();
    startDate.setDate(startDate.getDate() - daysAgo);
    
    for (let i = 0; i < days; i++) {
      if (Math.random() < probability) {
        const date = new Date(startDate);
        date.setDate(date.getDate() + i);
        if (date <= this.today) {
          hotDays.push(this.formatDate(date));
        }
      }
    }
    return hotDays;
  }
  
  // 生成节假日活动数据
  generateHolidayActivity() {
    const holidays = {};
    const year = this.today.getFullYear();
    
    // 添加一些常见节假日
    holidays[`${year}-01-01`] = 12; // 元旦
    holidays[`${year}-02-14`] = 8;  // 情人节
    holidays[`${year-1}-12-25`] = 10; // 去年圣诞节
    
    // 春节期间（假设2月初）
    for (let i = 1; i <= 7; i++) {
      holidays[`${year}-02-0${i}`] = Math.floor(Math.random() * 10) + 5;
    }
    
    // 五一假期
    holidays[`${year}-05-01`] = 15;
    holidays[`${year}-05-02`] = 13;
    holidays[`${year}-05-03`] = 11;
    
    // 国庆假期
    for (let i = 1; i <= 7; i++) {
      const day = i < 10 ? `0${i}` : i;
      holidays[`${year}-10-${day}`] = Math.floor(Math.random() * 8) + 8;
    }
    
    return holidays;
  }
  
  // 格式化日期
  formatDate(date) {
    const year = date.getFullYear();
    const month = String(date.getMonth() + 1).padStart(2, '0');
    const day = String(date.getDate()).padStart(2, '0');
    return `${year}-${month}-${day}`;
  }
}

export default {
  name: "GameRecordHeatmap",
  props: {
    // 可以传入自定义的查询参数
    queryParams: {
      type: Object,
      default: () => ({})
    },
    // 是否使用模拟数据
    useMockData: {
      type: Boolean,
      default: false
    }
  },
  data() {
    return {
      // 月份名称改为数字
      months: ['1月','2月','3月','4月','5月','6月','7月','8月','9月','10月','11月','12月'],
      // 周数改为1,3,7
      weekdays: ['1', '3', '7'],
      // 热力图数据
      recordsData: [],
      // 处理后的热力图数据
      heatmapData: [],
      // 月份标签位置计算
      calendarData: {
        monthLabels: [],
        startDate: null,
        endDate: null
      },
      // 悬浮提示相关
      showTooltipFlag: false,
      tooltipData: {
        date: '',
        dateDisplay: '',
        count: 0
      },
      tooltipStyle: {
        top: '0px',
        left: '0px'
      },
      // 是否数据加载中
      loading: false,
      // 模拟数据服务
      mockService: new MockHeatmapService(),
      // Canvas 相关
      cellSize: 15,
      cellGap: 3,
      canvasWidth: 0,
      canvasHeight: 0,
      // 热力图颜色 - 蓝色系
      colorLevels: [
        '#f0f0f0', // 无记录 - 更浅的灰色
        '#a6d0fa', // 少量记录 - 浅蓝色
        '#65a2e6', // 中等记录 - 中蓝色
        '#3674d9', // 较多记录 - 深蓝色
        '#194bab'  // 大量记录 - 深蓝色
      ],
      // 当前鼠标悬停的单元格
      hoveredCellIndex: -1,
      monthLabelHeight: 30, // 月份标签区域高度
    }
  },
  mounted() {
    window.addEventListener('resize', this.handleResize);
    // 组件创建时获取数据
    this.fetchRecordsData();
  },
  beforeDestroy() {
    window.removeEventListener('resize', this.handleResize);
  },
  methods: {
    // 获取对局记录数据
    fetchRecordsData() {
      this.loading = true;
      
      // 如果指定使用模拟数据或者是开发环境
      if (this.useMockData || process.env.NODE_ENV === 'development') {
        setTimeout(() => {
          this.recordsData = this.mockService.generateMockData();
          this.processHeatmapData();
          this.calculateMonthPositions();
          this.loading = false;
          this.$nextTick(() => {
            this.drawHeatmap();
          });
        }, 800); // 模拟加载延迟
        return;
      }
      
      // 从本地缓存获取用户ID (必传参数)
      const userId = localStorage.getItem('userId') || this.$store.getters.userId;
      if (!userId) {
        console.error('用户ID不能为空');
        this.loading = false;
        return;
      }
      
      // 构建请求参数
      const params = {
        ...this.queryParams,  // 包含游戏类型ID、是否AI对局、算法ID等
        id: userId  // 用户ID(必传)
      };
      
      // 调用实际API接口
      getRecordHeatmapData(params).then(response => {
        if (response.code === 200 && response.data) {
          this.recordsData = response.data;
        } else {
          // 接口返回异常时使用模拟数据
          this.recordsData = this.mockService.generateMockData();
        }
      }).catch(() => {
        // 发生错误时使用模拟数据
        this.recordsData = this.mockService.generateMockData();
      }).finally(() => {
        this.processHeatmapData();
        this.calculateMonthPositions();
        this.loading = false;
        this.$nextTick(() => {
          this.drawHeatmap();
        });
      });
    },

    // 格式化日期为 YYYY-MM-DD
    formatDate(date) {
      const year = date.getFullYear();
      const month = String(date.getMonth() + 1).padStart(2, '0');
      const day = String(date.getDate()).padStart(2, '0');
      return `${year}-${month}-${day}`;
    },
    
    // 格式化日期为中文显示：YYYY年MM月DD日 星期几
    formatChineseDate(dateStr) {
      const date = new Date(dateStr);
      const year = date.getFullYear();
      const month = date.getMonth() + 1;
      const day = date.getDate();
      const weekday = ['日', '一', '二', '三', '四', '五', '六'][date.getDay()];
      return `${year}年${month}月${day}日 星期${weekday}`;
    },

    // 处理热力图数据
    processHeatmapData() {
      // 生成最近一年的日期范围
      const end = new Date();
      const start = new Date();
      start.setFullYear(start.getFullYear() - 1);
      
      // 保存起始和结束日期，用于计算月份标签位置
      this.calendarData.startDate = new Date(start);
      this.calendarData.endDate = new Date(end);
      
      const result = [];
      
      // 确保起始日期是周一
      const dayOfWeek = start.getDay(); // 0=周日, 1=周一, ..., 6=周六
      start.setDate(start.getDate() - (dayOfWeek === 0 ? 6 : dayOfWeek - 1));
      
      // 填充所有日期
      for (let d = new Date(start); d <= end; d.setDate(d.getDate() + 1)) {
        const dateStr = this.formatDate(d);
        const record = this.recordsData.find(r => r.date === dateStr);
        
        result.push({
          date: dateStr,
          dateDisplay: this.formatChineseDate(dateStr),
          count: record ? record.count : 0,
          dayOfWeek: d.getDay(), // 0=周日, 1=周一, ..., 6=周六
          row: d.getDay() === 0 ? 6 : d.getDay() - 1,
          col: Math.floor((d - start) / (24 * 60 * 60 * 1000) / 7),
          month: d.getMonth(),
          year: d.getFullYear()
        });
      }
      
      this.heatmapData = result;
    },
    
    // 计算每个月份标签的位置
    calculateMonthPositions() {
      // 重构后的月份计算逻辑
      if (this.heatmapData.length === 0) return;

      const months = [];
      let currentMonth = -1;
      let currentYear = -1;
      let startCol = 0;

      // 按周遍历数据
      const weeks = Math.ceil(this.heatmapData.length / 7);
      for (let col = 0; col < weeks; col++) {
        // 获取该周周一的日期
        const firstDay = this.heatmapData[col * 7];
        if (!firstDay) continue;

        // 月份变化时记录位置
        if (firstDay.month !== currentMonth || firstDay.year !== currentYear) {
          if (currentMonth !== -1) {
            months.push({
              name: this.months[currentMonth],
              startCol: startCol,
              endCol: col - 1,
              month: currentMonth,
              year: currentYear
            });
          }
          currentMonth = firstDay.month;
          currentYear = firstDay.year;
          startCol = col;
        }
      }

      // 添加最后一个月
      if (currentMonth !== -1) {
        months.push({
          name: this.months[currentMonth],
          startCol: startCol,
          endCol: weeks - 1,
          month: currentMonth,
          year: currentYear
        });
      }

      // 最终位置计算
      this.calendarData.monthLabels = months.map(m => {
        const startPixel = m.startCol * (this.cellSize + this.cellGap);
        const endPixel = (m.endCol + 1) * (this.cellSize + this.cellGap);
        // 修正单列位置计算
        const centerPosition = startPixel + ((endPixel - startPixel) / 2);
        return {
          name: m.name,
          position: centerPosition,
          width: endPixel - startPixel,
          fontSize: this.cellSize * 1.5
        };
      });
    },

    // 根据对局数量获取颜色级别 (0-4)
    getColorLevel(count) {
      if (count === 0) return 0;
      if (count <= 2) return 1;
      if (count <= 4) return 2;
      if (count <= 7) return 3;
      return 4;
    },

    // 绘制热力图
    drawHeatmap() {
      if (!this.$refs.heatmapCanvas || this.heatmapData.length === 0) return;
      
      const canvas = this.$refs.heatmapCanvas;
      const ctx = canvas.getContext('2d');
      
      // 计算网格尺寸
      const containerWidth = canvas.parentElement.clientWidth - 50; // 减去星期标签宽度
      const weeks = Math.ceil(this.heatmapData.length / 7);
      
      // 增大基础单元格尺寸
      const baseCellSize = 18; // 从15调整为18
      this.cellSize = Math.min(baseCellSize, Math.floor((containerWidth - (weeks - 1) * this.cellGap) / weeks));
      
      // 设置canvas尺寸时增加10%的边距
      this.canvasWidth = Math.floor(weeks * (this.cellSize + this.cellGap) * 1.1);
      this.canvasHeight = 7 * (this.cellSize + this.cellGap) + 10; // 增加底部间距
      
      canvas.width = this.canvasWidth;
      canvas.height = this.canvasHeight;
      
      // 清空画布
      ctx.clearRect(0, 0, canvas.width, canvas.height);
      
      // 绘制每个单元格
      this.heatmapData.forEach((cell, index) => {
        const x = cell.col * (this.cellSize + this.cellGap);
        const y = cell.row * (this.cellSize + this.cellGap);
        const colorLevel = this.getColorLevel(cell.count);
        
        // 绘制单元格
        ctx.fillStyle = this.colorLevels[colorLevel];
        ctx.fillRect(x, y, this.cellSize, this.cellSize);
        
        // 绘制边框
        ctx.strokeStyle = colorLevel === 0 ? '#e0e0e0' : this.darkenColor(this.colorLevels[colorLevel], 0.2);
        ctx.lineWidth = 1;
        ctx.strokeRect(x, y, this.cellSize, this.cellSize);
        
        // 如果是当前悬停的单元格
        if (index === this.hoveredCellIndex) {
          ctx.strokeStyle = '#000';
          ctx.lineWidth = 2;
          ctx.strokeRect(x, y, this.cellSize, this.cellSize);
        }
      });
    },
    
    // 颜色加深函数
    darkenColor(color, amount) {
      // 如果是灰色，直接返回边框色
      if (color === '#f0f0f0') return '#e0e0e0';
      
      // 解析颜色
      let r = parseInt(color.substring(1, 3), 16);
      let g = parseInt(color.substring(3, 5), 16);
      let b = parseInt(color.substring(5, 7), 16);
      
      // 加深颜色
      r = Math.max(0, Math.floor(r * (1 - amount)));
      g = Math.max(0, Math.floor(g * (1 - amount)));
      b = Math.max(0, Math.floor(b * (1 - amount)));
      
      // 返回新颜色
      return `#${r.toString(16).padStart(2, '0')}${g.toString(16).padStart(2, '0')}${b.toString(16).padStart(2, '0')}`;
    },
    
    // Canvas 鼠标移动事件
    onCanvasMouseMove(event) {
      const canvas = this.$refs.heatmapCanvas;
      const rect = canvas.getBoundingClientRect();
      
      // 精确计算缩放比例
      const scaleX = canvas.width / rect.width;
      const scaleY = canvas.height / rect.height;

      // 修正后的坐标计算
      const x = (event.clientX - rect.left) * scaleX;
      const y = (event.clientY - rect.top) * scaleY;

      // 调整提示框位置
      this.tooltipStyle = {
        top: `${event.clientY + 20}px`,  // 增加垂直偏移
        left: `${event.clientX + 20}px`  // 增加水平偏移
      };
      
      // 计算单元格索引
      const col = Math.floor(x / (this.cellSize + this.cellGap));
      const row = Math.floor(y / (this.cellSize + this.cellGap));
      
      // 查找对应的数据
      const cellIndex = this.heatmapData.findIndex(cell => cell.col === col && cell.row === row);
      
      if (cellIndex !== -1) {
        const cell = this.heatmapData[cellIndex];
        
        // 显示提示框
        this.tooltipData = {
          date: cell.date,
          dateDisplay: cell.dateDisplay,
          count: cell.count
        };
        
        this.showTooltipFlag = true;
        
        // 更新悬停单元格并重绘
        if (this.hoveredCellIndex !== cellIndex) {
          this.hoveredCellIndex = cellIndex;
          this.drawHeatmap();
        }
      } else {
        this.hideTooltip();
      }
    },

    // 隐藏悬浮提示
    hideTooltip() {
      this.showTooltipFlag = false;
      if (this.hoveredCellIndex !== -1) {
        this.hoveredCellIndex = -1;
        this.drawHeatmap();
      }
    },
    
    // 提供给父组件的刷新方法
    refresh() {
      this.fetchRecordsData();
    },

    handleResize() {
      this.calculateMonthPositions();
      this.drawHeatmap();
    }
  }
}
</script>

<style scoped>
.record-heatmap-container {
  margin: 20px 0;
  position: relative;
  padding: 20px 20px 30px;
  min-height: 200px;
}

.heatmap-title {
  font-size: 20px;
  font-weight: bold;
  text-align: center;
  margin-bottom: 25px;
  color: #303133;
}

.loading-container {
  display: flex;
  flex-direction: column;
  justify-content: center;
  align-items: center;
  height: 200px;
  color: #909399;
}

.loading-container i {
  font-size: 30px;
  margin-bottom: 10px;
}

.record-heatmap-header {
  display: flex;
  flex-direction: column;
  margin-bottom: 10px;
  padding-left: 40px; /* 为左侧星期标签留出空间 */
  position: relative;
  height: 20px;
}

.record-heatmap-header .months {
  position: relative;
  width: 100%;
  height: 20px;
}

.month-label {
  position: absolute;
  font-size: 13px;
  font-weight: 600;
  color: #666;
  text-align: center;
  top: 0;
  padding-left: 3px; /* 左对齐调整 */
  transform: translate(-50%, -50%) !important;
  left: 0; /* 重置left属性 */
}

.record-heatmap-body {
  display: flex;
  align-items: flex-start;
  justify-content: center;
  margin: 0 auto;
  max-width: 90%;
}

.weekdays {
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  padding: 0 10px 0 0;
  height: 120px;
}

.weekday {
  font-size: 14px; /* 增大字号 */
  transform: translateX(-8px); /* 向左微调位置 */
  color: #606266;
  height: calc(18px + 3px); /* 与单元格高度+间距匹配 */
  line-height: 18px;
  margin: 1.5px 0;
  text-align: right; /* 右对齐 */
  padding-right: 5px; /* 右边距 */
}

.heatmap-canvas {
  display: block;
  margin: 0 auto;
  cursor: pointer;
  width: 100% !important;
  height: auto !important;
}

.record-heatmap-footer {
  margin-top: 15px;
  display: flex;
  justify-content: center;
}

.legend {
  display: flex;
  align-items: center;
  font-size: 14px;
  font-weight: 500;
  color: #303133;
}

.legend-cells {
  display: flex;
  margin: 0 8px;
}

.legend .heatmap-cell {
  width: 15px;
  height: 15px;
  margin: 0 2px;
  border-radius: 2px;
  border: 1px solid rgba(0, 0, 0, 0.1);
}

.legend .level-0 {
  background-color: #f0f0f0;
  border-color: #e0e0e0;
}

.legend .level-1 {
  background-color: #a6d0fa;
  border-color: #8ab0d5;
}

.legend .level-2 {
  background-color: #65a2e6;
  border-color: #5589c5;
}

.legend .level-3 {
  background-color: #3674d9;
  border-color: #2961b7;
}

.legend .level-4 {
  background-color: #194bab;
  border-color: #123c8a;
}

.tooltip {
  position: fixed;
  background-color: rgba(0, 0, 0, 0.9);
  color: white;
  padding: 10px 12px;
  border-radius: 4px;
  font-size: 13px;
  z-index: 99999;
  pointer-events: none;
  max-width: 200px;
  box-shadow: 0 4px 12px rgba(0, 0, 0, 0.4);
}

.tooltip-date {
  font-weight: bold;
  margin-bottom: 5px;
}

@media screen and (max-width: 768px) {
  .weekdays {
    height: 105px;
  }
}

/* 新增容器样式 */
.heatmap-wrapper {
  position: relative;
  margin: 0 auto;
  max-width: 1200px; /* 增大最大宽度 */
  padding-left: 50px; /* 为周标签留出空间 */
  padding-top: 30px; /* 为月份标签留出空间 */
}

.month-labels {
  position: absolute;
  top: 0;
  left: 50px; /* 与周标签对齐 */
  right: 0;
  height: 20px;
}

.heatmap-body {
  position: relative;
  display: flex;
}

.weekdays {
  position: absolute;
  left: -50px; /* 向左偏移 */
  top: 0;
  display: flex;
  flex-direction: column;
  justify-content: space-between;
  height: 100%;
  width: 40px;
}

.heatmap-canvas {
  flex: 1;
  min-width: 800px; /* 最小宽度保证热力图大小 */
  margin-top: 10px; /* 与月份标签间距 */
}

.month-labels {
  position: relative;
  margin-bottom: 10px;
}

.month-label {
  position: absolute;
  top: 50%;
  transform: translate(-50%, -50%);
  white-space: nowrap;
  font-weight: bold;
  color: #333;
  pointer-events: none;
}
</style> 