<template>
    <v-container fluid :class="isMobile ? 'pa-4' : 'pa-6'">
        <AdminDashboard v-if="user?.role === 'admin'" />
        <ManagerDashboard v-else-if="user?.role === 'manager'" />
        <DeveloperDashboard v-else-if="user?.role === 'developer'" />

        <div v-else class="text-center py-12">
            <v-icon size="64" color="primary" class="mb-4">mdi-view-dashboard</v-icon>
            <h2 class="text-h4 mb-4">Welcome to Dashboard</h2>
            <p class="text-body-1">Select an option from the sidebar to start working.</p>
        </div>
    </v-container>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { useDisplay } from 'vuetify'
import authService from '@/services/authService'
import AdminDashboard from '@/components/dashboard/AdminDashboard.vue'
import ManagerDashboard from '@/components/dashboard/ManagerDashboard.vue'
import DeveloperDashboard from '@/components/dashboard/DeveloperDashboard.vue'

const { mobile } = useDisplay()

const user = computed(() => authService.getStoredUser())
const isMobile = computed(() => mobile.value)
</script>

<style scoped></style>

<style scoped>
.dashboard-main {
    min-height: 100vh;
}

.mobile-header {
    position: fixed !important;
    top: 0;
    left: 0;
    right: 0;
    z-index: 1000;
}

@media (max-width: 960px) {
    .dashboard-main {
        padding-top: 56px;
    }

    .dashboard-main .v-container {
        padding-top: 1rem;
    }
}

@media (max-width: 600px) {
    .dashboard-main .v-container {
        padding-left: 8px;
        padding-right: 8px;
    }
}
</style>