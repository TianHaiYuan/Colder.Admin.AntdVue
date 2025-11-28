<template>
  <div :style="!$route.meta.hiddenHeaderContent ? null : null">
    <!-- pageHeader , route meta :true on hide -->
    <!-- <page-header v-if="!$route.meta.hiddenHeaderContent" :title="pageTitle" :logo="logo" :avatar="avatar">
      <slot slot="action" name="action"></slot>
      <slot slot="content" name="headerContent"></slot>
      <div slot="content" v-if="!this.$slots.headerContent && description">
        <p style="font-size: 14px;color: rgba(0,0,0,.65)">{{ description }}</p>
        <div class="link">
          <template v-for="(link, index) in linkList">
            <a :key="index" :href="link.href">
              <a-icon :type="link.icon" />
              <span>{{ link.title }}</span>
            </a>
          </template>
        </div>
      </div>
      <slot slot="extra" name="extra">
        <div class="extra-img">
          <img v-if="typeof extraImage !== 'undefined'" :src="extraImage"/>
        </div>
      </slot>
      <div slot="pageMenu">
        <div class="page-menu-search" v-if="search">
          <a-input-search
            style="width: 80%; max-width: 522px;"
            placeholder="请输入..."
            size="large"
            enterButton="搜索"
          />
        </div>
        <div class="page-menu-tabs" v-if="tabs && tabs.items">
          <a-tabs :tabBarStyle="{margin: 0}" :activeKey="tabs.active()" @change="tabs.callback">
            <a-tab-pane v-for="item in tabs.items" :tab="item.title" :key="item.key"></a-tab-pane>
          </a-tabs>
        </div>
      </div>
    </page-header> -->
    <div class="content">
      <div class="page-header-index-wide">
        <slot>
          <router-view v-slot="{ Component }">
            <keep-alive v-if="multiTab">
              <component :is="Component" ref="content" />
            </keep-alive>
            <component v-else :is="Component" ref="content" />
          </router-view>
        </slot>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUpdated } from 'vue'
import { useRoute } from 'vue-router'
import { useAppStore } from '@/store'
import PageHeader from '@/components/PageHeader/index.js'

const props = defineProps({
  avatar: {
    type: String,
    default: null
  },
  title: {
    type: [String, Boolean],
    default: true
  },
  logo: {
    type: String,
    default: null
  },
  directTabs: {
    type: Object,
    default: null
  }
})

const route = useRoute()
const appStore = useAppStore()

const content = ref(null)
const pageTitle = ref(null)
const description = ref(null)
const linkList = ref([])
const extraImage = ref('')
const search = ref(false)
const tabs = ref({})

const multiTab = ref(appStore.multiTab)

const getPageMeta = () => {
  pageTitle.value = (typeof props.title === 'string' || !props.title) ? props.title : route.meta?.title

  if (content.value) {
    if (content.value.pageMeta) {
      Object.assign({ pageTitle, description, linkList, extraImage, search, tabs }, content.value.pageMeta)
    } else {
      description.value = content.value.description
      linkList.value = content.value.linkList
      extraImage.value = content.value.extraImage
      search.value = content.value.search === true
      tabs.value = content.value.tabs
    }
  }
}

onMounted(() => {
  tabs.value = props.directTabs
  getPageMeta()
})

onUpdated(() => {
  getPageMeta()
})
</script>

<script>
export default {
  name: 'PageView'
}
</script>

<style lang="less" scoped>
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
    .extra-img{
      margin-top: 0;
      text-align: center;
      width: 96px;

      img{
        width: 100%;
      }
    }
  }
</style>
