<template>
  <div :class="prefixCls">
    <Toolbar
      style="border-bottom: 1px solid #ccc"
      :editor="editorRef"
      :defaultConfig="toolbarConfig"
      mode="default"
    />
    <Editor
      style="height: 300px; overflow-y: hidden;"
      v-model="valueHtml"
      :defaultConfig="editorConfig"
      mode="default"
      @onCreated="handleCreated"
      @onChange="handleChange"
    />
  </div>
</template>

<script setup>
import '@wangeditor/editor/dist/css/style.css'
import { ref, shallowRef, watch, onBeforeUnmount, inject, computed } from 'vue'
import { Editor, Toolbar } from '@wangeditor/editor-for-vue'

const props = defineProps({
  prefixCls: {
    type: String,
    default: 'ant-editor-wang'
  },
  value: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['update:value'])

const rootUrl = inject('rootUrl', '')

// 编辑器实例，必须用 shallowRef
const editorRef = shallowRef()
const valueHtml = ref(props.value || '')
let isChange = false

// 工具栏配置
const toolbarConfig = {}

// 编辑器配置
const editorConfig = computed(() => ({
  placeholder: '请输入内容...',
  MENU_CONF: {
    uploadImage: {
      server: `${rootUrl}/Base_Manage/Upload/UploadFileByForm`,
      fieldName: 'file',
      maxFileSize: 10 * 1024 * 1024,
      maxNumberOfFiles: 1,
      timeout: 3 * 60 * 1000,
      customInsert(res, insertFn) {
        insertFn(res.url, res.url, res.url)
      }
    }
  }
}))

const handleCreated = (editor) => {
  editorRef.value = editor
}

const handleChange = (editor) => {
  isChange = true
  emit('update:value', editor.getHtml())
}

// 监听外部value变化
watch(() => props.value, (val) => {
  if (!isChange && editorRef.value) {
    valueHtml.value = val || ''
  }
  isChange = false
})

// 组件销毁时，也及时销毁编辑器
onBeforeUnmount(() => {
  const editor = editorRef.value
  if (editor == null) return
  editor.destroy()
})
</script>

<script>
export default {
  name: 'WangEditor'
}
</script>

<style lang="less" scoped>
.ant-editor-wang {
  border: 1px solid #ccc;
  text-align: left;
}
</style>
