import { h, ref, computed, watch, onMounted } from 'vue'
import { useRoute, useRouter, RouterLink } from 'vue-router'
import { Menu } from 'ant-design-vue'
import * as Icons from '@ant-design/icons-vue'

export default {
  name: 'SMenu',
  props: {
    menu: {
      type: Array,
      required: true
    },
    theme: {
      type: String,
      default: 'dark'
    },
    mode: {
      type: String,
      default: 'inline'
    },
    collapsed: {
      type: Boolean,
      default: false
    }
  },
  emits: ['select'],
  setup(props, { emit }) {
    const route = useRoute()
    const router = useRouter()

    const openKeys = ref([])
    const selectedKeys = ref([])
    const cachedOpenKeys = ref([])

    const rootSubmenuKeys = computed(() => {
      return props.menu.map(item => item.path)
    })

    // 根据当前路由路径查找菜单项
    const findMenuByPath = (menus, path) => {
      for (const menu of menus) {
        if (menu.children && menu.children.length > 0) {
          for (const child of menu.children) {
            if (child.path === path) {
              return { parent: menu, child }
            }
          }
        }
      }
      return null
    }

    const updateMenu = () => {
      const currentPath = route.path

      // 查找当前路径对应的菜单
      const found = findMenuByPath(props.menu, currentPath)

      if (found) {
        selectedKeys.value = [found.child.path]
        if (props.mode === 'inline' && !props.collapsed) {
          openKeys.value = [found.parent.path]
        }
      } else {
        // 默认展开第一个菜单
        if (props.menu.length > 0 && props.mode === 'inline' && !props.collapsed) {
          openKeys.value = [props.menu[0].path]
        }
      }
    }

    watch(() => props.collapsed, (val) => {
      if (val) {
        cachedOpenKeys.value = openKeys.value.concat()
        openKeys.value = []
      } else {
        openKeys.value = cachedOpenKeys.value
      }
    })

    watch(() => route.path, () => {
      updateMenu()
    })

    onMounted(() => {
      updateMenu()
    })

    const onOpenChange = (keys) => {
      if (props.mode === 'horizontal') {
        openKeys.value = keys
        return
      }
      const latestOpenKey = keys.find(key => !openKeys.value.includes(key))
      if (!rootSubmenuKeys.value.includes(latestOpenKey)) {
        openKeys.value = keys
      } else {
        openKeys.value = latestOpenKey ? [latestOpenKey] : []
      }
    }

    const onSelect = (obj) => {
      selectedKeys.value = obj.selectedKeys
      emit('select', obj)
    }

    const renderIcon = (icon) => {
      if (!icon || icon === 'none') return null
      // 尝试从 Icons 中获取图标组件
      const iconName = icon.replace(/-([a-z])/g, (g) => g[1].toUpperCase())
      const capitalizedName = iconName.charAt(0).toUpperCase() + iconName.slice(1) + 'Outlined'
      const IconComponent = Icons[capitalizedName] || Icons[icon] || Icons['AppstoreOutlined']
      return IconComponent ? h(IconComponent) : null
    }

    const hasChildren = (menu) => {
      const result = menu.children && menu.children.length > 0
      return result
    }

    const renderMenuItem = (menu) => {
      const target = menu.meta?.target || null

      if (hasChildren(menu) && menu.hideChildrenInMenu) {
        menu.children.forEach(item => {
          item.meta = Object.assign(item.meta || {}, { hidden: true })
        })
      }

      const icon = renderIcon(menu.meta?.icon)
      const title = menu.meta?.title || ''

      if (target) {
        return h(Menu.Item, { key: menu.path }, {
          default: () => h('a', { href: menu.path, target }, [icon, h('span', null, title)])
        })
      }

      // 使用 RouterLink 组件进行导航
      return h(Menu.Item, { key: menu.path }, {
        default: () => h(RouterLink, { to: menu.path }, {
          default: () => [icon, h('span', null, title)]
        })
      })
    }

    const renderSubMenu = (menu) => {
      const children = []
      if (!menu.hideChildrenInMenu && hasChildren(menu)) {
        menu.children.forEach(item => {
          const rendered = renderItem(item)
          if (rendered) children.push(rendered)
        })
      }

      const icon = renderIcon(menu.meta?.icon)
      const title = menu.meta?.title || ''

      return h(Menu.SubMenu, { key: menu.path }, {
        title: () => [icon, h('span', null, title)],
        default: () => children
      })
    }

    const renderItem = (menu) => {
      if (menu.hidden) return null
      return hasChildren(menu) && !menu.hideChildrenInMenu
        ? renderSubMenu(menu)
        : renderMenuItem(menu)
    }

    return () => {
      const menuTree = props.menu
        .filter(item => !item.hidden)
        .map(item => renderItem(item))
        .filter(Boolean)

      return h(Menu, {
        mode: props.mode,
        theme: props.theme,
        openKeys: openKeys.value,
        selectedKeys: selectedKeys.value,
        onOpenChange,
        onSelect
      }, () => menuTree)
    }
  }
}
