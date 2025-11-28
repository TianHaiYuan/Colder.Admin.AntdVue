<template>
  <a-spin :spinning="loading">
    <div class="table-operator">
      <a-button class="editable-add-btn" type="default" @click="handleAdd">
        <template #icon><PlusOutlined /></template>
        添加权限
      </a-button>
    </div>
    <a-table :columns="columns" :data-source="data" bordered size="small" :pagination="false" :row-key="row => row.key">
      <template #bodyCell="{ column, record, text }">
        <template v-if="column.dataIndex === 'Name' || column.dataIndex === 'Value'">
          <a-input
            v-if="record.editable"
            style="margin: -5px 0"
            :value="text"
            @change="e => handleChange(e.target.value, record.key, column.dataIndex)"
          />
          <template v-else>{{ text }}</template>
        </template>
        <template v-else-if="column.dataIndex === 'operation'">
          <div class="editable-row-operations">
            <span v-if="record.editable">
              <a @click="save(record.key)">保存</a>
              <br />
              <a-popconfirm title="确认取消吗?" @confirm="cancel(record.key)">
                <a>取消</a>
              </a-popconfirm>
            </span>
            <span v-else>
              <a @click="edit(record.key)">编辑</a>
              <a-popconfirm v-if="data.length" title="确认删除吗?" @confirm="onDelete(record.key)">
                <a href="javascript:;">删除</a>
              </a-popconfirm>
            </span>
          </div>
        </template>
      </template>
    </a-table>
  </a-spin>
</template>

<script setup>
import { ref, getCurrentInstance } from 'vue'
import { message } from 'ant-design-vue'
import { PlusOutlined } from '@ant-design/icons-vue'
import { v4 as uuidv4 } from 'uuid'

const { proxy } = getCurrentInstance()

const data = ref([])
const loading = ref(false)
const parentId = ref(null)
const cacheData = ref([])

const columns = [
  { title: '权限名', dataIndex: 'Name', width: '30%' },
  { title: '权限值(唯一)', dataIndex: 'Value', width: '50%' },
  { title: '操作', dataIndex: 'operation' }
]

const handleChange = (value, key, column) => {
  const target = data.value.find(item => key === item.key)
  if (target) {
    target[column] = value
  }
}

const edit = (key) => {
  const target = data.value.find(item => key === item.key)
  if (target) {
    target.editable = true
  }
}

const save = (key) => {
  const target = data.value.find(item => key === item.key)
  if (target) {
    delete target.editable
    resetCache(data.value)
  }
}

const cancel = (key) => {
  const target = data.value.find(item => key === item.key)
  if (target) {
    const cached = cacheData.value.find(item => key === item.key)
    if (cached) {
      Object.assign(target, cached)
    }
    delete target.editable
  }
}

const onDelete = (key) => {
  data.value = data.value.filter(item => item.key !== key)
}

const handleAdd = () => {
  const newData = {
    key: uuidv4(),
    Name: '权限名',
    Value: '权限值',
    Type: 2,
    ParentId: parentId.value
  }
  data.value = [...data.value, newData]
}

const getPermissionList = () => {
  return data.value
}

const handleSave = async () => {
  loading.value = true
  try {
    const resJson = await proxy.$http.post('/Base_Manage/Base_Action/SavePermission', {
      parentId: parentId.value,
      permissionListJson: JSON.stringify(data.value)
    })
    if (resJson.Success) {
      message.success('权限设置成功')
      getDataList()
    } else {
      message.error('操作失败')
    }
  } finally {
    loading.value = false
  }
}

const resetCache = (dataSource) => {
  cacheData.value = dataSource.map(item => ({ ...item }))
}

const getDataList = async () => {
  loading.value = true
  try {
    const resJson = await proxy.$http.post('/Base_Manage/Base_Action/GetPermissionList', {
      parentId: parentId.value
    })
    resJson.Data.forEach(x => (x['key'] = uuidv4()))
    data.value = resJson.Data
    resetCache(data.value)
  } finally {
    loading.value = false
  }
}

const init = (id) => {
  parentId.value = id
  data.value = []
  if (parentId.value) {
    getDataList()
  }
}

defineExpose({
  init,
  getPermissionList
})
</script>

<script>
export default {
  name: 'PermissionList'
}
</script>

<style scoped>
.editable-row-operations a {
  margin-right: 8px;
}
.editable-add-btn {
  margin-bottom: 8px;
}
</style>
