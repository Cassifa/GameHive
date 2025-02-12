import request from '@/utils/request'

// 查询AI-Game具体产品列表
export function listAiProduct(query) {
  return request({
    url: '/aiSys/aiProduct/list',
    method: 'get',
    params: query
  })
}

// 查询AI-Game具体产品详细
export function getAiProduct(id) {
  return request({
    url: '/aiSys/aiProduct/' + id,
    method: 'get'
  })
}

// 新增AI-Game具体产品
export function addAiProduct(data) {
  return request({
    url: '/aiSys/aiProduct',
    method: 'post',
    data: data
  })
}

// 修改AI-Game具体产品
export function updateAiProduct(data) {
  return request({
    url: '/aiSys/aiProduct',
    method: 'put',
    data: data
  })
}

// 删除AI-Game具体产品
export function delAiProduct(id) {
  return request({
    url: '/aiSys/aiProduct/' + id,
    method: 'delete'
  })
}
