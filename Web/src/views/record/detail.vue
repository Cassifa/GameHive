<template>
  <div class="app-container">
    <div class="game-info" v-if="recordDetail">
      <el-row :gutter="20">
        <el-col :span="6" :xs="0" :sm="6" :md="6" :lg="6" :xl="6">
          <el-card class="info-card">
            <div slot="header" class="card-header">基本信息</div>
            <div class="info-item">
              <span class="label">对局ID:</span>
              <span class="value">{{ recordDetail.record_id }}</span>
            </div>
            <div class="info-item">
              <span class="label">游戏类型:</span>
              <span class="value">{{ recordDetail.game_name }}</span>
            </div>
            <div class="info-item">
              <span class="label">对局类型:</span>
              <span class="value">{{ getGameModeText(recordDetail.game_mode) }}</span>
            </div>
            <div class="info-item" v-if="recordDetail.game_mode === 0">
              <span class="label">算法名称:</span>
              <span class="value">{{ recordDetail.algorithm_name || '无' }}</span>
            </div>
            <div class="info-item">
              <span class="label">对局时间:</span>
              <span class="value">{{ formatTime(recordDetail.record_time) }}</span>
            </div>
            <div class="info-item">
              <span class="label">赢家:</span>
              <span class="value">{{ getWinnerText(recordDetail.winner) }}</span>
            </div>
          </el-card>
        </el-col>
        
        <el-col :span="12" :xs="14" :sm="12" :md="12" :lg="12" :xl="12">
          <el-card class="board-card">
            <div slot="header">
              <div class="board-header">
                <span class="card-header">对局回放</span>
                <div class="step-info">
                  步骤: {{ currentStep }} / {{ totalSteps }}
                </div>
              </div>
            </div>
            <div class="board-container">
              <canvas 
                ref="gameCanvas" 
                :width="boardSize" 
                :height="boardSize"
                class="game-canvas">
              </canvas>
            </div>
            <div class="play-controls">
              <el-button-group>
                <el-button @click="resetToStart" icon="el-icon-d-arrow-left" size="small">开始</el-button>
                <el-button @click="prevStep" icon="el-icon-arrow-left" size="small">上一步</el-button>
                <el-button @click="playPause" :icon="isPlaying ? 'el-icon-video-pause' : 'el-icon-video-play'" size="small">
                  {{ isPlaying ? '暂停' : '播放' }}
                </el-button>
                <el-button @click="nextStep" icon="el-icon-arrow-right" size="small">下一步</el-button>
                <el-button @click="goToEnd" icon="el-icon-d-arrow-right" size="small">结束</el-button>
              </el-button-group>
              <div class="speed-control">
                <span>速度:</span>
                <el-select v-model="playSpeed" size="small" style="width: 80px">
                  <el-option label="快" :value="500"></el-option>
                  <el-option label="中" :value="1000"></el-option>
                  <el-option label="慢" :value="2000"></el-option>
                </el-select>
              </div>
            </div>
          </el-card>
        </el-col>
        
        <el-col :span="6" :xs="10" :sm="6" :md="6" :lg="6" :xl="6">
          <el-card class="moves-card">
            <div slot="header" class="card-header">对局记录</div>
            <div class="moves-list">
              <div class="player-info">
                <div class="player-item">
                  <span class="player-label">先手玩家:</span>
                  <span class="player-name">{{ recordDetail.first_player_name }}</span>
                </div>
                <div class="player-item">
                  <span class="player-label">后手玩家:</span>
                  <span class="player-name">{{ recordDetail.second_player_name }}</span>
                </div>
              </div>
              
              <div class="moves-steps">
                <h4>步骤列表</h4>
                <div class="steps-container">
                  <div 
                    v-for="(move, index) in movesList" 
                    :key="index"
                    :class="['step-item', { 'active': index + 1 === currentStep, 'played': index + 1 <= currentStep }]"
                    @click="goToStep(index + 1)"
                  >
                    <div class="step-number">{{ move.step }}</div>
                    <div class="step-player" :class="move.color">{{ move.player }}</div>
                    <div class="step-position">{{ move.position }}</div>
                  </div>
                  <div v-if="movesList.length === 0" class="no-moves">
                    暂无步骤数据
                  </div>
                </div>
              </div>
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>
    
    <div v-else class="loading" v-loading="loading" element-loading-text="加载中...">
    </div>
  </div>
