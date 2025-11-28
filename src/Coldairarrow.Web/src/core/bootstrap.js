import { useAppStore } from '@/store/index.js'
import {
  DEFAULT_COLOR,
  DEFAULT_THEME,
  DEFAULT_LAYOUT_MODE,
  DEFAULT_COLOR_WEAK,
  SIDEBAR_TYPE,
  DEFAULT_FIXED_HEADER,
  DEFAULT_FIXED_HEADER_HIDDEN,
  DEFAULT_FIXED_SIDEMENU,
  DEFAULT_CONTENT_WIDTH_TYPE,
  DEFAULT_MULTI_TAB
} from '@/store/mutation-types.js'
import config from '@/config/defaultSettings.js'

// 简单的 localStorage 封装
const storage = {
  get(key, defaultValue) {
    const value = localStorage.getItem(key)
    if (value === null) return defaultValue
    try {
      return JSON.parse(value)
    } catch {
      return value
    }
  }
}

export default function Initializer() {
  const appStore = useAppStore()

  appStore.setSidebar(storage.get(SIDEBAR_TYPE, true))
  appStore.toggleTheme(storage.get(DEFAULT_THEME, config.navTheme))
  appStore.toggleLayoutMode(storage.get(DEFAULT_LAYOUT_MODE, config.layout))
  appStore.toggleFixedHeader(storage.get(DEFAULT_FIXED_HEADER, config.fixedHeader))
  appStore.toggleFixSiderbar(storage.get(DEFAULT_FIXED_SIDEMENU, config.fixSiderbar))
  appStore.toggleContentWidth(storage.get(DEFAULT_CONTENT_WIDTH_TYPE, config.contentWidth))
  appStore.toggleFixedHeaderHidden(storage.get(DEFAULT_FIXED_HEADER_HIDDEN, config.autoHideHeader))
  appStore.toggleWeak(storage.get(DEFAULT_COLOR_WEAK, config.colorWeak))
  appStore.toggleColor(storage.get(DEFAULT_COLOR, config.primaryColor))
  appStore.toggleMultiTab(storage.get(DEFAULT_MULTI_TAB, config.multiTab))
}
