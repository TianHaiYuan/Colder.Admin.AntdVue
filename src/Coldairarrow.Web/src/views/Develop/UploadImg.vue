<template>
  <a-card :bordered="false">
    <a-form :model="entity" :label-col="labelCol" :wrapper-col="wrapperCol">
      <a-form-item label="单张图片上传">
        <!--v-model为图片连接地址(可传单个或数组),maxCount为最大上传数:默认为1-->
        <CUploadImg v-model:value="entity.Img" :max-count="1" />
      </a-form-item>
      <a-form-item label="多张图片上传">
        <!--v-model为图片连接地址(可传单个或数组),maxCount为最大上传数:默认为1-->
        <CUploadImg v-model:value="entity.Imgs" :max-count="100" />
      </a-form-item>
      <a-form-item label="获取图片链接">
        <a-button @click="setImg()">设置图片链接</a-button>
        <a-button @click="getImgUrls()">获取图片链接</a-button>
      </a-form-item>
    </a-form>
  </a-card>
</template>

<script setup>
import { reactive } from 'vue'
import CUploadImg from '@/components/CUploadImg/CUploadImg.vue'
import logoImg from '@/assets/logo.png'

const labelCol = { xs: { span: 24 }, sm: { span: 7 } }
const wrapperCol = { xs: { span: 24 }, sm: { span: 13 } }

const entity = reactive({
  Img: logoImg,
  Imgs: [logoImg]
})

const getImgUrls = () => {
  console.log(entity)
}

const setImg = () => {
  entity.Img = logoImg
  entity.Imgs = [logoImg]
}
</script>

<script>
export default {
  name: 'UploadImgView'
}
</script>
