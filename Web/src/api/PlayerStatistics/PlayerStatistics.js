import request from '@/utils/request'

// 获取玩家统计信息
export function getPlayerStatistics(userId) {
  return request({
    url: '/PlayerStatistics/PlayerStatistics/' + userId,
    method: 'get'
  })
} 