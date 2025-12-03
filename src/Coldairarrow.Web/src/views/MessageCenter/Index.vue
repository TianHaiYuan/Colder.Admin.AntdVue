<template>
  <a-card :bordered="false">
    <a-tabs v-model:activeKey="activeTab">
      <a-tab-pane key="received" tab="我收到的">
        <a-table
          :columns="columns"
          :data-source="list"
          :pagination="pagination"
          :loading="loading"
          row-key="id"
          @change="handleTableChange">
          <template #bodyCell="{ column, record }">
		            <template v-if="column.dataIndex === 'title'">
		              <a @click="openMessage(record)">{{ getTitleText(record) }}</a>
		            </template>
		            <template v-else-if="column.dataIndex === 'content'">
		              {{ getContentText(record) }}
		            </template>
		            <template v-else-if="column.dataIndex === 'eventType'">
		              {{ formatEventType(record) }}
		            </template>
		            <template v-else-if="column.dataIndex === 'senderUserName'">
		              {{ record.SenderUserName || record.senderUserName || '-' }}
		            </template>
		            <template v-else-if="column.dataIndex === 'isRead'">
		              <a-tag :color="isRead(record) ? 'default' : 'blue'">
		                {{ isRead(record) ? '已读' : '未读' }}
		              </a-tag>
		            </template>
		            <template v-else-if="column.dataIndex === 'createdTime'">
		              {{ formatCreatedTime(record.CreatedTime || record.createdTime) }}
		            </template>
          </template>
        </a-table>
      </a-tab-pane>
      <a-tab-pane key="sent" tab="我发出的">
        <a-table
          :columns="columns"
          :data-source="list"
          :pagination="pagination"
          :loading="loading"
	          row-key="id"
          @change="handleTableChange">
          <template #bodyCell="{ column, record }">
		            <template v-if="column.dataIndex === 'title'">
		              <a @click="openMessage(record)">{{ getTitleText(record) }}</a>
		            </template>
		            <template v-else-if="column.dataIndex === 'content'">
		              {{ getContentText(record) }}
		            </template>
		            <template v-else-if="column.dataIndex === 'eventType'">
		              {{ formatEventType(record) }}
		            </template>
		            <template v-else-if="column.dataIndex === 'senderUserName'">
		              {{ record.SenderUserName || record.senderUserName || '-' }}
		            </template>
		            <template v-else-if="column.dataIndex === 'isRead'">
		              <a-tag :color="isRead(record) ? 'default' : 'blue'">
		                {{ isRead(record) ? '已读' : '未读' }}
		              </a-tag>
		            </template>
		            <template v-else-if="column.dataIndex === 'createdTime'">
		              {{ formatCreatedTime(record.CreatedTime || record.createdTime) }}
		            </template>
          </template>
        </a-table>
      </a-tab-pane>
    </a-tabs>
  </a-card>
</template>

<script setup>
import { ref, watch } from 'vue'
import { useRouter } from 'vue-router'
import { message as antdMessage } from 'ant-design-vue'
import { Axios } from '@/utils/plugin/axios-plugin.js'
import OperatorCache from '@/utils/cache/OperatorCache.js'
import config from '@/config/defaultSettings.js'

const router = useRouter()

const activeTab = ref('received')
const list = ref([])
const loading = ref(false)
const pagination = ref({ current: 1, pageSize: 10, total: 0, showTotal: total => `共 ${total} 条` })

// 从 SignalR Hub 地址推导出 NotificationCenter.Api 的 HTTP 根地址
const notificationApiRoot = (() => {
  const hubUrl = config.notificationHubUrl
  if (!hubUrl) return ''
  try {
    const url = new URL(hubUrl)
    return url.origin
  } catch {
    return hubUrl.replace('/hubs/notification', '')
  }
})()

const columns = [
	  { title: '标题', dataIndex: 'title', key: 'title' },
	  { title: '消息内容', dataIndex: 'content', key: 'content' },
	  { title: '类型', dataIndex: 'eventType', key: 'eventType', width: 160 },
	  { title: '发送人', dataIndex: 'senderUserName', key: 'senderUserName', width: 160 },
	  { title: '是否已读', dataIndex: 'isRead', key: 'isRead', width: 120 },
	  { title: '时间', dataIndex: 'createdTime', key: 'createdTime', width: 200 }
	]

const getTitleText = (record) => {
  if (!record) return '-'
	  const title = record.Title || record.title
	  if (title) return title
	  const businessType = record.BusinessType || record.businessType
	  const businessDisplay = businessType === 'Product'
	    ? '产品审批'
	    : (businessType || '单据')
	  const stepName = record.StepName || record.stepName
	  if (stepName) {
	    return `${businessDisplay} - ${stepName}`
	  }
	  return businessDisplay || '-'
}

