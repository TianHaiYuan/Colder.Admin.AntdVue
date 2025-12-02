<template>
  <a-modal
	    title="编辑分类"
	    width="40%"
	    :open="visible"
	    :confirmLoading="confirmLoading"
	    @ok="handleSubmit"
	    @cancel="()=>{this.visible=false}"
	  >
	    <a-spin :spinning="confirmLoading">
	      <a-form ref="form" :model="entity" :rules="rules" v-bind="layout">
	        <a-form-item label="分类名称" name="Name">
	          <a-input v-model:value="entity.Name" autocomplete="off" />
	        </a-form-item>
	        <a-form-item label="父级分类" name="ParentId">
	          <a-tree-select
	            v-model:value="entity.ParentId"
	            :tree-data="treeData"
	            placeholder="请选择父级分类"
	            allow-clear
	            tree-default-expand-all
	            :field-names="{ label: 'title', value: 'key', children: 'children' }"
	          />
	        </a-form-item>
	        <a-form-item label="描述" name="Description">
	          <a-textarea v-model:value="entity.Description" :rows="3" />
	        </a-form-item>
	        <a-form-item label="排序" name="Sort">
	          <a-input-number v-model:value="entity.Sort" :min="0" style="width: 100%" />
	        </a-form-item>
	        <a-form-item label="是否启用" name="Enabled">
	          <a-switch v-model:checked="entity.Enabled" />
	        </a-form-item>
	      </a-form>
	    </a-spin>
	  </a-modal>
</template>

<script>
export default {
  props: {
    afterSubmit: {
      type: Function,
      default: null
    },
    treeData: {
      type: Array,
      default: () => []
    }
  },
  data() {
    return {
      layout: {
        labelCol: { span: 5 },
        wrapperCol: { span: 18 }
      },
      visible: false,
      confirmLoading: false,
      entity: {},
      rules: {
        Name: [{ required: true, message: '请输入分类名称' }]
      }
    }
  },
  methods: {
    init() {
      this.visible = true
      this.entity = { Sort: 0, Enabled: true }
      this.$nextTick(() => {
        this.$refs['form'].clearValidate()
      })
    },
	    openForm(id) {
	      this.init()
	
	      if (id) {
	        this.confirmLoading = true
	        this.$http.post('/Product_Manage/ProductCategory/GetTheData', { id: id }).then(resJson => {
          this.confirmLoading = false
          this.entity = resJson.Data
        })
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
		      this.$http.post('/Product_Manage/ProductCategory/SaveData', this.entity).then(resJson => {
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

