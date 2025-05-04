import request from '@/utils/request'

// 查询算法类型列表
export function listAlgorithm(query) {
  return request({
    url: '/Algorithm/Algorithm/list',
    method: 'get',
    params: query
  })
}

// 查询算法类型详细
export function getAlgorithm(algorithmId) {
  return request({
    url: '/Algorithm/Algorithm/' + algorithmId,
    method: 'get'
  })
}

// 新增算法类型
export function addAlgorithm(data) {
  return request({
    url: '/Algorithm/Algorithm',
    method: 'post',
    data: data
  })
}

// 修改算法类型
export function updateAlgorithm(data) {
  return request({
    url: '/Algorithm/Algorithm',
    method: 'put',
    data: data
  })
}

// 删除算法类型
export function delAlgorithm(algorithmId) {
  return request({
    url: '/Algorithm/Algorithm/' + algorithmId,
    method: 'delete'
  })
}

// 获取算法类型下拉选项
export function listAlgorithmOptions() {
  return request({
    url: '/Algorithm/Algorithm/options',
    method: 'get'
  })
}
