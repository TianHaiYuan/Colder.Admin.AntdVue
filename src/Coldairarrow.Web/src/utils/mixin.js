import { computed } from 'vue'
import { storeToRefs } from 'pinia'
import { useAppStore } from '@/store/index.js'
import { deviceEnquire, DEVICE_TYPE } from '@/utils/device.js'

// Vue 3 Composable 替代 mixin
export function useAppSettings() {
  const appStore = useAppStore()
  const { layout, theme, color, weak, fixedHeader, fixSiderbar, contentWidth, autoHideHeader, sidebar, multiTab } = storeToRefs(appStore)

  const layoutMode = computed(() => layout.value)
  const navTheme = computed(() => theme.value)
  const primaryColor = computed(() => color.value)
  const colorWeak = computed(() => weak.value)
  const sidebarOpened = computed(() => sidebar.value)

  const isTopMenu = () => layoutMode.value === 'topmenu'
  const isSideMenu = () => !isTopMenu()

  return {
    layoutMode,
    navTheme,
    primaryColor,
    colorWeak,
    fixedHeader,
    fixSiderbar,
    fixSidebar: fixSiderbar,
    contentWidth,
    autoHideHeader,
    sidebarOpened,
    multiTab,
    isTopMenu,
    isSideMenu
  }
}

export function useDevice() {
  const appStore = useAppStore()
  const { device } = storeToRefs(appStore)

  const isMobile = () => device.value === DEVICE_TYPE.MOBILE
  const isDesktop = () => device.value === DEVICE_TYPE.DESKTOP
  const isTablet = () => device.value === DEVICE_TYPE.TABLET

  return {
    device,
    isMobile,
    isDesktop,
    isTablet
  }
}

// 保持向后兼容的 Options API mixin
const mixin = {
  computed: {
    layoutMode() {
      return useAppStore().layout
    },
    navTheme() {
      return useAppStore().theme
    },
    primaryColor() {
      return useAppStore().color
    },
    colorWeak() {
      return useAppStore().weak
    },
    fixedHeader() {
      return useAppStore().fixedHeader
    },
    fixSiderbar() {
      return useAppStore().fixSiderbar
    },
    fixSidebar() {
      return useAppStore().fixSiderbar
    },
    contentWidth() {
      return useAppStore().contentWidth
    },
    autoHideHeader() {
      return useAppStore().autoHideHeader
    },
    sidebarOpened() {
      return useAppStore().sidebar
    },
    multiTab() {
      return useAppStore().multiTab
    }
  },
  methods: {
    isTopMenu() {
      return this.layoutMode === 'topmenu'
    },
    isSideMenu() {
      return !this.isTopMenu()
    }
  }
}

const mixinDevice = {
  computed: {
    device() {
      return useAppStore().device
    }
  },
  methods: {
    isMobile() {
      return this.device === DEVICE_TYPE.MOBILE
    },
    isDesktop() {
      return this.device === DEVICE_TYPE.DESKTOP
    },
    isTablet() {
      return this.device === DEVICE_TYPE.TABLET
    }
  }
}

const AppDeviceEnquire = {
  mounted() {
    const appStore = useAppStore()
    deviceEnquire(deviceType => {
      switch (deviceType) {
        case DEVICE_TYPE.DESKTOP:
          appStore.toggleDevice('desktop')
          appStore.setSidebar(true)
          break
        case DEVICE_TYPE.TABLET:
          appStore.toggleDevice('tablet')
          appStore.setSidebar(false)
          break
        case DEVICE_TYPE.MOBILE:
        default:
          appStore.toggleDevice('mobile')
          appStore.setSidebar(true)
          break
      }
    })
  }
}

export { mixin, AppDeviceEnquire, mixinDevice }
