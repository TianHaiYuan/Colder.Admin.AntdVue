import dayjs from 'dayjs'
import 'dayjs/locale/zh-cn'
dayjs.locale('zh-cn')

// Vue 3 中过滤器被移除，改用函数方式
export function NumberFormat(value) {
  if (!value) {
    return '0'
  }
  return value.toString().replace(/(\d)(?=(?:\d{3})+$)/g, '$1,')
}

export function formatDate(dataStr, pattern = 'YYYY-MM-DD HH:mm:ss') {
  return dayjs(dataStr).format(pattern)
}

// 别名
export const formatDayjs = formatDate
export const formatMoment = formatDate
