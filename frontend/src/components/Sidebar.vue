<template>
    <v-navigation-drawer v-model="drawer" :rail="rail && !isMobile" :permanent="!isMobile" :temporary="isMobile"
        location="left" class="dashboard-sidebar" :width="isMobile ? 280 : 256">

        <div v-if="!isMobile && !rail" class="project-header pa-3">
            <router-link to="/dashboard" class="project-link">
                <v-icon class="me-2" color="white">mdi-anvil</v-icon>
                <span class="project-title">SprintForge</span>
            </router-link>
        </div>

        <div v-else-if="rail && !isMobile" class="d-flex justify-center pa-2 rail-anvil-header">
            <router-link to="/dashboard" title="SprintForge - Forge Your Projects">
                <v-btn variant="text" icon="mdi-anvil" @click.stop="rail = false" size="small" color="white"></v-btn>
            </router-link>
        </div>

        <div v-else-if="isMobile" class="mobile-sidebar-content pa-3">
        </div>

        <v-divider v-if="!rail && !isMobile"></v-divider>

        <div v-if="rail && !isMobile" class="d-flex justify-center pa-2 rail-menu-header">
            <v-btn variant="text" icon="mdi-menu" @click.stop="rail = false" title="Expand menu" size="small"></v-btn>
        </div>

        <v-list>
            <v-list-item>
                <template v-slot:prepend>
                    <v-avatar color="primary">
                        <v-icon>mdi-account</v-icon>
                    </v-avatar>
                </template>
                <v-list-item-title v-if="!rail || isMobile" class="text-h6">
                    {{ user?.username }}
                </v-list-item-title>
                <v-list-item-subtitle v-if="!rail || isMobile">
                    {{ getRoleDisplayName(user?.role) }}
                </v-list-item-subtitle>
                <template v-slot:append>
                    <v-btn variant="text" :icon="rail ? 'mdi-chevron-right' : 'mdi-chevron-left'"
                        @click.stop="rail = !rail" :title="rail ? 'Expand menu' : 'Collapse menu'"
                        v-if="!isMobile"></v-btn>
                    <v-btn variant="text" icon="mdi-close" @click.stop="drawer = false" title="Close menu"
                        v-else></v-btn>
                </template>
            </v-list-item>
        </v-list>

        <v-divider></v-divider>

        <v-list density="compact" nav>
            <v-list-item v-for="item in menuItems" :key="item.title" :to="item.to" :prepend-icon="item.icon"
                :title="item.title" :active="$route.path === item.to" @click="handleRouteChange"></v-list-item>
        </v-list>

        <template v-slot:append>
            <div class="pa-2">
                <!-- Theme selector -->
                <div class="mb-2">
                    <v-menu v-if="!rail || isMobile" offset-y>
                        <template v-slot:activator="{ props }">
                            <v-btn variant="outlined" block v-bind="props"
                                :prepend-icon="themeStore.getThemeIcon(themeStore.mode)">
                                {{ themeStore.getThemeDisplayName(themeStore.mode) }}
                                <v-icon class="ms-1">mdi-chevron-up</v-icon>
                            </v-btn>
                        </template>
                        <v-list>
                            <v-list-item v-for="themeOption in themeOptions" :key="themeOption.value"
                                @click="themeStore.setThemeMode(themeOption.value)"
                                :class="{ 'v-list-item--active': themeStore.mode === themeOption.value }">
                                <template v-slot:prepend>
                                    <v-icon>{{ themeOption.icon }}</v-icon>
                                </template>
                                <v-list-item-title>{{ themeOption.title }}</v-list-item-title>
                            </v-list-item>
                        </v-list>
                    </v-menu>

                    <v-menu v-else offset-y>
                        <template v-slot:activator="{ props }">
                            <v-btn variant="outlined" icon size="small" v-bind="props"
                                :title="`Theme: ${themeStore.getThemeDisplayName(themeStore.mode)}`">
                                <v-icon>{{ themeStore.getThemeIcon(themeStore.mode) }}</v-icon>
                            </v-btn>
                        </template>
                        <v-list>
                            <v-list-item v-for="themeOption in themeOptions" :key="themeOption.value"
                                @click="themeStore.setThemeMode(themeOption.value)"
                                :class="{ 'v-list-item--active': themeStore.mode === themeOption.value }">
                                <template v-slot:prepend>
                                    <v-icon>{{ themeOption.icon }}</v-icon>
                                </template>
                                <v-list-item-title>{{ themeOption.title }}</v-list-item-title>
                            </v-list-item>
                        </v-list>
                    </v-menu>
                </div>

                <!-- Logout button -->
                <v-btn v-if="rail && !isMobile" variant="outlined" icon="mdi-logout" @click="logout" title="Logout"
                    size="small" :loading="authStore.isLoggingOut" :disabled="authStore.isLoggingOut"></v-btn>
                <v-btn v-else variant="outlined" block @click="logout" prepend-icon="mdi-logout"
                    :loading="authStore.isLoggingOut" :disabled="authStore.isLoggingOut">
                    {{ authStore.isLoggingOut ? 'Logging out...' : 'Logout' }}
                </v-btn>
            </div>
        </template>
    </v-navigation-drawer>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { useRouter } from 'vue-router'
