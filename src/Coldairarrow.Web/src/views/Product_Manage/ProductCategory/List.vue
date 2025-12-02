<template>
  <a-card :bordered="false">
    <div class="table-operator">
      <a-button type="primary" @click="hanldleAdd()">
        <template #icon><PlusOutlined /></template>
        新建
      </a-button>
      <a-button
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
      :loading="loading"
      :row-selection="{ selectedRowKeys: selectedRowKeys, onChange: onSelectChange }"
      :bordered="true"
      :pagination="false"
      size="small"
      :defaultExpandAllRows="true"
    >
      <template #bodyCell="{ column, record }">
        <template v-if="column.dataIndex === 'Enabled'">
          <a-tag :color="record.Enabled ? 'green' : 'red'">
            {{ record.Enabled ? '启用' : '禁用' }}
          </a-tag>
        </template>
        <template v-if="column.dataIndex === 'action'">
          <a @click="handleEdit(record.Id)">编辑</a>
          <a-divider type="vertical" />
          <a @click="handleDelete([record.Id])">删除</a>
        </template>
      </template>
    </a-table>

    <EditForm ref="editFormRef" :afterSubmit="getDataList" :treeData="data" />
  </a-card>
</template>

<script setup>
import { ref, computed, onMounted, getCurrentInstance } from 'vue'
import { Modal, message } from 'ant-design-vue'
import { PlusOutlined, MinusOutlined } from '@ant-design/icons-vue'
import EditForm from './EditForm.vue'

const { proxy } = getCurrentInstance()

const tableRef = ref(null)
const editFormRef = ref(null)
const data = ref([])
const loading = ref(false)
const selectedRowKeys = ref([])

const columns = [
  { title: '分类名称', dataIndex: 'title', width: '25%' },
  { title: '描述', dataIndex: 'Description', width: '30%' },
  { title: '排序', dataIndex: 'Sort', width: '10%' },
  { title: '状态', dataIndex: 'Enabled', width: '10%' },
  { title: '操作', dataIndex: 'action', width: '25%' }
]

const hasSelected = computed(() => selectedRowKeys.value.length > 0)

	  const getDataList = async () => {
  selectedRowKeys.value = []
  loading.value = true

	    try {
	      const resJson = await proxy.$http.post('/Product_Manage/ProductCategory/GetTreeDataList', {})
    data.value = resJson.Data
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
	    const resJson = await proxy.$http.post('/Product_Manage/ProductCategory/DeleteData', ids)
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
  name: 'ProductCategoryList'
}
</script>

