<template>
  <transition name="showHeader">
    <div v-if="visible" class="header-animat">
      <a-layout-header
        v-if="visible"
        :class="[fixedHeader && 'ant-header-fixedHeader', sidebarOpened ? 'ant-header-side-opened' : 'ant-header-side-closed', ]"
        :style="{ padding: '0' }">
        <div v-if="mode === 'sidemenu'" class="header">
          <MenuFoldOutlined v-if="device==='mobile' && collapsed" class="trigger" @click="toggle"/>
          <MenuUnfoldOutlined v-else-if="device==='mobile'" class="trigger" @click="toggle"/>
          <MenuUnfoldOutlined v-else-if="collapsed" class="trigger" @click="toggle"/>
          <MenuFoldOutlined v-else class="trigger" @click="toggle"/>
          <UserMenu />
        </div>
        <div v-else :class="['top-nav-header-index', theme]">
          <div class="header-index-wide">
            <div class="header-index-left">
              <Logo class="top-nav-header" :show-title="device !== 'mobile'"/>
              <SMenu v-if="device !== 'mobile'" mode="horizontal" :menu="menus" :theme="theme" />
              <MenuFoldOutlined v-else-if="collapsed" class="trigger" @click="toggle"/>
              <MenuUnfoldOutlined v-else class="trigger" @click="toggle"/>
            </div>
            <UserMenu class="header-index-right" />
          </div>
        </div>
      </a-layout-header>
    </div>
  </transition>
</template>

<script setup>
import { ref, onMounted, onBeforeUnmount } from 'vue'
import { MenuFoldOutlined, MenuUnfoldOutlined } from '@ant-design/icons-vue'
import UserMenu from '../tools/UserMenu.vue'
import SMenu from '../Menu/index.js'
import Logo from '../tools/Logo.vue'
import { useAppSettings } from '@/utils/mixin.js'

const props = defineProps({
  mode: {
    type: String,
    default: 'sidemenu'
  },
  menus: {
    type: Array,
    required: true
  },
  theme: {
    type: String,
    default: 'dark'
  },
  collapsed: {
    type: Boolean,
    default: false
  },
  device: {
    type: String,
    default: 'desktop'
  }
})

const emit = defineEmits(['toggle'])

const { fixedHeader, autoHideHeader, sidebarOpened } = useAppSettings()

const visible = ref(true)
const oldScrollTop = ref(0)
let ticking = false

const handleScroll = () => {
  if (!autoHideHeader.value) {
    return
  }

  const scrollTop = document.body.scrollTop + document.documentElement.scrollTop
  if (!ticking) {
    ticking = true
    requestAnimationFrame(() => {
      if (oldScrollTop.value > scrollTop) {
        visible.value = true
      } else if (scrollTop > 300 && visible.value) {
        visible.value = false
      } else if (scrollTop < 300 && !visible.value) {
        visible.value = true
      }
      oldScrollTop.value = scrollTop
      ticking = false
    })
  }
}

const toggle = () => {
  emit('toggle')
}

onMounted(() => {
  document.addEventListener('scroll', handleScroll, { passive: true })
})

onBeforeUnmount(() => {
  document.body.removeEventListener('scroll', handleScroll, true)
})
</script>

<script>
export default {
  name: 'GlobalHeader'
}
</script>

<style lang="less">
@import '../index.less';

.header-animat{
  position: relative;
  z-index: @ant-global-header-zindex;
}
.showHeader-enter-active {
  transition: all 0.25s ease;
}
.showHeader-leave-active {
  transition: all 0.5s ease;
}
.showHeader-enter, .showHeader-leave-to {
  opacity: 0;
}
</style>
