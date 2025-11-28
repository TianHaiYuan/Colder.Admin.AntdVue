<template>
  <a-card :bordered="false">
    <a-button @click="getContent()">获取内容</a-button>
    <a-button @click="setContent()">清空内容</a-button>
    <WangEditor v-model:value="content" />
  </a-card>
</template>

<script setup>
import { ref } from 'vue'
import WangEditor from '@/components/WangEditor/WangEditor.vue'

const content = ref('')

const getContent = () => {
  console.log(content.value)
}

const setContent = () => {
  content.value = ''
}
</script>

<script>
export default {
  name: 'EditorView'
}
</script>