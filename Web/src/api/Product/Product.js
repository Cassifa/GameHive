import request from '@/utils/request'

// 查询AI产品列表
export function listProduct(query) {
  return request({
    url: '/Product/Product/list',
    method: 'get',
    params: query
  })
}

// 查询AI产品详细
export function getProduct(id) {
  return request({
    url: '/Product/Product/' + id,
    method: 'get'
  })
}

// 新增AI产品
export function addProduct(data) {
  return request({
    url: '/Product/Product',
    method: 'post',
    data: data
  })
}

// 修改AI产品
export function updateProduct(data) {
  return request({
    url: '/Product/Product',
    method: 'put',
    data: data
  })
}

// 删除AI产品
export function delProduct(id) {
  return request({
    url: '/Product/Product/' + id,
    method: 'delete'
  })
}
