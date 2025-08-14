<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">My Teams</h1>

                <div class="py-1">
                    <v-sheet border rounded>
                        <v-data-table :headers="headers" :hide-default-footer="teams !== null && teams.length < 11"
                            :items="teams ?? []">
                            <template v-slot:top>
                                <v-toolbar flat>
                                    <v-toolbar-title>
                                        <v-icon color="medium-emphasis" icon="mdi-account-group" size="x-small"
                                            start></v-icon>
                                        My Teams
                                    </v-toolbar-title>

                                    <!-- Manager nie może dodawać nowych zespołów -->
                                    <v-chip class="me-2" color="info" variant="tonal">
                                        <v-icon start>mdi-information</v-icon>
                                        View Only - Contact admin to modify teams
                                    </v-chip>
                                </v-toolbar>
                            </template>

                            <template v-slot:item.name="{ value }">
                                <v-chip :text="value" border="thin opacity-25" prepend-icon="mdi-account-group" label>
                                    <template v-slot:prepend>
                                        <v-icon color="medium-emphasis"></v-icon>
                                    </template>
                                </v-chip>
                            </template>

                            <!-- Manager nie ma żadnych akcji edycji/usuwania -->
                            <template v-slot:item.actions="{ item }">
                                <div class="d-flex ga-2 justify-end">
                                    <v-icon color="medium-emphasis" icon="mdi-eye" size="small"
                                        @click="viewTeamDetails(item.id)"></v-icon>
                                </div>
                            </template>

                            <template v-slot:no-data>
                                <v-btn prepend-icon="mdi-backup-restore" rounded="lg" text="Refresh" variant="text"
                                    border @click="refreshTeams"></v-btn>
                            </template>
                        </v-data-table>
                    </v-sheet>

                    <!-- Dialog tylko do wyświetlania szczegółów zespołu -->
                    <v-dialog v-model="viewDialog" max-width="500">
                        <v-card title="Team Details">
                            <template v-slot:text>
                                <v-row>
                                    <v-col cols="12">
                                        <v-text-field v-model="viewModel.name" label="Team Name"
                                            readonly></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="viewModel.manager.id" label="Manager ID"
                                            readonly></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="viewModel.manager.username" label="Manager Username"
                                            readonly></v-text-field>
                                    </v-col>

                                    <v-col cols="12">
                                        <v-alert type="info" variant="tonal">
                                            <div class="text-subtitle-2 mb-1">Manager Access</div>
                                            <div class="text-body-2">
                                                You can view your team details but cannot modify them.
                                                Please contact an administrator for any changes.
                                            </div>
                                        </v-alert>
                                    </v-col>
                                </v-row>
                            </template>

                            <v-divider></v-divider>

                            <v-card-actions class="bg-surface-light">
                                <v-spacer></v-spacer>
                                <v-btn text="Close" @click="viewDialog = false"></v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-dialog>
                </div>
            </v-container>
        </v-main>
    </v-layout>
</template>

<script setup lang="ts">
import { useAsyncData } from '@/composables/useAsyncData';
import authService from '@/services/authService';
import teamService from '@/services/teamsService';
import type { MinimalUser, Team } from '@/types';
import { DevelopmentLogger } from '@/utils/logger';
import { ref } from 'vue';

const logger = new DevelopmentLogger({ prefix: '[ManagerTeamsView]' });

const headers = [
    { title: 'Name', key: 'name', align: "start" as const },
    {
        title: 'Manager', key: 'manager', children: [
            { title: 'ID', key: 'manager.id' },
            { title: 'Username', key: 'manager.username' },
        ]
    },
    { title: 'Actions', key: 'actions', align: 'end' as const, sortable: false }
];

const currentUser = authService.getStoredUser();

// Manager gets only his teams
const {
    data: teams,
    load: refreshTeams
} = useAsyncData<Team[]>({
    fetchFunction: (signal) => {
        if (currentUser?.id) {
            return teamService.getTeamsByManager(currentUser.id, signal);
        } else {
            throw new Error('Manager user ID not found');
        }
    },
    loggerPrefix: '[ManagerTeamsView]'
});

const viewDialog = ref(false);
const viewModel = ref(createEmptyRecord());

function createEmptyRecord() {
    return {
        id: '',
        name: '',
        manager: {
            id: '',
            username: '',
        } as MinimalUser
    } as Team;
}

function viewTeamDetails(id: string) {
    const team = teams.value?.find(t => t.id === id);
    if (team) {
        viewModel.value = { ...team };
        viewDialog.value = true;
    } else {
        logger.error(`Team with ID ${id} not found.`);
    }
}
</script>

<style scoped></style>
