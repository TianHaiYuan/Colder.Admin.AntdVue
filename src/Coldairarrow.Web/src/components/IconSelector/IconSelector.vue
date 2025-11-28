<template>
  <div :class="prefixCls">
    <a-tabs v-model:activeKey="currentTab" @change="handleTabChange">
      <a-tab-pane v-for="v in iconList" :tab="v.title" :key="v.key">
        <ul>
          <li
            v-for="(icon, key) in v.icons"
            :key="`${v.key}-${key}`"
            :class="{ 'active': selectedIcon === icon }"
            @click="handleSelectedIcon(icon)"
          >
            <component :is="getIconComponent(icon)" :style="{ fontSize: '36px' }" />
          </li>
        </ul>
      </a-tab-pane>
    </a-tabs>
  </div>
</template>

<script setup>
import { ref, watch, onMounted, computed } from 'vue'
import * as Icons from '@ant-design/icons-vue'
import icons from './icons'

const props = defineProps({
  prefixCls: {
    type: String,
    default: 'ant-pro-icon-selector'
  },
  value: {
    type: String,
    default: ''
  }
})

const emit = defineEmits(['change', 'update:value'])

const selectedIcon = ref(props.value || '')
const currentTab = ref('directional')
const iconList = icons

// 将 kebab-case 转换为 PascalCase 并添加 Outlined 后缀
const getIconComponent = (iconName) => {
  const pascalCase = iconName
    .split('-')
    .map(word => word.charAt(0).toUpperCase() + word.slice(1))
    .join('')
  const componentName = pascalCase + 'Outlined'
  return Icons[componentName] || Icons['QuestionOutlined']
}

const handleSelectedIcon = (icon) => {
  selectedIcon.value = icon
  emit('change', icon)
  emit('update:value', icon)
}

const handleTabChange = (activeKey) => {
  currentTab.value = activeKey
}

const autoSwitchTab = () => {
  icons.some(item => {
    if (item.icons.some(icon => icon === props.value)) {
      currentTab.value = item.key
      return true
    }
    return false
  })
}

watch(() => props.value, (val) => {
  selectedIcon.value = val
  autoSwitchTab()
})

onMounted(() => {
  if (props.value) {
    autoSwitchTab()
  }
})
</script>

<script>
export default {
  name: 'IconSelect'
}
</script>

<style lang="less" scoped>
  @import "../index.less";

  ul{
    list-style: none;
    padding: 0;
    overflow-y: scroll;
    height: 250px;

    li{
      display: inline-block;
      padding: @padding-sm;
      margin: 3px 0;
      border-radius: @border-radius-base;

      &:hover, &.active{
        cursor: pointer;
        color: @white;
        background-color: @primary-color;
      }
    }
  }
</style>
