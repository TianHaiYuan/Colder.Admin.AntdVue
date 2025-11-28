<template>
  <a-layout-sider
    :class="['sider', isDesktop() ? null : 'shadow', theme, fixSiderbar ? 'ant-fixed-sidemenu' : null ]"
    width="200px"
    :collapsible="collapsible"
    :collapsed="collapsed"
    :trigger="null">
    <Logo />
    <SMenu
      :collapsed="collapsed"
      :menu="menus"
      :theme="theme"
      :mode="mode"
      @select="onSelect"
      style="padding: 16px 0px;" />
  </a-layout-sider>
</template>

<script setup>
import Logo from '@/components/tools/Logo.vue'
import SMenu from './index.js'
import { useAppSettings, useDevice } from '@/utils/mixin.js'

const props = defineProps({
  mode: {
    type: String,
    default: 'inline'
  },
  theme: {
    type: String,
    default: 'dark'
  },
  collapsible: {
    type: Boolean,
    default: false
  },
  collapsed: {
    type: Boolean,
    default: false
  },
  menus: {
    type: Array,
    required: true
  }
})

const emit = defineEmits(['menuSelect'])

const { fixSiderbar } = useAppSettings()
const { isDesktop } = useDevice()

const onSelect = (obj) => {
  emit('menuSelect', obj)
}
</script>

<script>
export default {
  name: 'SideMenu'
}
</script>
