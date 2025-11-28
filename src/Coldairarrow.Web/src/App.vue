<template>
  <a-config-provider :locale="zhCN">
    <div id="app">
      <router-view />
    </div>
  </a-config-provider>
</template>

<script setup>
import zhCN from 'ant-design-vue/es/locale/zh_CN'
import dayjs from 'dayjs'
import 'dayjs/locale/zh-cn'
dayjs.locale('zh-cn')

import { onMounted, onUnmounted } from 'vue'
import { useAppStore } from '@/store'

const appStore = useAppStore()

// 设备检测
let enquireHandler = null

onMounted(() => {
  if (typeof window.enquire !== 'undefined') {
    enquireHandler = window.enquire.register('only screen and (max-width: 767.99px)', {
      match: () => {
        appStore.toggleDevice('mobile')
      },
      unmatch: () => {
        appStore.toggleDevice('desktop')
      }
    })
  }
})

onUnmounted(() => {
  if (enquireHandler) {
    window.enquire.unregister(enquireHandler)
  }
})
</script>

<style>
#app {
  height: 100%;
}
</style>
