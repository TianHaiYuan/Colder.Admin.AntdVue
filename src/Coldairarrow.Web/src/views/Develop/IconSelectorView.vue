<template>
  <a-card :body-style="{padding: '24px 32px'}" :bordered="false">
    <IconSelector v-model:value="currentSelectedIcon" @change="handleIconChange" />

    <a-divider />
    <p>测试 IconSelector 组件 v-model 功能</p>
    <a-button @click="changeIcon('down')">改变 Icon-down</a-button>
    <a-divider type="vertical" />
    <a-button @click="changeIcon('cloud-download')">改变 Icon-cloud-download</a-button>
  </a-card>
</template>

<script setup>
import { ref } from 'vue'
import { message } from 'ant-design-vue'
import IconSelector from '@/components/IconSelector'

const currentSelectedIcon = ref('pause-circle')

const handleIconChange = (icon) => {
  console.log('change Icon', icon)
  message.info(`选中图标 ${icon}`)
}

const changeIcon = (type) => {
  currentSelectedIcon.value = type
}
</script>

<script>
export default {
  name: 'IconSelectorView'
}
</script>
