import request from '@/utils/request'

// 查询天梯排行列表
export function listRanking(query) {
  return request({
    url: '/ranking/ranking/list',
    method: 'get',
    params: query
  })
}

// 查询天梯排行详细
export function getRanking(userId) {
  return request({
    url: '/ranking/ranking/' + userId,
    method: 'get'
  })
}

// 新增天梯排行
export function addRanking(data) {
  return request({
    url: '/ranking/ranking',
    method: 'post',
    data: data
  })
}

// 修改天梯排行
export function updateRanking(data) {
  return request({
    url: '/ranking/ranking',
    method: 'put',
    data: data
  })
}

// 删除天梯排行
export function delRanking(userId) {
  return request({
    url: '/ranking/ranking/' + userId,
    method: 'delete'
  })
}
