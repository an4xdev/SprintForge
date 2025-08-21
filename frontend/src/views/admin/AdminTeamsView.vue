<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">Team Management</h1>

                <div class="py-1">
                    <v-sheet border rounded>
                        <v-data-table :headers="headers" :hide-default-footer="teams !== null && teams.length < 11"
                            :items="teams ?? []">
                            <template v-slot:top>
                                <v-toolbar flat>
                                    <v-toolbar-title>
                                        <v-icon color="medium-emphasis" icon="mdi-account-group" size="x-small"
                                            start></v-icon>
                                        All Teams
                                    </v-toolbar-title>

                                    <v-btn class="me-2" prepend-icon="mdi-plus" rounded="lg" text="Add a Team" border
                                        @click="addNewTeam"></v-btn>
                                </v-toolbar>
                            </template>

                            <template v-slot:item.name="{ value }">
                                <v-chip :text="value" border="thin opacity-25" prepend-icon="mdi-account-group" label>
                                    <template v-slot:prepend>
                                        <v-icon color="medium-emphasis"></v-icon>
                                    </template>
                                </v-chip>
                            </template>

                            <template v-slot:item.actions="{ item }">
                                <div class="d-flex ga-2 justify-end">
                                    <v-icon color="medium-emphasis" icon="mdi-pencil" size="small"
                                        @click="editTeam(item.id)"></v-icon>

                                    <v-icon color="medium-emphasis" icon="mdi-delete" size="small"
                                        @click="showDeleteConfirmation(item.id)"></v-icon>
                                </div>
                            </template>

                            <template v-slot:no-data>
                                <v-btn prepend-icon="mdi-backup-restore" rounded="lg" text="Refresh" variant="text"
                                    border @click="refreshTeams"></v-btn>
                            </template>
                        </v-data-table>
                    </v-sheet>

                    <v-dialog v-model="newEditDialog" max-width="500">
                        <v-card :title="`${isEditing ? 'Edit' : 'Add'} a Team`">
                            <template v-slot:text>
                                <v-row>
                                    <v-col cols="12">
                                        <v-text-field v-model="formModel.name" label="Name"></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.manager.id" label="Manager ID"></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.manager.username"
                                            label="Manager"></v-text-field>
                                    </v-col>
                                </v-row>
                            </template>

                            <v-divider></v-divider>

                            <v-card-actions class="bg-surface-light">
                                <v-btn text="Cancel" variant="plain" @click="newEditDialog = false"></v-btn>

                                <v-spacer></v-spacer>

                                <v-btn text="Save" @click="save"></v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-dialog>

                    <v-dialog v-model="confirmDeleteDialog" max-width="400px">
                        <v-card>
                            <v-card-title class="text-h6">Confirm Deletion</v-card-title>
                            <v-card-text>Are you sure you want to delete this team?</v-card-text>
                            <v-text-field v-model="teamNameToDelete" label="Team Name" readonly></v-text-field>
                            <v-divider></v-divider>
                            <v-card-actions class="bg-surface-light">
                                <v-btn text="Cancel" color="success" @click="cancelDelete"></v-btn>
                                <v-spacer></v-spacer>
                                <v-btn text="Confirm" color="error" @click="confirmDelete"></v-btn>
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
import teamService from '@/services/teamsService';
import type { MinimalUser, Team } from '@/types';
import { DevelopmentLogger } from '@/utils/logger';
import { ref, toRef } from 'vue';

const logger = new DevelopmentLogger({ prefix: '[AdminTeamsView]' });

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

// Admin always gets all teams
const {
    data: teams,
    load: refreshTeams
} = useAsyncData<Team[]>({
    fetchFunction: (signal) => teamService.getTeams(signal),
    loggerPrefix: '[AdminTeamsView]'
});

const newEditDialog = ref(false);
const confirmDeleteDialog = ref(false);
const formModel = ref(createNewRecord());
const teamNameToDelete = ref('');

function createNewRecord() {
    return {
        id: '',
        name: '',
        manager: {
            id: '',
            username: '',
        } as MinimalUser
    } as Team;
}
const isEditing = toRef(() => !!formModel.value.id)

function addNewTeam() {
    formModel.value = createNewRecord();
    newEditDialog.value = true;
}

function editTeam(id: string) {
    const team = teams.value?.find(t => t.id === id);
    if (team) {
        formModel.value = { ...team };
        newEditDialog.value = true;
    } else {
        logger.error(`Team with ID ${id} not found.`);
    }
}

function showDeleteConfirmation(id: string) {
    const team = teams.value?.find(t => t.id === id);
    if (team) {
        logger.log('Delete team:', team);
        teamNameToDelete.value = team.name;
        confirmDeleteDialog.value = true;
    } else {
        logger.error(`Team with ID ${id} not found.`);
    }
}

function confirmDelete() {
    if (teams.value == null) {
        logger.error('Teams data is not loaded yet.');
        return;
    }

    if (!teamNameToDelete.value) {
        logger.error('No team selected for deletion.');
        return;
    }

    teams.value = teams.value.filter(t => t.name !== teamNameToDelete.value);
    teamNameToDelete.value = '';

    logger.log(`Confirmed deletion of team: ${teamNameToDelete.value}`);
    confirmDeleteDialog.value = false;
}

function cancelDelete() {
    confirmDeleteDialog.value = false;
    teamNameToDelete.value = '';
}

function save() {
    if (teams.value === null) {
        logger.error('Teams data is not loaded yet.');
        return;
    }

    if (!formModel.value.name) {
        logger.error('Team name is required.');
        return;
    }

    if (isEditing.value) {
        const index = teams.value.findIndex(team => team.id === formModel.value.id)
        teams.value[index] = formModel.value

    } else {
        formModel.value.id = `${Date.now()}`; // fake
        formModel.value.manager.id = `${Date.now()}`; // fake
        formModel.value.manager.username = `user-${Date.now()}`; // fake
        teams.value.push(formModel.value)

    }

    newEditDialog.value = false
}
</script>

<style scoped></style>
