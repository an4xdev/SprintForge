<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">Tasks Management</h1>

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
                        <v-data-table :headers="headers" :hide-default-footer="tasks !== null && tasks.length < 11"
                            :items="tasks ?? []">
                            <template v-slot:top>
                                <v-toolbar flat>
                                    <v-toolbar-title>
                                        <v-icon color="medium-emphasis" icon="mdi-format-list-bulleted" size="x-small"
                                            start></v-icon>
                                        Tasks
                                    </v-toolbar-title>
                                </v-toolbar>
                            </template>

                            <template v-slot:item.name="{ value }">
                                <v-chip :text="value" border="thin opacity-25" prepend-icon="mdi-format-list-bulleted"
                                    label>
                                    <template v-slot:prepend>
                                        <v-icon color="medium-emphasis"></v-icon>
                                    </template>
                                </v-chip>
                            </template>

                            <template v-slot:item.taskType="{ value }">
                                <v-chip v-if="value" :color="getTaskTypeColor(value.name)" size="small">
                                    {{ value.name }}
                                </v-chip>
                                <span v-else class="text-medium-emphasis">No type</span>
                            </template>

                            <template v-slot:item.taskStatus="{ value }">
                                <v-chip v-if="value" :color="getTaskStatusColor(value.name)" size="small">
                                    {{ value.name }}
                                </v-chip>
                                <span v-else class="text-medium-emphasis">No status</span>
                            </template>

                            <template v-slot:no-data>
                                <v-btn prepend-icon="mdi-backup-restore" rounded="lg" text="Refresh" variant="text"
                                    border @click="loadTasks"></v-btn>
                            </template>
                        </v-data-table>
                    </v-sheet>
                </div>
            </v-container>
        </v-main>
    </v-layout>
</template>

<script setup lang="ts">
import authService from '@/services/authService';
import tasksService from '@/services/tasksService';
import { extractErrorMessage } from '@/utils/errorHandler';
import type { TaskExt } from '@/types';
import { DevelopmentLogger } from '@/utils/logger';
import { getTaskStatusColor, getTaskTypeColor } from '@/utils/taskColors';
import { onMounted, ref } from 'vue';

const logger = new DevelopmentLogger({ prefix: '[TasksView]' });

const headers = [
    { title: 'Name', key: 'name', align: "start" as const },
    { title: 'Description', key: 'description' },
    { title: 'Task Type', key: 'taskType' },
    { title: 'Task Status', key: 'taskStatus' },
    { title: 'Sprint', key: 'sprint.name' },
    { title: 'Project', key: 'project.name' }
];

const tasks = ref<TaskExt[] | null>(null);
const error = ref('');
const showError = ref(false);
const successMessage = ref('');
const showSuccess = ref(false);

onMounted(async () => {
    await loadTasks();
})

async function loadTasks() {
    try {
        const currentUser = authService.getStoredUser();
        tasks.value = await tasksService.getTasksByDeveloperExt(currentUser!.id);
        error.value = '';
        logger.log('Loaded extended tasks:', tasks.value);
    } catch (err) {
        const errorDetails = extractErrorMessage(err);
        error.value = errorDetails.message;
        showError.value = true;
        logger.error('Error loading tasks:', err);
    }
}

</script>

<style scoped></style>
