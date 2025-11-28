<template>
  <div class="clearfix">
    <a-upload
      :action="`${rootUrl}/Base_Manage/Upload/UploadFileByForm`"
      :headers="headers"
      list-type="picture"
      :file-list="fileList"
      @preview="handlePreview"
      @change="handleChange"
    >
      <div v-if="fileList.length < maxCount">
        <a-button><PlusOutlined />选择</a-button>
      </div>
    </a-upload>
    <a-modal :open="previewVisible" :footer="null" @cancel="handleCancel">
      <img alt="example" style="width: 100%" :src="previewImage" />
    </a-modal>
  </div>
</template>

<script setup>
import { ref, watch, onMounted, inject } from 'vue'
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

const checkType = (val) => {
  if (props.maxCount === 1 && TypeHelper.isArray(val)) {
    throw new Error('maxCount=1时model不能为Array')
  }
  if (props.maxCount > 1 && !TypeHelper.isArray(val)) {
    throw new Error('maxCount>1时model必须为Array<String>')
  }
}

const getFileName = (url) => {
  const reg = /^.*\/(.*?)$/
  const match = url.match(reg)
  if (match) {
    return match[1]
  } else {
    return ''
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
      return { name: getFileName(x), uid: uuidv4(), status: 'done', url: x }
    })
  } else {
    fileList.value = []
  }
}

const handleCancel = () => {
  previewVisible.value = false
}

const handlePreview = (file) => {
  const url = file.url || file.response.url
  window.open(url, 'tab')
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
  name: 'CUploadFile'
}
</script>