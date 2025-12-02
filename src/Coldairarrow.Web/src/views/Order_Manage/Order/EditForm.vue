<template>
  <a-modal
	    title="编辑订单"
	    width="70%"
	    :open="visible"
	    :confirmLoading="confirmLoading"
	    @ok="handleSubmit"
	    @cancel="()=>{this.visible=false}"
	  >
	    <a-spin :spinning="confirmLoading">
	      <a-form ref="form" :model="entity" :rules="rules" v-bind="layout">
	        <a-row :gutter="16">
	          <a-col :span="8">
	            <a-form-item label="客户名称" name="CustomerName">
	              <a-input v-model:value="entity.CustomerName" autocomplete="off" />
	            </a-form-item>
	          </a-col>
	          <a-col :span="8">
	            <a-form-item label="客户电话" name="CustomerPhone">
	              <a-input v-model:value="entity.CustomerPhone" autocomplete="off" />
	            </a-form-item>
	          </a-col>
	          <a-col :span="8">
	            <a-form-item label="订单状态" name="Status">
	              <a-select v-model:value="entity.Status" placeholder="请选择">
	                <a-select-option :value="0">待确认</a-select-option>
	                <a-select-option :value="1">已确认</a-select-option>
	                <a-select-option :value="2">已发货</a-select-option>
	                <a-select-option :value="3">已完成</a-select-option>
	                <a-select-option :value="4">已取消</a-select-option>
	              </a-select>
	            </a-form-item>
	          </a-col>
	        </a-row>
	        <a-row :gutter="16">
	          <a-col :span="16">
	            <a-form-item label="收货地址" name="Address" :labelCol="{ span: 3 }" :wrapperCol="{ span: 20 }">
	              <a-input v-model:value="entity.Address" autocomplete="off" />
	            </a-form-item>
	          </a-col>
	          <a-col :span="8">
	            <a-form-item label="支付状态" name="PaymentStatus">
	              <a-select v-model:value="entity.PaymentStatus" placeholder="请选择">
	                <a-select-option :value="0">未支付</a-select-option>
	                <a-select-option :value="1">已支付</a-select-option>
	                <a-select-option :value="2">已退款</a-select-option>
	              </a-select>
	            </a-form-item>
	          </a-col>
	        </a-row>
	        <a-form-item label="备注" name="Remark" :labelCol="{ span: 2 }" :wrapperCol="{ span: 21 }">
	          <a-textarea v-model:value="entity.Remark" :rows="2" />
	        </a-form-item>

	        <a-divider>订单明细</a-divider>
	        <div style="margin-bottom: 16px;">
	          <a-button type="primary" size="small" @click="addDetail">添加产品</a-button>
	        </div>
	        <a-table :columns="detailColumns" :data-source="entity.Details" :pagination="false" size="small" bordered row-key="Id">
	          <template #bodyCell="{ column, record, index }">
	            <template v-if="column.dataIndex === 'ProductId'">
	              <a-select v-model:value="record.ProductId" placeholder="选择产品" style="width: 100%" @change="(val) => onProductChange(val, index)" show-search :filter-option="filterOption">
	                <a-select-option v-for="p in productList" :key="p.Id" :value="p.Id">{{ p.Name }}</a-select-option>
	              </a-select>
	            </template>
	            <template v-if="column.dataIndex === 'UnitPrice'">
	              <a-input-number v-model:value="record.UnitPrice" :min="0" :precision="2" style="width: 100%" @change="() => calcSubTotal(index)" />
	            </template>
	            <template v-if="column.dataIndex === 'Quantity'">
	              <a-input-number v-model:value="record.Quantity" :min="1" style="width: 100%" @change="() => calcSubTotal(index)" />
	            </template>
	            <template v-if="column.dataIndex === 'SubTotal'">{{ record.SubTotal }}</template>
	            <template v-if="column.dataIndex === 'action'">
	              <a @click="removeDetail(index)">删除</a>
	            </template>
	          </template>
	        </a-table>
	        <div style="text-align: right; margin-top: 16px; font-size: 16px; font-weight: bold;">
	          订单总金额: ¥{{ totalAmount }}
	        </div>
	      </a-form>
	    </a-spin>
	  </a-modal>
</template>

<script>
export default {
  props: { afterSubmit: { type: Function, default: null } },
  data() {
    return {
      layout: { labelCol: { span: 6 }, wrapperCol: { span: 17 } },
      visible: false, confirmLoading: false,
      entity: { Details: [] },
      productList: [],
      rules: {
        CustomerName: [{ required: true, message: '请输入客户名称' }],
        CustomerPhone: [{ required: true, message: '请输入客户电话' }]
      },
      detailColumns: [
        { title: '产品', dataIndex: 'ProductId', width: '30%' },
        { title: '单价', dataIndex: 'UnitPrice', width: '15%' },
        { title: '数量', dataIndex: 'Quantity', width: '15%' },
        { title: '小计', dataIndex: 'SubTotal', width: '15%' },
        { title: '操作', dataIndex: 'action', width: '10%' }
      ]
    }
  },
  computed: {
    totalAmount() {
      return this.entity.Details?.reduce((sum, d) => sum + (d.SubTotal || 0), 0).toFixed(2) || '0.00'
    }
  },
  methods: {
    filterOption(input, option) { return option.children[0].children.toLowerCase().indexOf(input.toLowerCase()) >= 0 },
	    async init() {
	      this.visible = true
	      this.entity = { Status: 0, PaymentStatus: 0, Details: [] }
	      this.$nextTick(() => { this.$refs['form'].clearValidate() })
	      const res = await this.$http.post('/Product_Manage/Product/GetDataList', { PageIndex: 1, PageRows: 1000, Search: { Status: 1 } })
      this.productList = res.Data || []
    },
    async openForm(id) {
	      await this.init()
	      if (id) {
	        this.confirmLoading = true
	        const resJson = await this.$http.post('/Order_Manage/Order/GetTheData', { id: id })
        this.confirmLoading = false
        this.entity = resJson.Data
      }
    },
    addDetail() {
      this.entity.Details.push({ Id: Date.now().toString(), ProductId: undefined, ProductName: '', UnitPrice: 0, Quantity: 1, SubTotal: 0 })
    },
    removeDetail(index) { this.entity.Details.splice(index, 1) },
    onProductChange(productId, index) {
      const product = this.productList.find(p => p.Id === productId)
      if (product) {
        this.entity.Details[index].ProductName = product.Name
        this.entity.Details[index].UnitPrice = product.Price
        this.calcSubTotal(index)
      }
    },
    calcSubTotal(index) {
      const d = this.entity.Details[index]
      d.SubTotal = ((d.UnitPrice || 0) * (d.Quantity || 0)).toFixed(2) * 1
    },
	    async handleSubmit() {
		      try {
		        await this.$refs['form'].validate()
		      } catch (e) {
		        // 校验未通过，直接返回
		        return
		      }
		      this.confirmLoading = true
		      this.$http.post('/Order_Manage/Order/SaveData', this.entity).then(resJson => {
	          this.confirmLoading = false
	          if (resJson.Success) { this.$message.success('操作成功!'); this.afterSubmit(); this.visible = false }
	          else { this.$message.error(resJson.Msg) }
	        })
	    }
  }
}
</script>

