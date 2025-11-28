import { createApp } from 'vue'
import App from './App.vue'
import router from './router/index.js'
import { createPinia } from 'pinia'
import AxiosPlugin from '@/utils/plugin/axios-plugin.js'
import operatorPlugin from '@/utils/plugin/operator-plugin.js'

// Ant Design Vue
import Antd from 'ant-design-vue'
import 'ant-design-vue/dist/reset.css'

// dayjs 配置
import dayjs from 'dayjs'
import 'dayjs/locale/zh-cn'
dayjs.locale('zh-cn')

import bootstrap from './core/bootstrap.js'
import './permission.js' // permission control
import './utils/filter.js' // global filter

const app = createApp(App)
const pinia = createPinia()

app.use(pinia)
app.use(router)
app.use(Antd)
app.use(AxiosPlugin)
app.use(operatorPlugin)

// 执行 bootstrap
bootstrap()

app.mount('#app')
