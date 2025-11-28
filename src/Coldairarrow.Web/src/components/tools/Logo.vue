<template>
  <div class="logo">
    <router-link :to="{ path: '/' }">
      <img src="@/assets/logo.svg" alt="logo" />
      <h1 v-if="showTitle">{{ title }}</h1>
    </router-link>
  </div>
</template>

<script setup>
defineProps({
  title: {
    type: String,
    default: import.meta.env.VITE_APP_ProjectName || 'Admin'
  },
  showTitle: {
    type: Boolean,
    default: true
  }
})
</script>

<script>
export default {
  name: 'Logo'
}
</script>
