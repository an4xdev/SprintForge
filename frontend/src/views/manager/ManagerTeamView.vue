<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <div class="d-flex align-center mb-6">
                    <h1 class="text-h4">My Team</h1>
                    <v-spacer></v-spacer>
                    <v-btn prepend-icon="mdi-refresh" variant="outlined" @click="refreshTeam">
                        Refresh
                    </v-btn>
                </div>

                <v-row v-if="team">
                    <v-col cols="12" md="6">
                        <v-card>
                            <v-card-title class="d-flex align-center">
                                <v-icon color="primary" icon="mdi-account-group" class="me-2"></v-icon>
                                Team Information
                            </v-card-title>
                            <v-card-text>
                                <v-row>
                                    <v-col cols="12">
                                        <v-text-field v-model="team.name" label="Team Name"
                                            prepend-icon="mdi-account-group" readonly variant="outlined"></v-text-field>
                                    </v-col>
                                    <v-col cols="12">
                                        <v-text-field v-model="team.id" label="Team ID" prepend-icon="mdi-identifier"
                                            readonly variant="outlined"></v-text-field>
                                    </v-col>
                                </v-row>
                            </v-card-text>
                        </v-card>
                    </v-col>

                    <v-col cols="12" md="6">
                        <v-card>
                            <v-card-title class="d-flex align-center">
                                <v-icon color="success" icon="mdi-account-star" class="me-2"></v-icon>
                                Manager Information
                            </v-card-title>
                            <v-card-text>
                                <v-row>
                                    <v-col cols="12">
                                        <v-text-field v-model="team.manager.username" label="Manager Username"
                                            prepend-icon="mdi-account" readonly variant="outlined"></v-text-field>
                                    </v-col>
                                    <v-col cols="12">
                                        <v-text-field v-model="team.manager.id" label="Manager ID"
                                            prepend-icon="mdi-identifier" readonly variant="outlined"></v-text-field>
                                    </v-col>
                                </v-row>
                            </v-card-text>
                        </v-card>
                    </v-col>

                    <v-col cols="12">
                        <v-alert type="info" variant="tonal" prominent>
                            <template v-slot:prepend>
                                <v-icon icon="mdi-information"></v-icon>
                            </template>
                            <div class="text-subtitle-1 mb-2">Manager Access</div>
                            <div class="text-body-1">
                                You can view your team details but cannot modify them.
                                Please contact an administrator for any changes to team settings.
                            </div>
                        </v-alert>
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
                        <v-btn prepend-icon="mdi-refresh" variant="outlined" @click="refreshTeam">
                            Try Again
                        </v-btn>
                    </v-card-text>
                </v-card>
            </v-container>
        </v-main>
    </v-layout>
</template>

<script setup lang="ts">
import { useAsyncData } from '@/composables/useAsyncData';
import authService from '@/services/authService';
import teamService from '@/services/teamsService';
import type { Team } from '@/types';
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
</script>

<style scoped></style>
