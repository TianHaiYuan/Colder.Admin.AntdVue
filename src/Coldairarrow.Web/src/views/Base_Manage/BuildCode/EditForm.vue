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
        <a-form-item label="生成类型" name="buildTypes">
          <a-checkbox-group v-model:value="entity.buildTypes">
            <a-checkbox value="0">实体层</a-checkbox>
            <a-checkbox value="1">业务层</a-checkbox>
            <a-checkbox value="2">接口层</a-checkbox>
            <a-checkbox value="3">页面层</a-checkbox>
          </a-checkbox-group>
        </a-form-item>
        <a-form-item label="生成区域" name="areaName">
          <a-input v-model:value="entity.areaName" autocomplete="off" />
        </a-form-item>
      </a-form>
    </a-spin>
  </a-modal>
</template>

<script setup>
import { ref, reactive, nextTick, getCurrentInstance } from 'vue'
import { message } from 'ant-design-vue'

const { proxy } = getCurrentInstance()

const formRef = ref(null)
const visible = ref(false)
const confirmLoading = ref(false)
const entity = reactive({
  buildTypes: [],
  areaName: '',
  tables: [],
  linkId: ''
})

const rules = {
  buildTypes: [{ required: true, message: '必填' }],
  areaName: [{ required: true, message: '必填' }]
}

const init = async () => {
  visible.value = true
  Object.assign(entity, { buildTypes: ['0', '1', '2', '3'], areaName: '', tables: [], linkId: '' })

  await nextTick()
  formRef.value?.clearValidate()
}

const openForm = async (tables, linkId) => {
  await init()
  entity.tables = tables
  entity.linkId = linkId
}

const handleSubmit = async () => {
  try {
    await formRef.value.validate()

    confirmLoading.value = true
    const resJson = await proxy.$http.post('/Base_Manage/BuildCode/Build', entity)
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

defineExpose({
  openForm
})
</script>

<script>
export default {
  name: 'BuildCodeEditForm'
}
</script>