</template>

<script>
import { getRecordDetail } from "@/api/record/record";
import { GameEngine } from "@/components/GameBoard/GameEngine.js";
import { GameBoard } from "@/components/GameBoard/GameBoard.js";
import { GameBoardInfo } from "@/components/GameBoard/GameBoardInfo.js";

export default {
  name: "RecordDetail",
  data() {
    return {
      recordId: null,
      recordDetail: null,
      loading: true,
      gameEngine: null,
      gameBoard: null,
      boardSize: 600,
      // 播放控制
      currentStep: 0,
      totalSteps: 0,
      isPlaying: false,
      playSpeed: 1000, // 毫秒
      playTimer: null,
      movesList: [] // 解析后的步骤列表
    };
  },
  mounted() {
    // 获取路由参数中的recordId
    this.recordId = this.$route.query.recordId;
    
    if (this.recordId) {
      this.getRecordDetailData();
    } else {
      this.loading = false;
    }
  },
  beforeDestroy() {
    if (this.gameEngine) {
      this.gameEngine.stop();
      this.gameEngine.clearObjects();
    }
  },
  watch: {
    '$route'(to, from) {
      // 路由变化时重新加载数据
      if (to.query.recordId !== from.query.recordId) {
        this.recordId = to.query.recordId;
        this.resetData();
        if (this.recordId) {
          this.getRecordDetailData();
        }
      }
    }
  },
  methods: {
    /** 重置数据 */
    resetData() {
      this.loading = true;
      this.recordDetail = null;
      this.currentStep = 0;
      this.totalSteps = 0;
      this.isPlaying = false;
      this.movesList = [];
      if (this.playTimer) {
        clearInterval(this.playTimer);
        this.playTimer = null;
      }
      if (this.gameEngine) {
        this.gameEngine.stop();
        this.gameEngine.clearObjects();
        this.gameEngine = null;
        this.gameBoard = null;
      }
      // 强制清空Canvas
      if (this.$refs.gameCanvas) {
        const ctx = this.$refs.gameCanvas.getContext('2d');
        ctx.clearRect(0, 0, this.boardSize, this.boardSize);
      }
    },

    /** 获取对局详情数据 */
    getRecordDetailData() {
      this.loading = true;
      getRecordDetail(this.recordId).then(response => {
        this.recordDetail = response.data;
        this.loading = false;
        this.parseMovesList();
        this.$nextTick(() => {
          this.initGameBoard();
        });
      }).catch(error => {
        this.loading = false;
        this.$message.error('获取对局详情失败');
      });
    },

    /** 解析步骤列表 */
    parseMovesList() {
      if (!this.recordDetail) return;
      
      const firstPlayerPieces = this.parsePlayerPieces(this.recordDetail.first_player_pieces);
      const secondPlayerPieces = this.parsePlayerPieces(this.recordDetail.second_player_pieces);
      
      this.movesList = [];
      
      // 交替添加先手和后手的步骤
      const maxLength = Math.max(firstPlayerPieces.length, secondPlayerPieces.length);
      
      for (let i = 0; i < maxLength; i++) {
        // 先添加先手的步骤
        if (i < firstPlayerPieces.length) {
          this.movesList.push({
            step: this.movesList.length + 1,
            player: '先手',
            position: firstPlayerPieces[i].original,
            color: 'black',
            isFirstPlayer: true
          });
        }
        
        // 再添加后手的步骤
        if (i < secondPlayerPieces.length) {
          this.movesList.push({
            step: this.movesList.length + 1,
            player: '后手', 
            position: secondPlayerPieces[i].original,
            color: 'white',
            isFirstPlayer: false
          });
        }
      }

      
      this.totalSteps = this.movesList.length;
      this.currentStep = this.totalSteps; // 默认显示最终状态
    },

    /** 解析棋子位置字符串 */
    parsePlayerPieces(piecesString) {
      if (!piecesString) return [];
      
      const pieces = [];
      
      try {
        // 尝试解析JSON数组格式
        const jsonData = JSON.parse(piecesString);
        if (Array.isArray(jsonData)) {
          jsonData.forEach(pos => {
            if (pos.x !== undefined && pos.y !== undefined) {
              // JSON格式：{x: 行索引, y: 列索引}
              const row = pos.x;
              const col = pos.y;
              const colChar = String.fromCharCode('A'.charCodeAt(0) + pos.y);
              const original = `(${colChar},${pos.x})`;
              pieces.push({ 
                row, 
                col, 
                original 
              });
            }
          });
          return pieces;
        }
              } catch (e) {
          // 不是JSON格式，使用传统解析方式
        }
      
      // 传统格式解析：A0,B1,C2
      const positions = piecesString.split(',');
      positions.forEach(pos => {
        const trimmed = pos.trim();
        if (trimmed) {
          const col = trimmed.charCodeAt(0) - 'A'.charCodeAt(0);
          const row = (this.recordDetail.board_size || 15) - 1 - parseInt(trimmed.substring(1));
          const colChar = trimmed.charAt(0);
          const rowNum = trimmed.substring(1);
          pieces.push({ 
            row, 
            col, 
            original: `(${colChar},${rowNum})` 
          });
        }
              });
        
        return pieces;
    },

    /** 初始化游戏棋盘 */
    initGameBoard() {
      if (!this.$refs.gameCanvas || !this.recordDetail) return;

      // 确保先清理旧的游戏引擎
      if (this.gameEngine) {
        this.gameEngine.stop();
        this.gameEngine.clearObjects();
        this.gameEngine = null;
        this.gameBoard = null;
      }

      const canvas = this.$refs.gameCanvas;
      
      // 清空Canvas
      const ctx = canvas.getContext('2d');
      ctx.clearRect(0, 0, this.boardSize, this.boardSize);
      
      // 根据游戏类型确定棋盘大小和落子方式
      const boardSize = this.recordDetail.board_size || 15;
      const isCenter = this.recordDetail.is_cell_center === true || this.recordDetail.is_cell_center === 1;
      
      // 创建棋盘信息
      const boardInfo = new GameBoardInfo(boardSize, isCenter);
      
      // 创建游戏引擎
      this.gameEngine = new GameEngine(canvas);
      
      // 直接使用原始JSON数据
      const currentMoves = {
        first_player_pieces: this.recordDetail.first_player_pieces,
        second_player_pieces: this.recordDetail.second_player_pieces
      };
      
      // 创建棋盘对象
      this.gameBoard = new GameBoard(ctx, boardInfo, currentMoves);
      
      // 添加到游戏引擎
      this.gameEngine.addObject(this.gameBoard);
      
      // 启动游戏引擎
      this.gameEngine.start();
    },

    /** 获取对局类型文本 */
    getGameModeText(gameMode) {
      const gameModeMap = {
        0: '本地对战',
        1: '与大模型对战', 
        2: '联机对战'
      };
      return gameModeMap[gameMode] || '未知';
    },

    /** 获取赢家文本 */
    getWinnerText(winner) {
      const winnerMap = {
        0: '先手获胜',
        1: '后手获胜',
        2: '平局',
        3: '游戏终止'
      };
      return winnerMap[winner] || '未知';
    },

    /** 格式化时间 */
    formatTime(timeStr) {
      if (!timeStr) return '未知';
      const date = new Date(timeStr);
      return date.toLocaleString('zh-CN');
    },

    /** 播放控制方法 */
    playPause() {
      if (this.isPlaying) {
        this.pausePlay();
      } else {
        this.startPlay();
      }
    },

    startPlay() {
      if (this.currentStep >= this.totalSteps) {
        this.currentStep = 0;
      }
      this.isPlaying = true;
      this.playTimer = setInterval(() => {
        this.nextStep();
        if (this.currentStep >= this.totalSteps) {
          this.pausePlay();
        }
      }, this.playSpeed);
    },

    pausePlay() {
      this.isPlaying = false;
      if (this.playTimer) {
        clearInterval(this.playTimer);
        this.playTimer = null;
      }
    },

    nextStep() {
      if (this.currentStep < this.totalSteps) {
        this.currentStep++;
        this.updateBoard();
      }
    },

    prevStep() {
      if (this.currentStep > 0) {
        this.currentStep--;
        this.updateBoard();
      }
    },

    resetToStart() {
      this.pausePlay();
      this.currentStep = 0;
      this.updateBoard();
    },

    goToEnd() {
      this.pausePlay();
      this.currentStep = this.totalSteps;
      this.updateBoard();
    },

    goToStep(step) {
      this.pausePlay();
      this.currentStep = step;
      this.updateBoard();
    },

    updateBoard() {
      if (this.gameBoard) {
        // 创建当前步骤的数据
        const currentMoves = this.getCurrentStepMovesData();
        this.gameBoard.setMoves(currentMoves);
      }
    },

    getCurrentStepMovesData() {
      // 解析原始JSON数据
      const firstPlayerPieces = this.parsePlayerPieces(this.recordDetail.first_player_pieces);
      const secondPlayerPieces = this.parsePlayerPieces(this.recordDetail.second_player_pieces);
      
      // 根据当前步骤筛选棋子
      const currentFirstPieces = firstPlayerPieces.slice(0, Math.ceil(this.currentStep / 2));
      const currentSecondPieces = secondPlayerPieces.slice(0, Math.floor(this.currentStep / 2));
      
      // 转换回JSON格式
      const result = {
        first_player_pieces: JSON.stringify(currentFirstPieces.map(p => ({x: p.row, y: p.col}))),
        second_player_pieces: JSON.stringify(currentSecondPieces.map(p => ({x: p.row, y: p.col})))
      };
      

      return result;
    }
  }
};
</script>

