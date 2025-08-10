<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">Projects Management</h1>

                <div class="py-1">
                    <v-sheet border rounded>
                        <v-data-table :headers="headers"
                            :hide-default-footer="projects !== null && projects.length < 11" :items="projects ?? []">
                            <template v-slot:top>
                                <v-toolbar flat>
                                    <v-toolbar-title>
                                        <v-icon color="medium-emphasis" icon="mdi-folder-multiple" size="x-small"
                                            start></v-icon>
                                        Projects
                                    </v-toolbar-title>

                                    <v-btn class="me-2" prepend-icon="mdi-plus" rounded="lg" text="Add a Project" border
                                        @click="addNewProject"></v-btn>
                                </v-toolbar>
                            </template>

                            <template v-slot:item.name="{ value }">
                                <v-chip :text="value" border="thin opacity-25" prepend-icon="mdi-folder-multiple" label>
                                    <template v-slot:prepend>
                                        <v-icon color="medium-emphasis"></v-icon>
                                    </template>
                                </v-chip>
                            </template>

                            <template v-slot:item.actions="{ item }">
                                <div class="d-flex ga-2 justify-end">
                                    <v-icon color="medium-emphasis" icon="mdi-pencil" size="small"
                                        @click="editProject(item.id)"></v-icon>

                                    <v-icon color="medium-emphasis" icon="mdi-delete" size="small"
                                        @click="showDeleteConfirmation(item.id)"></v-icon>
                                </div>
                            </template>

                            <template v-slot:no-data>
                                <v-btn prepend-icon="mdi-backup-restore" rounded="lg" text="Refresh" variant="text"
                                    border @click="refreshProjects"></v-btn>
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
                                        <v-text-field v-model="formModel.startDate" label="Start Date"
                                            type="date"></v-text-field>
                                    </v-col>
                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.endDate" label="End Date"
                                            type="date"></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.companyDto.id"
                                            label="Company ID"></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.companyDto.name"
                                            label="Company Name"></v-text-field>
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
                            <v-card-text>Are you sure you want to delete this project?</v-card-text>
                            <v-text-field v-model="projectNameToDelete" label="Project Name" readonly></v-text-field>
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
import type { Project } from '@/types';
import { DevelopmentLogger } from '@/utils/logger';
import { ref, toRef } from 'vue';

const logger = new DevelopmentLogger({ prefix: '[ProjectsView]' });

const headers = [
    { title: 'Name', key: 'name', align: "start" as const },
    { title: 'Start date', key: 'startDate' },
    { title: 'End date', key: 'endDate' },
    {
        title: 'Company', key: 'companyDto', children: [
            { title: 'ID', key: 'companyDto.id' },
            { title: 'Name', key: 'companyDto.name' },
        ]
    },
    { title: 'Actions', key: 'actions', align: 'end' as const, sortable: false }
];

const {
    data: projects,
    load: refreshProjects
} = useAsyncData<Project[]>({
    fetchFunction: (signal) => projectsService.getProjects(signal),
    loggerPrefix: '[ProjectsView]'
});

const newEditDialog = ref(false);
const confirmDeleteDialog = ref(false);
const formModel = ref(createNewRecord());
const projectNameToDelete = ref('');

function createNewRecord() {
    return {
        id: '',
        name: '',
        startDate: new Date(),
        endDate: new Date(),
        companyDto: { id: 0, name: '' }
    } as Project;
}
const isEditing = toRef(() => !!formModel.value.id)

function addNewProject() {
    formModel.value = createNewRecord();
    newEditDialog.value = true;
}

function editProject(id: string) {
    const project = projects.value?.find(p => p.id === id);
    if (project) {
        formModel.value = { ...project };
        newEditDialog.value = true;
    } else {
        logger.error(`Project with ID ${id} not found.`);
    }
}

function showDeleteConfirmation(id: string) {
    const project = projects.value?.find(p => p.id === id);
    if (project) {
        logger.log('Delete project:', project);
        projectNameToDelete.value = project.name;
        confirmDeleteDialog.value = true;
    } else {
        logger.error(`Project with ID ${id} not found.`);
    }
}

function confirmDelete() {

    if (projects.value == null) {
        logger.error('Projects data is not loaded yet.');
        return;
    }

    if (!projectNameToDelete.value) {
        logger.error('No project selected for deletion.');
        return;
    }

    projects.value = projects.value.filter(p => p.name !== projectNameToDelete.value);
    projectNameToDelete.value = '';

    logger.log(`Confirmed deletion of project: ${projectNameToDelete.value}`);
    confirmDeleteDialog.value = false;
}

function cancelDelete() {
    confirmDeleteDialog.value = false;
    projectNameToDelete.value = '';
}

function save() {
    if (projects.value === null) {
        logger.error('Projects data is not loaded yet.');
        return;
    }

    if (!formModel.value.name) {
        logger.error('Project name is required.');
        return;
    }

    if (isEditing.value) {
        const index = projects.value.findIndex(project => project.id === formModel.value.id)
        projects.value[index] = formModel.value

    } else {
        formModel.value.id = `${Date.now()}`; // fake
        formModel.value.companyDto.id = Math.floor(Math.random() * 1000) + 1; // fake
        formModel.value.companyDto.name = `Company ${Date.now()}`; // fake
        projects.value.push(formModel.value)

    }

    newEditDialog.value = false
}
</script>

<style scoped></style>
