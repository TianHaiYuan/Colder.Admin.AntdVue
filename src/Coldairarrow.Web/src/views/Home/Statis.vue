<!--需要复杂的请参考:https://preview.pro.loacg.com/dashboard/analysis-->
<template>
  <div class="page-header-index-wide">
    <a-row :gutter="24">
      <a-col :sm="24" :md="12" :xl="6" :style="{ marginBottom: '24px' }">
        <ChartCard :loading="loading" title="今日销售额" total="￥10,000">
          <template #action>
            <a-tooltip title="指标说明">
              <InfoCircleOutlined />
            </a-tooltip>
          </template>
        </ChartCard>
      </a-col>
      <a-col :sm="24" :md="12" :xl="6" :style="{ marginBottom: '24px' }">
        <ChartCard :loading="loading" title="昨日销售额" total="￥8,000">
          <template #action>
            <a-tooltip title="指标说明">
              <InfoCircleOutlined />
            </a-tooltip>
          </template>
        </ChartCard>
      </a-col>
      <a-col :sm="24" :md="12" :xl="6" :style="{ marginBottom: '24px' }">
        <ChartCard :loading="loading" title="本月销售额" total="￥300,000">
          <template #action>
            <a-tooltip title="指标说明">
              <InfoCircleOutlined />
            </a-tooltip>
          </template>
        </ChartCard>
      </a-col>
      <a-col :sm="24" :md="12" :xl="6" :style="{ marginBottom: '24px' }">
        <ChartCard :loading="loading" title="本年销售额" total="￥4000,000">
          <template #action>
            <a-tooltip title="指标说明">
              <InfoCircleOutlined />
            </a-tooltip>
          </template>
        </ChartCard>
      </a-col>
    </a-row>
  </div>
</template>

<script setup>
import { ref } from 'vue'
import { InfoCircleOutlined } from '@ant-design/icons-vue'
import ChartCard from '@/components/ChartCard/ChartCard.vue'

const loading = ref(false)
</script>

<script>
export default {
  name: 'StatisView'
}
</script>