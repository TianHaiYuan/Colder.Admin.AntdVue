<template>
  <a-select
    ref="selectRef"
    :allow-clear="allowClear"
    :show-search="showSearch"
    :filter-option="filterOption"
    @search="handleSearch"
    @change="handleChange"
    :mode="mode"
    v-model:value="thisValue"
  >
    <a-select-option v-for="item in thisOptions" :key="item.value" :value="item.value">{{ item.text }}</a-select-option>
  </a-select>
</template>

<script setup>
import { ref, watch, onMounted, getCurrentInstance } from 'vue'

const props = defineProps({
  value: { default: null },
  url: {
    // 远程获取选项接口地址,接口返回数据结构:[{value:'',text:''}]
    type: String,
    default: null
  },
  allowClear: {
    // 允许清空
    type: Boolean,
    default: true
  },
  searchMode: {
    // 搜索模式,'':关闭搜索,'local':本地搜索,'server':服务端搜索
    type: String,
    default: ''
  },
  options: {
    // 下拉项配置,若无url则必选,结构:[{value:'',text:''}]
    type: Array,
    default: () => []
  },
  multiple: {
    type: Boolean,
    default: false
  }
})

const emit = defineEmits(['update:value'])

const { proxy } = getCurrentInstance()

const selectRef = ref(null)
const thisOptions = ref([])
const mode = ref('')
const showSearch = ref(false)
const filterOption = ref(false)
const thisValue = ref('')
let timeout = null
let qGlobal = ''

const reload = (q) => {
  if (!props.url) {
    return
  }
  qGlobal = q
  clearTimeout(timeout)
  timeout = setTimeout(() => {
    let selected = []
    if (props.multiple && thisValue.value) {
      selected = Array.isArray(thisValue.value) ? thisValue.value : [thisValue.value]
    }
    proxy.$http
      .post(props.url, {
        q: q || '',
        selectedValues: selected || []
      })
      .then((resJson) => {
        if (resJson.Success && q === qGlobal) {
          thisOptions.value = resJson.Data
        }
      })
  }, 300)
}

const handleSearch = (value) => {
  reload(value)
}

const handleChange = (value) => {
  emit('update:value', value)
}

watch(() => props.value, (value) => {
  thisValue.value = value
})

onMounted(() => {
  mode.value = props.multiple ? 'multiple' : undefined
  if (props.searchMode) {
    showSearch.value = true
    if (props.searchMode === 'local') {
      filterOption.value = (input, option) => {
        const label = option.children?.()[0]?.children || ''
        return String(label).toLowerCase().indexOf(input.toLowerCase()) >= 0
      }
    } else {
      filterOption.value = false
    }
  }
  if (!props.url && props.options.length > 0) {
    thisOptions.value = props.options
  }
  thisValue.value = props.value
  reload()
})
</script>

<script>
export default {
  name: 'CSelect'
}
</script>
