import { createRouter, createWebHistory } from 'vue-router'
import { constantRouterMap } from '@/config/router.config.js'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  scrollBehavior: () => ({ top: 0 }),
  routes: constantRouterMap
})

export default router
