<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <div class="d-flex align-center mb-6">
                    <h1 class="text-h4">My Team</h1>
                    <v-spacer></v-spacer>
                    <v-btn prepend-icon="mdi-refresh" variant="outlined" @click="refreshData">
                        Refresh
                    </v-btn>
                </div>

                <v-row v-if="team">
                    <v-col cols="12" lg="6">
                        <v-card class="h-100">
                            <v-card-title class="d-flex align-center">
                                <v-icon color="primary" icon="mdi-account-group" class="me-2"></v-icon>
                                Team Information
                            </v-card-title>
                            <v-card-text>
                                <div class="mb-4">
                                    <div class="text-subtitle-2 text-medium-emphasis mb-1">Team Name</div>
                                    <div class="text-h6">{{ team.name }}</div>
                                </div>
                                <div class="mb-4">
                                    <div class="text-subtitle-2 text-medium-emphasis mb-1">Manager</div>
                                    <div class="text-h6">{{ team.manager.username }}</div>
                                </div>
                                <v-alert type="info" variant="tonal" density="compact" class="mt-4">
                                    <div class="text-body-2">
                                        You can view your team details but cannot modify them.
                                        Please contact an administrator for any changes.
                                    </div>
                                </v-alert>
                            </v-card-text>
                        </v-card>
                    </v-col>

                    <v-col cols="12" lg="6">
                        <v-card class="h-100">
                            <v-card-title class="d-flex align-center">
                                <v-icon color="success" icon="mdi-account-multiple" class="me-2"></v-icon>
                                Team Developers
                                <v-spacer></v-spacer>
                                <v-chip color="primary" variant="tonal" size="small">
                                    {{ developers?.length || 0 }}
                                </v-chip>
                            </v-card-title>
                            <v-card-text>
                                <div v-if="loadingDevelopers" class="d-flex justify-center align-center py-8">
                                    <v-progress-circular color="primary" indeterminate size="32"></v-progress-circular>
                                </div>
                                <div v-else-if="developers && developers.length > 0">
                                    <v-list density="compact">
                                        <v-list-item v-for="developer in developers" :key="developer.id">
                                            <template v-slot:prepend>
                                                <v-avatar size="40" class="me-3">
                                                    <span class="text-subtitle-1 font-weight-bold"
                                                        :style="{ color: 'white' }">
                                                        {{ getInitials(developer.username) }}
                                                    </span>
                                                </v-avatar>
                                            </template>
                                            <v-list-item-title>{{ developer.username }}</v-list-item-title>
                                            <v-list-item-subtitle>{{ developer.firstName }} {{ developer.lastName
                                            }}</v-list-item-subtitle>
                                            <template v-slot:append>
                                                <v-chip color="success" variant="tonal" size="small">
                                                    Developer
                                                </v-chip>
                                            </template>
                                        </v-list-item>
                                    </v-list>
                                </div>
                                <div v-else class="text-center py-8">
                                    <v-icon color="grey" icon="mdi-account-off" size="48" class="mb-2"></v-icon>
                                    <div class="text-body-1 text-medium-emphasis">
                                        No developers assigned to this team
                                    </div>
                                </div>
                            </v-card-text>
                        </v-card>
                    </v-col>
                </v-row>

                <div v-else-if="loading" class="d-flex justify-center align-center" style="min-height: 300px;">
                    <v-progress-circular color="primary" indeterminate size="64"></v-progress-circular>
                </div>

                <v-card v-else>
                    <v-card-text class="text-center pa-8">
                        <v-icon color="grey" icon="mdi-account-group-outline" size="64" class="mb-4"></v-icon>
                        <div class="text-h6 mb-2">No Team Found</div>
                        <div class="text-body-1 text-medium-emphasis mb-4">
                            You are not currently assigned to manage any team.
                        </div>
                        <v-btn prepend-icon="mdi-refresh" variant="outlined" @click="refreshData">
                            Try Again
                        </v-btn>
                    </v-card-text>
                </v-card>
            </v-container>
        </v-main>
    </v-layout>
</template>

<script setup lang="ts">
import { watch } from 'vue';
import { useAsyncData } from '@/composables/useAsyncData';
import authService from '@/services/authService';
import teamService from '@/services/teamsService';
import usersService from '@/services/usersService';
import type { Team, User } from '@/types';
import { DevelopmentLogger } from '@/utils/logger';

const logger = new DevelopmentLogger({ prefix: '[ManagerTeamView]' });

const currentUser = authService.getStoredUser();

const {
    data: team,
    loading,
    load: refreshTeam
} = useAsyncData<Team | null>({
    fetchFunction: async (signal) => {
        if (currentUser?.id) {
            return await teamService.getTeamsByManager(currentUser.id, signal);
        } else {
            throw new Error('Manager user ID not found');
        }
    },
    loggerPrefix: '[ManagerTeamView]'
});

const {
    data: developers,
    loading: loadingDevelopers,
    load: loadDevelopers
} = useAsyncData<User[]>({
    fetchFunction: async (signal) => {
        if (team.value?.id) {
            return await usersService.getDevelopersByTeam(team.value.id, signal);
        } else {
            return [];
        }
    },
    loggerPrefix: '[ManagerTeamView]',
    autoLoad: false
});

// Watch for team changes to load developers
watch(team, (newTeam) => {
    if (newTeam?.id) {
        loadDevelopers();
    }
}, { immediate: true });

const getInitials = (username?: string): string => {
    if (!username) return 'U'
    return username.charAt(0).toUpperCase()
}

const refreshData = async () => {
    await refreshTeam();
    if (team.value?.id) {
        await loadDevelopers();
    }
};
</script>

<style scoped>
.v-avatar {
    background: linear-gradient(135deg, #667eea 0%, #764ba2 100%);
}
</style>