const getContentText = (record) => {
		  if (!record) return '-'
		  const content = record.Content || record.content
		  if (content) return content
		  const eventType = record.EventType || record.eventType
		  const title = record.Title || record.title
		  const stepName = record.StepName || record.stepName
	  const businessType = record.BusinessType || record.businessType
	  const status = record.Status || record.status
	  const businessDisplay = businessType === 'Product'
	    ? '产品审批'
	    : (businessType || '单据')
	  const statusTextMap = {
	    Pending: '进行中',
	    Approved: '已通过',
	    Rejected: '已驳回',
	    Cancelled: '已取消',
	    Waiting: '未开始'
	  }
	  if (!eventType) {
	    return title || businessDisplay || '-'
	  }
	  if (eventType === 'approval.step.pending') {
	    return `${title || businessDisplay}${stepName ? ' - ' + stepName : ''}`
	  }
	  if (eventType === 'approval.completed') {
	    const statusText = statusTextMap[status] || status || '已结束'
	    return `${title || businessDisplay} 已完成，状态：${statusText}`
	  }
	  if (eventType === 'approval.step.approved') {
	    return `${title || businessDisplay}${stepName ? ' - ' + stepName : ''}`
	  }
	  if (eventType === 'approval.step.rejected') {
	    return `${title || businessDisplay}${stepName ? ' - ' + stepName : ''}`
	  }
	  return `${eventType || ''} - ${title || businessDisplay || ''}`
}

const formatEventType = (record) => {
	  if (!record) return '-'
	  const type = record.EventType || record.eventType
	  const status = record.Status || record.status
	  if (!type) return '-'

  if (type === 'approval.step.pending') {
    return '待审批'
  }
  if (type === 'approval.step.approved') {
    return '审批步骤通过'
  }
  if (type === 'approval.step.rejected') {
    return '审批步骤驳回'
  }
  if (type === 'approval.completed') {
    if (status === 'Approved') return '审批通过'
    if (status === 'Rejected') return '审批驳回'
    return '审批完成'
  }
  return type
}

const isRead = (record) => {
	  if (!record) return false
	  const value = record.IsRead
	  if (typeof value === 'boolean') return value
	  if (typeof value === 'number') return value !== 0
	  if (typeof record.isRead === 'boolean') return record.isRead
	  if (typeof record.isRead === 'number') return record.isRead !== 0
	  return false
}

const formatCreatedTime = (value) => {
  if (!value) return '-'
  try {
    const date = new Date(value)
    if (Number.isNaN(date.getTime())) {
      return value
    }
    const pad = (n) => (n < 10 ? `0${n}` : `${n}`)
    const y = date.getFullYear()
    const m = pad(date.getMonth() + 1)
    const d = pad(date.getDate())
    const hh = pad(date.getHours())
    const mm = pad(date.getMinutes())
    const ss = pad(date.getSeconds())
    return `${y}-${m}-${d} ${hh}:${mm}:${ss}`
  } catch (e) {
    console.error(e)
    return value
  }
}

const loadData = async () => {
  await OperatorCache.init().catch(() => {})
  const userId = OperatorCache.info.Id
  if (!userId) {
    return
  }
  loading.value = true
  try {
    const res = await Axios.get(`${notificationApiRoot}/api/notifications`, {
      params: {
        userId,
        box: activeTab.value,
        page: pagination.value.current,
        pageSize: pagination.value.pageSize
      }
    })
    if (res) {
      list.value = res.items || []
      pagination.value.total = res.total || 0
    }
  } catch (e) {
    console.error(e)
  } finally {
    loading.value = false
  }
}

const handleTableChange = pager => {
  pagination.value.current = pager.current
  pagination.value.pageSize = pager.pageSize
  loadData()
}

const openMessage = async record => {
  try {
    await OperatorCache.init().catch(() => {})
    const userId = OperatorCache.info.Id
	    const id = record?.Id || record?.id
	    if (record && !isRead(record) && id && userId) {
	      await Axios.post(`${notificationApiRoot}/api/notifications/${id}/read`, null, {
        params: { userId }
      })
	      record.IsRead = true
	      record.isRead = true
    }
  } catch (e) {
    console.error(e)
  }

	  const businessType = record.BusinessType || record.businessType
	  const businessId = record.BusinessId || record.businessId
  if (businessType === 'Product' && businessId) {
    router.push({ path: '/Product_Manage/Product/List', query: { id: businessId, fromNotice: '1' } })
  } else {
    antdMessage.info('该消息暂不支持跳转')
  }
}

watch(activeTab, () => {
  pagination.value.current = 1
  loadData()
}, { immediate: true })
</script>

<script>
export default {
  name: 'MessageCenter'
}
</script>
