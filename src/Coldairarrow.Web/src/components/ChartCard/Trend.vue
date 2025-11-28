<template>
  <div class="chart-trend">
    {{ term }}
    <span>{{ rate }}%</span>
    <span :class="['trend-icon', trend]">
      <CaretUpOutlined v-if="trend === 'up'" />
      <CaretDownOutlined v-else />
    </span>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'
import { CaretUpOutlined, CaretDownOutlined } from '@ant-design/icons-vue'

const props = defineProps({
  term: {
    type: String,
    default: '',
    required: true
  },
  percentage: {
    type: Number,
    default: null
  },
  type: {
    type: Boolean,
    default: null
  },
  target: {
    type: Number,
    default: 0
  },
  value: {
    type: Number,
    default: 0
  },
  fixed: {
    type: Number,
    default: 2
  }
})

const trend = ref(props.type ? 'up' : 'down')
const rate = ref(props.percentage)

onMounted(() => {
  const type = props.type === null ? props.value >= props.target : props.type
  trend.value = type ? 'up' : 'down'
  rate.value = (props.percentage === null ? Math.abs(props.value - props.target) * 100 / props.target : props.percentage).toFixed(props.fixed)
})
</script>

<script>
export default {
  name: 'Trend'
}
</script>

<style lang="less" scoped>
  .chart-trend {
    display: inline-block;
    font-size: 14px;
    line-height: 22px;

    .trend-icon {
      font-size: 12px;

      &.up, &.down {
        margin-left: 4px;
        position: relative;
        top: 1px;

        i {
          font-size: 12px;
          transform: scale(.83);
        }
      }

      &.up {
        color: #f5222d;
      }
      &.down {
        color: #52c41a;
        top: -1px;
      }
    }
  }
</style>
