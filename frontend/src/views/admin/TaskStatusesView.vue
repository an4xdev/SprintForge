<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">Task statuses Management</h1>

                <v-card>
                    <v-card-title class="text-h5 d-flex align-center">
                        Task Statuses List
                        <v-spacer></v-spacer>
                        <v-btn color="primary" @click="addNewTaskStatus" prepend-icon="mdi-plus">
                            Add Task Status
                        </v-btn>
                    </v-card-title>

                    <v-divider></v-divider>

                    <v-card-text>
                        <v-alert v-if="error" type="error" class="mb-4" closable @click:close="error = ''">
                            {{ error }}
                        </v-alert>

                        <div v-if="(!taskStatuses || !taskStatuses.length) && !loading && !error"
                            class="text-center py-12">
                            <v-icon size="64" color="primary" class="mb-4">mdi-tag-multiple</v-icon>
                            <div class="text-h6 mb-2">No task statuses</div>
                        </div>

                        <v-progress-linear v-if="loading" indeterminate class="mb-4"></v-progress-linear>

                        <v-list v-if="taskStatuses && taskStatuses.length > 0">
                            <v-list-item v-for="taskStatus in taskStatuses" :key="taskStatus.id"
                                :title="taskStatus.name" :subtitle="`ID: ${taskStatus.id}`"
                                prepend-icon="mdi-tag-multiple">
                                <template v-slot:append>
                                    <v-btn icon="mdi-pencil" size="small" variant="text"
                                        @click="editTaskStatus(taskStatus.id)"></v-btn>
                                    <v-btn icon="mdi-delete" size="small" variant="text"
                                        @click="showDeleteConfirmation(taskStatus.id)"></v-btn>
                                </template>
                            </v-list-item>
                        </v-list>
                    </v-card-text>
                </v-card>
            </v-container>
        </v-main>
        <v-dialog v-model="newEditDialog" max-width="500px">
            <v-card :title="isEditing ? 'Edit Task Status' : 'Create New Task Status'">
                <template v-slot:text>
                    <v-row>
                        <v-col cols="12">
                            <v-text-field v-model="formModel.name" label="Task Status Name" required></v-text-field>
                        </v-col>
                    </v-row>
                </template>
                <v-divider></v-divider>
                <v-card-actions class="bg-surface-light">
                    <v-btn text="Cancel" color="danger" @click="newEditDialog = false"></v-btn>
                    <v-spacer></v-spacer>
                    <v-btn :prepend-icon="isEditing ? 'mdi-pencil' : 'mdi-plus'" color="primary" @click="save">
                        {{ isEditing ? 'Update' : 'Create' }}
                    </v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>

        <v-dialog v-model="confirmDeleteDialog" max-width="400px">
            <v-card>
                <v-card-title class="text-h6">Confirm Deletion</v-card-title>
                <v-card-text>Are you sure you want to delete this task status?</v-card-text>
                <v-text-field v-model="taskStatusNameToDelete" label="Task Status Name" readonly></v-text-field>
                <v-divider></v-divider>
                <v-card-actions class="bg-surface-light">
                    <v-btn text="Cancel" color="success" @click="cancelDelete"></v-btn>
                    <v-spacer></v-spacer>
                    <v-btn text="Confirm" color="error" @click="confirmDelete"></v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </v-layout>
</template>

<script setup lang="ts">
import type { TaskStatus } from '@/types';
import { useAsyncData } from '@/composables/useAsyncData';
import { DevelopmentLogger } from '@/utils/logger';
import { ref, toRef } from 'vue';
import taskStatusesService from '@/services/taskStatusesService';

const logger = new DevelopmentLogger({ prefix: '[CompaniesView]' });

const {
    data: taskStatuses,
    loading,
    error
} = useAsyncData<TaskStatus[]>({
    fetchFunction: (signal) => taskStatusesService.getTaskStatuses(signal),
    loggerPrefix: '[TaskStatusesView]'
});

const newEditDialog = ref(false);
const confirmDeleteDialog = ref(false);
const formModel = ref(createNewRecord());
const taskStatusNameToDelete = ref('');

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
        confirmDeleteDialog.value = true;
    } else {
        logger.error(`Task Status with ID ${id} not found.`);
    }
}

function confirmDelete() {

    if (taskStatuses.value == null) {
        logger.error('Task Statuses data is not loaded yet.');
        return;
    }

    if (!taskStatusNameToDelete.value) {
        logger.error('No task status selected for deletion.');
        return;
    }

    taskStatuses.value = taskStatuses.value.filter(st => st.name !== taskStatusNameToDelete.value);
    taskStatusNameToDelete.value = '';

    logger.log(`Confirmed deletion of task status: ${taskStatusNameToDelete.value}`);
    confirmDeleteDialog.value = false;
}

function cancelDelete() {
    confirmDeleteDialog.value = false;
    taskStatusNameToDelete.value = '';
}

function save() {
    if (taskStatuses.value === null) {
        logger.error('Task Statuses data is not loaded yet.');
        return;
    }

    if (!formModel.value.name) {
        logger.error('Task Status name is required.');
        return;
    }

    if (isEditing.value) {
        const index = taskStatuses.value.findIndex(taskStatus => taskStatus.id === formModel.value.id)
        taskStatuses.value[index] = formModel.value

    } else {
        formModel.value.id = taskStatuses.value.length + 1
        taskStatuses.value.push(formModel.value)

    }

    newEditDialog.value = false
}

</script>
<style scoped></style>
