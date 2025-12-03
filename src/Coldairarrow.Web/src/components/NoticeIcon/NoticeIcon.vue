<template>
	<span class="header-notice" @click="goMessageCenter" ref="noticeRef">
	  <a-badge :count="unreadCount" :overflow-count="99">
	    <BellOutlined style="font-size: 16px; padding: 4px" />
	  </a-badge>
	</span>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { BellOutlined } from '@ant-design/icons-vue'
import { Axios } from '@/utils/plugin/axios-plugin.js'
import OperatorCache from '@/utils/cache/OperatorCache.js'
import config from '@/config/defaultSettings.js'

const noticeRef = ref(null)
const unreadCount = ref(0)
const router = useRouter()

// 从 SignalR Hub 地址推导出 NotificationCenter.Api 的 HTTP 根地址
const notificationApiRoot = (() => {
  const hubUrl = config.notificationHubUrl
  if (!hubUrl) return ''
  try {
    const url = new URL(hubUrl)
    return url.origin
  } catch {
    return hubUrl.replace('/hubs/notification', '')
  }
})()

const refreshUnread = async () => {
  try {
    await OperatorCache.init().catch(() => {})
    const userId = OperatorCache.info.Id
    if (!userId) return
    const res = await Axios.get(`${notificationApiRoot}/api/notifications/unread-count`, {
      params: { userId }
    })
    if (res && typeof res.count === 'number') {
      unreadCount.value = res.count
    }
  } catch (e) {
    console.error(e)
  }
}

const goMessageCenter = () => {
  router.push('/MessageCenter')
}

onMounted(() => {
  refreshUnread()
})
</script>

<script>
export default {
  name: 'HeaderNotice'
}
</script>

<style lang="css">
  .header-notice-wrapper {
    top: 50px !important;
  }
</style>
<style lang="less" scoped>
  .header-notice{
    display: inline-block;
    transition: all 0.3s;

    span {
      vertical-align: initial;
    }
  }
</style>
