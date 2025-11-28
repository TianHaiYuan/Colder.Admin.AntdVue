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
        <a-form-item label="应用Id" name="AppId">
          <a-input v-model:value="entity.AppId" autocomplete="off" />
        </a-form-item>
        <a-form-item label="密钥" name="AppSecret">
          <a-input v-model:value="entity.AppSecret" autocomplete="off" />
        </a-form-item>
        <a-form-item label="应用名" name="AppName">
          <a-input v-model:value="entity.AppName" autocomplete="off" />
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
  AppId: '',
  AppSecret: '',
  AppName: ''
})

const rules = {
  AppId: [{ required: true, message: '必填' }],
  AppSecret: [{ required: true, message: '必填' }],
  AppName: [{ required: true, message: '必填' }]
}

const init = async () => {
  visible.value = true
  Object.assign(entity, { AppId: '', AppSecret: '', AppName: '', Id: null })

  await nextTick()
  formRef.value?.clearValidate()
}

const openForm = async (id) => {
  await init()

  if (id) {
    const resJson = await proxy.$http.post('/Base_Manage/Base_AppSecret/GetTheData', { id })
    Object.assign(entity, resJson.Data)
  }
}

const handleSubmit = async () => {
  try {
    await formRef.value.validate()

    confirmLoading.value = true
    const resJson = await proxy.$http.post('/Base_Manage/Base_AppSecret/SaveData', entity)
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
  name: 'Base_AppSecretEditForm'
}
</script>