<style scoped>
.app-container {
  padding: 20px;
}

.info-card, .board-card, .moves-card {
  height: 100%;
}

.card-header {
  text-align: center;
  font-weight: 600;
  font-size: 16px;
}

.info-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 12px;
  padding: 8px 12px;
  border-bottom: 1px solid #f0f0f0;
}

.info-item:last-child {
  border-bottom: none;
  margin-bottom: 0;
}

.label {
  font-weight: 600;
  color: #606266;
  min-width: 80px;
  text-align: left;
  flex-shrink: 0;
}

.value {
  color: #303133;
  font-weight: 500;
  flex: 1;
  text-align: center;
  margin-left: 20px;
}

.board-container {
  display: flex;
  justify-content: center;
  align-items: center;
  padding: 20px;
}

.game-canvas {
  border: 2px solid #dcdfe6;
  border-radius: 8px;
  background-color: #fff;
}

.moves-list {
  max-height: 500px;
  overflow-y: auto;
}

.player-info {
  margin-bottom: 16px;
  padding-bottom: 12px;
  border-bottom: 2px solid #f0f0f0;
}

.player-item {
  display: flex;
  justify-content: space-between;
  align-items: center;
  margin-bottom: 6px;
  padding: 4px 8px;
  background-color: #fafafa;
  border-radius: 4px;
}

