<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">Task Statuses Management</h1>

                <v-snackbar v-model="showError" color="error" timeout="3000" location="top right">
                    <v-icon start>mdi-alert-circle</v-icon>
                    {{ error }}
                </v-snackbar>

                <v-snackbar v-model="showSuccess" color="success" timeout="3000" location="top right">
                    <v-icon start>mdi-check-circle</v-icon>
                    {{ successMessage }}
                </v-snackbar>

                <div class="py-1">
                    <v-sheet border rounded>
                        <v-data-table :headers="headers"
                            :hide-default-footer="taskStatuses !== null && taskStatuses.length < 11"
                            :items="taskStatuses ?? []">
                            <template v-slot:top>
                                <v-toolbar flat>
                                    <v-toolbar-title>
                                        <v-icon color="medium-emphasis" icon="mdi-tag-multiple" size="x-small"
                                            start></v-icon>
                                        Task Statuses
                                    </v-toolbar-title>

                                    <v-btn class="me-2" prepend-icon="mdi-plus" rounded="lg" text="Add a Task Status"
                                        border @click="addNewTaskStatus"></v-btn>
                                </v-toolbar>
                            </template>

                            <template v-slot:item.name="{ value }">
                                <v-chip :text="value" :color="getTaskStatusColor(value)" border="thin opacity-25"
                                    prepend-icon="mdi-tag-multiple" label>
                                    <template v-slot:prepend>
                                        <v-icon color="medium-emphasis"></v-icon>
                                    </template>
                                </v-chip>
                            </template>

                            <template v-slot:item.actions="{ item }">
                                <div class="d-flex ga-2 justify-end">
                                    <v-icon color="medium-emphasis" icon="mdi-pencil" size="small"
                                        @click="editTaskStatus(item.id)"></v-icon>

                                    <v-icon color="medium-emphasis" icon="mdi-delete" size="small"
                                        @click="showDeleteConfirmation(item.id)"></v-icon>
                                </div>
                            </template>

                            <template v-slot:no-data>
                                <v-btn prepend-icon="mdi-backup-restore" rounded="lg" text="Refresh" variant="text"
                                    border @click="refreshTaskStatuses"></v-btn>
                            </template>
                        </v-data-table>
                    </v-sheet>

                    <v-dialog v-model="newEditDialog" max-width="500px">
                        <v-card :title="isEditing ? 'Edit Task Status' : 'Create New Task Status'">
                            <template v-slot:text>
                                <v-row>
                                    <v-col cols="12">
                                        <v-text-field v-model="formModel.name" label="Task Status Name"
                                            required></v-text-field>
                                    </v-col>
                                </v-row>
                            </template>
                            <v-divider></v-divider>
                            <v-card-actions class="bg-surface-light">
                                <v-btn text="Cancel" color="danger" @click="newEditDialog = false"></v-btn>
                                <v-spacer></v-spacer>
                                <v-btn :prepend-icon="isEditing ? 'mdi-pencil' : 'mdi-plus'" color="primary"
                                    @click="save">
                                    {{ isEditing ? 'Update' : 'Create' }}
                                </v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-dialog>

                    <v-dialog v-model="confirmDeleteDialog" max-width="400px">
                        <v-card>
                            <v-card-title class="text-h6">Confirm Deletion</v-card-title>
                            <v-card-text>Are you sure you want to delete this task status?</v-card-text>
                            <v-text-field v-model="taskStatusNameToDelete" label="Task Status Name"
                                readonly></v-text-field>
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
import type { TaskStatus } from '@/types';
import { useAsyncData } from '@/composables/useAsyncData';
import { extractErrorMessage } from '@/utils/errorHandler';
import { DevelopmentLogger } from '@/utils/logger';
import { ref, toRef } from 'vue';
import taskStatusesService from '@/services/taskStatusesService';
import { getTaskStatusColor } from '@/utils/taskColors';

const logger = new DevelopmentLogger({ prefix: '[CompaniesView]' });

const headers = [
    { title: 'Name', key: 'name', align: "start" as const },
    { title: 'ID', key: 'id' },
    { title: 'Actions', key: 'actions', align: 'end' as const, sortable: false }
];

const {
    data: taskStatuses,
    load: refreshTaskStatuses
} = useAsyncData<TaskStatus[]>({
    fetchFunction: (signal) => taskStatusesService.getTaskStatuses(signal),
    loggerPrefix: '[TaskStatusesView]'
});

const newEditDialog = ref(false);
const confirmDeleteDialog = ref(false);
const formModel = ref(createNewRecord());
const taskStatusNameToDelete = ref('');
const taskStatusIdToDelete = ref<number | null>(null);
const error = ref('');
const showError = ref(false);
const successMessage = ref('');
const showSuccess = ref(false);

function createNewRecord() {
    return {
        id: 0,
        name: '',
    } as TaskStatus;
}

const isEditing = toRef(() => !!formModel.value.id)

function addNewTaskStatus() {
    formModel.value = createNewRecord();
    newEditDialog.value = true;
}

function editTaskStatus(id: number) {
    const taskStatus = taskStatuses.value?.find(st => st.id === id);
    if (taskStatus) {
        formModel.value = { ...taskStatus };
        newEditDialog.value = true;
    } else {
        logger.error(`Task Status with ID ${id} not found.`);
    }
}

function showDeleteConfirmation(id: number) {
    const taskStatus = taskStatuses.value?.find(st => st.id === id);
    if (taskStatus) {
        logger.log('Delete task status:', taskStatus);
        taskStatusNameToDelete.value = taskStatus.name;
        taskStatusIdToDelete.value = taskStatus.id;
        confirmDeleteDialog.value = true;
    } else {
        logger.error(`Task Status with ID ${id} not found.`);
        error.value = `Task status ${id} not found`;
    }
}

async function confirmDelete() {
    if (taskStatusIdToDelete.value == null) {
        logger.error('No task status selected for deletion.');
        error.value = 'No task status selected for deletion';
        return;
    }

    try {
        await taskStatusesService.deleteTaskStatus(taskStatusIdToDelete.value);
        await refreshTaskStatuses();
        logger.log(`Confirmed deletion of task status: ${taskStatusNameToDelete.value}`);
        error.value = '';
    } catch (err) {
        logger.error('Failed to delete task status:', err);
        error.value = (err instanceof Error ? err.message : 'Failed to delete task status');
    } finally {
        taskStatusIdToDelete.value = null;
        taskStatusNameToDelete.value = '';
        confirmDeleteDialog.value = false;
    }
}

function cancelDelete() {
    confirmDeleteDialog.value = false;
    taskStatusNameToDelete.value = '';
}

async function save() {
    if (!formModel.value.name) {
        logger.error('Task Status name is required.');
        error.value = 'Task Status name is required';
        return;
    }

    try {
        if (isEditing.value) {
            await taskStatusesService.updateTaskStatus(formModel.value.id, { name: formModel.value.name });
            logger.log('Updated task status', formModel.value);
        } else {
            await taskStatusesService.createTaskStatus({ name: formModel.value.name });
            logger.log('Created task status', formModel.value.name);
        }

        await refreshTaskStatuses();
        newEditDialog.value = false;
        error.value = '';
    } catch (err) {
        logger.error('Failed to save task status:', err);
        error.value = (err instanceof Error ? err.message : 'Failed to save task status');
    }
}

</script>
<style scoped></style>
