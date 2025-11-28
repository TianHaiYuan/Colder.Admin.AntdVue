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
      :pagination="false"
      :loading="loading"
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
        <template v-else-if="column.dataIndex === 'icon'">
          <component v-if="record.icon" :is="getIconComponent(record.icon)" />
        </template>
        <template v-else-if="column.dataIndex === 'PermissionValuesText'">
          <template v-for="(item, index) in record.PermissionValues" :key="index">
            <br v-if="index !== 0" />
            {{ item }}
          </template>
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
import * as Icons from '@ant-design/icons-vue'
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
  { title: '菜单名', dataIndex: 'Text', width: '15%' },
  { title: '类型', dataIndex: 'TypeText', width: '5%' },
  { title: '路径', dataIndex: 'Url', width: '25%' },
  { title: '需要权限', dataIndex: 'NeedActionText', width: '10%' },
  { title: '页面权限', dataIndex: 'PermissionValuesText', width: '20%' },
  { title: '图标', dataIndex: 'icon', width: '5%' },
  { title: '排序', dataIndex: 'Sort', width: '5%' },
  { title: '操作', dataIndex: 'action' }
]

const hasSelected = computed(() => selectedRowKeys.value.length > 0)

const getIconComponent = (iconName) => {
  if (!iconName) return null
  const pascalCase = iconName
    .split('-')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join('')
  const componentName = pascalCase + 'Outlined'
  return Icons[componentName] || null
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
    const resJson = await proxy.$http.post('/Base_Manage/Base_Action/GetMenuTreeList', {
      PageIndex: pagination.current,
      PageRows: pagination.pageSize,
      SortField: sorter.value.field || 'Id',
      SortType: sorter.value.order === 'ascend' ? 'asc' : 'desc',
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
  const resJson = await proxy.$http.post('/Base_Manage/Base_Action/DeleteData', ids)
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
  name: 'Base_ActionList'
}
</script>