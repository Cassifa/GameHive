import request from '@/utils/request'

// 查询对局记录列表
export function listRecord(query) {
  return request({
    url: '/Record/Record/list',
    method: 'get',
    params: query
  })
}

// 查询对局记录详细
export function getRecord(recordId) {
  return request({
    url: '/Record/Record/' + recordId,
    method: 'get'
  })
}

// 新增对局记录
export function addRecord(data) {
  return request({
    url: '/Record/Record',
    method: 'post',
    data: data
  })
}

// 修改对局记录
export function updateRecord(data) {
  return request({
    url: '/Record/Record',
    method: 'put',
    data: data
  })
}

// 删除对局记录
export function delRecord(recordId) {
  return request({
    url: '/Record/Record/' + recordId,
    method: 'delete'
  })
}

// 导出对局记录
export function exportRecord(query) {
  return request({
    url: '/Record/Record/export',
    method: 'post',
    data: query,
    responseType: 'blob'
  })
}

// 获取对局详情（包含对局记录和游戏类型信息）
export function getRecordDetail(recordId) {
  return request({
    url: '/Record/Record/detail/' + recordId,
    method: 'get'
  })
}
