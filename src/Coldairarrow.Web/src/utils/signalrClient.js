import * as signalR from '@microsoft/signalr'
import defaultSettings from '@/config/defaultSettings.js'
import OperatorCache from '@/utils/cache/OperatorCache.js'

let connection = null

function registerHandler(onEvent) {
  if (!connection || !onEvent) return

  connection.off('ApprovalEvent')
  connection.on('ApprovalEvent', (message) => {
    try {
      const data = typeof message === 'string' ? JSON.parse(message) : message
      onEvent(data)
    } catch (e) {
      // eslint-disable-next-line no-console
      console.error('解析审批事件失败', e)
    }
  })
}

export async function setupApprovalNotification(onEvent) {
  const hubUrl = defaultSettings.notificationHubUrl
  if (!hubUrl) {
    // eslint-disable-next-line no-console
    console.warn('未配置消息通知中心地址 (VUE_APP_NotificationHubUrl)，跳过 SignalR 连接')
    return
  }

  try {
    if (!OperatorCache.inited()) {
      await OperatorCache.init()
    }
  } catch (e) {
    // eslint-disable-next-line no-console
    console.error('初始化当前用户信息失败，无法建立 SignalR 连接', e)
    return
  }

  const user = OperatorCache.info || {}
  const userId = user.Id || user.id
  if (!userId) {
    // eslint-disable-next-line no-console
    console.warn('当前用户信息中缺少 Id，跳过 SignalR 连接')
    return
  }

  if (!connection) {
    connection = new signalR.HubConnectionBuilder()
      .withUrl(`${hubUrl}?userId=${encodeURIComponent(userId)}`)
      .withAutomaticReconnect()
      .build()

    connection.onreconnected(() => {
      // eslint-disable-next-line no-console
      console.info('SignalR 重新连接成功')
    })

    connection.onclose((err) => {
      if (err) {
        // eslint-disable-next-line no-console
        console.warn('SignalR 连接已关闭', err)
      }
    })

    try {
      await connection.start()
      // eslint-disable-next-line no-console
      console.info('SignalR 已连接到消息通知中心')
    } catch (e) {
      // eslint-disable-next-line no-console
      console.error('SignalR 连接失败', e)
      return
    }
  }

  // 注册事件处理
  registerHandler(onEvent)

  return connection
}