import { useDisplay } from 'vuetify'
import { useAuthStore } from '@/stores/auth'
import { useThemeStore, type ThemeMode } from '@/stores/theme'

const router = useRouter()
const { mobile } = useDisplay()
const authStore = useAuthStore()
const themeStore = useThemeStore()
const drawer = ref(true)
const rail = ref(false)

const isMobile = computed(() => mobile.value)

const user = computed(() => authStore.user)

const themeOptions = [
    { value: 'light' as ThemeMode, title: 'Light', icon: 'mdi-white-balance-sunny' },
    { value: 'dark' as ThemeMode, title: 'Dark', icon: 'mdi-moon-waning-crescent' },
    { value: 'system' as ThemeMode, title: 'System', icon: 'mdi-theme-light-dark' }
]

const initializeMobileState = () => {
    if (isMobile.value) {
        drawer.value = false
        rail.value = false
    } else {
        drawer.value = true
    }
}

const handleRouteChange = () => {
    if (isMobile.value) {
        drawer.value = false
    }
}

const allMenuItems = [
    // common
    {
        title: 'Dashboard',
        to: '/dashboard',
        icon: 'mdi-view-dashboard',
        roles: ['admin', 'manager', 'developer']
    },
    // admin
    {
        title: 'Companies',
        to: '/admin/companies',
        icon: 'mdi-domain',
        roles: ['admin']
    },
    {
        title: 'Projects',
        to: '/admin/projects',
        icon: 'mdi-folder-multiple',
        roles: ['admin']
    },
    {
        title: 'Task statuses',
        to: '/admin/statuses',
        icon: 'mdi-tag-multiple',
        roles: ['admin']
    },
    {
        title: 'Task types',
        to: '/admin/task-types',
        icon: 'mdi-shape',
        roles: ['admin']
    },
    {
        title: 'Team Management',
        to: '/admin/teams',
        icon: 'mdi-account-tie',
        roles: ['admin']
    },
    {
        title: 'Sprints',
        to: '/admin/sprints',
        icon: 'mdi-rocket-launch',
        roles: ['admin']
    },
    {
        title: 'Tasks',
        to: '/admin/tasks',
        icon: 'mdi-format-list-bulleted',
        roles: ['admin']
    },
    {
        title: 'Users',
        to: '/admin/users',
        icon: 'mdi-account-multiple',
        roles: ['admin']
    },
    // manager
    {
        title: 'My team Management',
        to: '/manager/teams',
        icon: 'mdi-account-tie',
        roles: ['manager']
    },
    {
        title: 'Sprints',
        to: '/manager/sprints',
        icon: 'mdi-rocket-launch',
        roles: ['manager']
    },
    {
        title: 'Tasks',
        to: '/manager/tasks',
        icon: 'mdi-format-list-bulleted',
        roles: ['manager']
    },
    // developer
    {
        title: 'Tasks',
        to: '/developer/tasks',
        icon: 'mdi-format-list-bulleted',
        roles: ['developer']
    },
    // common
    {
        title: 'Profile',
        to: '/profile',
        icon: 'mdi-account',
        roles: ['admin', 'manager', 'developer']
    }
]

const menuItems = computed(() => {
    if (!user.value) return []

    return allMenuItems.filter(item =>
        item.roles.includes(user.value!.role)
    )
})

