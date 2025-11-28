<template>
  <!-- 两步验证 -->
  <a-modal
    centered
    :open="visible"
    @cancel="handleCancel"
    :mask-closable="false"
  >
    <template #title>
      <div :style="{ textAlign: 'center' }">两步验证</div>
    </template>
    <template #footer>
      <div :style="{ textAlign: 'center' }">
        <a-button key="back" @click="handleCancel">返回</a-button>
        <a-button key="submit" type="primary" :loading="stepLoading" @click="handleStepOk">
          继续
        </a-button>
      </div>
    </template>

    <a-spin :spinning="stepLoading">
      <a-form ref="formRef" layout="vertical" :model="formState" :rules="rules">
        <div class="step-form-wrapper">
          <p style="text-align: center" v-if="!stepLoading">请在手机中打开 Google Authenticator 或两步验证 APP<br />输入 6 位动态码</p>
          <p style="text-align: center" v-else>正在验证..<br/>请稍后</p>
          <a-form-item
            :style="{ textAlign: 'center' }"
            has-feedback
            name="stepCode"
          >
            <a-input v-model:value="formState.stepCode" :style="{ textAlign: 'center' }" @keyup.enter="handleStepOk" placeholder="000000" />
          </a-form-item>
          <p style="text-align: center">
            <a @click="onForgeStepCode">遗失手机?</a>
          </p>
        </div>
      </a-form>
    </a-spin>
  </a-modal>
</template>

<script setup>
import { ref, reactive } from 'vue'

const props = defineProps({
  visible: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:visible', 'success', 'error', 'cancel'])

const formRef = ref(null)
const stepLoading = ref(false)
const formState = reactive({
  stepCode: ''
})

const rules = {
  stepCode: [
    { required: true, message: '请输入 6 位动态码!', trigger: 'blur' },
    { pattern: /^\d{6}$/, message: '请输入 6 位数字', trigger: 'blur' }
  ]
}

const handleStepOk = async () => {
  stepLoading.value = true
  try {
    await formRef.value.validate()
    setTimeout(() => {
      stepLoading.value = false
      emit('success', { values: formState })
    }, 2000)
  } catch (err) {
    stepLoading.value = false
    emit('error', { err })
  }
}

const handleCancel = () => {
  emit('update:visible', false)
  emit('cancel')
}

const onForgeStepCode = () => {
  // Handle forgot step code
}
</script>

<script>
export default {
  name: 'TwoStepCaptcha'
}
</script>

<style lang="less" scoped>
  .step-form-wrapper {
    margin: 0 auto;
    width: 80%;
    max-width: 400px;
  }
</style>
