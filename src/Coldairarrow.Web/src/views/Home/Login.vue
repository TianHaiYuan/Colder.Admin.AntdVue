<template>
  <div class="main">
    <a-spin :spinning="loading">
      <a-form
        id="formLogin"
        class="user-layout-login"
        ref="formRef"
        :model="formState"
        @finish="handleSubmit"
      >
        <a-tabs
          :activeKey="customActiveKey"
          :tabBarStyle="{ textAlign: 'center', borderBottom: 'unset' }"
          @change="handleTabClick"
        >
          <a-tab-pane key="tab1" tab="账号密码登录">
            <a-form-item name="userName" :rules="[{ required: true, message: '请输入用户名' }]">
              <a-input
                size="large"
                v-model:value="formState.userName"
                placeholder="请输入用户名"
              >
                <template #prefix>
                  <UserOutlined :style="{ color: 'rgba(0,0,0,.25)' }" />
                </template>
              </a-input>
            </a-form-item>

            <a-form-item name="password" :rules="[{ required: true, message: '请输入密码' }]">
              <a-input-password
                size="large"
                v-model:value="formState.password"
                placeholder="请输入密码"
              >
                <template #prefix>
                  <LockOutlined :style="{ color: 'rgba(0,0,0,.25)' }" />
                </template>
              </a-input-password>
            </a-form-item>
            <a-form-item name="savePwd">
              <a-checkbox v-model:checked="formState.savePwd">记住密码</a-checkbox>
            </a-form-item>
          </a-tab-pane>
        </a-tabs>

        <a-form-item style="margin-top:24px">
          <a-button size="large" type="primary" html-type="submit" class="login-button">确定</a-button>
        </a-form-item>
      </a-form>
    </a-spin>
  </div>
</template>

<script setup>
import { ref, reactive, onMounted, getCurrentInstance } from 'vue'
import { useRouter } from 'vue-router'
import { message } from 'ant-design-vue'
import { UserOutlined, LockOutlined } from '@ant-design/icons-vue'
import TokenCache from '@/utils/cache/TokenCache.js'

const { proxy } = getCurrentInstance()
const router = useRouter()

const formRef = ref(null)
const loading = ref(false)
const customActiveKey = ref('tab1')

const formState = reactive({
  userName: '',
  password: '',
  savePwd: false
})

onMounted(() => {
  const userName = localStorage.getItem('userName')
  const password = localStorage.getItem('password')
  if (userName && password) {
    formState.userName = userName
    formState.password = password
    formState.savePwd = true
  }
})

const handleTabClick = (key) => {
  customActiveKey.value = key
}

const handleSubmit = async () => {
  loading.value = true
  try {
    const resJson = await proxy.$http.post('/Base_Manage/Home/SubmitLogin', formState)
    loading.value = false

    if (resJson.Success) {
      TokenCache.setToken(resJson.Data)
      // 保存密码
      if (formState.savePwd) {
        localStorage.setItem('userName', formState.userName)
        localStorage.setItem('password', formState.password)
      } else {
        localStorage.removeItem('userName')
        localStorage.removeItem('password')
      }
      // 跳转到首页，permission.js 会处理动态路由加载
      router.push({ path: '/Home/Introduce' })
    } else {
      message.error(resJson.Msg)
    }
  } catch (error) {
    loading.value = false
    message.error('登录失败')
  }
}
</script>

<script>
export default {
  name: 'Login'
}
</script>

<style lang="less" scoped>
.user-layout-login {
  label {
    font-size: 14px;
  }

  .getCaptcha {
    display: block;
    width: 100%;
    height: 40px;
  }

  .forge-password {
    font-size: 14px;
  }

  button.login-button {
    padding: 0 15px;
    font-size: 16px;
    height: 40px;
    width: 100%;
  }

  .user-login-other {
    text-align: left;
    margin-top: 24px;
    line-height: 22px;

    .item-icon {
      font-size: 24px;
      color: rgba(0, 0, 0, 0.2);
      margin-left: 16px;
      vertical-align: middle;
      cursor: pointer;
      transition: color 0.3s;

      &:hover {
        color: #1890ff;
      }
    }

    .register {
      float: right;
    }
  }
}
</style>
