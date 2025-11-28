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
        <a-form-item label="用户名" name="UserName">
          <a-input v-model:value="entity.UserName" autocomplete="off" />
        </a-form-item>
        <a-form-item label="密码" name="newPwd">
          <a-input-password v-model:value="entity.newPwd" autocomplete="off" />
        </a-form-item>
        <a-form-item label="姓名" name="RealName">
          <a-input v-model:value="entity.RealName" autocomplete="off" />
        </a-form-item>
        <a-form-item label="性别" name="Sex">
          <a-radio-group v-model:value="entity.Sex">
            <a-radio :value="0">女</a-radio>
            <a-radio :value="1">男</a-radio>
          </a-radio-group>
        </a-form-item>
        <a-form-item label="生日" name="Birthday">
          <a-date-picker v-model:value="entity.Birthday" format="YYYY-MM-DD" value-format="YYYY-MM-DD" />
        </a-form-item>
        <a-form-item label="部门" name="DepartmentId">
          <a-tree-select
            v-model:value="entity.DepartmentId"
            allow-clear
            :tree-data="DepartmentIdTreeData"
            placeholder="请选择部门"
            tree-default-expand-all
          />
        </a-form-item>
        <a-form-item label="角色" name="RoleIdList">
          <a-select v-model:value="entity.RoleIdList" allow-clear mode="multiple">
            <a-select-option v-for="item in RoleOptionList" :key="item.Id" :value="item.Id">
              {{ item.RoleName }}
            </a-select-option>
          </a-select>
        </a-form-item>
      </a-form>
    </a-spin>
  </a-modal>
</template>

<script setup>
import { ref, reactive, nextTick, getCurrentInstance } from 'vue'
import { message } from 'ant-design-vue'
import dayjs from 'dayjs'

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
  UserName: '',
  newPwd: '',
  RealName: '',
  Sex: null,
  Birthday: null,
  DepartmentId: null,
  RoleIdList: []
})
const DepartmentIdTreeData = ref([])
const RoleOptionList = ref([])

const rules = {
  UserName: [{ required: true, message: '必填' }],
  RealName: [{ required: true, message: '必填' }],
  Sex: [{ required: true, message: '必填' }]
}

const init = async () => {
  visible.value = true
  Object.assign(entity, {
    UserName: '',
    newPwd: '',
    RealName: '',
    Sex: null,
    Birthday: null,
    DepartmentId: null,
    RoleIdList: []
  })

  await nextTick()
  formRef.value?.clearValidate()

  const [deptRes, roleRes] = await Promise.all([
    proxy.$http.post('/Base_Manage/Base_Department/GetTreeDataList', {}),
    proxy.$http.post('/Base_Manage/Base_Role/GetDataList', {})
  ])

  if (deptRes.Success) {
    DepartmentIdTreeData.value = deptRes.Data
  }
  if (roleRes.Success) {
    RoleOptionList.value = roleRes.Data
  }
}

const openForm = async (id) => {
  await init()

  if (id) {
    const resJson = await proxy.$http.post('/Base_Manage/Base_User/GetTheData', { id })
    Object.assign(entity, resJson.Data)
    if (entity.Birthday) {
      entity.Birthday = dayjs(entity.Birthday).format('YYYY-MM-DD')
    }
  }
}

const handleSubmit = async () => {
  try {
    await formRef.value.validate()

    confirmLoading.value = true
    const resJson = await proxy.$http.post('/Base_Manage/Base_User/SaveData', entity)
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
  name: 'Base_UserEditForm'
}
</script>
