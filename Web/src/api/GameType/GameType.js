import request from '@/utils/request'

// 查询游戏类型列表
export function listGameType(query) {
  return request({
    url: '/GameType/GameType/list',
    method: 'get',
    params: query
  })
}

// 查询游戏类型详细
export function getGameType(gameId) {
  return request({
    url: '/GameType/GameType/' + gameId,
    method: 'get'
  })
}

// 新增游戏类型
export function addGameType(data) {
  return request({
    url: '/GameType/GameType',
    method: 'post',
    data: data
  })
}

// 修改游戏类型
export function updateGameType(data) {
  return request({
    url: '/GameType/GameType',
    method: 'put',
    data: data
  })
}

// 删除游戏类型
export function delGameType(gameId) {
  return request({
    url: '/GameType/GameType/' + gameId,
    method: 'delete'
  })
}
