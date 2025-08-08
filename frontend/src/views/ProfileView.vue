<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">User Profile</h1>
                <v-row justify="center">
                    <v-col cols="12" md="8" lg="6">
                        <v-card>
                            <v-card-title class="text-h5 text-center">Profile</v-card-title>
                            <v-card-text class="text-center">
                                <v-avatar size="120" class="mb-4">
                                    <v-img v-if="userProfile?.avatar" :src="userProfile.avatar"
                                        :alt="userProfile.username" cover />
                                    <span v-else class="text-h2 font-weight-bold" :style="{ color: 'white' }">
                                        {{ getInitials(userProfile?.username) }}
                                    </span>
                                </v-avatar>

                                <div class="text-h6 mb-2">{{ userProfile?.username }}</div>
                                <div class="text-body-2 text-medium-emphasis mb-4">
                                    {{ getRoleDisplayName(authStore.user?.role) }}
                                </div>

                                <v-form @submit.prevent="handleAvatarUpload" ref="avatarForm">
                                    <v-file-input v-model="selectedFile" label="Choose new avatar" accept="image/*"
                                        prepend-icon="mdi-camera" variant="outlined" density="compact" class="mb-3"
                                        :rules="fileRules" show-size />

                                    <v-btn type="submit" color="primary" :loading="isUploadingAvatar"
                                        :disabled="!currentFile || isUploadingAvatar" block variant="outlined">
                                        <v-icon start>mdi-upload</v-icon>
                                        Update Avatar
                                    </v-btn>
                                </v-form>
                            </v-card-text>
                        </v-card>
                    </v-col>
                </v-row>

                <v-overlay v-if="isLoading" contained class="align-center justify-center">
                    <v-progress-circular color="primary" indeterminate size="64" />
                </v-overlay>

                <v-alert v-if="error" type="error" class="mt-4" dismissible @click:close="error = null">
                    {{ error }}
                </v-alert>

                <v-snackbar v-model="showSuccess" color="success" timeout="3000">
                    {{ successMessage }}
                </v-snackbar>
            </v-container>
        </v-main>
    </v-layout>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import profileService from '@/services/profileService'
import type { AvatarChangeResponse, Profile } from '@/types'

const authStore = useAuthStore()

const userProfile = ref<Profile | null>(null)
const selectedFile = ref<File[] | File | null>(null)
const isLoading = ref(false)
const isUploadingAvatar = ref(false)
const error = ref<string | null>(null)
const showSuccess = ref(false)
const successMessage = ref('')
const avatarForm = ref()

const currentFile = computed(() => {
    if (Array.isArray(selectedFile.value)) {
        return selectedFile.value[0] || null;
    } else if (selectedFile.value instanceof File) {
        return selectedFile.value;
    }
    return null;
})

const fileRules = [
    (files: File[] | File | null | undefined) => {
        let file: File | null = null;

        if (Array.isArray(files)) {
            file = files.length > 0 ? files[0] : null;
        } else if (files instanceof File) {
            file = files;
        }

        if (!file) return true;

        return file.size <= 5 * 1024 * 1024 || 'File size must be less than 5MB';
    },
    (files: File[] | File | null | undefined) => {
        let file: File | null = null;

        if (Array.isArray(files)) {
            file = files.length > 0 ? files[0] : null;
        } else if (files instanceof File) {
            file = files;
        }

        if (!file) return true;

        return file.type.startsWith('image/') || 'File must be an image';
    }
]

const getInitials = (username?: string): string => {
    if (!username) return 'U'
    return username.charAt(0).toUpperCase()
}

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

const loadProfile = async () => {
    if (!authStore.user?.id) return

    isLoading.value = true
    error.value = null

    try {
        userProfile.value = await profileService.getProfile(authStore.user.id)
    } catch (err) {
        error.value = 'Failed to load profile'
        console.error('Error loading profile:', err)
    } finally {
        isLoading.value = false
    }
}

const handleAvatarUpload = async () => {
    if (!currentFile.value || !authStore.user?.id) return

    const { valid } = await avatarForm.value.validate()
    if (!valid) return

    isUploadingAvatar.value = true
    error.value = null

    try {
        const updatedProfile = await profileService.updateAvatar(
            currentFile.value,
            authStore.user.id
        )

        // Pobierz aktualny profil z backend po udanej zmianie awatara
        await loadProfile()

        selectedFile.value = null
        successMessage.value = 'Avatar updated successfully!'
        showSuccess.value = true

        window.dispatchEvent(new CustomEvent('avatar-updated', {
            detail: userProfile.value
        }))

        avatarForm.value.reset()
    } catch (err) {
        error.value = 'Failed to update avatar'
        console.error('Error updating avatar:', err)
    } finally {
        isUploadingAvatar.value = false
    }
}

onMounted(() => {
    loadProfile()
})
</script>

<style scoped>
.v-avatar {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}

.v-avatar .v-img {
    border-radius: 50%;
}

.v-card {
    transition: all 0.3s ease;
}

.v-card:hover {
    transform: translateY(-2px);
    box-shadow: 0 8px 25px rgba(0, 0, 0, 0.15);
}

.v-file-input :deep(.v-field__input) {
    font-size: 0.875rem;
}

.v-progress-circular {
    margin: 2rem auto;
}

.v-overlay--contained {
    backdrop-filter: blur(4px);
    background: rgba(255, 255, 255, 0.8);
}

.theme--dark .v-overlay--contained {
    background: rgba(0, 0, 0, 0.8);
}
</style>
