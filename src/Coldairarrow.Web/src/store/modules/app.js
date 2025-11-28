import { defineStore } from 'pinia'
import {
  SIDEBAR_TYPE,
  DEFAULT_THEME,
  DEFAULT_LAYOUT_MODE,
  DEFAULT_COLOR,
  DEFAULT_COLOR_WEAK,
  DEFAULT_FIXED_HEADER,
  DEFAULT_FIXED_SIDEMENU,
  DEFAULT_FIXED_HEADER_HIDDEN,
  DEFAULT_CONTENT_WIDTH_TYPE,
  DEFAULT_MULTI_TAB
} from '@/store/mutation-types'

// 简单的 localStorage 封装
const storage = {
  get(key) {
    const value = localStorage.getItem(key)
    try {
      return value ? JSON.parse(value) : null
    } catch {
      return value
    }
  },
  set(key, value) {
    localStorage.setItem(key, JSON.stringify(value))
  }
}

export const useAppStore = defineStore('app', {
  state: () => ({
    sidebar: true,
    device: 'desktop',
    theme: '',
    layout: '',
    contentWidth: '',
    fixedHeader: false,
    fixSiderbar: false,
    autoHideHeader: false,
    color: null,
    weak: false,
    multiTab: true
  }),

  getters: {
    sidebarOpened: (state) => state.sidebar,
    navTheme: (state) => state.theme,
    primaryColor: (state) => state.color,
  },

  actions: {
    setSidebar(type) {
      this.sidebar = type
      storage.set(SIDEBAR_TYPE, type)
    },
    closeSidebar() {
      storage.set(SIDEBAR_TYPE, true)
      this.sidebar = false
    },
    toggleDevice(device) {
      this.device = device
    },
    toggleTheme(theme) {
      storage.set(DEFAULT_THEME, theme)
      this.theme = theme
    },
    toggleLayoutMode(layout) {
      storage.set(DEFAULT_LAYOUT_MODE, layout)
      this.layout = layout
    },
    toggleFixedHeader(fixed) {
      if (!fixed) {
        this.autoHideHeader = false
        storage.set(DEFAULT_FIXED_HEADER_HIDDEN, false)
      }
      storage.set(DEFAULT_FIXED_HEADER, fixed)
      this.fixedHeader = fixed
    },
    toggleFixSiderbar(fixed) {
      storage.set(DEFAULT_FIXED_SIDEMENU, fixed)
      this.fixSiderbar = fixed
    },
    toggleFixedHeaderHidden(show) {
      storage.set(DEFAULT_FIXED_HEADER_HIDDEN, show)
      this.autoHideHeader = show
    },
    toggleContentWidth(type) {
      storage.set(DEFAULT_CONTENT_WIDTH_TYPE, type)
      this.contentWidth = type
    },
    toggleColor(color) {
      storage.set(DEFAULT_COLOR, color)
      this.color = color
    },
    toggleWeak(flag) {
      storage.set(DEFAULT_COLOR_WEAK, flag)
      this.weak = flag
    },
    toggleMultiTab(bool) {
      storage.set(DEFAULT_MULTI_TAB, bool)
      this.multiTab = bool
    }
  }
})
