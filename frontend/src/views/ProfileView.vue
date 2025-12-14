<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">User Profile</h1>

                <v-snackbar v-model="showError" color="error" timeout="3000" location="top right">
                    <v-icon start>mdi-alert-circle</v-icon>
                    {{ error }}
                </v-snackbar>

                <v-row justify="center">
                    <v-col cols="12" md="8" lg="6">
                        <v-card class="mb-4">
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

                        <v-card>
                            <v-card-title class="text-h5">Edit Profile Information</v-card-title>
                            <v-card-text>
                                <v-form @submit.prevent="handleUpdateProfile" ref="profileFormRef">
                                    <v-row>
                                        <v-col cols="12">
                                            <v-text-field v-model="profileForm.username" label="Username"
                                                prepend-inner-icon="mdi-account" variant="outlined"
                                                density="comfortable" :rules="[v => !!v || 'Username is required']" />
                                        </v-col>
                                        <v-col cols="12">
                                            <v-text-field v-model="profileForm.email" label="Email" type="email"
                                                prepend-inner-icon="mdi-email" variant="outlined" density="comfortable"
                                                :rules="[v => !!v || 'Email is required', v => /.+@.+\..+/.test(v) || 'Email must be valid']" />
                                        </v-col>
                                        <v-col cols="12" sm="6">
                                            <v-text-field v-model="profileForm.firstName" label="First Name"
                                                prepend-inner-icon="mdi-account-details" variant="outlined"
                                                density="comfortable" />
                                        </v-col>
                                        <v-col cols="12" sm="6">
                                            <v-text-field v-model="profileForm.lastName" label="Last Name"
                                                prepend-inner-icon="mdi-account-details" variant="outlined"
                                                density="comfortable" />
                                        </v-col>
                                    </v-row>

                                    <v-btn type="submit" color="primary" :loading="isUpdatingProfile"
                                        :disabled="isUpdatingProfile" block class="mt-2">
                                        <v-icon start>mdi-content-save</v-icon>
                                        Save Changes
                                    </v-btn>
                                </v-form>
                            </v-card-text>
                        </v-card>
                    </v-col>
                </v-row>

                <v-overlay v-if="isLoading" contained class="align-center justify-center">
                    <v-progress-circular color="primary" indeterminate size="64" />
                </v-overlay>

                <v-snackbar v-model="showError" color="error" timeout="3000" location="top right">
                    <v-icon start>mdi-alert-circle</v-icon>
                    {{ error }}
                </v-snackbar>

                <v-snackbar v-model="showSuccess" color="success" timeout="3000" location="top right">
                    <v-icon start>mdi-check-circle</v-icon>
                    {{ successMessage }}
                </v-snackbar>
            </v-container>
        </v-main>
    </v-layout>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { useAuthStore } from '@/stores/auth'
import profileService from '@/services/usersService'
import { extractErrorMessage } from '@/utils/errorHandler'
import type { Profile, User } from '@/types'

const authStore = useAuthStore()

const userProfile = ref<Profile | null>(null)
const userDetails = ref<User | null>(null)
const selectedFile = ref<File[] | File | null>(null)
const isLoading = ref(false)
const isUploadingAvatar = ref(false)
const isUpdatingProfile = ref(false)
const error = ref<string | null>(null)
const showError = ref(false)
const showSuccess = ref(false)
const successMessage = ref('')
const avatarForm = ref()
const profileFormRef = ref()
const profileForm = ref({
    username: '',
    email: '',
    firstName: '',
    lastName: ''
})

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
    if (!authStore.user?.id) return;

    isLoading.value = true;
    error.value = null;

    try {
        userProfile.value = await profileService.getProfile(authStore.user.id);

        userDetails.value = await profileService.getUser(authStore.user.id);

        if (userDetails.value) {
            profileForm.value = {
                username: userDetails.value.username || '',
                email: userDetails.value.email || '',
                firstName: userDetails.value.firstName || '',
                lastName: userDetails.value.lastName || ''
            };
        }
    } catch (err) {
        const errorDetails = extractErrorMessage(err);
        error.value = errorDetails.message;
        showError.value = true;
        console.error('Error loading profile:', err);
    } finally {
        isLoading.value = false;
    }
}

const handleAvatarUpload = async () => {
    if (!currentFile.value || !authStore.user?.id) return;

    const { valid } = await avatarForm.value.validate();
    if (!valid) return;

    isUploadingAvatar.value = true;
    error.value = null;

    try {
        await profileService.updateAvatar(
            currentFile.value,
            authStore.user.id
        );
        await loadProfile();

        selectedFile.value = null;
        successMessage.value = 'Avatar updated successfully!';
        showSuccess.value = true;

        window.dispatchEvent(new CustomEvent('avatar-updated', {
            detail: userProfile.value
        }));

        avatarForm.value.reset();
    } catch (err) {
        const errorDetails = extractErrorMessage(err);
        error.value = errorDetails.message;
        showError.value = true;
        console.error('Error updating avatar:', err);
    } finally {
        isUploadingAvatar.value = false;
    }
}

const handleUpdateProfile = async () => {
    if (!authStore.user?.id) return;

    const { valid } = await profileFormRef.value.validate();
    if (!valid) return;

    isUpdatingProfile.value = true;
    error.value = null;

    try {
        await profileService.updateProfile(authStore.user.id, {
            username: profileForm.value.username || null,
            email: profileForm.value.email || null,
            firstName: profileForm.value.firstName || null,
            lastName: profileForm.value.lastName || null
        });

        await loadProfile();

        if (authStore.user) {
            authStore.user.username = profileForm.value.username;
            authStore.user.email = profileForm.value.email;
            authStore.user.firstName = profileForm.value.firstName;
            authStore.user.lastName = profileForm.value.lastName;
        }

        successMessage.value = 'Profile updated successfully!';
        showSuccess.value = true;
    } catch (err) {
        const errorDetails = extractErrorMessage(err);
        error.value = errorDetails.message;
        showError.value = true;
        console.error('Error updating profile:', err);
    } finally {
        isUpdatingProfile.value = false;
    }
}

onMounted(() => {
    loadProfile();
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
