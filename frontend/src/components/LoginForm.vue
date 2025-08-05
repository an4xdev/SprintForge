<template>
    <v-form ref="form" v-model="valid" lazy-validation>
        <v-text-field v-model="credentials.username" :rules="[rules.required]" label="Username" name="username"
            prepend-inner-icon="mdi-account" type="text" :disabled="authStore.isLoading" variant="outlined" class="mb-4"
            color="primary" hide-details="auto"></v-text-field>

        <v-text-field v-model="credentials.password" :rules="[rules.required]" label="Password" name="password"
            prepend-inner-icon="mdi-lock" :type="showPassword ? 'text' : 'password'"
            :append-inner-icon="showPassword ? 'mdi-eye' : 'mdi-eye-off'"
            @click:append-inner="showPassword = !showPassword" :disabled="authStore.isLoading" @keyup.enter="login"
            variant="outlined" class="mb-6" color="primary" hide-details="auto"></v-text-field>

        <v-btn :disabled="!valid || authStore.isLoading" :loading="authStore.isLoading" color="primary" @click="login"
            block size="large" rounded class="text-none mb-4">
            <v-icon left class="mr-2">mdi-login</v-icon>
            Sign In
        </v-btn>

        <v-alert v-if="error" type="error" variant="tonal" dismissible @click:close="error = ''" class="mt-4">
            {{ error }}
        </v-alert>
    </v-form>
</template>

<script setup lang="ts">
import { ref, reactive } from 'vue'
import { useAuthStore } from '@/stores/auth'
import type { LoginCredentials, User } from '@/types'
import { DevelopmentLogger } from '@/utils/logger'

const authStore = useAuthStore()
const logger = new DevelopmentLogger({ prefix: '[LoginForm]' });

const valid = ref(false)
const showPassword = ref(false)
const error = ref('')

const credentials = reactive<LoginCredentials>({
    username: '',
    password: ''
})

const rules = {
    required: (value: string) => !!value || 'This field is required'
}

const emit = defineEmits<{
    loginSuccess: [user: User]
}>()

const login = async () => {
    if (!valid.value) return;

    error.value = '';

    try {
        const user = await authStore.login(credentials);
        logger.log('User data after login:', user);
        logger.log('User role:', user?.role);
        logger.log('Auth store state:', authStore.isAuthenticated);

        if (user && user.role) {
            emit('loginSuccess', user);
        } else {
            logger.error('Missing user data or role:', { user, hasRole: !!user?.role });
            throw new Error('User role undefined');
        }
    } catch (err: any) {
        error.value = err.message || 'Error during login';
    }
};
</script>