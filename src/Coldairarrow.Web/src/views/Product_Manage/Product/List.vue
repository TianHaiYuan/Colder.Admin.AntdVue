<template>
  <a-card :bordered="false">
    <div class="table-page-search-wrapper">
      <a-form layout="inline">
        <a-row :gutter="16" style="width: 100%">
          <a-col :md="4" :sm="24">
            <a-input v-model:value="queryParam.Keyword" placeholder="产品名称/编码" allow-clear />
          </a-col>
          <a-col :md="4" :sm="24">
            <a-tree-select
                v-model:value="queryParam.CategoryId"
                :tree-data="categoryList"
                placeholder="产品分类"
                allow-clear
                tree-default-expand-all
                :field-names="{ label: 'title', value: 'key', children: 'children' }"
              />
          </a-col>
          <a-col :md="4" :sm="24">
            <a-select v-model:value="queryParam.Status" placeholder="状态" allow-clear>
                <a-select-option :value="0">草稿</a-select-option>
                <a-select-option :value="1">上架</a-select-option>
                <a-select-option :value="2">下架</a-select-option>
              </a-select>
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
        <template v-if="column.dataIndex === 'ImageUrl'">
          <a-image v-if="record.ImageUrl" :src="record.ImageUrl" :width="60" :preview="false" />
        </template>
        <template v-else-if="column.dataIndex === 'Status'">
          <a-tag :color="record.Status === 1 ? 'green' : record.Status === 2 ? 'red' : 'orange'">
            {{ record.StatusText }}
          </a-tag>
        </template>
        <template v-else-if="column.dataIndex === 'action'">
          <a @click="handleEdit(record.Id)">编辑</a>
          <a-divider type="vertical" />
          <a @click="handleDelete([record.Id])">删除</a>
        </template>
      </template>
    </a-table>

    <EditForm ref="editFormRef" :afterSubmit="getDataList" :categoryList="categoryList" />
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
const categoryList = ref([])
const loading = ref(false)
const selectedRowKeys = ref([])
const queryParam = reactive({ Keyword: '', CategoryId: undefined, Status: undefined })

const pagination = reactive({
  current: 1,
  pageSize: 10,
  total: 0,
  showTotal: (total, range) => `总数:${total} 当前:${range[0]}-${range[1]}`
})

const filters = ref({})
const sorter = ref({})

	const columns = [
	  { title: '图片', dataIndex: 'ImageUrl', width: '10%' },
	  { title: '产品编码', dataIndex: 'ProductCode', width: '10%' },
	  { title: '产品名称', dataIndex: 'Name', width: '15%' },
  { title: '分类', dataIndex: 'CategoryName', width: '10%' },
  { title: '单价', dataIndex: 'Price', width: '8%' },
  { title: '库存', dataIndex: 'Stock', width: '8%' },
  { title: '单位', dataIndex: 'Unit', width: '6%' },
  { title: '状态', dataIndex: 'Status', width: '8%' },
  { title: '创建时间', dataIndex: 'CreateTime', width: '12%', sorter: true },
  { title: '操作', dataIndex: 'action' }
]

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
	      const resJson = await proxy.$http.post('/Product_Manage/Product/GetDataList', {
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

	  const getCategoryList = async () => {
	    const resJson = await proxy.$http.post('/Product_Manage/ProductCategory/GetTreeDataList', {})
  categoryList.value = resJson.Data
}

const onSelectChange = (keys) => { selectedRowKeys.value = keys }
const hanldleAdd = () => { editFormRef.value?.openForm() }
const handleEdit = (id) => { editFormRef.value?.openForm(id) }
const handleSearch = () => { pagination.current = 1; getDataList() }
const handleReset = () => { Object.assign(queryParam, { Keyword: '', CategoryId: undefined, Status: undefined }); getDataList() }

const handleDelete = (ids) => {
  Modal.confirm({ title: '确认删除吗?', onOk() { return submitDelete(ids) } })
}

	  const submitDelete = async (ids) => {
	    const resJson = await proxy.$http.post('/Product_Manage/Product/DeleteData', ids)
  if (resJson.Success) { message.success('操作成功!'); getDataList() } else { message.error(resJson.Msg) }
}

onMounted(() => { getCategoryList(); getDataList() })
</script>

<script>
export default { name: 'ProductList' }
</script>

