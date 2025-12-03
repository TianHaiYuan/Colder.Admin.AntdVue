// eslint-disable-next-line
import { UserLayout, BasicLayout, RouteView, PageView } from '@/layouts/index.js'
import defaultSettings from '@/config/defaultSettings.js'

/**
 * 基础路由
 * @type { *[] }
 */
export const constantRouterMap = [
  {
    path: '/',
    redirect: '/Home/Login'
  },
  {
    path: '/Home',
    component: UserLayout,
    redirect: '/Home/Login',
    hidden: true,
    children: [
      {
        path: '/Home/Login',
        name: 'Login',
        component: () => import('@/views/Home/Login.vue')
      }
    ]
  },
  {
    path: '/404',
    name: '404',
    component: () => import('@/views/exception/404.vue')
  }
  // 注意：/:pathMatch(.*)*  路由在动态路由加载后添加，避免刷新时跳转404
]

/**
 * 动态路由 - 登录后根据权限动态添加
 */
export const asyncRouterMap = [
  {
    path: '/',
    name: 'index',
    component: BasicLayout,
    meta: { title: '首页' },
    redirect: defaultSettings.desktopPath || '/Home/Introduce',
    children: [
      // 首页
      {
        path: '/Home/Introduce',
        name: 'Introduce',
        component: () => import('@/views/Home/Introduce.vue'),
        meta: { title: '首页', icon: 'home', keepAlive: true }
      },
      // 系统管理
      {
        path: '/Base_Manage',
        name: 'Base_Manage',
        redirect: '/Base_Manage/Base_User/List',
        component: RouteView,
        meta: { title: '系统管理', icon: 'setting', keepAlive: true },
        children: [
          {
            path: '/Base_Manage/Base_User/List',
            name: 'Base_User_List',
            component: () => import('@/views/Base_Manage/Base_User/List.vue'),
            meta: { title: '用户管理', keepAlive: true }
          },
          {
            path: '/Base_Manage/Base_Role/List',
            name: 'Base_Role_List',
            component: () => import('@/views/Base_Manage/Base_Role/List.vue'),
            meta: { title: '角色管理', keepAlive: true }
          },
          {
            path: '/Base_Manage/Base_Department/List',
            name: 'Base_Department_List',
            component: () => import('@/views/Base_Manage/Base_Department/List.vue'),
            meta: { title: '部门管理', keepAlive: true }
          },
          {
            path: '/Base_Manage/Base_Action/List',
            name: 'Base_Action_List',
            component: () => import('@/views/Base_Manage/Base_Action/List.vue'),
            meta: { title: '权限管理', keepAlive: true }
          },
          {
            path: '/Base_Manage/Base_AppSecret/List',
            name: 'Base_AppSecret_List',
            component: () => import('@/views/Base_Manage/Base_AppSecret/List.vue'),
            meta: { title: '密钥管理', keepAlive: true }
          },
          {
            path: '/Base_Manage/Base_UserLog/List',
            name: 'Base_UserLog_List',
            component: () => import('@/views/Base_Manage/Base_UserLog/List.vue'),
            meta: { title: '操作日志', keepAlive: true }
          }
        ]
      },
      // 开发
      {
        path: '/Develop',
        name: 'Develop',
        redirect: '/Base_Manage/Base_DbLink/List',
        component: RouteView,
        meta: { title: '开发', icon: 'code', keepAlive: true },
        children: [
          {
            path: '/Base_Manage/Base_DbLink/List',
            name: 'Base_DbLink_List',
            component: () => import('@/views/Base_Manage/Base_DbLink/List.vue'),
            meta: { title: '数据库连接', keepAlive: true }
          },
          {
            path: '/Base_Manage/BuildCode/List',
            name: 'BuildCode_List',
            component: () => import('@/views/Base_Manage/BuildCode/List.vue'),
            meta: { title: '代码生成', keepAlive: true }
          }
        ]
      },
      // 消息中心
      {
        path: '/MessageCenter',
        name: 'MessageCenter',
        component: () => import('@/views/MessageCenter/Index.vue'),
        meta: { title: '消息中心', icon: 'bell', keepAlive: true }
      }
    ]
  }
]
 