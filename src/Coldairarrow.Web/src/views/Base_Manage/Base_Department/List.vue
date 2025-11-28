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
      v-if="data && data.length"
      ref="tableRef"
      :columns="columns"
      :row-key="row => row.Id"
      :data-source="data"
      :loading="loading"
      :pagination="false"
      @change="handleTableChange"
      :row-selection="{ selectedRowKeys: selectedRowKeys, onChange: onSelectChange }"
      :bordered="true"
      :default-expand-all-rows="true"
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
const sorter = ref({})

const columns = [
  { title: '部门名', dataIndex: 'Text', width: '20%' },
  { title: '操作', dataIndex: 'action' }
]

// 构建排序参数
const buildSorts = (srt) => {
  if (srt && srt.field) {
    return [{ Field: srt.field, Type: srt.order === 'ascend' ? 'asc' : 'desc' }]
  }
  return null
}

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
    const resJson = await proxy.$http.post('/Base_Manage/Base_Department/GetTreeDataList', {
      PageIndex: pagination.current,
      PageRows: pagination.pageSize,
      Sorts: buildSorts(sorter.value),
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
  const resJson = await proxy.$http.post('/Base_Manage/Base_Department/DeleteData', ids)
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
  name: 'Base_DepartmentList'
}
</script>