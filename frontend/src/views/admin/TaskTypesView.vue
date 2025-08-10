<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">Task types Management</h1>

                <v-card>
                    <v-card-title class="text-h5 d-flex align-center">
                        Task Types List
                        <v-spacer></v-spacer>
                        <v-btn color="primary" @click="addNewTaskType" prepend-icon="mdi-plus">
                            Add Task Type
                        </v-btn>
                    </v-card-title>

                    <v-divider></v-divider>

                    <v-card-text>
                        <v-alert v-if="error" type="error" class="mb-4" closable @click:close="error = ''">
                            {{ error }}
                        </v-alert>

                        <div v-if="(!taskTypes || !taskTypes.length) && !loading && !error" class="text-center py-12">
                            <v-icon size="64" color="primary" class="mb-4">mdi-shape</v-icon>
                            <div class="text-h6 mb-2">No task types</div>
                        </div>

                        <v-progress-linear v-if="loading" indeterminate class="mb-4"></v-progress-linear>

                        <v-list v-if="taskTypes && taskTypes.length > 0">
                            <v-list-item v-for="taskType in taskTypes" :key="taskType.id" :title="taskType.name"
                                :subtitle="`ID: ${taskType.id}`" prepend-icon="mdi-shape">
                                <template v-slot:append>
                                    <v-btn icon="mdi-pencil" size="small" variant="text"
                                        @click="editTaskType(taskType.id)"></v-btn>
                                    <v-btn icon="mdi-delete" size="small" variant="text"
                                        @click="showDeleteConfirmation(taskType.id)"></v-btn>
                                </template>
                            </v-list-item>
                        </v-list>
                    </v-card-text>
                </v-card>
            </v-container>
        </v-main>
        <v-dialog v-model="newEditDialog" max-width="500px">
            <v-card :title="isEditing ? 'Edit Task Type' : 'Create New Task Type'">
                <template v-slot:text>
                    <v-row>
                        <v-col cols="12">
                            <v-text-field v-model="formModel.name" label="Task Type Name" required></v-text-field>
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
                <v-card-text>Are you sure you want to delete this task type?</v-card-text>
                <v-text-field v-model="taskTypeNameToDelete" label="Task Type Name" readonly></v-text-field>
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
import type { TaskType } from '@/types';
import { useAsyncData } from '@/composables/useAsyncData';
import { DevelopmentLogger } from '@/utils/logger';
import { ref, toRef } from 'vue';
import taskTypesService from '@/services/taskTypesService';

const logger = new DevelopmentLogger({ prefix: '[TaskTypesView]' });

const {
    data: taskTypes,
    loading,
    error
} = useAsyncData<TaskType[]>({
    fetchFunction: (signal) => taskTypesService.getTaskTypes(signal),
    loggerPrefix: '[TaskTypesView]'
});

const newEditDialog = ref(false);
const confirmDeleteDialog = ref(false);
const formModel = ref(createNewRecord());
const taskTypeNameToDelete = ref('');

function createNewRecord() {
    return {
        id: 0,
        name: '',
    } as TaskType;
}

const isEditing = toRef(() => !!formModel.value.id)

function addNewTaskType() {
    formModel.value = createNewRecord();
    newEditDialog.value = true;
}

function editTaskType(id: number) {
    const taskType = taskTypes.value?.find(tt => tt.id === id);
    if (taskType) {
        formModel.value = { ...taskType };
        newEditDialog.value = true;
    } else {
        logger.error(`Task Type with ID ${id} not found.`);
    }
}

function showDeleteConfirmation(id: number) {
    const taskType = taskTypes.value?.find(tt => tt.id === id);
    if (taskType) {
        logger.log('Delete task type:', taskType);
        taskTypeNameToDelete.value = taskType.name;
        confirmDeleteDialog.value = true;
    } else {
        logger.error(`Company with ID ${id} not found.`);
    }
}

function confirmDelete() {

    if (taskTypes.value == null) {
        logger.error('Task Types data is not loaded yet.');
        return;
    }

    if (!taskTypeNameToDelete.value) {
        logger.error('No task type selected for deletion.');
        return;
    }

    taskTypes.value = taskTypes.value.filter(tt => tt.name !== taskTypeNameToDelete.value);
    taskTypeNameToDelete.value = '';

    logger.log(`Confirmed deletion of task type: ${taskTypeNameToDelete.value}`);
    confirmDeleteDialog.value = false;
}

function cancelDelete() {
    confirmDeleteDialog.value = false;
    taskTypeNameToDelete.value = '';
}

function save() {
    if (taskTypes.value === null) {
        logger.error('Task Types data is not loaded yet.');
        return;
    }

    if (!formModel.value.name) {
        logger.error('Task Type name is required.');
        return;
    }

    if (isEditing.value) {
        const index = taskTypes.value.findIndex(tt => tt.id === formModel.value.id)
        taskTypes.value[index] = formModel.value

    } else {
        formModel.value.id = taskTypes.value.length + 1
        taskTypes.value.push(formModel.value)

    }

    newEditDialog.value = false
}

</script>
<style scoped></style>
