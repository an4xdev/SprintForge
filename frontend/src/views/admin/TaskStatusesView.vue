<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">Task statuses Management</h1>

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
                                <v-chip :text="value" border="thin opacity-25" prepend-icon="mdi-tag-multiple" label>
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
import { DevelopmentLogger } from '@/utils/logger';
import { ref, toRef } from 'vue';
import taskStatusesService from '@/services/taskStatusesService';

const logger = new DevelopmentLogger({ prefix: '[CompaniesView]' });

const headers = [
    { title: 'Name', key: 'name', align: "start" as const },
    { title: 'ID', key: 'id' },
    { title: 'Actions', key: 'actions', align: 'end' as const, sortable: false }
];

const {
    data: taskStatuses,
    loading,
    error,
    load: refreshTaskStatuses
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
