<template>
  <a-modal
    title="修改密码"
    width="40%"
    :open="visible"
    :confirmLoading="confirmLoading"
    @ok="handleSubmit"
    @cancel="handleCancel"
  >
    <a-spin :spinning="confirmLoading">
      <a-form
        ref="formRef"
        :model="formState"
        :label-col="labelCol"
        :wrapper-col="wrapperCol"
      >
        <a-form-item label="原密码" name="oldPwd" :rules="[{ required: true, message: '必填' }]">
          <a-input-password v-model:value="formState.oldPwd" />
        </a-form-item>
        <a-form-item label="新密码" name="newPwd" :rules="[{ required: true, message: '必填' }]">
          <a-input-password v-model:value="formState.newPwd" />
        </a-form-item>
      </a-form>
    </a-spin>
  </a-modal>
</template>

<script setup>
import { ref, reactive, getCurrentInstance } from 'vue'
import { message } from 'ant-design-vue'

const { proxy } = getCurrentInstance()

const formRef = ref(null)
const labelCol = { xs: { span: 24 }, sm: { span: 7 } }
const wrapperCol = { xs: { span: 24 }, sm: { span: 13 } }
const visible = ref(false)
const confirmLoading = ref(false)

const formState = reactive({
  oldPwd: '',
  newPwd: ''
})

const init = () => {
  formState.oldPwd = ''
  formState.newPwd = ''
  visible.value = true
}

const open = () => {
  init()
}

const handleSubmit = async () => {
  try {
    await formRef.value.validate()

    confirmLoading.value = true
    const resJson = await proxy.$http.post('/Base_Manage/Home/ChangePwd', formState)
    confirmLoading.value = false

    if (resJson.Success) {
      message.success('操作成功!')
      visible.value = false
    } else {
      message.error(resJson.Msg)
    }
  } catch (error) {
    confirmLoading.value = false
  }
}

const handleCancel = () => {
  visible.value = false
}

defineExpose({
  open
})
</script>

<script>
export default {
  name: 'ChangePwdForm'
}
</script>
