<template>
  <v-app>
    <div v-if="showDebug"
      style="position: fixed; top: 0; right: 0; background: red; color: white; padding: 5px; z-index: 9999; font-size: 12px;">
      Auth: {{ authStore.isAuthenticated }} | Route: {{ route.path }} | Show Sidebar: {{ isAuthenticated }}
    </div>

    <v-btn v-if="isDev" @click="showDebug = !showDebug"
      style="position: fixed; bottom: 20px; right: 20px; z-index: 9998;" color="orange" size="small" icon
      :title="showDebug ? 'Hide debug' : 'Show debug'">
      <v-icon>{{ showDebug ? 'mdi-bug-stop' : 'mdi-bug' }}</v-icon>
    </v-btn>

    <v-layout v-if="isAuthenticated">
      <Sidebar ref="sidebarRef" />

      <v-main class="app-main">
        <v-app-bar v-if="isMobile" density="compact" class="mobile-header">
          <v-app-bar-nav-icon @click="toggleSidebar" color="white"></v-app-bar-nav-icon>
          <v-icon class="me-2" color="white">mdi-anvil</v-icon>
          <v-toolbar-title class="text-white text-h6 font-weight-bold">SprintForge</v-toolbar-title>
        </v-app-bar>

        <router-view :class="isMobile ? 'mobile-content' : ''" />
      </v-main>
    </v-layout>

    <v-main v-else>
      <router-view />
    </v-main>

    <v-overlay :model-value="overlayActive" class="align-center justify-center logout-overlay"
      :class="{ 'logout-active': authStore.isLoggingOut }">
      <div class="text-center logout-content">
        <v-progress-circular v-if="authStore.isLoggingOut" color="white" indeterminate size="80" width="6">
        </v-progress-circular>
        <v-progress-circular v-else color="primary" indeterminate size="64">
        </v-progress-circular>
        <div v-if="authStore.isLoggingOut" class="mt-6 text-h5 text-white font-weight-medium">
          Logging out...
        </div>
      </div>
    </v-overlay>

    <v-snackbar v-model="snackbar.show" :color="snackbar.color" :timeout="snackbar.timeout" location="top right">
      {{ snackbar.message }}

      <template v-slot:actions>
        <v-btn variant="text" @click="snackbar.show = false">
          Close
        </v-btn>
      </template>
    </v-snackbar>
  </v-app>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, provide, watch } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useDisplay } from 'vuetify'
import { useAuthStore } from '@/stores/auth'
import Sidebar from '@/components/Sidebar.vue'

const router = useRouter()
const route = useRoute()
const { mobile } = useDisplay()
const authStore = useAuthStore()
const sidebarRef = ref<InstanceType<typeof Sidebar> | null>(null)

const showDebug = ref(false)
const isDev = import.meta.env.DEV

const loading = ref(false)
const snackbar = ref({
  show: false,
  message: '',
  color: 'success',
  timeout: 4000
})

const isAuthenticated = computed(() => {
  return authStore.isAuthenticated && route.path !== '/login'
})

const overlayActive = computed(() => {
  if (authStore.isLoggingOut) return true
  const isLoginRoute = route.path === '/login'
  return loading.value || (!isLoginRoute && authStore.isLoading)
})

const isMobile = computed(() => mobile.value)

watch(() => authStore.isAuthenticated, (newValue) => {
  if (newValue && route.path === '/login') {
    router.push('/dashboard')
  } else if (!newValue && route.path !== '/login') {
    setTimeout(() => {
      if (!authStore.isAuthenticated && route.path !== '/login') {
        router.push('/login')
      }
    }, 100)
  }
})

const toggleSidebar = () => {
  if (sidebarRef.value) {
    sidebarRef.value.toggleDrawer()
  }
}

const setLoading = (state: boolean) => {
  loading.value = state
}

const showNotification = (message: string, color: 'success' | 'error' | 'warning' | 'info' = 'success') => {
  snackbar.value = {
    show: true,
    message,
    color,
    timeout: color === 'error' ? 6000 : 4000
  }
}

provide('setLoading', setLoading)
provide('showNotification', showNotification)

window.addEventListener('auth-error', () => {
  if (route.path === '/login') {
    return;
  }

  if (!authStore.isAuthenticated) {
    return;
  }

  showNotification('Session expired. You will be redirected to the login page.', 'warning')
  setTimeout(() => {
    if (route.path !== '/login') {
      router.push('/login')
    }
  }, 2000)
})

onMounted(() => {
  authStore.initializeAuth()

  if (!authStore.isAuthenticated && route.path !== '/login') {
    router.push('/login')
  }
})
</script>

<style>
.v-application {
  font-family: 'Roboto', sans-serif !important;
}

.v-main {
  min-height: 100vh;
}

.app-main {
  min-height: 100vh;
}

.mobile-header {
  position: fixed !important;
  top: 0;
  left: 0;
  right: 0;
  z-index: 1000;
  background: linear-gradient(135deg, #ff5722, #ff9800, #ffc107, #f44336) !important;
  border-bottom: 1px solid rgba(255, 255, 255, 0.1);
}

.mobile-header .v-toolbar-title {
  text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.5);
}

.mobile-header .v-icon {
  filter: drop-shadow(1px 1px 2px rgba(0, 0, 0, 0.5));
}

.mobile-content {
  padding-top: 56px;
}

@media (max-width: 960px) {
  .mobile-content {
    padding-top: 56px;
  }
}

::-webkit-scrollbar {
  width: 8px;
}

::-webkit-scrollbar-track {
  background: #f1f1f1;
}

::-webkit-scrollbar-thumb {
  background: #c1c1c1;
  border-radius: 4px;
}

::-webkit-scrollbar-thumb:hover {
  background: #a1a1a1;
}

.logout-overlay.logout-active {
  background: rgba(0, 0, 0, 1) !important;
  z-index: 9999 !important;
}

.logout-content {
  position: relative;
  z-index: 10000;
}

.logout-overlay.logout-active .v-overlay__content {
  background: none !important;
}
</style>