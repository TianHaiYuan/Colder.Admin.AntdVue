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
        <a-form-item label="角色名" name="RoleName">
          <a-input v-model:value="entity.RoleName" autocomplete="off" />
        </a-form-item>
        <a-form-item label="权限" name="Actions">
          <a-tree
            v-if="actionsTreeData && actionsTreeData.length"
            show-line
            checkable
            :check-strictly="true"
            :auto-expand-parent="true"
            :default-expand-all="true"
            v-model:checkedKeys="checkedKeys"
            :tree-data="actionsTreeData"
            @check="onCheck"
          />
        </a-form-item>
      </a-form>
    </a-spin>
  </a-modal>
</template>

<script setup>
import { ref, reactive, nextTick, getCurrentInstance } from 'vue'
import { message } from 'ant-design-vue'
import TreeHelper from '@/utils/helper/TreeHelper'

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
  RoleName: ''
})
const actionsTreeData = ref([])
const allActionList = ref([])
const checkedKeys = ref([])

const rules = {
  RoleName: [{ required: true, message: '必填' }]
}

const init = async () => {
  visible.value = true
  Object.assign(entity, { RoleName: '', Id: null })
  checkedKeys.value = []

  await nextTick()
  formRef.value?.clearValidate()

  const [treeRes, listRes] = await Promise.all([
    proxy.$http.post('/Base_Manage/Base_Action/GetActionTreeList', {}),
    proxy.$http.post('/Base_Manage/Base_Action/GetAllActionList', {})
  ])

  if (treeRes.Success) {
    actionsTreeData.value = treeRes.Data
  }
  if (listRes.Success) {
    allActionList.value = listRes.Data
  }
}

const openForm = async (id) => {
  await init()

  if (id) {
    const resJson = await proxy.$http.post('/Base_Manage/Base_Role/GetTheData', { id })
    Object.assign(entity, resJson.Data)
    checkedKeys.value = entity.Actions || []
  }
}

const onCheck = (keys, e) => {
  const value = e.node.key
  let newChecked = []

  if (e.checked) {
    const parentIds = TreeHelper.getParentIds(value, allActionList.value)
    const children = TreeHelper.getChildrenIds(value, allActionList.value)
    const addNodes = parentIds.concat(children).filter(item => !checkedKeys.value.includes(item))
    newChecked = checkedKeys.value.concat(addNodes)
  } else {
    const children = TreeHelper.getChildrenIds(value, allActionList.value)
    children.push(value)
    newChecked = checkedKeys.value.filter(item => !children.includes(item))
  }

  checkedKeys.value = newChecked
}

const handleSubmit = async () => {
  try {
    await formRef.value.validate()

    confirmLoading.value = true
    entity.Actions = checkedKeys.value

    const resJson = await proxy.$http.post('/Base_Manage/Base_Role/SaveData', entity)
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
  name: 'Base_RoleEditForm'
}
</script>
