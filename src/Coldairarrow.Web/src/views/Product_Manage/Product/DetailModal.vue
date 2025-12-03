<template>
  <a-modal
    title="产品详情"
    width="60%"
    :visible="visible"
    :footer="null"
    @cancel="() => { this.visible = false }"
  >
    <a-spin :spinning="loading">
      <a-descriptions title="基本信息" bordered :column="2">
        <a-descriptions-item label="产品编码">{{ product.ProductCode }}</a-descriptions-item>
        <a-descriptions-item label="产品名称">{{ product.Name }}</a-descriptions-item>
        <a-descriptions-item label="分类">{{ product.CategoryName }}</a-descriptions-item>
        <a-descriptions-item label="单价">
          <span style="color: #f5222d; font-weight: bold;">¥{{ product.Price }}</span>
        </a-descriptions-item>
        <a-descriptions-item label="库存">{{ product.Stock }}</a-descriptions-item>
        <a-descriptions-item label="单位">{{ product.Unit }}</a-descriptions-item>
        <a-descriptions-item label="状态">
          <a-tag :color="getStatusColor(product.Status)">{{ product.StatusText || '-' }}</a-tag>
        </a-descriptions-item>
        <a-descriptions-item label="创建时间">{{ product.CreateTime }}</a-descriptions-item>
        <a-descriptions-item label="备注" :span="2">{{ product.Remark || '-' }}</a-descriptions-item>
      </a-descriptions>

      <a-divider>审批进度</a-divider>
      <div v-if="approvalInstance">
        <p>
          当前审批状态：
          <a-tag :color="getInstanceStatusColor(approvalInstance.Status)">
            {{ getInstanceStatusText(approvalInstance.Status) }}
          </a-tag>
        </p>
        <a-timeline>
          <a-timeline-item
            v-for="step in approvalSteps"
            :key="step.Id"
            :color="getStepColor(step.Status)"
          >
            <p>{{ step.StepOrder }}. {{ step.StepName }}（{{ getStepStatusText(step.Status) }}）</p>
            <p v-if="step.ActionTime">处理时间：{{ step.ActionTime }}</p>
            <p v-if="step.Remark">备注：{{ step.Remark }}</p>
          </a-timeline-item>
        </a-timeline>
      </div>
      <div v-else>
        <a-empty description="暂无审批记录" />
      </div>

      <a-divider v-if="approvalInstance">审批操作</a-divider>
      <div v-if="approvalInstance">
        <div v-if="approvalInstance.Status === 'Pending'">
          <div v-if="canCurrentUserApprove">
            <a-space direction="vertical" style="width: 100%">
              <a-textarea
                v-model:value="handleRemark"
                :rows="3"
                placeholder="请输入审批意见（可选）"
              />
              <a-space>
                <a-button type="primary" @click="handleApprove" :loading="handleLoading">
                  审批通过
                </a-button>
                <a-button danger @click="handleReject" :loading="handleLoading">
                  驳回
                </a-button>
              </a-space>
            </a-space>
          </div>
          <a-alert
            v-else
            type="info"
            message="当前还有待审批步骤，但您不是本步骤审批人"
            show-icon
          />
        </div>
        <a-alert
          v-else
          type="info"
          :message="`当前流程已结束，状态：${getInstanceStatusText(approvalInstance.Status)}`"
          show-icon
        />
      </div>

      <a-divider>操作</a-divider>
      <a-space>
        <a-button type="primary" @click="submitApproval" :loading="submitLoading">
          提交审批
        </a-button>
      </a-space>
    </a-spin>
  </a-modal>
</template>

<script>
import axios from 'axios'
import defaultSettings from '@/config/defaultSettings.js'

