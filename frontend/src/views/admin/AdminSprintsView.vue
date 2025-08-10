<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">Sprints Management</h1>

                <div class="py-1">
                    <v-sheet border rounded>
                        <v-data-table :headers="headers" :hide-default-footer="sprints !== null && sprints.length < 11"
                            :items="sprints ?? []">
                            <template v-slot:top>
                                <v-toolbar flat>
                                    <v-toolbar-title>
                                        <v-icon color="medium-emphasis" icon="mdi-rocket-launch" size="x-small"
                                            start></v-icon>
                                        Sprints
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

                                    <v-icon color="medium-emphasis" icon="mdi-delete" size="small"
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
                                        <v-text-field v-model="formModel.name" label="Name"></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.startDate" label="Start Date"
                                            type="date"></v-text-field>
                                    </v-col>
                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.endDate" label="End Date"
                                            type="date"></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.teamId" label="Team ID"></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.managerId" label="Manager ID"></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.projectId" label="Project ID"></v-text-field>
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
import projectsService from '@/services/projectsService';
import sprintsService from '@/services/sprintsService';
import type { Project, Sprint } from '@/types';
import { DevelopmentLogger } from '@/utils/logger';
import { ref, toRef } from 'vue';

const logger = new DevelopmentLogger({ prefix: '[SprintsView]' });

const headers = [
    { title: 'Name', key: 'name', align: "start" as const },
    { title: 'Start date', key: 'startDate' },
    { title: 'End date', key: 'endDate' },
    { title: 'Team id', key: 'teamId' },
    { title: 'Manager id', key: 'managerId' },
    { title: 'Project id', key: 'projectId' },
    { title: 'Actions', key: 'actions', align: 'end' as const, sortable: false }
];

const {
    data: sprints,
    load: refreshSprints
} = useAsyncData<Sprint[]>({
    fetchFunction: (signal) => sprintsService.getSprints(signal),
    loggerPrefix: '[SprintsView]'
});

const newEditDialog = ref(false);
const confirmDeleteDialog = ref(false);
const formModel = ref(createNewRecord());
const sprintNameToDelete = ref('');

function createNewRecord() {
    return {
        id: '',
        name: '',
        startDate: new Date(),
        endDate: new Date(),
        teamId: '',
        managerId: '',
        projectId: ''
    } as Sprint;
}
const isEditing = toRef(() => !!formModel.value.id)

function addNewSprint() {
    formModel.value = createNewRecord();
    newEditDialog.value = true;
}

function editSprint(id: string) {
    const sprint = sprints.value?.find(s => s.id === id);
    if (sprint) {
        formModel.value = { ...sprint };
        newEditDialog.value = true;
    } else {
        logger.error(`Sprint with ID ${id} not found.`);
    }
}

function showDeleteConfirmation(id: string) {
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

    if (isEditing.value) {
        const index = sprints.value.findIndex(sprint => sprint.id === formModel.value.id)
        sprints.value[index] = formModel.value

    } else {
        formModel.value.id = `${Date.now()}`; // fake
        formModel.value.teamId = 'team-id'; // fake
        formModel.value.managerId = 'manager-id'; // fake
        formModel.value.projectId = 'project-id'; // fake
        sprints.value.push(formModel.value)

    }

    newEditDialog.value = false
}
</script>

<style scoped></style>
