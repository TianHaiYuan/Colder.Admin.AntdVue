import { Axios } from "@/utils/plugin/axios-plugin.js"

let permissions = []
let inited = false

let OperatorCache = {
    info: {},
    inited() {
        return inited
    },
    // 改为 Promise 方式以支持 async/await
    init() {
        return new Promise((resolve, reject) => {
            if (inited) {
                resolve()
            } else {
                Axios.post('/Base_Manage/Home/GetOperatorInfo').then(resJson => {
                    if (resJson.Success) {
                        this.info = resJson.Data.UserInfo
                        permissions = resJson.Data.Permissions
                        inited = true
                        resolve()
                    } else {
                        reject(new Error(resJson.Msg))
                    }
                }).catch(reject)
            }
        })
    },
    hasPermission(thePermission) {
        return permissions.includes(thePermission)
    },
    clear() {
        inited = false
        permissions = []
        this.info = {}
    }
}

export default OperatorCache