export default {
  data() {
    return {
      visible: false,
      loading: false,
      submitLoading: false,
      handleLoading: false,
      handleRemark: '',
      product: {},
      approvalInstance: null,
      approvalSteps: []
    }
  },
  computed: {
    currentPendingStep() {
      if (!this.approvalSteps || !this.approvalSteps.length) return null
      return this.approvalSteps.find(x => x.Status === 'Pending') || null
    },
    canCurrentUserApprove() {
      if (!this.approvalInstance || this.approvalInstance.Status !== 'Pending') return false
      const step = this.currentPendingStep
      if (!step || !step.ApproverUserIds) return false
      const user = this.$op && this.$op.info
      const userId = user && (user.Id || user.id)
      if (!userId) return false
      const ids = step.ApproverUserIds.split(',')
        .map(x => x.trim())
        .filter(x => x)
      return ids.indexOf(userId) >= 0
    }
  },
  methods: {
    getStatusColor(status) {
      const colors = { 0: 'orange', 1: 'green', 2: 'red' }
      return colors[status] || 'default'
    },
    getInstanceStatusColor(status) {
      const map = {
        Pending: 'blue',
        Approved: 'green',
        Rejected: 'red',
        Cancelled: 'default',
        Waiting: 'default'
      }
      return map[status] || 'default'
    },
    getStepColor(status) {
      const map = {
        Pending: 'blue',
        Waiting: 'gray',
        Approved: 'green',
        Rejected: 'red',
        Cancelled: 'default'
      }
      return map[status] || 'default'
    },
    getInstanceStatusText(status) {
      const map = {
        Pending: '进行中',
        Approved: '已通过',
        Rejected: '已驳回',
        Cancelled: '已取消'
      }
      return map[status] || status || '-'
    },
    getStepStatusText(status) {
      const map = {
        Pending: '待审批',
        Waiting: '未开始',
        Approved: '已通过',
        Rejected: '已驳回',
        Cancelled: '已取消'
      }
      return map[status] || status || '-'
    },
    async openModal(id) {
      this.visible = true
      this.loading = true
      this.approvalInstance = null
      this.approvalSteps = []
      try {
        const [productRes] = await Promise.all([
          this.$http.post('/Product_Manage/Product/GetTheData', { id })
        ])
        this.product = productRes.Data || {}
        await this.loadApproval(this.product.Id)
      } finally {
        this.loading = false
      }
    },
    async loadApproval(productId) {
      const baseUrl = defaultSettings.approvalApiRootUrl
      if (!baseUrl) {
        return
      }
      try {
        const res = await axios.get(`${baseUrl}/api/approvals/Product/${productId}`)
        this.approvalInstance = res.data.instance
        this.approvalSteps = res.data.steps || []
      } catch (e) {
        if (e.response && e.response.status === 404) {
          this.approvalInstance = null
          this.approvalSteps = []
        } else {
          // eslint-disable-next-line no-console
          console.error('加载审批进度失败', e)
        }
      }
    },
    async submitApproval() {
      if (!this.product || !this.product.Id) {
        this.$message.error('请先选择产品')
        return
      }
      const baseUrl = defaultSettings.approvalApiRootUrl
      if (!baseUrl) {
        this.$message.error('未配置审批服务地址 (VUE_APP_ApprovalApiRootUrl)')
        return
      }
      const user = this.$op && this.$op.info
      const initiatorUserId = user && (user.Id || user.id)
      const initiatorUserName = user && (user.RealName || user.realName || user.UserName || user.userName)

      this.submitLoading = true
      try {
        await axios.post(`${baseUrl}/api/approvals/submit`, {
          BusinessType: 'Product',
          BusinessId: this.product.Id,
          Title: this.product.Name,
          InitiatorUserId: initiatorUserId,
          InitiatorUserName: initiatorUserName
        })
        this.$message.success('提交审批成功')
        await this.loadApproval(this.product.Id)
      } catch (e) {
        const msg = (e.response && e.response.data && e.response.data.message) || e.message || '提交审批失败'
        this.$message.error(`提交审批失败：${msg}`)
      } finally {
        this.submitLoading = false
      }
    },
    async handleApprove() {
      if (!this.approvalInstance) {
        this.$message.error('当前没有审批实例')
        return
      }
      const step = this.currentPendingStep
      if (!step) {
        this.$message.error('当前没有待审批的步骤')
        return
      }
      const baseUrl = defaultSettings.approvalApiRootUrl
      if (!baseUrl) {
        this.$message.error('未配置审批服务地址 (VUE_APP_ApprovalApiRootUrl)')
        return
      }
      const user = this.$op && this.$op.info
      const approverUserId = user && (user.Id || user.id)
      const approverUserName = user && (user.RealName || user.realName || user.UserName || user.userName)
      if (!approverUserId) {
        this.$message.error('当前登录用户信息缺失，无法执行审批')
        return
      }
      this.handleLoading = true
      try {
        await axios.post(`${baseUrl}/api/approvals/${this.approvalInstance.Id}/steps/${step.StepOrder}/approve`, {
          ApproverUserId: approverUserId,
          ApproverUserName: approverUserName,
          Remark: this.handleRemark
        })
        this.$message.success('审批通过')
        this.handleRemark = ''
        await this.loadApproval(this.product.Id)
      } catch (e) {
        const msg2 = (e.response && e.response.data && e.response.data.message) || e.message || '审批通过失败'
        this.$message.error(`审批通过失败：${msg2}`)
      } finally {
        this.handleLoading = false
      }
    },
    async handleReject() {
      if (!this.approvalInstance) {
        this.$message.error('当前没有审批实例')
        return
      }
      const step = this.currentPendingStep
      if (!step) {
        this.$message.error('当前没有待审批的步骤')
        return
      }
      const baseUrl = defaultSettings.approvalApiRootUrl
      if (!baseUrl) {
        this.$message.error('未配置审批服务地址 (VUE_APP_ApprovalApiRootUrl)')
        return
      }
      const user = this.$op && this.$op.info
      const approverUserId = user && (user.Id || user.id)
      const approverUserName = user && (user.RealName || user.realName || user.UserName || user.userName)
      if (!approverUserId) {
        this.$message.error('当前登录用户信息缺失，无法执行审批')
        return
      }
      this.handleLoading = true
      try {
        await axios.post(`${baseUrl}/api/approvals/${this.approvalInstance.Id}/steps/${step.StepOrder}/reject`, {
          ApproverUserId: approverUserId,
          ApproverUserName: approverUserName,
          Remark: this.handleRemark
        })
        this.$message.success('已驳回')
        this.handleRemark = ''
        await this.loadApproval(this.product.Id)
      } catch (e) {
        const msg2 = (e.response && e.response.data && e.response.data.message) || e.message || '审批驳回失败'
        this.$message.error(`审批驳回失败：${msg2}`)
      } finally {
        this.handleLoading = false
      }
    }
  }
}
</script>
