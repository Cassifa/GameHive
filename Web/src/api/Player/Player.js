import request from '@/utils/request'

// 查询玩家列表
export function listPlayer(query) {
  return request({
    url: '/Player/Player/list',
    method: 'get',
    params: query
  })
}

// 查询玩家详细
export function getPlayer(userId) {
  return request({
    url: '/Player/Player/' + userId,
    method: 'get'
  })
}

// 新增玩家
export function addPlayer(data) {
  return request({
    url: '/Player/Player',
    method: 'post',
    data: data
  })
}

// 修改玩家
export function updatePlayer(data) {
  return request({
    url: '/Player/Player',
    method: 'put',
    data: data
  })
}

// 删除玩家
export function delPlayer(userId) {
  return request({
    url: '/Player/Player/' + userId,
    method: 'delete'
  })
}
