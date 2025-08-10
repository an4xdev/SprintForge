<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">Tasks Management</h1>

                <div class="py-1">
                    <v-sheet border rounded>
                        <v-data-table :headers="headers" :hide-default-footer="tasks !== null && tasks.length < 11"
                            :items="tasks ?? []">
                            <template v-slot:top>
                                <v-toolbar flat>
                                    <v-toolbar-title>
                                        <v-icon color="medium-emphasis" icon="mdi-format-list-bulleted" size="x-small"
                                            start></v-icon>
                                        Tasks
                                    </v-toolbar-title>

                                    <v-btn class="me-2" prepend-icon="mdi-plus" rounded="lg" text="Add a Task" border
                                        @click="addNewTask"></v-btn>
                                </v-toolbar>
                            </template>

                            <template v-slot:item.title="{ value }">
                                <v-chip :text="value" border="thin opacity-25" prepend-icon="mdi-format-list-bulleted"
                                    label>
                                    <template v-slot:prepend>
                                        <v-icon color="medium-emphasis"></v-icon>
                                    </template>
                                </v-chip>
                            </template>

                            <template v-slot:item.actions="{ item }">
                                <div class="d-flex ga-2 justify-end">
                                    <v-icon color="medium-emphasis" icon="mdi-pencil" size="small"
                                        @click="editTask(item.id)"></v-icon>

                                    <v-icon color="medium-emphasis" icon="mdi-delete" size="small"
                                        @click="showDeleteConfirmation(item.id)"></v-icon>
                                </div>
                            </template>

                            <template v-slot:no-data>
                                <v-btn prepend-icon="mdi-backup-restore" rounded="lg" text="Refresh" variant="text"
                                    border @click="refreshTasks"></v-btn>
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
                                        <v-text-field v-model="formModel.description"
                                            label="Description"></v-text-field>
                                    </v-col>
                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.developerId"
                                            label="Developer ID"></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.sprintId" label="Sprint ID"></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.taskStatusId"
                                            label="Task Status ID"></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.taskTypeId"
                                            label="Task Type ID"></v-text-field>
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
                            <v-card-text>Are you sure you want to delete this task?</v-card-text>
                            <v-text-field v-model="taskNameToDelete" label="Task Name" readonly></v-text-field>
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
import tasksService from '@/services/tasksService';
import type { Task } from '@/types';
import { DevelopmentLogger } from '@/utils/logger';
import { ref, toRef } from 'vue';

const logger = new DevelopmentLogger({ prefix: '[TasksView]' });

const headers = [
    { title: 'Name', key: 'name', align: "start" as const },
    { title: 'Description', key: 'description' },
    { title: 'Developer id', key: 'developerId' },
    { title: 'Sprint id', key: 'sprintId' },
    { title: 'Task Status id', key: 'taskStatusId' },
    { title: 'Task Type id', key: 'taskTypeId' },
    { title: 'Actions', key: 'actions', align: 'end' as const, sortable: false }
];

const {
    data: tasks,
    load: refreshTasks
} = useAsyncData<Task[]>({
    fetchFunction: (signal) => tasksService.getTasks(signal),
    loggerPrefix: '[TasksView]'
});

const newEditDialog = ref(false);
const confirmDeleteDialog = ref(false);
const formModel = ref(createNewRecord());
const taskNameToDelete = ref('');

function createNewRecord() {
    return {
        id: '',
        name: '',
        description: '',
        developerId: null,
        sprintId: null,
        taskStatusId: 0,
        taskTypeId: 0
    } as Task;
}
const isEditing = toRef(() => !!formModel.value.id)

function addNewTask() {
    formModel.value = createNewRecord();
    newEditDialog.value = true;
}

function editTask(id: string) {
    const task = tasks.value?.find(t => t.id === id);
    if (task) {
        formModel.value = { ...task };
        newEditDialog.value = true;
    } else {
        logger.error(`Task with ID ${id} not found.`);
    }
}

function showDeleteConfirmation(id: string) {
    const task = tasks.value?.find(t => t.id === id);
    if (task) {
        logger.log('Delete task:', task);
        taskNameToDelete.value = task.name;
        confirmDeleteDialog.value = true;
    } else {
        logger.error(`Task with ID ${id} not found.`);
    }
}

function confirmDelete() {

    if (tasks.value == null) {
        logger.error('Tasks data is not loaded yet.');
        return;
    }

    if (!taskNameToDelete.value) {
        logger.error('No task selected for deletion.');
        return;
    }

    tasks.value = tasks.value.filter(t => t.name !== taskNameToDelete.value);
    taskNameToDelete.value = '';

    logger.log(`Confirmed deletion of task: ${taskNameToDelete.value}`);
    confirmDeleteDialog.value = false;
}

function cancelDelete() {
    confirmDeleteDialog.value = false;
    taskNameToDelete.value = '';
}

function save() {
    if (tasks.value === null) {
        logger.error('Tasks data is not loaded yet.');
        return;
    }

    if (!formModel.value.name) {
        logger.error('Task name is required.');
        return;
    }

    if (isEditing.value) {
        const index = tasks.value.findIndex(task => task.id === formModel.value.id)
        tasks.value[index] = formModel.value

    } else {
        formModel.value.id = `${Date.now()}`; // fake
        formModel.value.developerId = 'team-id'; // fake
        formModel.value.sprintId = 'sprint-id'; // fake
        formModel.value.taskStatusId = 0; // fake
        formModel.value.taskTypeId = 0; // fake
        tasks.value.push(formModel.value)

    }

    newEditDialog.value = false
}
</script>

<style scoped></style>
