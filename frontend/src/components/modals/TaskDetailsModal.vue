<template>
    <v-dialog :model-value="modelValue" @update:model-value="$emit('update:modelValue', $event)" max-width="800">
        <v-card v-if="task">
            <v-card-title class="d-flex align-center bg-primary">
                <v-icon class="me-3" color="white">mdi-clipboard-text</v-icon>
                <span class="text-white">Task Details</span>
            </v-card-title>

            <v-card-text class="pt-4">
                <v-container>
                    <v-row>
                        <!-- Task Name -->
                        <v-col cols="12">
                            <div class="text-h5 mb-3 d-flex align-center">
                                <v-icon class="me-2" color="primary">mdi-format-title</v-icon>
                                {{ task.name }}
                            </div>
                        </v-col>

                        <!-- Description -->
                        <v-col cols="12" v-if="task.description">
                            <v-card variant="outlined" class="pa-3 mb-3">
                                <div class="text-subtitle-2 text-medium-emphasis mb-2">
                                    <v-icon size="small" class="me-1">mdi-text</v-icon>
                                    Description
                                </div>
                                <div class="text-body-1">{{ task.description }}</div>
                            </v-card>
                        </v-col>

                        <!-- Task Type and Status -->
                        <v-col cols="12" md="6">
                            <v-card variant="outlined" class="pa-3 mb-3">
                                <div class="text-subtitle-2 text-medium-emphasis mb-2">
                                    <v-icon size="small" class="me-1">mdi-shape</v-icon>
                                    Task Type
                                </div>
                                <v-chip :color="getTaskTypeColor(task.taskType.name)" variant="flat"
                                    class="font-weight-medium">
                                    {{ task.taskType.name }}
                                </v-chip>
                            </v-card>
                        </v-col>

                        <v-col cols="12" md="6">
                            <v-card variant="outlined" class="pa-3 mb-3">
                                <div class="text-subtitle-2 text-medium-emphasis mb-2">
                                    <v-icon size="small" class="me-1">mdi-progress-check</v-icon>
                                    Status
                                </div>
                                <v-chip :color="getTaskStatusColor(task.taskStatus.name)" variant="flat"
                                    class="font-weight-medium">
                                    {{ task.taskStatus.name }}
                                </v-chip>
                            </v-card>
                        </v-col>

                        <!-- Developer -->
                        <v-col cols="12" v-if="task.developer">
                            <v-card variant="outlined" class="pa-3 mb-3">
                                <div class="text-subtitle-2 text-medium-emphasis mb-2">
                                    <v-icon size="small" class="me-1">mdi-account</v-icon>
                                    Developer
                                </div>
                                <div class="text-body-1 font-weight-medium">
                                    {{ task.developer.firstName }} {{ task.developer.lastName }} ({{
                                        task.developer.username }})
                                </div>
                            </v-card>
                        </v-col>

                        <!-- Sprint -->
                        <v-col cols="12" v-if="task.sprint">
                            <v-card variant="outlined" class="pa-3 mb-3">
                                <div class="text-subtitle-2 text-medium-emphasis mb-2">
                                    <v-icon size="small" class="me-1">mdi-rocket</v-icon>
                                    Sprint
                                </div>
                                <div class="text-body-1 font-weight-medium">{{ task.sprint.name }}</div>
                                <div class="text-caption text-medium-emphasis mt-1">
                                    {{ formatDate(task.sprint.startDate) }} - {{ formatDate(task.sprint.endDate) }}
                                </div>
                            </v-card>
                        </v-col>

                        <!-- Project -->
                        <v-col cols="12" v-if="task.project">
                            <v-card variant="outlined" class="pa-3 mb-3">
                                <div class="text-subtitle-2 text-medium-emphasis mb-2">
                                    <v-icon size="small" class="me-1">mdi-briefcase</v-icon>
                                    Project
                                </div>
                                <div class="text-body-1 font-weight-medium">{{ task.project.name }}</div>
                                <div class="text-caption text-medium-emphasis mt-1">
                                    {{ formatDate(task.project.startDate) }} - {{ formatDate(task.project.endDate) }}
                                </div>
                            </v-card>
                        </v-col>

                        <!-- Task History Section -->
                        <v-col cols="12">
                            <v-divider class="my-3"></v-divider>
                            <div class="d-flex align-center justify-space-between mb-3">
                                <div class="text-h6 d-flex align-center">
                                    <v-icon class="me-2" color="primary">mdi-history</v-icon>
                                    Task History
                                </div>
                                <v-btn color="primary" variant="tonal" prepend-icon="mdi-refresh"
                                    @click="loadTaskHistory" :loading="loadingHistory">
                                    Load History
                                </v-btn>
                            </div>

                            <!-- History List -->
                            <v-card v-if="taskHistory.length > 0" variant="outlined" class="pa-0">
                                <v-list>
                                    <v-list-item v-for="(history, index) in taskHistory" :key="history.id"
                                        :class="{ 'border-b': index < taskHistory.length - 1 }">
                                        <template v-slot:prepend>
                                            <v-icon color="primary">mdi-clock-outline</v-icon>
                                        </template>
                                        <v-list-item-title>
                                            Status changed from
                                            <v-chip v-if="history.old_status" size="small"
                                                :color="getTaskStatusColor(history.old_status.name)" class="mx-1">
                                                {{ history.old_status.name }}
                                            </v-chip>
                                            <span v-else class="mx-1 font-italic">None</span>
                                            to
                                            <v-chip size="small" :color="getTaskStatusColor(history.new_status.name)"
                                                class="mx-1">
                                                {{ history.new_status.name }}
                                            </v-chip>
                                        </v-list-item-title>
                                        <v-list-item-subtitle>
                                            {{ formatDateTime(history.change_date) }}
                                        </v-list-item-subtitle>
                                    </v-list-item>
                                </v-list>
                            </v-card>

                            <v-alert v-else-if="historyLoaded && taskHistory.length === 0" type="info" variant="tonal"
                                class="mt-2">
                                No history records found for this task.
                            </v-alert>
                        </v-col>
                    </v-row>
                </v-container>
            </v-card-text>

            <v-card-actions>
                <v-spacer></v-spacer>
                <v-btn color="primary" variant="text" @click="$emit('update:modelValue', false)">
                    Close
                </v-btn>
            </v-card-actions>
        </v-card>
    </v-dialog>
