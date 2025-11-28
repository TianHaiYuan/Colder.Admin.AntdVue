<template>
  <a-card :bordered="false">
    <a-form :model="entity" :label-col="labelCol" :wrapper-col="wrapperCol">
      <a-form-item label="文件上传">
        <!--v-model为图片连接地址(可传单个或数组),maxCount为最大上传数:默认为1-->
        <CUploadFile v-model:value="entity.File" :max-count="5" />
      </a-form-item>
      <a-form-item label="获取图片链接">
        <a-button @click="setFile()">设置文件链接</a-button>
        <a-button @click="getFileUrls()">获取文件链接</a-button>
      </a-form-item>
    </a-form>
  </a-card>
</template>

<script setup>
import { reactive } from 'vue'
import CUploadFile from '@/components/CUploadFile/CUploadFile.vue'
import logoImg from '@/assets/logo.png'

const labelCol = { xs: { span: 24 }, sm: { span: 7 } }
const wrapperCol = { xs: { span: 24 }, sm: { span: 13 } }

const entity = reactive({
  File: [logoImg]
})

const getFileUrls = () => {
  console.log(entity)
}

const setFile = () => {
  entity.File = logoImg
}
</script>

<script>
export default {
  name: 'UploadFileView'
}
</script>
