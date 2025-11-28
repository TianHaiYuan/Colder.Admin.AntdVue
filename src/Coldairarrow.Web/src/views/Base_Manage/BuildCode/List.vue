<template>
  <a-card :bordered="false">
    <div class="table-page-search-wrapper">
      <a-form layout="inline">
        <a-row :gutter="16" style="width: 100%">
          <a-col :md="8" :sm="24">
            <a-form-item label="选择数据库">
              <a-select v-model:value="linkId" @change="onLinkChange" style="width: 200px">
                <a-select-option v-for="item in dbs" :key="item.Id" :value="item.Id">{{ item.LinkName }}</a-select-option>
              </a-select>
            </a-form-item>
          </a-col>
        </a-row>
      </a-form>
    </div>

    <div class="table-operator">
      <a-button type="primary" @click="init()">
        <template #icon><RedoOutlined /></template>
        刷新
      </a-button>
      <a-button
        type="primary"
        @click="openForm()"
        :disabled="!hasSelected"
        :loading="loading"
      >
        <template #icon><PlusOutlined /></template>
        生成代码
      </a-button>
    </div>

    <a-table
      ref="tableRef"
      :columns="columns"
      :row-key="row => row.TableName"
      :data-source="data"
      :pagination="false"
      :loading="loading"
      :row-selection="{ selectedRowKeys: selectedRowKeys, onChange: onSelectChange }"
      :bordered="true"
      size="small"
    />

    <EditForm ref="editFormRef" />
  </a-card>
</template>

<script setup>
import { ref, computed, onMounted, getCurrentInstance } from 'vue'
import { RedoOutlined, PlusOutlined } from '@ant-design/icons-vue'
import EditForm from './EditForm.vue'

const { proxy } = getCurrentInstance()

const tableRef = ref(null)
const editFormRef = ref(null)
const data = ref([])
const loading = ref(false)
const selectedRowKeys = ref([])
const dbs = ref([])
const linkId = ref('')

const columns = [
  { title: '表名', dataIndex: 'TableName', width: '20%' },
  { title: '描述', dataIndex: 'Description', width: '20%' },
  { title: '', dataIndex: 'action' }
]

const hasSelected = computed(() => selectedRowKeys.value.length > 0)

const init = async () => {
  const resJson = await proxy.$http.post('/Base_Manage/BuildCode/GetAllDbLink', {})
  dbs.value = resJson.Data
  if (dbs.value && dbs.value.length > 0) {
    linkId.value = dbs.value[0].Id
  }
  await getDataList()
}

const getDataList = async () => {
  selectedRowKeys.value = []
  loading.value = true

  try {
    const resJson = await proxy.$http.post('/Base_Manage/BuildCode/GetDbTableList', {
      linkId: linkId.value
    })
    data.value = resJson.Data
  } finally {
    loading.value = false
  }
}

const onLinkChange = () => {
  getDataList()
}

const onSelectChange = (keys) => {
  selectedRowKeys.value = keys
}

const openForm = () => {
  editFormRef.value?.openForm(selectedRowKeys.value, linkId.value)
}

onMounted(() => {
  init()
})
</script>

<script>
export default {
  name: 'BuildCodeList'
}
</script>