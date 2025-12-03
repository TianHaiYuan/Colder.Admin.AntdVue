import router from '@/router/index.js'
import { Axios } from '@/utils/plugin/axios-plugin.js'
import { BasicLayout, PageView } from '@/layouts/index.js'
import ProcessHelper from '@/utils/helper/ProcessHelper.js'
import defaultSettings from '@/config/defaultSettings.js'
import { v4 as uuidv4 } from 'uuid'
import { ref } from 'vue'

// 使用响应式变量存储路由
const addRouter = ref([])

export const getAddRouter = () => {
  return addRouter.value
}

export const getAddRouterRef = () => {
  return addRouter
}

// 前端未找到页面路由（固定不用改）
const notFoundRouter = {
  path: '/:pathMatch(.*)*',
  redirect: '/404',
  hidden: true
}

// 开发模式额外路由
const devRouter = [
  {
    title: '开发',
    icon: 'code',
    children: [
      {
        path: '/Base_Manage/Base_DbLink/List',
        title: '数据库连接'
      },
      {
        path: '/Base_Manage/BuildCode/List',
        title: '代码生成'
      },
      {
        path: '/Develop/IconSelectorView',
        title: '图标选择'
      },
      {
        path: '/Develop/UploadImg',
        title: '图片上传Demo'
      },
      {
        path: '/Develop/UploadFile',
        title: '文件上传Demo'
      },
      {
        path: '/Develop/Editor',
        title: '富文本Demo'
      },
      {
        path: '/Develop/SelectSearch',
        title: '下拉搜索Demo'
      }
    ]
  }
]

export const initRouter = async (to, from, routerInstance) => {
  const dynamicRouter = await generatorDynamicRouter()
  // Vue Router 4 使用 addRoute 逐个添加
  dynamicRouter.forEach(route => {
    routerInstance.addRoute(route)
  })
  addRouter.value = dynamicRouter
}

/**
 * 获取路由菜单信息
 */
const generatorDynamicRouter = async () => {
  const res = await getRouterByUser()
  let allRouters = []

  // 首页根路由
  let rootRouter = {
    path: '/',
    redirect: defaultSettings.desktopPath,
    name: uuidv4(),
    component: BasicLayout,
    meta: { title: '首页' },
    children: []
  }
  allRouters.push(rootRouter)

  if (!ProcessHelper.isProduction()) {
    res.push(...devRouter)
  }

  rootRouter.children = generator(res)

  // 固定追加“消息中心”路由，保证头像左侧消息入口可以正常跳转
  rootRouter.children.push({
    path: '/MessageCenter',
    name: uuidv4(),
    component: () => import('@/views/MessageCenter/Index.vue'),
    meta: { title: '消息中心', icon: 'bell' }
  })
  allRouters.push(notFoundRouter)

  return allRouters
}

/**
 * 获取后端路由信息的 axios API
 */
const getRouterByUser = async () => {
  try {
    const resJson = await Axios.post('/Base_Manage/Home/GetOperatorMenuList', {})
    if (resJson.Success) {
      return resJson.Data
    }
    return []
  } catch (error) {
    console.error('GetOperatorMenuList error:', error)
    return []
  }
}

// 动态导入视图组件的映射
const viewModules = import.meta.glob('@/views/**/*.vue')

/**
 * 格式化 后端 结构信息并递归生成层级路由表
 */
const generator = (routerMap) => {
  return routerMap.map(item => {
    let hasChildren = item.children && item.children.length > 0
    let component = null

    if (hasChildren) {
      component = PageView
    } else if (item.path) {
      // Vite 动态导入 - 尝试多种路径格式
      const possiblePaths = [
        `/src/views${item.path}.vue`,
        `../views${item.path}.vue`,
        `@/views${item.path}.vue`
      ]

      let foundPath = null
      for (const p of possiblePaths) {
        if (viewModules[p]) {
          foundPath = p
          break
        }
      }

      component = foundPath ? viewModules[foundPath] : (() => import('@/views/exception/404.vue'))
    }

    let currentRouter = {
      path: '',
      name: uuidv4(),
      component: component,
      meta: { title: item.title, icon: item.icon || undefined }
    }

    if (hasChildren) {
      currentRouter.path = `/${uuidv4()}`
    } else if (item.path) {
      currentRouter.path = item.path.replace('//', '/')
    }

    item.redirect && (currentRouter.redirect = item.redirect)

    if (hasChildren) {
      currentRouter.children = generator(item.children, currentRouter)
    }
    return currentRouter
  })
}
