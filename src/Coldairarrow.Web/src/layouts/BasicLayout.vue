<template>
  <a-layout :class="['layout', device]">
    <!-- SideMenu -->
    <a-drawer
      v-if="isMobile()"
      placement="left"
      :class="`drawer-sider ${navTheme}`"
      :closable="false"
      :open="collapsed"
      @close="drawerClose"
    >
      <SideMenu
        mode="inline"
        :menus="menus"
        :theme="navTheme"
        :collapsed="false"
        :collapsible="true"
        @menuSelect="menuSelect"
      />
    </a-drawer>

    <SideMenu
      v-else-if="isSideMenu()"
      mode="inline"
      :menus="menus"
      :theme="navTheme"
      :collapsed="collapsed"
      :collapsible="true"
    />

    <a-layout
      :class="[layoutMode, `content-width-${contentWidth}`]"
      :style="{ paddingLeft: contentPaddingLeft, minHeight: '100vh' }"
    >
      <!-- layout header -->
      <GlobalHeader
        :mode="layoutMode"
        :menus="menus"
        :theme="navTheme"
        :collapsed="collapsed"
        :device="device"
        @toggle="toggle"
      />

      <!-- layout content -->
      <a-layout-content
        :style="{ height: '100%', margin: '24px 24px 0', paddingTop: fixedHeader ? '64px' : '0' }"
      >
        <MultiTab v-if="multiTab" />
        <div class="content">
          <div class="page-header-index-wide">
            <slot>
              <router-view v-slot="{ Component }">
                <transition name="page-transition">
                  <keep-alive v-if="multiTab">
                    <component :is="Component" />
                  </keep-alive>
                  <component v-else :is="Component" />
                </transition>
              </router-view>
            </slot>
          </div>
        </div>
      </a-layout-content>
    </a-layout>
  </a-layout>
</template>

