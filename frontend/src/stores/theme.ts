import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export type ThemeMode = 'light' | 'dark' | 'system'

export const useThemeStore = defineStore('theme', () => {
    const mode = ref<ThemeMode>('system')

    const loadThemePreference = () => {
        const saved = localStorage.getItem('theme-mode')
        if (saved && ['light', 'dark', 'system'].includes(saved)) {
            mode.value = saved as ThemeMode
        }
    }

    const saveThemePreference = (newMode: ThemeMode) => {
        localStorage.setItem('theme-mode', newMode)
    }

    const systemPrefersDark = ref(window.matchMedia('(prefers-color-scheme: dark)').matches)

    const mediaQuery = window.matchMedia('(prefers-color-scheme: dark)')
    mediaQuery.addEventListener('change', (e) => {
        systemPrefersDark.value = e.matches
    })

    const actualTheme = computed(() => {
        if (mode.value === 'system') {
            return systemPrefersDark.value ? 'dark' : 'light'
        }
        return mode.value
    })

    const setThemeMode = (newMode: ThemeMode) => {
        mode.value = newMode
        saveThemePreference(newMode)
    }

    const getThemeDisplayName = (themeMode: ThemeMode): string => {
        switch (themeMode) {
            case 'light':
                return 'Light'
            case 'dark':
                return 'Dark'
            case 'system':
                return 'System'
            default:
                return 'System'
        }
    }

    const getThemeIcon = (themeMode: ThemeMode): string => {
        switch (themeMode) {
            case 'light':
                return 'mdi-white-balance-sunny'
            case 'dark':
                return 'mdi-moon-waning-crescent'
            case 'system':
                return 'mdi-theme-light-dark'
            default:
                return 'mdi-theme-light-dark'
        }
    }

    const initializeTheme = () => {
        loadThemePreference()
    }

    return {
        mode,
        actualTheme,
        systemPrefersDark,
        setThemeMode,
        getThemeDisplayName,
        getThemeIcon,
        initializeTheme
    }
})
