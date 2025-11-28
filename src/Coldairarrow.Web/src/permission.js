import router from '@/router/index.js'
import NProgress from 'nprogress' // progress bar
import 'nprogress/nprogress.css' // progress bar style
import { setDocumentTitle, domTitle } from '@/utils/domUtil.js'
import TokenCache from '@/utils/cache/TokenCache.js'
import OperatorCache from '@/utils/cache/OperatorCache.js'
import { initRouter } from '@/utils/routerUtil.js'
import defaultSettings from '@/config/defaultSettings.js'

NProgress.configure({ showSpinner: false }) // NProgress Configuration

const whiteList = ['Login'] // no redirect whitelist
let routerInited = false

router.beforeEach(async (to, from) => {
  NProgress.start() // start progress bar
  to.meta && (typeof to.meta.title !== 'undefined' && setDocumentTitle(`${to.meta.title} - ${domTitle}`))

  // 已授权
  if (TokenCache.getToken()) {
    try {
      await OperatorCache.init()

      if (to.path === '/Home/Login') {
        NProgress.done()
        return { path: defaultSettings.desktopPath || '/Home/Introduce' }
      }

      // 初始化动态路由（只执行一次）
      if (!routerInited) {
        await initRouter(to, from, router)
        routerInited = true
        // 路由添加完成后，重新导航到目标路由
        // 使用 fullPath 保留查询参数
        return { path: to.fullPath, replace: true }
      }

      return true
    } catch (error) {
      console.error('Permission error:', error)
      return true
    }
  } else {
    if (whiteList.includes(to.name)) {
      // 在免登录白名单，直接进入
      return true
    } else {
      NProgress.done()
      return { path: '/Home/Login', query: { redirect: to.fullPath } }
    }
  }
})

router.afterEach(() => {
  NProgress.done() // finish progress bar
})

// 导出重置函数，用于登出时重置状态
export const resetRouterState = () => {
  routerInited = false
}
