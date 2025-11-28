<template>
  <a-card :bordered="false">
    <div class="table-page-search-wrapper">
      <a-form layout="inline">
        <a-row :gutter="16" style="width: 100%">
          <a-col :md="5" :sm="24">
            <a-form-item label="内容">
              <a-input v-model:value="queryParam.logContent" placeholder="" allow-clear />
            </a-form-item>
          </a-col>
          <a-col :md="4" :sm="24">
            <a-form-item label="类别">
              <a-select v-model:value="queryParam.logType" allow-clear style="min-width: 120px">
                <a-select-option v-for="item in LogTypeList" :key="item.text" :value="item.text">{{ item.text }}</a-select-option>
              </a-select>
            </a-form-item>
          </a-col>
          <a-col :md="4" :sm="24">
            <a-form-item label="操作人">
              <a-input v-model:value="queryParam.opUserName" placeholder="" allow-clear />
            </a-form-item>
          </a-col>
          <a-col :md="7" :sm="24">
            <a-form-item label="时间">
              <a-space>
                <a-date-picker v-model:value="queryParam.startTime" show-time format="YYYY-MM-DD HH:mm:ss" />
                <span>~</span>
                <a-date-picker v-model:value="queryParam.endTime" show-time format="YYYY-MM-DD HH:mm:ss" />
              </a-space>
            </a-form-item>
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

    <a-table
      ref="tableRef"
      :columns="columns"
      :row-key="row => row.Id"
      :data-source="data"
      :pagination="pagination"
      :loading="loading"
      @change="handleTableChange"
      :bordered="true"
      size="small"
      style="word-break:break-all;"
    >
      <template #bodyCell="{ column, text }">
        <template v-if="column.dataIndex === 'LogContent'">
          <span v-for="(item, index) in (text || '').replace(/\r\n/g, '\n').split('\n')" :key="index">
            {{ item }}<br />
          </span>
        </template>
      </template>
    </a-table>
  </a-card>
</template>

<script setup>
import { ref, reactive, onMounted, getCurrentInstance } from 'vue'

const { proxy } = getCurrentInstance()

const tableRef = ref(null)
const data = ref([])
const loading = ref(false)
const LogTypeList = ref([])
const queryParam = reactive({
  logContent: '',
  logType: undefined,
  opUserName: '',
  startTime: null,
  endTime: null
})

const pagination = reactive({
  current: 1,
  pageSize: 10,
  total: 0,
  showTotal: (total, range) => `总数:${total} 当前:${range[0]}-${range[1]}`
})

const filters = ref({})
const sorter = ref({ field: 'Id', order: 'asc' })

const columns = [
  { title: '内容', dataIndex: 'LogContent', width: '50%' },
  { title: '类别', dataIndex: 'LogType', width: '10%' },
  { title: '操作人', dataIndex: 'CreatorRealName', width: '5%' },
  { title: '时间', dataIndex: 'CreateTime', width: '10%' }
]

const handleTableChange = (pag, flt, srt) => {
  pagination.current = pag.current
  pagination.pageSize = pag.pageSize
  filters.value = { ...flt }
  sorter.value = { ...srt }
  getDataList()
}

const init = async () => {
  const resJson = await proxy.$http.post('/Base_Manage/Base_UserLog/GetLogTypeList')
  LogTypeList.value = resJson.Data
}

const getDataList = async () => {
  loading.value = true

  try {
    const resJson = await proxy.$http.post('/Base_Manage/Base_UserLog/GetLogList', {
      PageIndex: pagination.current,
      PageRows: pagination.pageSize,
      SortField: 'CreateTime',
      SortType: 'desc',
      ...filters.value,
      Search: queryParam
    })

    data.value = resJson.Data
    pagination.total = resJson.Total
  } finally {
    loading.value = false
  }
}

const handleSearch = () => {
  pagination.current = 1
  getDataList()
}

const handleReset = () => {
  Object.assign(queryParam, {
    logContent: '',
    logType: undefined,
    opUserName: '',
    startTime: null,
    endTime: null
  })
  getDataList()
}

onMounted(() => {
  init()
  getDataList()
})
</script>

<script>
export default {
  name: 'Base_UserLogList'
}
</script>