const getRoleDisplayName = (role?: string): string => {
    switch (role) {
        case 'admin':
            return 'Administrator'
        case 'manager':
            return 'Manager'
        case 'developer':
            return 'Developer'
        default:
            return 'User'
    }
}

const logout = async () => {
    await authStore.logout()
    router.push('/login')
}

onMounted(() => {
    initializeMobileState()
})

defineExpose({
    drawer,
    toggleDrawer: () => {
        drawer.value = !drawer.value
    }
})
</script>

<style scoped>
.dashboard-sidebar {
    border-right: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}

.project-header {
    background: linear-gradient(135deg, #ff5722, #ff9800, #ffc107, #f44336);
    border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
    box-shadow: 0 2px 12px rgba(255, 87, 34, 0.4);
    position: relative;
    overflow: hidden;
}

.project-header::before {
    content: '';
    position: absolute;
    top: 0;
    left: -100%;
    width: 100%;
    height: 100%;
    background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.2), transparent);
    animation: flame-shimmer 3s infinite;
}

@keyframes flame-shimmer {
    0% {
        left: -100%;
    }

    100% {
        left: 100%;
    }
}

.rail-anvil-header {
    background: linear-gradient(135deg, #ff5722, #ff9800, #ffc107, #f44336);
    border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
    position: relative;
    overflow: hidden;
}

.rail-anvil-header::before {
    content: '';
    position: absolute;
    top: 0;
    left: -100%;
    width: 100%;
    height: 100%;
    background: linear-gradient(90deg, transparent, rgba(255, 255, 255, 0.2), transparent);
    animation: flame-shimmer 3s infinite;
}

.rail-menu-header {
    /* Bez ognistego tła dla ikony menu */
    background: transparent;
    border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}

.rail-menu-header .v-btn {
    color: rgb(var(--v-theme-on-surface)) !important;
}

.project-link {
    display: flex;
    align-items: center;
    text-decoration: none;
    color: #fff;
    transition: all 0.3s ease;
    padding: 8px 12px;
    border-radius: 8px;
    position: relative;
    z-index: 1;
    text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.5);
}

.project-link:hover {
    background: rgba(255, 255, 255, 0.15);
    transform: translateX(2px) scale(1.02);
    box-shadow: 0 2px 8px rgba(0, 0, 0, 0.3);
}

.project-title {
    font-size: 1.1rem;
    font-weight: 600;
    letter-spacing: 0.5px;
}

.mobile-sidebar-content {
    min-height: 0;
    padding: 0 !important;
}

.project-header .v-icon {
    filter: drop-shadow(1px 1px 2px rgba(0, 0, 0, 0.5));
    animation: anvil-glow 2s ease-in-out infinite alternate;
}

.rail-anvil-header .v-btn .v-icon {
    filter: drop-shadow(1px 1px 2px rgba(0, 0, 0, 0.5));
    animation: anvil-glow 2s ease-in-out infinite alternate;
}

.rail-menu-header .v-btn .v-icon {
    filter: drop-shadow(1px 1px 2px rgba(0, 0, 0, 0.3));
    color: rgb(var(--v-theme-on-surface)) !important;
}

@keyframes anvil-glow {
    0% {
        filter: drop-shadow(1px 1px 2px rgba(0, 0, 0, 0.5));
    }

    100% {
        filter: drop-shadow(1px 1px 4px rgba(255, 255, 255, 0.3)) drop-shadow(0 0 8px rgba(255, 193, 7, 0.4));
    }
}

.dashboard-sidebar:deep(.v-navigation-drawer--rail) {
    .v-list-item {
        justify-content: center;
    }

    .v-list-item__prepend {
        margin-inline-end: 0;
    }
}

@media (max-width: 960px) {
    .dashboard-sidebar {
        z-index: 1005 !important;
    }

    .dashboard-sidebar :deep(.v-navigation-drawer__content) {
        padding-bottom: env(safe-area-inset-bottom);
    }
}

.dashboard-sidebar :deep(.v-list-item-title) {
    font-size: 0.9rem;
    line-height: 1.2;
}

.dashboard-sidebar :deep(.v-list-item-subtitle) {
    font-size: 0.8rem;
    opacity: 0.7;
}

.dashboard-sidebar:deep(.v-navigation-drawer--rail) .v-btn {
    margin: 0 auto;
}
</style>
