<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <div class="d-flex align-center mb-6">
                    <v-icon color="primary" size="large" class="me-3">mdi-history</v-icon>
                    <h1 class="text-h4">Team Task History</h1>
                </div>

                <v-snackbar v-model="showError" color="error" timeout="3000" location="top right">
                    <v-icon start>mdi-alert-circle</v-icon>
                    {{ error }}
                </v-snackbar>

                <v-snackbar v-model="showSuccess" color="success" timeout="3000" location="top right">
                    <v-icon start>mdi-check-circle</v-icon>
                    {{ successMessage }}
                </v-snackbar>

                <v-alert v-if="!currentTeam" type="info" variant="tonal" class="mb-4">
                    <v-alert-title>No Team Assigned</v-alert-title>
                    You need to have a team assigned to view task history.
                </v-alert>

                <div class="py-1" v-else>
                    <v-card class="mb-4" variant="outlined">
                        <v-card-text>
                            <div class="d-flex align-center">
                                <v-icon color="primary" class="me-2">mdi-account-group</v-icon>
                                <div>
                                    <div class="text-subtitle-2 text-medium-emphasis">Your Team</div>
                                    <div class="text-h6">{{ currentTeam.name }}</div>
                                </div>
                            </div>
                        </v-card-text>
                    </v-card>

                    <v-sheet border rounded>
                        <v-data-table :headers="headers"
                            :hide-default-footer="groupedHistories !== null && groupedHistories.length < 11"
                            :items="groupedHistories ?? []" :loading="loading" class="elevation-1" show-expand
                            v-model:expanded="expanded" item-value="task.id">
                            <template v-slot:top>
                                <v-toolbar flat>
                                    <v-toolbar-title>
                                        <v-icon color="medium-emphasis" icon="mdi-history" size="x-small"
                                            start></v-icon>
                                        Team Task History (Grouped by Task)
                                    </v-toolbar-title>
                                    <v-spacer></v-spacer>
                                    <v-btn prepend-icon="mdi-refresh" rounded="lg" text="Refresh" variant="text"
                                        @click="refreshData"></v-btn>
                                </v-toolbar>
                            </template>

                            <template v-slot:item.task="{ value }">
                                <div class="py-2">
                                    <div class="font-weight-medium">{{ value.name }}</div>
                                    <div v-if="value.description" class="text-caption text-medium-emphasis">
                                        {{ value.description }}
                                    </div>
                                </div>
                            </template>

                            <template v-slot:item.changesCount="{ value }">
                                <v-chip color="primary" size="small" variant="tonal">
                                    {{ value }} {{ value === 1 ? 'change' : 'changes' }}
                                </v-chip>
                            </template>

                            <template v-slot:item.currentStatus="{ value }">
                                <v-chip :color="getTaskStatusColor(value.name)" size="small" variant="flat">
                                    {{ value.name }}
                                </v-chip>
                            </template>

                            <template v-slot:item.lastChangeDate="{ value }">
                                <div class="text-body-2">{{ formatDateTime(value) }}</div>
                            </template>

                            <template v-slot:item.actions="{ item }">
                                <v-btn color="primary" variant="text" size="small" prepend-icon="mdi-eye"
                                    @click="viewTaskDetails(item.task.id)">
                                    View Task
                                </v-btn>
                            </template>

                            <template v-slot:expanded-row="{ columns, item }">
                                <tr>
                                    <td :colspan="columns.length" class="pa-0">
                                        <v-sheet color="grey-darken-4" class="pa-4">
                                            <div class="d-flex align-center justify-space-between mb-3">
                                                <div class="text-subtitle-2 font-weight-bold text-grey-lighten-2">Status
                                                    Change History</div>
                                                <v-btn-group variant="outlined" density="compact" size="small">
                                                    <v-btn :variant="historySortOrder === 'desc' ? 'flat' : 'outlined'"
                                                        :color="historySortOrder === 'desc' ? 'primary' : undefined"
                                                        size="small" @click="historySortOrder = 'desc'">
                                                        <v-icon>mdi-sort-calendar-descending</v-icon>
                                                        <v-tooltip activator="parent" location="top">Newest
                                                            First</v-tooltip>
                                                    </v-btn>
                                                    <v-btn :variant="historySortOrder === 'asc' ? 'flat' : 'outlined'"
                                                        :color="historySortOrder === 'asc' ? 'primary' : undefined"
                                                        size="small" @click="historySortOrder = 'asc'">
                                                        <v-icon>mdi-sort-calendar-ascending</v-icon>
                                                        <v-tooltip activator="parent" location="top">Oldest
                                                            First</v-tooltip>
                                                    </v-btn>
                                                </v-btn-group>
                                            </div>
                                            <v-table density="compact" theme="dark">
                                                <thead>
                                                    <tr>
                                                        <th>Previous Status</th>
                                                        <th>New Status</th>
                                                        <th>Change Date</th>
                                                    </tr>
                                                </thead>
                                                <tbody>
                                                    <tr v-for="(history, idx) in getSortedHistories(item.histories)"
                                                        :key="idx">
                                                        <td>
                                                            <v-chip v-if="history.old_status"
                                                                :color="getTaskStatusColor(history.old_status.name)"
                                                                size="x-small" variant="flat">
                                                                {{ history.old_status.name }}
                                                            </v-chip>
                                                            <span v-else
                                                                class="text-caption text-medium-emphasis font-italic">None</span>
                                                        </td>
                                                        <td>
                                                            <v-chip :color="getTaskStatusColor(history.new_status.name)"
                                                                size="x-small" variant="flat">
                                                                {{ history.new_status.name }}
                                                            </v-chip>
                                                        </td>
                                                        <td class="text-caption">{{ formatDateTime(history.change_date)
                                                            }}</td>
                                                    </tr>
                                                </tbody>
                                            </v-table>
                                        </v-sheet>
                                    </td>
                                </tr>
                            </template>

                            <template v-slot:no-data>
                                <div class="text-center py-8">
                                    <v-icon size="64" color="grey-lighten-1">mdi-history</v-icon>
                                    <div class="text-h6 text-medium-emphasis mt-2">No task history records found for
                                        your team</div>
                                    <v-btn prepend-icon="mdi-backup-restore" rounded="lg" text="Refresh" variant="text"
                                        border class="mt-4" @click="refreshData"></v-btn>
                                </div>
                            </template>
                        </v-data-table>
                    </v-sheet>
                </div>

                <TaskDetailsModal v-model="taskDetailsDialog" :task="selectedTaskDetails" />
            </v-container>
        </v-main>
    </v-layout>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import tasksService from '@/services/tasksService';
