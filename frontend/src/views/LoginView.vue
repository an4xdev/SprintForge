<template>
    <v-app>
        <v-main>
            <div class="login-background">
                <v-container class="fill-height" fluid>
                    <v-row align="center" justify="center" class="fill-height">
                        <v-col cols="12" sm="10" md="8" lg="6" xl="4">
                            <v-card class="login-card elevation-2" rounded="lg">
                                <div class="login-header text-center pa-8">
                                    <div class="logo-container mb-4">
                                        <v-avatar size="60" class="logo-avatar">
                                            <v-icon size="30" color="white">mdi-anvil</v-icon>
                                        </v-avatar>
                                    </div>
                                    <h1 class="text-h4 font-weight-medium mb-2">SprintForge</h1>
                                    <p class="text-body-1 text-medium-emphasis">Forge Your Projects with Agile Precision
                                    </p>
                                </div>

                                <div class="login-form-container pa-8">
                                    <h2 class="text-h6 text-center mb-6">Sign in to your account</h2>
                                    <LoginForm @login-success="handleLoginSuccess" />
                                </div>
                            </v-card>
                        </v-col>
                    </v-row>
                </v-container>
            </div>
        </v-main>
    </v-app>
</template>

<script setup lang="ts">
import { inject } from 'vue'
import LoginForm from '@/components/LoginForm.vue'
import type { User } from '@/types'

const showNotification = inject('showNotification') as Function

const handleLoginSuccess = (user: User) => {
    showNotification(`Welcome ${user.username}! Login successful.`, 'success')
}
</script>

<style scoped>
.login-background {
    min-height: 100vh;
    background: rgb(var(--v-theme-background));
    background-image:
        radial-gradient(circle at 20% 80%, rgba(var(--v-theme-primary), 0.05) 0%, transparent 50%),
        radial-gradient(circle at 80% 20%, rgba(var(--v-theme-primary), 0.03) 0%, transparent 50%);
    position: relative;
    overflow: hidden;
}

.login-card {
    backdrop-filter: blur(10px);
    background: rgb(var(--v-theme-surface)) !important;
    border: 1px solid rgba(var(--v-border-color), 0.12);
    overflow: hidden;
}

.login-header {
    background: linear-gradient(135deg, #ff5722, #ff9800, #ffc107, #f44336);
    border-bottom: 1px solid rgba(var(--v-border-color), 0.12);
    position: relative;
    overflow: hidden;
    color: white;
}

.login-header::before {
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

.login-header h1,
.login-header p {
    position: relative;
    z-index: 1;
    text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.5);
}

.logo-avatar {
    background: linear-gradient(135deg, #ff5722, #ff9800, #ffc107, #f44336) !important;
    border: 2px solid rgba(255, 255, 255, 0.3);
    box-shadow: 0 0 20px rgba(255, 87, 34, 0.4);
}

.logo-avatar .v-icon {
    filter: drop-shadow(1px 1px 2px rgba(0, 0, 0, 0.5));
    animation: anvil-glow 2s ease-in-out infinite alternate;
}

@keyframes anvil-glow {
    0% {
        filter: drop-shadow(1px 1px 2px rgba(0, 0, 0, 0.5));
    }

    100% {
        filter: drop-shadow(1px 1px 4px rgba(255, 255, 255, 0.3)) drop-shadow(0 0 8px rgba(255, 193, 7, 0.4));
    }
}

.login-form-container {
    background: transparent;
}

@media (max-width: 600px) {
    .login-header {
        padding: 2rem 1rem !important;
    }

    .login-form-container {
        padding: 2rem 1rem !important;
    }

    .login-card {
        margin: 1rem;
    }

    .logo-avatar {
        transform: scale(0.8);
    }
}
</style>