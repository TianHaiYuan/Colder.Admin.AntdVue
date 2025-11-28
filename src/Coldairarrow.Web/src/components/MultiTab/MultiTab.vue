<template>
  <div class="ant-pro-multi-tab">
    <div class="ant-pro-multi-tab-wrapper">
      <a-tabs
        :activeKey="activeKey"
        type="editable-card"
        hide-add
        :tabBarStyle="{ background: '#FFF', margin: 0, paddingLeft: '16px', paddingTop: '1px' }"
        @edit="onEdit"
        @change="onChange"
      >
        <a-tab-pane
          v-for="page in pages"
          :key="page.fullPath"
          :closable="pages.length > 1"
          style="height: 0"
        >
          <template #tab>
            <a-dropdown :trigger="['contextmenu']">
              <span style="user-select: none">{{ page.meta?.title }}</span>
              <template #overlay>
                <a-menu @click="({ key }) => closeMenuClick(key, page.fullPath)">
                  <a-menu-item key="closeThat">关闭当前标签</a-menu-item>
                  <a-menu-item key="closeRight">关闭右侧</a-menu-item>
                  <a-menu-item key="closeLeft">关闭左侧</a-menu-item>
                  <a-menu-item key="closeAll">关闭全部</a-menu-item>
                </a-menu>
              </template>
            </a-dropdown>
          </template>
        </a-tab-pane>
      </a-tabs>
    </div>
  </div>
</template>

<script setup>
import { ref, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import { message } from 'ant-design-vue'

const route = useRoute()
const router = useRouter()

const fullPathList = ref([])
const pages = ref([])
const activeKey = ref('')

// 初始化
onMounted(() => {
  pages.value.push({ ...route })
  fullPathList.value.push(route.fullPath)
  selectedLastPath()
})

const selectedLastPath = () => {
  activeKey.value = fullPathList.value[fullPathList.value.length - 1]
}

const onEdit = (targetKey, action) => {
  if (action === 'remove') {
    remove(targetKey)
  }
}

const onChange = (key) => {
  activeKey.value = key
  router.push({ path: key })
}

const remove = (targetKey) => {
  pages.value = pages.value.filter(page => page.fullPath !== targetKey)
  fullPathList.value = fullPathList.value.filter(path => path !== targetKey)
  if (!fullPathList.value.includes(activeKey.value)) {
    selectedLastPath()
  }
}

const closeThat = (e) => {
  if (fullPathList.value.length > 1) {
    remove(e)
  } else {
    message.info('这是最后一个标签了, 无法被关闭')
  }
}

const closeLeft = (e) => {
  const currentIndex = fullPathList.value.indexOf(e)
  if (currentIndex > 0) {
    const toRemove = fullPathList.value.slice(0, currentIndex)
    toRemove.forEach(item => remove(item))
  } else {
    message.info('左侧没有标签')
  }
}

const closeRight = (e) => {
  const currentIndex = fullPathList.value.indexOf(e)
  if (currentIndex < fullPathList.value.length - 1) {
    const toRemove = fullPathList.value.slice(currentIndex + 1)
    toRemove.forEach(item => remove(item))
  } else {
    message.info('右侧没有标签')
  }
}

const closeAll = (e) => {
  const currentIndex = fullPathList.value.indexOf(e)
  const toRemove = fullPathList.value.filter((_, index) => index !== currentIndex)
  toRemove.forEach(item => remove(item))
}

const closeMenuClick = (key, routePath) => {
  const actions = { closeThat, closeLeft, closeRight, closeAll }
  actions[key]?.(routePath)
}

watch(() => route.fullPath, (newPath) => {
  activeKey.value = newPath
  if (!fullPathList.value.includes(newPath)) {
    fullPathList.value.push(newPath)
    pages.value.push({ ...route })
  }
})
</script>

<script>
export default {
  name: 'MultiTab'
}
</script>
