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
        <a-form-item label="连接名" name="LinkName">
          <a-input v-model:value="entity.LinkName" autocomplete="off" />
        </a-form-item>
        <a-form-item label="连接字符串" name="ConnectionStr">
          <a-textarea v-model:value="entity.ConnectionStr" autocomplete="off" />
        </a-form-item>
        <a-form-item label="数据库类型" name="DbType">
          <a-select v-model:value="entity.DbType">
            <a-select-option value="SqlServer">SqlServer</a-select-option>
            <a-select-option value="MySql">MySql</a-select-option>
            <a-select-option value="Oracle">Oracle</a-select-option>
            <a-select-option value="PostgreSql">PostgreSql</a-select-option>
          </a-select>
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
  LinkName: '',
  ConnectionStr: '',
  DbType: null
})

const rules = {
  LinkName: [{ required: true, message: '必填' }],
  ConnectionStr: [{ required: true, message: '必填' }],
  DbType: [{ required: true, message: '必填' }]
}

const init = async () => {
  visible.value = true
  Object.assign(entity, { LinkName: '', ConnectionStr: '', DbType: null, Id: null })

  await nextTick()
  formRef.value?.clearValidate()
}

const openForm = async (id) => {
  await init()

  if (id) {
    const resJson = await proxy.$http.post('/Base_Manage/Base_DbLink/GetTheData', { id })
    Object.assign(entity, resJson.Data)
  }
}

const handleSubmit = async () => {
  try {
    await formRef.value.validate()

    confirmLoading.value = true
    const resJson = await proxy.$http.post('/Base_Manage/Base_DbLink/SaveData', entity)
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
  name: 'Base_DbLinkEditForm'
}
</script>
