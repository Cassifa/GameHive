<template>
  <div class="app-container">
    <div class="welcome-container">
      <h1 class="welcome-title">欢迎使用GameHive系统</h1>
    </div>

    <!-- 游戏矩阵组件 -->
    <div class="matrix-container">
      <!-- 标题 -->
      <h2 class="matrix-title">算法游戏产品矩阵</h2>
      
      <!-- 矩阵图表 -->
      <canvas ref="matrixCanvas" class="matrix-canvas"></canvas>
    </div>
  </div>
</template>

<script>
import { listAlgorithm, listAlgorithmOptions } from "@/api/Algorithm/Algorithm";
import { listGameType, listGameTypeOptions } from "@/api/GameType/GameType";
import { listProduct } from "@/api/Product/Product";

export default {
  name: "Index",
  data() {
    return {
      // AI类型列表
      aiTypes: [],
      // 游戏类型列表
      gameTypes: [],
      // AI产品列表
      products: [],
      // Canvas配置
      canvas: null,
      ctx: null,
      // 坐标系配置
      padding: 120,      // 内边距
      cellSize: {
        x: 120,         // 横轴单元格大小
        y: 60,          // 纵轴间距
      },
      fontSize: 14,     // 字体大小
      axisLabelSize: 14, // 轴标题字体大小
      labelPadding: 30, // 标签与轴线的间距
      arrowSize: 10,    // 箭头大小
      checkSize: 30,    // 对勾大小
      titleOffset: 15,  // 标题与箭头的距离
    };
  },
  computed: {
    // 计算画布所需的实际尺寸
    canvasSize() {
      const width = this.padding * 2 + this.cellSize.x * (this.aiTypes.length + 1);
      const height = this.padding * 2 + this.cellSize.y * (this.gameTypes.length + 1);
      return { width, height };
    }
  },
  async created() {
    try {
      await this.getList();
    } catch (error) {
      console.error('获取数据失败:', error);
    }
  },
  mounted() {
    this.$nextTick(() => {
      this.initCanvas();
      this.drawMatrix();
    });
    window.addEventListener('resize', this.handleResize);
  },
  beforeDestroy() {
    window.removeEventListener('resize', this.handleResize);
  },
  methods: {
    /** 获取所有数据 */
    async getList() {
      try {
        const [aiResponse, gameResponse, productResponse] = await Promise.all([
          listAlgorithmOptions(),
          listGameTypeOptions(),
          listProduct()
        ]);
        
        this.aiTypes = aiResponse.data || [];
        this.gameTypes = gameResponse.data || [];
        this.products = productResponse.rows || [];
        
        if (this.aiTypes.length && this.gameTypes.length) {
          this.$nextTick(() => {
            this.initCanvas();
            this.drawMatrix();
          });
        } else {
          console.warn('算法类型或游戏类型数据为空');
        }
      } catch (error) {
        console.error('获取数据失败:', error);
        throw error;
      }
    },
    initCanvas() {
      this.canvas = this.$refs.matrixCanvas;
      
      // 设置画布大小
      const rect = this.canvas.getBoundingClientRect();
      const dpr = window.devicePixelRatio || 1;
      
      this.canvas.width = this.canvasSize.width * dpr;
      this.canvas.height = this.canvasSize.height * dpr;
      
      this.canvas.style.width = this.canvasSize.width + 'px';
      this.canvas.style.height = this.canvasSize.height + 'px';
      
      this.ctx = this.canvas.getContext('2d');
      
      this.ctx.scale(dpr, dpr);
      
      // 设置默认样式
      this.ctx.font = `${this.fontSize}px Arial`;
      this.ctx.textAlign = 'center';
      this.ctx.textBaseline = 'middle';
    },
    drawMatrix() {
      if (!this.ctx) return;
      
      // 清空画布
      this.ctx.clearRect(0, 0, this.canvas.width, this.canvas.height);
      
      this.drawAxes();       // 1. 绘制坐标轴和标题
      this.drawGrid();       // 2. 绘制网格
      this.drawLabels();     // 3. 绘制格点标签
      this.drawProducts();   // 4. 绘制对勾
    },
    drawAxes() {
      const { ctx, padding, arrowSize, titleOffset, cellSize } = this;
      const width = this.canvasSize.width;
      const height = this.canvasSize.height;
      
      // 计算坐标轴终点 (延伸一个单元格的距离)
      const xAxisEnd = padding + cellSize.x * (this.aiTypes.length + 1);
      const yAxisEnd = height - padding - cellSize.y * (this.gameTypes.length + 1);
      
      ctx.beginPath();
      ctx.strokeStyle = '#303133';
      ctx.lineWidth = 2;
      
      // X轴
      ctx.moveTo(padding, height - padding);
      ctx.lineTo(xAxisEnd, height - padding);
      
      // Y轴
      ctx.moveTo(padding, height - padding);
      ctx.lineTo(padding, yAxisEnd);
      
      // 绘制箭头
      // X轴箭头
      ctx.moveTo(xAxisEnd, height - padding);
      ctx.lineTo(xAxisEnd - arrowSize, height - padding - arrowSize);
      ctx.moveTo(xAxisEnd, height - padding);
      ctx.lineTo(xAxisEnd - arrowSize, height - padding + arrowSize);
      
      // Y轴箭头
      ctx.moveTo(padding, yAxisEnd);
      ctx.lineTo(padding - arrowSize, yAxisEnd + arrowSize);
      ctx.moveTo(padding, yAxisEnd);
      ctx.lineTo(padding + arrowSize, yAxisEnd + arrowSize);
      
      ctx.stroke();
      
      // 绘制坐标轴标题
      ctx.font = `${this.axisLabelSize}px Arial`;
      ctx.fillStyle = '#303133';
      
      // X轴标题 (放在箭头右侧)
      ctx.save();
      ctx.textAlign = 'left';
      ctx.textBaseline = 'middle';
      ctx.fillText('算法等级结构', xAxisEnd + titleOffset, height - padding);
      ctx.restore();
      
      // Y轴标题 (放在箭头上方)
      ctx.save();
      ctx.textAlign = 'center';
      ctx.textBaseline = 'bottom';
      ctx.fillText('游戏产品簇', padding, yAxisEnd - titleOffset);
      ctx.restore();
    },
    drawGrid() {
      const { ctx, padding, cellSize } = this;
      
      ctx.strokeStyle = '#DCDFE6';
      ctx.lineWidth = 0.5;
      ctx.setLineDash([4, 4]);
      
      // 绘制水平网格线
      this.gameTypes.forEach((_, index) => {
        const y = this.canvasSize.height - padding - cellSize.y * (index + 1);
        ctx.beginPath();
        ctx.moveTo(padding, y);
        // 网格线只到最后一个单元格
        ctx.lineTo(padding + cellSize.x * this.aiTypes.length, y);
        ctx.stroke();
      });
      
      // 绘制垂直网格线
      this.aiTypes.forEach((_, index) => {
        const x = padding + cellSize.x * (index + 1);
        ctx.beginPath();
        ctx.moveTo(x, this.canvasSize.height - padding);
        ctx.lineTo(x, this.canvasSize.height - padding - cellSize.y * this.gameTypes.length);
        ctx.stroke();
      });
      
      ctx.setLineDash([]);
    },
    drawLabels() {
      const { ctx, padding, cellSize, labelPadding } = this;
      
      ctx.font = `${this.fontSize}px Arial`;
      ctx.fillStyle = '#606266';
      
      // Y轴标签
      this.gameTypes.forEach((game, index) => {
        const y = this.canvasSize.height - padding - cellSize.y * (index + 1);
        const labelText = game.label || '';
        ctx.textAlign = 'right';
        ctx.fillText(labelText, padding - labelPadding, y);
      });
      
      // X轴标签
      this.aiTypes.forEach((ai, index) => {
        const x = padding + cellSize.x * (index + 1);
        const labelText = ai.label || '';
        ctx.textAlign = 'center';
        ctx.fillText(labelText, x, this.canvasSize.height - padding + labelPadding);
      });
    },
    drawProducts() {
      const { ctx, padding, cellSize, checkSize } = this;
      
      ctx.strokeStyle = '#67C23A';
      ctx.lineWidth = 3;
      
      this.gameTypes.forEach((game, i) => {
        this.aiTypes.forEach((ai, j) => {
          if (this.checkProduct(game.value, ai.value)) {
            const x = padding + cellSize.x * (j + 1);
            const y = this.canvasSize.height - padding - cellSize.y * (i + 1);
            
            // 绘制大号对勾
            ctx.beginPath();
            ctx.moveTo(x - checkSize, y);
            ctx.lineTo(x - checkSize/2, y + checkSize/2);
            ctx.lineTo(x + checkSize, y - checkSize);
            ctx.stroke();
          }
        });
      });
    },
    /** 判断是否存在对应产品 */
    checkProduct(gameId, aiId) {
      if (!gameId || !aiId) return false;
      
      return this.products.some(product => {
        const productGameId = Number(product.gameTypeId);
        const productAiId = Number(product.algorithmTypeId);
        return productGameId === Number(gameId) && productAiId === Number(aiId);
      });
    },
    handleResize() {
      this.$nextTick(() => {
        this.initCanvas();
        this.drawMatrix();
      });
    }
  }
};
</script>

<style scoped lang="scss">
.welcome-container {
  padding: 40px;
  background: #fff;
  border-radius: 4px;
  box-shadow: 0 2px 12px 0 rgba(0,0,0,0.1);
  margin: 20px;
  text-align: center;
}

.welcome-title {
  color: #303133;
  font-size: 28px;
  font-weight: bold;
}

.matrix-container {
  padding: 40px;
  background: #fff;
  border-radius: 4px;
  box-shadow: 0 2px 12px 0 rgba(0,0,0,0.1);
  margin: 20px;
}

.matrix-title {
  text-align: center;
  margin-bottom: 30px;
  color: #303133;
  font-size: 24px;
  font-weight: bold;
}

.matrix-canvas {
  display: block;
  margin: 0 auto;
}
</style>


