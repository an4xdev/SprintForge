<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">{{ pageTitle }}</h1>

                <div class="py-1">
                    <v-sheet border rounded>
                        <v-data-table :headers="headers" :hide-default-footer="sprints !== null && sprints.length < 11"
                            :items="sprints ?? []">
                            <template v-slot:top>
                                <v-toolbar flat>
                                    <v-toolbar-title>
                                        <v-icon color="medium-emphasis" icon="mdi-rocket-launch" size="x-small"
                                            start></v-icon>
                                        {{ tableTitle }}
                                    </v-toolbar-title>

                                    <v-btn class="me-2" prepend-icon="mdi-plus" rounded="lg" text="Add a Sprint" border
                                        @click="addNewSprint"></v-btn>
                                </v-toolbar>
                            </template>

                            <template v-slot:item.name="{ value }">
                                <v-chip :text="value" border="thin opacity-25" prepend-icon="mdi-rocket-launch" label>
                                    <template v-slot:prepend>
                                        <v-icon color="medium-emphasis"></v-icon>
                                    </template>
                                </v-chip>
                            </template>

                            <template v-slot:item.actions="{ item }">
                                <div class="d-flex ga-2 justify-end">
                                    <v-icon color="medium-emphasis" icon="mdi-pencil" size="small"
                                        @click="editSprint(item.id)"></v-icon>

                                    <v-icon v-if="isAdmin" color="medium-emphasis" icon="mdi-delete" size="small"
                                        @click="showDeleteConfirmation(item.id)"></v-icon>
                                </div>
                            </template>

                            <template v-slot:no-data>
                                <v-btn prepend-icon="mdi-backup-restore" rounded="lg" text="Refresh" variant="text"
                                    border @click="refreshSprints"></v-btn>
                            </template>
                        </v-data-table>
                    </v-sheet>

                    <v-dialog v-model="newEditDialog" max-width="500">
                        <v-card :title="`${isEditing ? 'Edit' : 'Add'} a Sprint`">
                            <template v-slot:text>
                                <v-row>
                                    <v-col cols="12">
                                        <v-text-field v-model="formModel.name" label="Sprint Name"
                                            required></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.startDate" label="Start Date" type="date"
                                            required></v-text-field>
                                    </v-col>
                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.endDate" label="End Date" type="date"
                                            required></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.teamId" label="Team ID"
                                            required></v-text-field>
                                    </v-col>

                                    <v-col v-if="isAdmin" cols="12" md="6">
                                        <v-text-field v-model="formModel.managerId" label="Manager ID" required
                                            hint="Select which manager will oversee this sprint"></v-text-field>
                                    </v-col>

                                    <v-col v-if="isManager" cols="12" md="6">
                                        <v-text-field :model-value="currentUser?.id" label="Manager ID" readonly
                                            hint="Automatically assigned to you" persistent-hint></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.projectId" label="Project ID"
                                            required></v-text-field>
                                    </v-col>

                                    <v-col v-if="isManager && !isEditing" cols="12">
                                        <v-alert type="info" variant="tonal">
                                            <div class="text-subtitle-2 mb-1">Auto Assignment</div>
                                            <div class="text-body-2">
                                                This sprint will be automatically assigned to you as the manager.
                                            </div>
                                        </v-alert>
                                    </v-col>
                                </v-row>
                            </template>

                            <v-divider></v-divider>

                            <v-card-actions class="bg-surface-light">
                                <v-btn text="Cancel" variant="plain" @click="newEditDialog = false"></v-btn>

                                <v-spacer></v-spacer>

                                <v-btn text="Save" color="primary" @click="save"></v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-dialog>

                    <v-dialog v-if="isAdmin" v-model="confirmDeleteDialog" max-width="400px">
                        <v-card>
                            <v-card-title class="text-h6">Confirm Deletion</v-card-title>
                            <v-card-text>Are you sure you want to delete this sprint?</v-card-text>
                            <v-text-field v-model="sprintNameToDelete" label="Sprint Name" readonly></v-text-field>
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
import authService from '@/services/authService';
import sprintsService from '@/services/sprintsService';
import type { Sprint } from '@/types';
import { DevelopmentLogger } from '@/utils/logger';
import { ref, toRef, computed } from 'vue';

const logger = new DevelopmentLogger({ prefix: '[SprintsView]' });

const currentUser = authService.getStoredUser();
const isAdmin = computed(() => currentUser?.role === 'admin');
const isManager = computed(() => currentUser?.role === 'manager');

const pageTitle = computed(() => {
    if (isAdmin.value) return 'Sprint Management';
    if (isManager.value) return 'My Sprints';
    return 'Sprints';
});

const tableTitle = computed(() => {
    if (isAdmin.value) return 'All Sprints';
    if (isManager.value) return 'My Sprints';
    return 'Sprints';
});

