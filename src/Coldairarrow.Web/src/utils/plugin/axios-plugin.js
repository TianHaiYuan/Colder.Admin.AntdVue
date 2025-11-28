import axios from 'axios'
import TokenCache from '@/utils/cache/TokenCache.js'
import defaultSettings from '@/config/defaultSettings.js'
import ProcessHelper from '@/utils/helper/ProcessHelper.js'
import dayjs from 'dayjs'
import { v4 as uuidv4 } from 'uuid'
import md5 from 'md5'

const rootUrl = () => {
    if (ProcessHelper.isProduction() || ProcessHelper.isPreview()) {
        return defaultSettings.publishRootUrl
    } else {
        return defaultSettings.localRootUrl
    }
}

export const Axios = axios.create({
    baseURL: rootUrl(),
    timeout: defaultSettings.apiTimeout
})

// 在发送请求之前做某件事
Axios.interceptors.request.use(config => {
    // CheckSign签名检验
    let appId = defaultSettings.appId
    let appSecret = defaultSettings.appSecret
    let guid = uuidv4()
    let time = dayjs().format("YYYY-MM-DD HH:mm:ss")
    let body = ''
    if (config.data) {
        body = JSON.stringify(config.data)
    }
    let sign = md5(appId + time + guid + body + appSecret)

    config.headers.appId = appId
    config.headers.time = time
    config.headers.guid = guid
    config.headers.sign = sign

    // 携带token
    if (TokenCache.getToken()) {
        config.headers.Authorization = 'Bearer ' + TokenCache.getToken()
    }
    return config
}, error => {
    return Promise.reject(error)
})

// 返回状态判断(添加响应拦截器)
Axios.interceptors.response.use(res => {
    return res.data
}, error => {
    if (error && error.response) {
        switch (error.response.status) {
            case 400:
                error.message = '请求错误'
                break
            case 401:
                error.message = '未授权，请登录'
                TokenCache.deleteToken()
                location.href = '/'
                break
            case 403:
                error.message = '拒绝访问'
                break
            case 404:
                error.message = `请求地址出错: ${error.response.config.url}`
                break
            case 408:
                error.message = '请求超时'
                break
            case 500:
                error.message = '服务器内部错误'
                break
            case 501:
                error.message = '服务未实现'
                break
            case 502:
                error.message = '网关错误'
                break
            case 503:
                error.message = '服务不可用'
                break
            case 504:
                error.message = '网关超时'
                break
            case 505:
                error.message = 'HTTP版本不受支持'
                break
            default:
        }
    }

    return Promise.resolve({ Success: false, Msg: error.message })
})

export default {
    install(app) {
        // Vue 3 使用 globalProperties
        app.config.globalProperties.$http = Axios
        app.config.globalProperties.$rootUrl = rootUrl()
    }
}