<template>
  <a-modal
    title="订单详情"
    width="60%"
    :visible="visible"
    :footer="null"
    @cancel="() => { this.visible = false }"
  >
    <a-spin :spinning="loading">
      <a-descriptions title="订单信息" bordered :column="2">
        <a-descriptions-item label="订单编号">{{ order.OrderNo }}</a-descriptions-item>
        <a-descriptions-item label="订单状态">
          <a-tag :color="getStatusColor(order.Status)">{{ order.StatusText }}</a-tag>
        </a-descriptions-item>
        <a-descriptions-item label="客户名称">{{ order.CustomerName }}</a-descriptions-item>
        <a-descriptions-item label="客户电话">{{ order.CustomerPhone }}</a-descriptions-item>
        <a-descriptions-item label="收货地址" :span="2">{{ order.Address }}</a-descriptions-item>
        <a-descriptions-item label="支付状态">
          <a-tag :color="getPaymentStatusColor(order.PaymentStatus)">{{ order.PaymentStatusText }}</a-tag>
        </a-descriptions-item>
        <a-descriptions-item label="订单金额">
          <span style="color: #f5222d; font-size: 18px; font-weight: bold;">¥{{ order.TotalAmount }}</span>
        </a-descriptions-item>
        <a-descriptions-item label="创建时间">{{ order.CreateTime }}</a-descriptions-item>
        <a-descriptions-item label="支付时间">{{ order.PaymentTime || '-' }}</a-descriptions-item>
        <a-descriptions-item label="发货时间">{{ order.ShippingTime || '-' }}</a-descriptions-item>
        <a-descriptions-item label="完成时间">{{ order.CompleteTime || '-' }}</a-descriptions-item>
        <a-descriptions-item label="备注" :span="2">{{ order.Remark || '-' }}</a-descriptions-item>
      </a-descriptions>

	      <a-divider>订单明细</a-divider>
	      <a-table :columns="columns" :data-source="order.Details" :pagination="false" size="small" bordered row-key="Id">
	        <template #bodyCell="{ column, record }">
	          <template v-if="column.dataIndex === 'ProductImageUrl'">
	            <a-image v-if="record.ProductImageUrl" :src="record.ProductImageUrl" :width="60" :preview="false" />
	          </template>
	          <template v-else-if="column.dataIndex === 'SubTotal'">
	            <span style="color: #f5222d;">¥{{ record.SubTotal }}</span>
	          </template>
	        </template>
	      </a-table>

      <a-divider>操作</a-divider>
      <a-space>
        <a-button v-if="order.Status === 0" type="primary" @click="updateStatus(1)">确认订单</a-button>
        <a-button v-if="order.Status === 1" type="primary" @click="updateStatus(2)">发货</a-button>
        <a-button v-if="order.Status === 2" type="primary" @click="updateStatus(3)">完成订单</a-button>
        <a-button v-if="order.Status < 3" danger @click="updateStatus(4)">取消订单</a-button>
        <a-button v-if="order.PaymentStatus === 0" @click="updatePaymentStatus(1)">确认支付</a-button>
        <a-button v-if="order.PaymentStatus === 1" danger @click="updatePaymentStatus(2)">退款</a-button>
      </a-space>
    </a-spin>
  </a-modal>
</template>

<script>
export default {
  data() {
    return {
      visible: false,
      loading: false,
	      order: {},
	      columns: [
	        { title: '图片', dataIndex: 'ProductImageUrl', width: '15%' },
	        { title: '产品名称', dataIndex: 'ProductName', width: '25%' },
	        { title: '单价', dataIndex: 'UnitPrice', width: '15%' },
	        { title: '数量', dataIndex: 'Quantity', width: '15%' },
	        { title: '小计', dataIndex: 'SubTotal', width: '15%' },
	        { title: '备注', dataIndex: 'Remark', width: '15%' }
	      ]
    }
  },
  methods: {
    getStatusColor(status) {
      const colors = { 0: 'orange', 1: 'blue', 2: 'cyan', 3: 'green', 4: 'red' }
      return colors[status] || 'default'
    },
    getPaymentStatusColor(status) {
      const colors = { 0: 'orange', 1: 'green', 2: 'red' }
      return colors[status] || 'default'
    },
	    async openModal(id) {
      this.visible = true
      this.loading = true
	      try {
	        const resJson = await this.$http.post('/Order_Manage/Order/GetTheData', { id: id })
        this.order = resJson.Data || {}
      } finally {
        this.loading = false
      }
    },
	    async updateStatus(status) {
      this.loading = true
	      try {
	        const resJson = await this.$http.post('/Order_Manage/Order/UpdateStatus', { Id: this.order.Id, Status: status })
        if (resJson.Success) {
          this.$message.success('操作成功!')
          this.openModal(this.order.Id)
        } else {
          this.$message.error(resJson.Msg)
        }
      } finally {
        this.loading = false
      }
    },
	    async updatePaymentStatus(paymentStatus) {
      this.loading = true
	      try {
	        const resJson = await this.$http.post('/Order_Manage/Order/UpdatePaymentStatus', { Id: this.order.Id, PaymentStatus: paymentStatus })
        if (resJson.Success) {
          this.$message.success('操作成功!')
          this.openModal(this.order.Id)
        } else {
          this.$message.error(resJson.Msg)
        }
      } finally {
        this.loading = false
      }
    }
  }
}
</script>

