import OperatorCache from "@/utils/cache/OperatorCache.js"

export default {
    install(app) {
        // Vue 3 使用 globalProperties
        app.config.globalProperties.$op = OperatorCache
        app.config.globalProperties.$hasPerm = OperatorCache.hasPermission.bind(OperatorCache)
    }
}