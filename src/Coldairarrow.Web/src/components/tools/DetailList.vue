<template>
  <a-descriptions :title="title" :column="col" :bordered="bordered">
    <slot></slot>
  </a-descriptions>
</template>

<script setup>
defineProps({
  title: {
    type: String,
    default: ''
  },
  col: {
    type: Number,
    default: 3
  },
  bordered: {
    type: Boolean,
    default: false
  }
})
</script>

<script>
export default {
  name: 'DetailList'
}
</script>