<script setup>
import { ref, computed, watch, onMounted, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { notification } from 'ant-design-vue'
import { triggerWindowResizeEvent } from '@/utils/util.js'
import { useAppSettings, useDevice } from '@/utils/mixin.js'
import config from '@/config/defaultSettings.js'

import RouteView from './RouteView.vue'
import MultiTab from '@/components/MultiTab/index.js'
import SideMenu from '@/components/Menu/SideMenu.vue'
import GlobalHeader from '@/components/GlobalHeader/index.js'
import GlobalFooter from '@/components/GlobalFooter/index.js'
import { getAddRouterRef } from '@/utils/routerUtil.js'
import { useAppStore } from '@/store/index.js'
import { setupApprovalNotification } from '@/utils/signalrClient.js'

const appStore = useAppStore()
const router = useRouter()
const { layoutMode, navTheme, fixedHeader, fixSiderbar, contentWidth, sidebarOpened, multiTab, isTopMenu, isSideMenu } = useAppSettings()
const { device, isMobile, isDesktop } = useDevice()

const production = config.production
const collapsed = ref(false)

// 动态主路由 - 使用响应式引用
const addRouterRef = getAddRouterRef()

// 菜单 - 使用计算属性从主路由中获取
const menus = computed(() => {
  const rootRoute = addRouterRef.value.find(item => item.path === '/')
  return rootRoute ? rootRoute.children : []
})

const contentPaddingLeft = computed(() => {
  if (!fixSiderbar.value || isMobile()) {
    return '0'
  }
  if (sidebarOpened.value) {
    return '200px'
  }
  return '80px'
})

watch(sidebarOpened, (val) => {
  collapsed.value = !val
})

// created
collapsed.value = !sidebarOpened.value

onMounted(() => {
  const userAgent = navigator.userAgent
  if (userAgent.indexOf('Edge') > -1) {
    nextTick(() => {
      collapsed.value = !collapsed.value
      setTimeout(() => {
        collapsed.value = !collapsed.value
      }, 16)
    })
  }
})

	// 启动审批消息的 SignalR 实时通知
	const handleApprovalEvent = (event) => {
	  if (!event) return
	  const eventType = event.EventType || event.eventType
	  const title = event.Title || ''
	  const stepName = event.StepName || ''
	  const businessType = event.BusinessType || event.businessType || ''
	  const businessId = event.BusinessId || event.businessId || ''
	  const status = event.Status || event.status || ''

	  let message = '审批消息'
	  let description = ''
	  const statusTextMap = {
	    Pending: '进行中',
	    Approved: '已通过',
	    Rejected: '已驳回',
	    Cancelled: '已取消',
	    Waiting: '未开始'
	  }
		  const businessDisplay = businessType === 'Product'
		    ? '产品审批'
		    : (businessType || '单据')

	  if (eventType === 'approval.step.pending') {
	    message = '您有新的审批待处理'
		    description = `${title || businessDisplay}${stepName ? ' - ' + stepName : ''}`
	  } else if (eventType === 'approval.completed') {
	    message = '审批结果通知'
	    const statusText = statusTextMap[status] || status || '已结束'
		    description = `${title || businessDisplay} 已完成，状态：${statusText}`
	  } else if (eventType === 'approval.step.approved') {
	    message = '审批步骤已通过'
		    description = `${title || businessDisplay}${stepName ? ' - ' + stepName : ''}`
	  } else if (eventType === 'approval.step.rejected') {
	    message = '审批步骤已被驳回'
		    description = `${title || businessDisplay}${stepName ? ' - ' + stepName : ''}`
	  } else {
		    description = `${eventType || ''} - ${title || businessDisplay || ''}`
	  }

	  const onClick = () => {
	    // 目前只对产品审批做深度跳转，后续可按业务类型扩展
	    if (!businessType || !businessId) return
	    if (businessType === 'Product') {
	      router.push({
	        path: '/Product_Manage/Product/List',
	        query: { id: businessId, fromNotice: '1' }
	      })
	    }
	  }

	  notification.info({
	    message,
	    description,
	    duration: 5,
	    onClick
	  })
	}

onMounted(() => {
  setupApprovalNotification(handleApprovalEvent)
})

const toggle = () => {
  collapsed.value = !collapsed.value
  appStore.setSidebar(!collapsed.value)
  triggerWindowResizeEvent()
}

const paddingCalc = () => {
  let left = ''
  if (sidebarOpened.value) {
    left = isDesktop() ? '200px' : '80px'
  } else {
    left = (isMobile() && '0') || (fixSiderbar.value && '80px') || '0'
  }
  return left
}

const menuSelect = () => {
  if (!isDesktop()) {
    collapsed.value = false
  }
}

const drawerClose = () => {
  collapsed.value = false
}
</script>

<script>
export default {
  name: 'BasicLayout'
}
</script>

<style lang="less">
@import url('../components/global.less');

/*
 * The following styles are auto-applied to elements with
 * transition="page-transition" when their visibility is toggled
 * by Vue.js.
 *
 * You can easily play with the page transition by editing
 * these styles.
 */

.page-transition-enter {
  opacity: 0;
}

.page-transition-leave-active {
  opacity: 0;
}

.page-transition-enter .page-transition-container,
.page-transition-leave-active .page-transition-container {
  -webkit-transform: scale(1.1);
  transform: scale(1.1);
}

.content {
  margin: 24px 0px 0;
  .link {
    margin-top: 16px;
    &:not(:empty) {
      margin-bottom: 16px;
    }
    a {
      margin-right: 32px;
      height: 24px;
      line-height: 24px;
      display: inline-block;
      i {
        font-size: 24px;
        margin-right: 8px;
        vertical-align: middle;
      }
      span {
        height: 24px;
        line-height: 24px;
        display: inline-block;
        vertical-align: middle;
      }
    }
  }
}
.page-menu-search {
  text-align: center;
  margin-bottom: 16px;
}
.page-menu-tabs {
  margin-top: 48px;
}

.extra-img {
  margin-top: -60px;
  text-align: center;
  width: 195px;

  img {
    width: 100%;
  }
}

.mobile {
  .extra-img {
    margin-top: 0;
    text-align: center;
    width: 96px;

    img {
      width: 100%;
    }
  }
}
</style>
