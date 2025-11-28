<template>
  <div class="user-wrapper">
    <div class="content-box">
      <a-dropdown>
        <span class="action ant-dropdown-link user-dropdown-menu">
          <a-avatar size="small">
            <template #icon><UserOutlined /></template>
          </a-avatar>
          <span>{{ op().UserName }}</span>
        </span>
        <template #overlay>
          <a-menu class="user-dropdown-menu-wrapper">
            <a-menu-item key="1">
              <a href="javascript:;" @click="handleChangePwd()">
                <LockOutlined />
                <span>修改密码</span>
              </a>
              <ChangePwdForm ref="changePwdRef" />
            </a-menu-item>
            <a-menu-divider />
            <a-menu-item key="3">
              <a href="javascript:;" @click="handleLogout()">
                <LogoutOutlined />
                <span>退出登录</span>
              </a>
            </a-menu-item>
          </a-menu>
        </template>
      </a-dropdown>
    </div>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { Modal } from 'ant-design-vue'
import { UserOutlined, LockOutlined, LogoutOutlined } from '@ant-design/icons-vue'
import OperatorCache from '@/utils/cache/OperatorCache'
import TokenCache from '@/utils/cache/TokenCache'
import ChangePwdForm from './ChangePwdForm.vue'

const changePwdRef = ref(null)

const op = () => {
  return OperatorCache.info
}

const handleLogout = () => {
  Modal.confirm({
    title: '提示',
    content: '真的要注销登录吗 ?',
    onOk() {
      TokenCache.deleteToken()
      OperatorCache.clear()
      location.reload()
    }
  })
}

const handleChangePwd = () => {
  changePwdRef.value?.open()
}
</script>

<script>
export default {
  name: 'UserMenu'
}
</script>
