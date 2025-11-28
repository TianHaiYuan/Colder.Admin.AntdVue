<template>
  <router-view v-slot="{ Component }">
    <keep-alive v-if="shouldKeepAlive">
      <component :is="Component" />
    </keep-alive>
    <component v-else :is="Component" />
  </router-view>
</template>

<script setup>
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import { useAppStore } from '@/store'

const props = defineProps({
  keepAlive: {
    type: Boolean,
    default: true
  }
})

const route = useRoute()
const appStore = useAppStore()

const shouldKeepAlive = computed(() => {
  const meta = route.meta || {}
  const multiTab = appStore.multiTab

  // 当开启了 multiTab 时，应当全部组件皆缓存
  if (!multiTab && !!meta.keepAlive) {
    return false
  }
  return props.keepAlive || multiTab || meta.keepAlive
})
</script>

<script>
export default {
  name: 'RouteView'
}
</script>
