<template>
    <v-container fluid class="fill-height">
        <v-row align="center" justify="center" class="text-center">
            <v-col cols="12" md="6">
                <div class="error-content">
                    <h1 class="display-1 font-weight-bold text-error mb-4">403</h1>
                    <h2 class="text-h4 mb-4">Access Denied</h2>
                    <p class="text-body-1 text-medium-emphasis mb-4">
                        Sorry, you don't have permission to access this page.
                    </p>
                    <p v-if="attemptedPath" class="text-body-2 text-medium-emphasis mb-2">
                        Attempted to access: <code class="text-error">{{ attemptedPath }}</code>
                    </p>
                    <p class="text-body-2 text-medium-emphasis mb-6">
                        Your current role: <strong>{{ getRoleDisplayName(userRole) }}</strong>
                    </p>

                    <div class="mb-6">
                        <v-icon size="120" color="error">mdi-shield-remove-outline</v-icon>
                    </div>

                    <v-alert v-if="requiredRoles.length > 0" type="info" variant="tonal" class="mb-6 text-start"
                        icon="mdi-information-outline">
                        <div class="text-body-2">
                            <strong>Required roles for this page:</strong>
                            <ul class="mt-2">
                                <li v-for="role in requiredRoles" :key="role">
                                    {{ getRoleDisplayName(role) }}
                                </li>
                            </ul>
                        </div>
                    </v-alert>

                    <div class="d-flex flex-column flex-sm-row gap-4 justify-center">
                        <v-btn color="primary" size="large" variant="flat" prepend-icon="mdi-home" @click="goHome">
                            Home Page
                        </v-btn>

                        <v-btn color="secondary" size="large" variant="outlined" prepend-icon="mdi-arrow-left"
                            @click="goBack">
                            Go Back
                        </v-btn>
                    </div>
                </div>
            </v-col>
        </v-row>
    </v-container>
</template>

<script setup lang="ts">
import { computed, onMounted, ref } from 'vue'
import { useRouter, useRoute } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const route = useRoute()
const authStore = useAuthStore()

const requiredRoles = ref<string[]>([])
const attemptedPath = ref<string>('')

const userRole = computed(() => authStore.user?.role || '')

const getRoleDisplayName = (role: string): string => {
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

const goHome = () => {
    router.push('/dashboard')
}

const goBack = () => {
    router.go(-1)
}

// Pobierz wymagane role z query parametrów
onMounted(() => {
    const roles = route.query.roles as string
    const attempted = route.query.attempted as string

    if (roles) {
        try {
            requiredRoles.value = JSON.parse(roles)
        } catch {
            requiredRoles.value = []
        }
    }

    if (attempted) {
        attemptedPath.value = attempted
    }
})
</script>

<style scoped>
.error-content {
    max-width: 600px;
    margin: 0 auto;
}

.error-content ul {
    list-style-type: disc;
    margin-left: 1.5rem;
}

.error-content li {
    margin-bottom: 0.25rem;
}
</style>
