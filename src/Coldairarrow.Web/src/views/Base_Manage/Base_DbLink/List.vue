<template>
  <a-card :bordered="false">
    <div class="table-operator">
      <a-button type="primary" @click="hanldleAdd()">
        <template #icon><PlusOutlined /></template>
        新建
      </a-button>
      <a-button
        type="primary"
        @click="handleDelete(selectedRowKeys)"
        :disabled="!hasSelected"
        :loading="loading"
      >
        <template #icon><MinusOutlined /></template>
        删除
      </a-button>
      <a-button type="primary" @click="getDataList()">
        <template #icon><RedoOutlined /></template>
        刷新
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
        <template v-if="column.dataIndex === 'action'">
          <a @click="handleEdit(record.Id)">编辑</a>
          <a-divider type="vertical" />
          <a @click="handleDelete([record.Id])">删除</a>
        </template>
      </template>
    </a-table>

    <EditForm ref="editFormRef" :afterSubmit="getDataList" />
  </a-card>
</template>

<script setup>
import { ref, reactive, computed, onMounted, getCurrentInstance } from 'vue'
import { Modal, message } from 'ant-design-vue'
import { PlusOutlined, MinusOutlined, RedoOutlined } from '@ant-design/icons-vue'
import EditForm from './EditForm.vue'

const { proxy } = getCurrentInstance()

const tableRef = ref(null)
const editFormRef = ref(null)
const data = ref([])
const loading = ref(false)
const selectedRowKeys = ref([])

const pagination = reactive({
  current: 1,
  pageSize: 10,
  total: 0,
  showTotal: (total, range) => `总数:${total} 当前:${range[0]}-${range[1]}`
})

const filters = ref({})
const sorter = ref({ field: 'Id', order: 'asc' })

const columns = [
  { title: '连接名', dataIndex: 'LinkName', width: '10%' },
  { title: '连接字符串', dataIndex: 'ConnectionStr', width: '60%' },
  { title: '数据库类型', dataIndex: 'DbType', width: '10%' },
  { title: '操作', dataIndex: 'action' }
]

const hasSelected = computed(() => selectedRowKeys.value.length > 0)

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
    const resJson = await proxy.$http.post('/Base_Manage/Base_DbLink/GetDataList', {
      PageIndex: pagination.current,
      PageRows: pagination.pageSize,
      SortField: sorter.value.field || 'Id',
      SortType: sorter.value.order,
      ...filters.value
    })

    data.value = resJson.Data
    pagination.total = resJson.Total
  } finally {
    loading.value = false
  }
}

const onSelectChange = (keys) => {
  selectedRowKeys.value = keys
}

const hanldleAdd = () => {
  editFormRef.value?.openForm()
}

const handleEdit = (id) => {
  editFormRef.value?.openForm(id)
}

const handleDelete = (ids) => {
  Modal.confirm({
    title: '确认删除吗?',
    onOk() {
      return submitDelete(ids)
    }
  })
}

const submitDelete = async (ids) => {
  const resJson = await proxy.$http.post('/Base_Manage/Base_DbLink/DeleteData', ids)
  if (resJson.Success) {
    message.success('操作成功!')
    getDataList()
  } else {
    message.error(resJson.Msg)
  }
}

onMounted(() => {
  getDataList()
})
</script>

<script>
export default {
  name: 'Base_DbLinkList'
}
</script>