// Warunkowe headery - dla managera ukrywamy Manager ID
const headers = computed(() => {
    const baseHeaders: any[] = [
        { title: 'Name', key: 'name', align: "start" as const },
        { title: 'Start Date', key: 'startDate' },
        { title: 'End Date', key: 'endDate' },
        { title: 'Team ID', key: 'teamId' },
    ];

    // Dodaj Manager ID tylko dla administratora
    if (isAdmin.value) {
        baseHeaders.push({ title: 'Manager ID', key: 'managerId' });
    }

    baseHeaders.push(
        { title: 'Project ID', key: 'projectId' },
        { title: 'Actions', key: 'actions', align: 'start' as const, sortable: false }
    );

    return baseHeaders;
});

// Warunkowe ładowanie danych
const {
    data: sprints,
    load: refreshSprints
} = useAsyncData<Sprint[]>({
    fetchFunction: (signal) => {
        if (isAdmin.value) {
            return sprintsService.getSprints(signal);
        } else if (isManager.value && currentUser?.id) {
            return sprintsService.getByManager(currentUser.id, signal);
        } else {
            throw new Error('User not authorized to view sprints');
        }
    },
    loggerPrefix: '[SprintsView]'
});

const newEditDialog = ref(false);
const confirmDeleteDialog = ref(false);
const formModel = ref<any>(createNewRecord());
const sprintNameToDelete = ref('');

function createNewRecord(): any {
    return {
        id: '',
        name: '',
        startDate: new Date().toISOString().split('T')[0], // Format YYYY-MM-DD
        endDate: new Date().toISOString().split('T')[0],
        teamId: '',
        managerId: isManager.value ? currentUser?.id || '' : '', // Auto-assign dla managera
        projectId: ''
    };
}

const isEditing = toRef(() => !!formModel.value.id);

function addNewSprint() {
    formModel.value = createNewRecord();
    newEditDialog.value = true;
}

function editSprint(id: string) {
    const sprint = sprints.value?.find(s => s.id === id);
    if (sprint) {
        formModel.value = { ...sprint };
        // Konwersja dat do formatu YYYY-MM-DD dla inputów
        if (formModel.value.startDate) {
            (formModel.value as any).startDate = new Date(formModel.value.startDate).toISOString().split('T')[0];
        }
        if (formModel.value.endDate) {
            (formModel.value as any).endDate = new Date(formModel.value.endDate).toISOString().split('T')[0];
        }
        newEditDialog.value = true;
    } else {
        logger.error(`Sprint with ID ${id} not found.`);
    }
}

function showDeleteConfirmation(id: string) {
    if (!isAdmin.value) {
        logger.error('Only administrators can delete sprints');
        return;
    }

    const sprint = sprints.value?.find(s => s.id === id);
    if (sprint) {
        logger.log('Delete sprint:', sprint);
        sprintNameToDelete.value = sprint.name;
        confirmDeleteDialog.value = true;
    } else {
        logger.error(`Sprint with ID ${id} not found.`);
    }
}

function confirmDelete() {
    if (!isAdmin.value) {
        logger.error('Only administrators can delete sprints');
        return;
    }

    if (sprints.value == null) {
        logger.error('Sprints data is not loaded yet.');
        return;
    }

    if (!sprintNameToDelete.value) {
        logger.error('No sprint selected for deletion.');
        return;
    }

    sprints.value = sprints.value.filter(s => s.name !== sprintNameToDelete.value);
    sprintNameToDelete.value = '';

    logger.log(`Confirmed deletion of sprint: ${sprintNameToDelete.value}`);
    confirmDeleteDialog.value = false;
}

function cancelDelete() {
    confirmDeleteDialog.value = false;
    sprintNameToDelete.value = '';
}

function save() {
    if (sprints.value === null) {
        logger.error('Sprints data is not loaded yet.');
        return;
    }

    if (!formModel.value.name) {
        logger.error('Sprint name is required.');
        return;
    }

    if (!formModel.value.startDate || !formModel.value.endDate) {
        logger.error('Start and end dates are required.');
        return;
    }

    if (!formModel.value.teamId || !formModel.value.projectId) {
        logger.error('Team ID and Project ID are required.');
        return;
    }

    // Automatyczne przypisanie managerId dla managera
    if (isManager.value && currentUser?.id) {
        formModel.value.managerId = currentUser.id;
    }

    if (!formModel.value.managerId) {
        logger.error('Manager ID is required.');
        return;
    }

    // Walidacja dat
    const startDate = new Date(formModel.value.startDate);
    const endDate = new Date(formModel.value.endDate);

    if (endDate <= startDate) {
        logger.error('End date must be after start date.');
        return;
    }

    if (isEditing.value) {
        const index = sprints.value.findIndex(sprint => sprint.id === formModel.value.id);
        if (index !== -1) {
            sprints.value[index] = { ...formModel.value };
        }
    } else {
        formModel.value.id = `${Date.now()}`; // fake ID for demo
        sprints.value.push({ ...formModel.value });
    }

    logger.log(`Sprint ${isEditing.value ? 'updated' : 'created'}:`, formModel.value);
    newEditDialog.value = false;
}
</script>

<style scoped></style>