.player-label {
  font-weight: 600;
  color: #606266;
  font-size: 12px;
  text-align: left;
  flex-shrink: 0;
}

.player-name {
  color: #303133;
  font-weight: 500;
  font-size: 12px;
  text-align: center;
  flex: 1;
  margin-left: 10px;
}

.board-header {
  display: flex;
  justify-content: space-between;
  align-items: center;
}

.board-header .card-header {
  flex: 1;
  text-align: center;
}

.step-info {
  font-size: 14px;
  color: #909399;
  font-weight: normal;
}

.play-controls {
  display: flex;
  justify-content: space-between;
  align-items: center;
  padding: 15px 20px;
  border-top: 1px solid #f0f0f0;
}

.speed-control {
  display: flex;
  align-items: center;
  gap: 8px;
  font-size: 14px;
  color: #606266;
}

.moves-steps {
  margin-top: 15px;
}

.moves-steps h4 {
  margin: 0 0 10px 0;
  color: #409eff;
  font-size: 14px;
  font-weight: 600;
}

.steps-container {
  max-height: 320px;
  overflow-y: auto;
  padding-right: 4px;
}

.steps-container::-webkit-scrollbar {
  width: 6px;
}

.steps-container::-webkit-scrollbar-track {
  background: #f1f1f1;
  border-radius: 3px;
}

