<template>
  <a-modal
    title="编辑表单"
    width="40%"
    :open="visible"
    :confirmLoading="confirmLoading"
    @ok="handleSubmit"
    @cancel="visible = false"
  >
    <a-spin :spinning="confirmLoading">
      <a-form ref="formRef" :model="entity" :rules="rules" :label-col="{ span: 5 }" :wrapper-col="{ span: 18 }">
        <a-form-item label="部门名" name="Name">
          <a-input v-model:value="entity.Name" autocomplete="off" />
        </a-form-item>
        <a-form-item label="上级部门" name="ParentId">
          <a-tree-select
            v-model:value="entity.ParentId"
            allow-clear
            :tree-data="ParentIdTreeData"
            placeholder="请选择上级部门"
            tree-default-expand-all
          />
        </a-form-item>
      </a-form>
    </a-spin>
  </a-modal>
</template>

<script setup>
import { ref, reactive, nextTick, getCurrentInstance } from 'vue'
import { message } from 'ant-design-vue'

const props = defineProps({
  afterSubmit: {
    type: Function,
    default: null
  }
})

const { proxy } = getCurrentInstance()

const formRef = ref(null)
const visible = ref(false)
const confirmLoading = ref(false)
const entity = reactive({
  Name: '',
  ParentId: null
})
const ParentIdTreeData = ref([])

const rules = {
  Name: [{ required: true, message: '必填' }]
}

const init = async () => {
  visible.value = true
  Object.assign(entity, { Name: '', ParentId: null, Id: null })

  await nextTick()
  formRef.value?.clearValidate()

  const resJson = await proxy.$http.post('/Base_Manage/Base_Department/GetTreeDataList', {})
  if (resJson.Success) {
    ParentIdTreeData.value = resJson.Data
  }
}

const openForm = async (id) => {
  await init()

  if (id) {
    const resJson = await proxy.$http.post('/Base_Manage/Base_Department/GetTheData', { id })
    Object.assign(entity, resJson.Data)
  }
}

const handleSubmit = async () => {
  try {
    await formRef.value.validate()

    confirmLoading.value = true
    const resJson = await proxy.$http.post('/Base_Manage/Base_Department/SaveData', entity)
    confirmLoading.value = false

    if (resJson.Success) {
      message.success('操作成功!')
      props.afterSubmit?.()
      visible.value = false
    } else {
      message.error(resJson.Msg)
    }
  } catch (error) {
    confirmLoading.value = false
  }
}

defineExpose({
  openForm
})
</script>

<script>
export default {
  name: 'Base_DepartmentEditForm'
}
</script>
