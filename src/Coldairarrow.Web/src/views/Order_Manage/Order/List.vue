<template>
  <a-card :bordered="false">
    <div class="table-page-search-wrapper">
      <a-form layout="inline">
        <a-row :gutter="16" style="width: 100%">
          <a-col :md="4" :sm="24">
            <a-input v-model:value="queryParam.Keyword" placeholder="订单编号/客户名称" allow-clear />
          </a-col>
          <a-col :md="3" :sm="24">
            <a-select v-model:value="queryParam.Status" placeholder="订单状态" allow-clear>
                <a-select-option :value="0">待确认</a-select-option>
                <a-select-option :value="1">已确认</a-select-option>
                <a-select-option :value="2">已发货</a-select-option>
                <a-select-option :value="3">已完成</a-select-option>
                <a-select-option :value="4">已取消</a-select-option>
              </a-select>
          </a-col>
          <a-col :md="3" :sm="24">
            <a-select v-model:value="queryParam.PaymentStatus" placeholder="支付状态" allow-clear>
                <a-select-option :value="0">未支付</a-select-option>
                <a-select-option :value="1">已支付</a-select-option>
                <a-select-option :value="2">已退款</a-select-option>
              </a-select>
          </a-col>
          <a-col :md="4" :sm="24">
            <a-range-picker v-model:value="dateRange" @change="onDateChange" :placeholder="['支付日期', '支付日期']" />
          </a-col>
          <a-col :md="4" :sm="24">
            <a-space>
              <a-button type="primary" @click="handleSearch">查询</a-button>
              <a-button @click="handleReset">重置</a-button>
            </a-space>
          </a-col>
        </a-row>
      </a-form>
    </div>

    <div class="table-operator">
      <a-button type="primary" @click="hanldleAdd()">
        <template #icon><PlusOutlined /></template>
        新建
      </a-button>
      <a-button type="primary" danger @click="handleDelete(selectedRowKeys)" :disabled="!hasSelected" :loading="loading">
        <template #icon><MinusOutlined /></template>
        删除
      </a-button>
    </div>

    <a-table
      ref="tableRef"
      :columns="columns"
      :row-key="row => row.Id"
      :data-source="data"
      :pagination="pagination"
      :loading="loading"
      @change="handleTableChange"
      :row-selection="{ selectedRowKeys: selectedRowKeys, onChange: onSelectChange }"
      :bordered="true"
      size="small"
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.dataIndex === 'Status'">
          <a-tag :color="getStatusColor(record.Status)">{{ record.StatusText }}</a-tag>
        </template>
        <template v-if="column.dataIndex === 'PaymentStatus'">
          <a-tag :color="getPaymentStatusColor(record.PaymentStatus)">{{ record.PaymentStatusText }}</a-tag>
        </template>
        <template v-if="column.dataIndex === 'action'">
          <a @click="handleEdit(record.Id)">编辑</a>
          <a-divider type="vertical" />
          <a @click="handleDetail(record.Id)">详情</a>
          <a-divider type="vertical" />
          <a @click="handleDelete([record.Id])">删除</a>
        </template>
      </template>
    </a-table>

    <EditForm ref="editFormRef" :afterSubmit="getDataList" />
    <DetailModal ref="detailRef" />
  </a-card>
</template>

<script setup>
import { ref, reactive, computed, onMounted, getCurrentInstance } from 'vue'
import { Modal, message } from 'ant-design-vue'
import { PlusOutlined, MinusOutlined } from '@ant-design/icons-vue'
import EditForm from './EditForm.vue'
import DetailModal from './DetailModal.vue'

const { proxy } = getCurrentInstance()

const tableRef = ref(null)
const editFormRef = ref(null)
const detailRef = ref(null)
const data = ref([])
const loading = ref(false)
const selectedRowKeys = ref([])
const dateRange = ref([])
const queryParam = reactive({ Keyword: '', Status: undefined, PaymentStatus: undefined, StartDate: undefined, EndDate: undefined })

const pagination = reactive({
  current: 1, pageSize: 10, total: 0,
  showTotal: (total, range) => `总数:${total} 当前:${range[0]}-${range[1]}`
})

const filters = ref({})
const sorter = ref({})

const columns = [
  { title: '订单编号', dataIndex: 'OrderNo', width: '12%' },
  { title: '客户名称', dataIndex: 'CustomerName', width: '10%' },
  { title: '客户电话', dataIndex: 'CustomerPhone', width: '10%' },
  { title: '总金额', dataIndex: 'TotalAmount', width: '8%' },
  { title: '订单状态', dataIndex: 'Status', width: '8%' },
  { title: '支付状态', dataIndex: 'PaymentStatus', width: '8%' },
  { title: '创建时间', dataIndex: 'CreateTime', width: '12%', sorter: true },
  { title: '操作', dataIndex: 'action' }
]

const getStatusColor = (status) => {
  const colors = { 0: 'orange', 1: 'blue', 2: 'cyan', 3: 'green', 4: 'red' }
  return colors[status] || 'default'
}

const getPaymentStatusColor = (status) => {
  const colors = { 0: 'orange', 1: 'green', 2: 'red' }
  return colors[status] || 'default'
}

const buildSorts = (srt) => {
  if (srt && srt.field) return [{ Field: srt.field, Type: srt.order === 'ascend' ? 'asc' : 'desc' }]
  return null
}

const hasSelected = computed(() => selectedRowKeys.value.length > 0)

const onDateChange = (dates) => {
  if (dates && dates.length === 2) {
    queryParam.StartDate = dates[0].format('YYYY-MM-DD')
    queryParam.EndDate = dates[1].format('YYYY-MM-DD')
  } else {
    queryParam.StartDate = undefined
    queryParam.EndDate = undefined
  }
}

const handleTableChange = (pag, flt, srt) => {
  pagination.current = pag.current
  pagination.pageSize = pag.pageSize
  filters.value = { ...flt }
  sorter.value = { ...srt }
  getDataList()
}

	    const getDataList = async () => {
  selectedRowKeys.value = []
  loading.value = true
	    try {
	      const resJson = await proxy.$http.post('/Order_Manage/Order/GetDataList', {
      PageIndex: pagination.current, PageRows: pagination.pageSize, Sorts: buildSorts(sorter.value), Search: queryParam, ...filters.value
    })
    data.value = resJson.Data
    pagination.total = resJson.Total
  } finally { loading.value = false }
}

const onSelectChange = (keys) => { selectedRowKeys.value = keys }
const hanldleAdd = () => { editFormRef.value?.openForm() }
const handleEdit = (id) => { editFormRef.value?.openForm(id) }
const handleDetail = (id) => { detailRef.value?.openModal(id) }
const handleSearch = () => { pagination.current = 1; getDataList() }
const handleReset = () => { Object.assign(queryParam, { Keyword: '', Status: undefined, PaymentStatus: undefined, StartDate: undefined, EndDate: undefined }); dateRange.value = []; getDataList() }
const handleDelete = (ids) => { Modal.confirm({ title: '确认删除吗?', onOk() { return submitDelete(ids) } }) }
	    const submitDelete = async (ids) => {
	      const resJson = await proxy.$http.post('/Order_Manage/Order/DeleteData', ids)
  if (resJson.Success) { message.success('操作成功!'); getDataList() } else { message.error(resJson.Msg) }
}

onMounted(() => { getDataList() })
</script>

<script>
export default { name: 'OrderList' }
</script>

