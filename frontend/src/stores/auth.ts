import { defineStore } from 'pinia'
import { ref, computed } from 'vue'
import authService from '@/services/authService'
import type { User, LoginCredentials } from '@/types'

export const useAuthStore = defineStore('auth', () => {
    const user = ref<User | null>(null)
    const isLoading = ref(false)
    const isLoggingOut = ref(false)
    const isLoggedIn = ref(false)

    const isAuthenticated = computed(() => {
        return isLoggedIn.value && user.value !== null
    })

    const login = async (credentials: LoginCredentials) => {
        isLoading.value = true
        try {
            await authService.login(credentials)
            user.value = authService.getStoredUser()
            isLoggedIn.value = true
            return user.value
        } catch (error) {
            throw error
        } finally {
            isLoading.value = false
        }
    }

    const logout = async () => {
        isLoggingOut.value = true

        try {
            await new Promise(resolve => setTimeout(resolve, 300))

            authService.logout()
            user.value = null
            isLoggedIn.value = false
        } finally {
            isLoggingOut.value = false
        }
    }

    const initializeAuth = () => {
        const storedUser = authService.getStoredUser()
        if (storedUser && authService.isAuthenticated()) {
            user.value = storedUser
            isLoggedIn.value = true
        } else {
            user.value = null
            isLoggedIn.value = false
        }
    }

    const hasRole = (role: string): boolean => {
        return user.value?.role === role
    }

    return {
        user,
        isLoading,
        isLoggingOut,
        isAuthenticated,
        isLoggedIn,
        login,
        logout,
        initializeAuth,
        hasRole
    }
})
