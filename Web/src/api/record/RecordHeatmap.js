import request from '@/utils/request'

// 获取对局记录热力图数据
export function getRecordHeatmapData(params) {
  // 热力图数据参数:
  // gameTypeId: 游戏类型ID (可选)
  // gameMode: 游戏模式（0-本地对战，1-与大模型对战，2-联机对战） (可选)
  // algorithmId: 算法ID (可选)
  // winner: 赢家 (可选)
  // playerName: 玩家名称，用于模糊匹配 (可选)
  // 注意：用户ID由后端自动获取，无需传递
  return request({
    url: '/Record/Record/heatmap',
    method: 'get',
    params
  })
} 