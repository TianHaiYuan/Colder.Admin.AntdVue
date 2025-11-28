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
        <a-form-item label="菜单名" name="Name">
          <a-input v-model:value="entity.Name" autocomplete="off" />
        </a-form-item>
        <a-form-item label="上级菜单" name="ParentId">
          <a-tree-select
            v-model:value="entity.ParentId"
            allow-clear
            :tree-data="ParentIdTreeData"
            placeholder="请选择上级菜单"
            tree-default-expand-all
          />
        </a-form-item>
        <a-form-item label="类型" name="Type">
          <a-radio-group v-model:value="entity.Type">
            <a-radio :value="0">菜单</a-radio>
            <a-radio :value="1">页面</a-radio>
          </a-radio-group>
        </a-form-item>
        <a-form-item label="路径" name="Url">
          <a-input v-model:value="entity.Url" autocomplete="off" />
        </a-form-item>
        <a-form-item label="是否需要权限" name="NeedAction">
          <a-radio-group v-model:value="entity.NeedAction">
            <a-radio :value="false">否</a-radio>
            <a-radio :value="true">是</a-radio>
          </a-radio-group>
        </a-form-item>
        <a-form-item label="图标" name="Icon">
          <a-input v-model:value="entity.Icon" autocomplete="off" />
        </a-form-item>
        <a-form-item label="排序" name="Sort">
          <a-input-number v-model:value="entity.Sort" autocomplete="off" />
        </a-form-item>
        <a-card title="页面权限" :bordered="false">
          <PermissionList ref="permissionListRef" />
        </a-card>
      </a-form>
    </a-spin>
  </a-modal>
</template>

<script setup>
import { ref, reactive, nextTick, getCurrentInstance } from 'vue'
import { message } from 'ant-design-vue'
import PermissionList from './PermissionList.vue'

const props = defineProps({
  afterSubmit: {
    type: Function,
    default: null
  }
})

const { proxy } = getCurrentInstance()

const formRef = ref(null)
const permissionListRef = ref(null)
const visible = ref(false)
const confirmLoading = ref(false)
const entity = reactive({
  Name: '',
  ParentId: null,
  Type: null,
  Url: '',
  NeedAction: null,
  Icon: '',
  Sort: 0
})
const ParentIdTreeData = ref([])

const rules = {
  Name: [{ required: true, message: '必填' }],
  Type: [{ required: true, message: '必填' }],
  NeedAction: [{ required: true, message: '必填' }]
}

const init = async (id) => {
  visible.value = true
  Object.assign(entity, {
    Name: '',
    ParentId: null,
    Type: null,
    Url: '',
    NeedAction: null,
    Icon: '',
    Sort: 0,
    Id: null
  })

  await nextTick()
  permissionListRef.value?.init(id)
  formRef.value?.clearValidate()

  const resJson = await proxy.$http.post('/Base_Manage/Base_Action/GetMenuTreeList', {})
  if (resJson.Success) {
    ParentIdTreeData.value = resJson.Data
  }
}

const openForm = async (id) => {
  await init(id)

  if (id) {
    const resJson = await proxy.$http.post('/Base_Manage/Base_Action/GetTheData', { id })
    Object.assign(entity, resJson.Data)
  }
}

const handleSubmit = async () => {
  try {
    await formRef.value.validate()

    confirmLoading.value = true
    entity.permissionList = permissionListRef.value?.getPermissionList() || []

    const resJson = await proxy.$http.post('/Base_Manage/Base_Action/SaveData', entity)
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
  name: 'Base_ActionEditForm'
}
</script>
