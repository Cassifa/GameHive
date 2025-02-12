import request from '@/utils/request'

// 查询AI类型列表
export function listAiType(query) {
  return request({
    url: '/aiSys/aiType/list',
    method: 'get',
    params: query
  })
}

// 查询AI类型详细
export function getAiType(aiId) {
  return request({
    url: '/aiSys/aiType/' + aiId,
    method: 'get'
  })
}

// 新增AI类型
export function addAiType(data) {
  return request({
    url: '/aiSys/aiType',
    method: 'post',
    data: data
  })
}

// 修改AI类型
export function updateAiType(data) {
  return request({
    url: '/aiSys/aiType',
    method: 'put',
    data: data
  })
}

// 删除AI类型
export function delAiType(aiId) {
  return request({
    url: '/aiSys/aiType/' + aiId,
    method: 'delete'
  })
}