.steps-container::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 3px;
}

.steps-container::-webkit-scrollbar-thumb:hover {
  background: #a8a8a8;
}

.step-item {
  display: flex;
  align-items: center;
  padding: 6px 8px;
  margin-bottom: 2px;
  border-radius: 4px;
  cursor: pointer;
  transition: all 0.3s;
  border: 1px solid transparent;
  font-size: 12px;
}

.step-item:hover {
  background-color: #f5f7fa;
}

.step-item.played {
  background-color: #f0f9ff;
  border-color: #e1f5fe;
}

.step-item.active {
  background-color: #409eff;
  color: white;
  border-color: #409eff;
}

.step-number {
  min-width: 24px;
  font-weight: 600;
  font-size: 11px;
  text-align: center;
}

.step-player {
  min-width: 32px;
  font-size: 11px;
  font-weight: 500;
  text-align: center;
}

.step-player.black {
  color: #303133;
  font-weight: 600;
}

.step-player.white {
  color: #606266;
  font-weight: 600;
}

.step-item.active .step-player {
  color: white;
}

.step-position {
  flex: 1;
  text-align: center;
  font-family: 'Courier New', monospace;
  font-size: 11px;
  font-weight: 600;
  min-width: 50px;
}

.no-moves {
  text-align: center;
  color: #909399;
  padding: 20px;
  font-size: 14px;
}

.loading {
  display: flex;
  justify-content: center;
  align-items: center;
  height: 400px;
}

/* 响应式设计 */
@media (max-width: 1200px) {
  .game-canvas {
    width: 100%;
    max-width: 500px;
    height: auto;
  }
}

@media (max-width: 992px) {
  .board-container {
    padding: 15px;
  }
  
  .game-canvas {
    max-width: 450px;
  }
  
  .play-controls {
    flex-direction: column;
    gap: 10px;
    align-items: center;
  }
  
  .speed-control {
    margin-top: 5px;
  }
}

@media (max-width: 768px) {
  .app-container {
    padding: 10px;
  }
  
  .board-container {
    padding: 10px;
  }
  
  .game-canvas {
    max-width: 350px;
  }
  
  .moves-card .el-card__body {
    padding: 15px;
  }
  
  .player-info {
    margin-bottom: 12px;
  }
  
  .steps-container {
    max-height: 250px;
  }
}

@media (max-width: 576px) {
  .app-container {
    padding: 5px;
  }
  
  .game-canvas {
    max-width: 300px;
  }
  
  .play-controls {
    padding: 10px;
  }
  
  .play-controls .el-button-group .el-button {
    padding: 5px 8px;
    font-size: 12px;
  }
  
  .board-header .step-info {
    font-size: 12px;
  }
}
</style> 