</template>

<script setup lang="ts">
import { ref, watch } from 'vue';
import type { TaskExt, TaskHistoryExt } from '@/types';
import tasksService from '@/services/tasksService';
import { getTaskStatusColor, getTaskTypeColor } from '@/utils/taskColors';
import { DevelopmentLogger } from '@/utils/logger';

const logger = new DevelopmentLogger({ prefix: '[TaskDetailsModal]' });

const props = defineProps<{
    modelValue: boolean;
    task: TaskExt | null;
}>();

const emit = defineEmits<{
    (e: 'update:modelValue', value: boolean): void;
}>();

const taskHistory = ref<TaskHistoryExt[]>([]);
const loadingHistory = ref(false);
const historyLoaded = ref(false);

watch(() => props.modelValue, (newValue) => {
    if (!newValue) {
        taskHistory.value = [];
        historyLoaded.value = false;
    }
});

const loadTaskHistory = async () => {
    if (!props.task) return;

    loadingHistory.value = true;
    try {
        taskHistory.value = await tasksService.getTaskHistoriesByTaskExt(props.task.id);
        historyLoaded.value = true;
    } catch (error) {
        logger.error('Error loading task history:', error);
    } finally {
        loadingHistory.value = false;
    }
};

const formatDate = (date: Date | string): string => {
    if (!date) return 'N/A';
    const d = new Date(date);
    return d.toLocaleDateString('en-US', { year: 'numeric', month: 'long', day: 'numeric' });
};

const formatDateTime = (date: Date | string): string => {
    if (!date) return 'N/A';
    const d = new Date(date);
    return d.toLocaleString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
};
</script>

<style scoped>
.border-b {
    border-bottom: 1px solid rgba(0, 0, 0, 0.12);
}
</style>
