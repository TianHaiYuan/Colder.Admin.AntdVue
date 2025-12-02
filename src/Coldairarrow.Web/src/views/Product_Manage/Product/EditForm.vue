<template>
  <a-modal
	title="编辑产品"
	width="50%"
	:open="visible"
	:confirmLoading="confirmLoading"
	@ok="handleSubmit"
	@cancel="()=>{this.visible=false}"
	>
	  <a-spin :spinning="confirmLoading">
	    <a-form ref="form" :model="entity" :rules="rules" v-bind="layout">
	      <a-row :gutter="16">
	        <a-col :span="12">
	          <a-form-item label="产品编码" name="ProductCode">
	            <a-input v-model:value="entity.ProductCode" autocomplete="off" />
	          </a-form-item>
	        </a-col>
	        <a-col :span="12">
	          <a-form-item label="产品名称" name="Name">
	            <a-input v-model:value="entity.Name" autocomplete="off" />
	          </a-form-item>
	        </a-col>
	      </a-row>
	      <a-row :gutter="16">
	        <a-col :span="12">
	          <a-form-item label="产品分类" name="CategoryId">
	            <a-tree-select
	              v-model:value="entity.CategoryId"
	              :tree-data="categoryList"
	              placeholder="请选择分类"
	              allow-clear
	              tree-default-expand-all
	              :field-names="{ label: 'title', value: 'key', children: 'children' }"
	            />
	          </a-form-item>
	        </a-col>
	        <a-col :span="12">
	          <a-form-item label="计量单位" name="Unit">
	            <a-input v-model:value="entity.Unit" autocomplete="off" placeholder="如：个、件、kg等" />
	          </a-form-item>
	        </a-col>
	      </a-row>
	      <a-row :gutter="16">
	        <a-col :span="12">
	          <a-form-item label="单价" name="Price">
	            <a-input-number v-model:value="entity.Price" :min="0" :precision="2" style="width: 100%" />
	          </a-form-item>
	        </a-col>
	        <a-col :span="12">
	          <a-form-item label="成本价" name="CostPrice">
	            <a-input-number v-model:value="entity.CostPrice" :min="0" :precision="2" style="width: 100%" />
	          </a-form-item>
	        </a-col>
	      </a-row>
	      <a-row :gutter="16">
	        <a-col :span="12">
	          <a-form-item label="库存" name="Stock">
	            <a-input-number v-model:value="entity.Stock" :min="0" style="width: 100%" />
	          </a-form-item>
	        </a-col>
	        <a-col :span="12">
	          <a-form-item label="状态" name="Status">
	            <a-select v-model:value="entity.Status" placeholder="请选择状态">
	              <a-select-option :value="0">草稿</a-select-option>
	              <a-select-option :value="1">上架</a-select-option>
	              <a-select-option :value="2">下架</a-select-option>
	            </a-select>
	          </a-form-item>
	        </a-col>
	      </a-row>
		      <a-form-item label="图片地址" name="ImageUrl" :labelCol="{ span: 3 }" :wrapperCol="{ span: 20 }">
		        <a-input v-model:value="entity.ImageUrl" autocomplete="off" />
		      </a-form-item>
			  <a-form-item label="图片预览" :labelCol="{ span: 3 }" :wrapperCol="{ span: 20 }">
		        <a-image v-if="entity.ImageUrl" :src="entity.ImageUrl" :width="120" />
		      </a-form-item>
		      <a-form-item label="上传图片" :labelCol="{ span: 3 }" :wrapperCol="{ span: 20 }">
		        <a-upload
		          :customRequest="handleImageUpload"
		          :show-upload-list="false"
		          accept="image/*"
		        >
		          <a-button type="primary">上传图片</a-button>
		        </a-upload>
		      </a-form-item>
	      <a-form-item label="产品描述" name="Description" :labelCol="{ span: 3 }" :wrapperCol="{ span: 20 }">
	        <a-textarea v-model:value="entity.Description" :rows="3" />
	      </a-form-item>
	      <a-form-item label="排序" name="Sort" :labelCol="{ span: 3 }" :wrapperCol="{ span: 6 }">
	        <a-input-number v-model:value="entity.Sort" :min="0" style="width: 100%" />
	      </a-form-item>
	    </a-form>
	  </a-spin>
	</a-modal>
</template>

<script>
import axios from 'axios'
import defaultSettings from '@/config/defaultSettings.js'

export default {
  props: {
    afterSubmit: { type: Function, default: null },
    categoryList: { type: Array, default: () => [] }
  },
  data() {
    return {
      layout: { labelCol: { span: 6 }, wrapperCol: { span: 17 } },
      visible: false,
      confirmLoading: false,
      entity: {},
      rules: {
        ProductCode: [{ required: true, message: '请输入产品编码' }],
        Name: [{ required: true, message: '请输入产品名称' }],
        CategoryId: [{ required: true, message: '请选择产品分类' }],
        Price: [{ required: true, message: '请输入单价' }]
      }
    }
  },
  methods: {
    init() {
      this.visible = true
      this.entity = { Price: 0, CostPrice: 0, Stock: 0, Status: 0, Sort: 0 }
      this.$nextTick(() => { this.$refs['form'].clearValidate() })
    },
	    openForm(id) {
	      this.init()
	      if (id) {
	        this.confirmLoading = true
	        this.$http.post('/Product_Manage/Product/GetTheData', { id: id }).then(resJson => {
          this.confirmLoading = false
          this.entity = resJson.Data
        })
      }
	    },
	    async handleImageUpload({ file, onSuccess, onError }) {
	      const formData = new FormData()
	      formData.append('file', file)
	      formData.append('storage_id', defaultSettings.beeImgStorageId)
	      try {
	        const res = await axios.post(defaultSettings.beeImgUploadUrl, formData, {
	          headers: {
	            Authorization: `Bearer ${defaultSettings.beeImgToken}`,
	            Accept: 'application/json'
	          }
	        })
	        if (res.data && res.data.status === 'success' && res.data.data && res.data.data.public_url) {
	          this.entity.ImageUrl = res.data.data.public_url
	          this.$message.success('图片上传成功')
	          onSuccess && onSuccess(res.data)
	        } else {
	          const msg = (res.data && res.data.message) || '图片上传失败'
	          this.$message.error(msg)
	          onError && onError(new Error(msg))
	        }
	      } catch (e) {
	        this.$message.error('图片上传失败')
	        onError && onError(e)
	      }
	    },
		    async handleSubmit() {
		      try {
		        await this.$refs['form'].validate()
		      } catch (e) {
		        // 校验未通过，直接返回
		        return
		      }
		      this.confirmLoading = true
		      this.$http.post('/Product_Manage/Product/SaveData', this.entity).then(resJson => {
	          this.confirmLoading = false
	          if (resJson.Success) {
	            this.$message.success('操作成功!')
	            this.afterSubmit()
	            this.visible = false
	          } else {
	            this.$message.error(resJson.Msg)
	          }
	        })
	    }
  }
}
</script>