import teamsService from '@/services/teamsService';
import { extractErrorMessage } from '@/utils/errorHandler';
import type { TaskHistoryExt, TaskExt, Team } from '@/types';
import { DevelopmentLogger } from '@/utils/logger';
import TaskDetailsModal from '@/components/modals/TaskDetailsModal.vue';
import { useAuthStore } from '@/stores/auth';
import { getTaskStatusColor } from '@/utils/taskColors';

const logger = new DevelopmentLogger({ prefix: '[ManagerTaskHistoryView]' });
const authStore = useAuthStore();

const expanded = ref<string[]>([]);
const historySortOrder = ref<'asc' | 'desc'>('desc');
const error = ref('');
const showError = ref(false);
const successMessage = ref('');
const showSuccess = ref(false);

const headers = [
    { title: 'Task', key: 'task', align: 'start' as const, sortable: false },
    { title: 'Changes', key: 'changesCount', align: 'center' as const },
    { title: 'Current Status', key: 'currentStatus', align: 'center' as const },
    { title: 'Last Change', key: 'lastChangeDate' },
    { title: 'Actions', key: 'actions', align: 'end' as const, sortable: false }
];

const currentTeam = ref<Team | null>(null);
const loadingTeam = ref(false);
const taskDetailsDialog = ref(false);
const selectedTaskDetails = ref<TaskExt | null>(null);
const taskHistories = ref<TaskHistoryExt[]>([]);
const loadingHistory = ref(false);

const loading = computed(() => loadingHistory.value || loadingTeam.value);

const groupedHistories = computed(() => {
    if (!taskHistories.value || taskHistories.value.length === 0) return [];

    const grouped = new Map<string, {
        task: TaskHistoryExt['task'];
        histories: TaskHistoryExt[];
        changesCount: number;
        currentStatus: TaskHistoryExt['new_status'];
        lastChangeDate: Date | string;
    }>();

    const sortedHistories = [...taskHistories.value].sort((a, b) => {
        return new Date(b.change_date).getTime() - new Date(a.change_date).getTime();
    });

    sortedHistories.forEach(history => {
        const taskId = history.task.id;

        if (!grouped.has(taskId)) {
            grouped.set(taskId, {
                task: history.task,
                histories: [],
                changesCount: 0,
                currentStatus: history.new_status,
                lastChangeDate: history.change_date
            });
        }

        const group = grouped.get(taskId)!;
        group.histories.push(history);
        group.changesCount++;
    });

    return Array.from(grouped.values()).sort((a, b) => {
        return new Date(b.lastChangeDate).getTime() - new Date(a.lastChangeDate).getTime();
    });
});

const getSortedHistories = (histories: TaskHistoryExt[]) => {
    return [...histories].sort((a, b) => {
        const timeA = new Date(a.change_date).getTime();
        const timeB = new Date(b.change_date).getTime();
        return historySortOrder.value === 'desc' ? timeB - timeA : timeA - timeB;
    });
};

const refreshHistory = async () => {
    if (!authStore.user?.id) return;

    loadingHistory.value = true;
    try {
        taskHistories.value = await tasksService.getTaskHistoriesByManagerExt(authStore.user.id);
        error.value = '';
    } catch (err) {
        const errorDetails = extractErrorMessage(err);
        error.value = errorDetails.message;
        logger.error('Error loading task histories:', err);
        taskHistories.value = [];
    } finally {
        loadingHistory.value = false;
    }
};

const loadTeamData = async () => {
    if (!authStore.user?.id) return;

    loadingTeam.value = true;
    try {
        currentTeam.value = await teamsService.getTeamByManager(authStore.user.id);
        error.value = '';
    } catch (err) {
        const errorDetails = extractErrorMessage(err);
        error.value = errorDetails.message;
        showError.value = true;
        logger.error('Error loading team data:', err);
        currentTeam.value = null;
    } finally {
        loadingTeam.value = false;
    }
};

const refreshData = async () => {
    await Promise.all([
        refreshHistory(),
        loadTeamData()
    ]);
};

const formatDateTime = (date: Date | string): string => {
    if (!date) return 'N/A';
    const d = new Date(date);
    return d.toLocaleString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
};

const viewTaskDetails = async (taskId: string) => {
    try {
        const task = await tasksService.getTaskExtById(taskId);
        selectedTaskDetails.value = task;
        taskDetailsDialog.value = true;
        error.value = '';
    } catch (err) {
        const errorDetails = extractErrorMessage(err);
        error.value = errorDetails.message;
        showError.value = true;
        logger.error('Error loading task details:', err);
    }
};

onMounted(async () => {
    await loadTeamData();
    await refreshHistory();
});
</script>

<style scoped></style>
