<template>
  <div class="clearfix">
    <a-upload
      :action="`${rootUrl}/Base_Manage/Upload/UploadFileByForm`"
      list-type="picture-card"
      :headers="headers"
      :file-list="fileList"
      @preview="handlePreview"
      @change="handleChange"
      accept="image/*"
      :multiple="isMultiple"
    >
      <div v-if="fileList.length < maxCount">
        <PlusOutlined />
        <div class="ant-upload-text">选择</div>
      </div>
    </a-upload>
    <a-modal :open="previewVisible" :footer="null" @cancel="handleCancel">
      <img alt="example" style="width: 100%" :src="previewImage" />
    </a-modal>
  </div>
</template>

<script setup>
import { ref, computed, watch, onMounted, inject } from 'vue'
import { PlusOutlined } from '@ant-design/icons-vue'
import { v4 as uuidv4 } from 'uuid'
import TypeHelper from '@/utils/helper/TypeHelper'
import TokenCache from '@/utils/cache/TokenCache'

const props = defineProps({
  value: { default: '' }, // 字符串或字符串数组
  maxCount: {
    type: Number,
    default: 1
  }
})

const emit = defineEmits(['update:value'])

const rootUrl = inject('rootUrl', '')
const previewVisible = ref(false)
const previewImage = ref('')
const fileList = ref([])
const headers = ref({ Authorization: 'Bearer ' + TokenCache.getToken() })
let internalValue = null

const isMultiple = computed(() => props.maxCount > 1)

const checkType = (val) => {
  if (props.maxCount === 1 && TypeHelper.isArray(val)) {
    throw new Error('maxCount=1时model不能为Array')
  }
  if (props.maxCount > 1 && !TypeHelper.isArray(val)) {
    throw new Error('maxCount>1时model必须为Array<String>')
  }
}

const refresh = (val) => {
  if (props.maxCount < 1) {
    throw new Error('maxCount必须>=1')
  }
  if (val) {
    let urls = []
    if (TypeHelper.isString(val)) {
      urls.push(val)
    } else if (TypeHelper.isArray(val)) {
      urls.push(...val)
    } else {
      throw new Error('value必须为字符串或数组')
    }
    fileList.value = urls.map((x) => {
      return { name: x, uid: uuidv4(), status: 'done', url: x }
    })
  } else {
    fileList.value = []
  }
}

const handleCancel = () => {
  previewVisible.value = false
}

const handlePreview = (file) => {
  previewImage.value = file.url || file.thumbUrl
  previewVisible.value = true
}

const handleChange = ({ file, fileList: newFileList }) => {
  fileList.value = newFileList
  if (file.status === 'done' || file.status === 'removed') {
    const urls = fileList.value.filter((x) => x.status === 'done').map((x) => x.url || x.response.url)
    const newValue = props.maxCount === 1 ? urls[0] : urls
    internalValue = newValue
    emit('update:value', newValue)
  }
}

watch(() => props.value, (val) => {
  // 内部触发事件不处理,仅回传数据
  if (val === internalValue) return
  checkType(val)
  refresh(val)
})

onMounted(() => {
  let val = props.value
  if (props.maxCount === 1) {
    val = val || ''
  } else {
    val = val || []
  }
  checkType(val)
  refresh(val)
})
</script>

<script>
export default {
  name: 'CUploadImg'
}
</script>

<style>
/* you can make up upload button and sample style by using stylesheets */
.ant-upload-select-picture-card i {
  font-size: 32px;
  color: #999;
}

.ant-upload-select-picture-card .ant-upload-text {
  margin-top: 8px;
  color: #666;
}
</style>