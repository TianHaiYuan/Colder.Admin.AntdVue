<template>
  <a-card :bordered="false">
    <div class="table-page-search-wrapper">
      <a-form layout="inline">
        <a-row :gutter="16" style="width: 100%">
          <a-col :md="6" :sm="24">
            <a-form-item label="关键字">
              <a-input v-model:value="queryParam.keyword" placeholder="" allow-clear />
            </a-form-item>
          </a-col>
          <a-col :md="6" :sm="24">
            <a-space>
              <a-button type="primary" @click="handleSearch">查询</a-button>
              <a-button @click="handleReset">重置</a-button>
            </a-space>
          </a-col>
        </a-row>
      </a-form>
    </div>

    <div class="table-operator">
      <a-button v-if="hasPerm('Base_User.Add')" type="primary" @click="hanldleAdd()">
        <template #icon><PlusOutlined /></template>
        新建
      </a-button>
      <a-button
        v-if="hasPerm('Base_User.Delete')"
        type="primary"
        danger
        @click="handleDelete(selectedRowKeys)"
        :disabled="!hasSelected"
        :loading="loading"
      >
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
        <template v-if="column.dataIndex === 'action'">
          <template v-if="hasPerm('Base_User.Edit')">
            <a @click="handleEdit(record.Id)">编辑</a>
            <a-divider type="vertical" />
          </template>
          <template v-if="hasPerm('Base_User.Add')">
            <a @click="handleCopy(record.Id)">复制</a>
            <a-divider type="vertical" />
          </template>
          <a v-if="hasPerm('Base_User.Delete')" @click="handleDelete([record.Id])">删除</a>
        </template>
      </template>
    </a-table>

    <EditForm ref="editFormRef" :afterSubmit="getDataList" />
  </a-card>
</template>

<script setup>
import { ref, reactive, computed, onMounted, getCurrentInstance } from 'vue'
import { Modal, message } from 'ant-design-vue'
import { PlusOutlined, MinusOutlined } from '@ant-design/icons-vue'
import EditForm from './EditForm.vue'

const { proxy } = getCurrentInstance()

const tableRef = ref(null)
const editFormRef = ref(null)
const data = ref([])
const loading = ref(false)
const selectedRowKeys = ref([])
const queryParam = reactive({ keyword: '' })

const pagination = reactive({
  current: 1,
  pageSize: 10,
  total: 0,
  showTotal: (total, range) => `总数:${total} 当前:${range[0]}-${range[1]}`
})

const filters = ref({})
const sorter = ref({})

const columns = [
  { title: '用户名', dataIndex: 'UserName', width: '10%' },
  { title: '姓名', dataIndex: 'RealName', width: '10%' },
  { title: '性别', dataIndex: 'SexText', width: '5%' },
  { title: '出生日期', dataIndex: 'BirthdayText', width: '10%' },
  { title: '所属部门', dataIndex: 'DepartmentName', width: '10%' },
  { title: '所属角色', dataIndex: 'RoleNames', width: '20%' },
  { title: '创建时间', dataIndex: 'CreateTime', width: '12%', sorter: true },
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

const hasPerm = (perm) => {
  return proxy.$hasPerm(perm)
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
    const resJson = await proxy.$http.post('/Base_Manage/Base_User/GetDataList', {
      PageIndex: pagination.current,
      PageRows: pagination.pageSize,
      Sorts: buildSorts(sorter.value),
      Search: queryParam,
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

  const handleCopy = (id) => {
    editFormRef.value?.openForm(id, true)
  }

const handleSearch = () => {
  pagination.current = 1
  getDataList()
}

const handleReset = () => {
  queryParam.keyword = ''
  getDataList()
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
  const resJson = await proxy.$http.post('/Base_Manage/Base_User/DeleteData', ids)
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
  name: 'Base_UserList'
}
</script>