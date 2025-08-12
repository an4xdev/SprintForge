<template>
    <div>
        <h1 class="text-h4 mb-6">Manager Panel</h1>

        <v-alert v-if="dashboardData && !dashboardData.isActive" type="warning" variant="outlined" class="mb-6"
            prominent>
            <template v-slot:prepend>
                <v-icon>mdi-alert-circle</v-icon>
            </template>
            <v-alert-title>No Active Sprint</v-alert-title>
            Current sprint has ended and no new sprint has been created yet. The data below shows the last known sprint
            information.
        </v-alert>

        <v-row class="mb-6">
            <v-col cols="12" md="6">
                <v-card>
                    <v-card-text>
                        <div class="d-flex align-center">
                            <v-icon size="48" :color="dashboardData?.isActive ? 'primary' : 'warning'" class="mr-4">
                                mdi-rocket-launch
                            </v-icon>
                            <div>
                                <div class="text-h5 font-weight-bold">
                                    {{ currentSprintName }}
                                </div>
                                <div class="text-caption">
                                    {{ dashboardData?.isActive ? 'Active sprint' : 'Inactive sprint' }}
                                </div>
                            </div>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" md="6">
                <v-card>
                    <v-card-text>
                        <div class="text-caption mb-2">Team progress</div>
                        <v-progress-linear :model-value="teamProgress"
                            :key="`progress-${selectedSprintIndex}-${teamProgress}`" height="20"
                            :color="dashboardData?.isActive ? 'success' : 'warning'">
                            <template v-slot:default="{ value }">
                                <span class="text-white font-weight-bold">{{ Math.round(value) }}%</span>
                            </template>
                        </v-progress-linear>
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row>

        <v-card v-if="dashboardData && dashboardData.sprints.length > 0" class="mb-6">
            <v-tabs v-model="selectedSprintIndex" class="mb-4">
                <v-tab v-for="(sprint, index) in dashboardData.sprints" :key="sprint.id" :value="index"
                    :class="!dashboardData.isActive ? 'text-warning' : ''">
                    <v-icon v-if="!dashboardData.isActive" size="small" class="mr-2" color="warning">
                        mdi-alert-circle-outline
                    </v-icon>
                    {{ sprint.name }}
                </v-tab>
            </v-tabs>
        </v-card>

        <v-card>
            <v-card-title class="text-h5 d-flex align-center">
                Team task board
                <v-chip v-if="dashboardData && !dashboardData.isActive" color="warning" size="small" class="ml-3">
                    Historical Data
                </v-chip>
            </v-card-title>
            <v-card-text>
                <v-row>
                    <v-col cols="12" md="4">
                        <div class="text-h6 mb-3">To Do</div>
                        <div v-if="getTasksByStatus('To Do').length > 0">
                            <v-card v-for="task in getTasksByStatus('To Do')" :key="task.statusId" variant="outlined"
                                class="mb-3" :class="!dashboardData?.isActive ? 'opacity-75' : ''">
                                <v-card-text>
                                    <div class="font-weight-medium">{{ task.statusName }}</div>
                                    <div class="text-caption text-medium-emphasis">Count: {{ task.count }}</div>
                                </v-card-text>
                            </v-card>
                        </div>
                        <div v-else class="text-center py-4 text-grey">
                            No tasks
                        </div>
                    </v-col>
                    <v-col cols="12" md="4">
                        <div class="text-h6 mb-3">In Progress</div>
                        <div v-if="getTasksByStatus('In Progress').length > 0">
                            <v-card v-for="task in getTasksByStatus('In Progress')" :key="task.statusId"
                                variant="outlined" class="mb-3" :class="!dashboardData?.isActive ? 'opacity-75' : ''">
                                <v-card-text>
                                    <div class="font-weight-medium">{{ task.statusName }}</div>
                                    <div class="text-caption text-medium-emphasis">Count: {{ task.count }}</div>
                                </v-card-text>
                            </v-card>
                        </div>
                        <div v-else class="text-center py-4 text-grey">
                            No tasks
                        </div>
                    </v-col>
                    <v-col cols="12" md="4">
                        <div class="text-h6 mb-3">Completed</div>
                        <div v-if="getTasksByStatus('Completed').length > 0">
                            <v-card v-for="task in getTasksByStatus('Completed')" :key="task.statusId"
                                variant="outlined" class="mb-3 opacity-75">
                                <v-card-text>
                                    <div class="font-weight-medium">{{ task.statusName }}</div>
                                    <div class="text-caption text-medium-emphasis">Count: {{ task.count }}</div>
                                </v-card-text>
                            </v-card>
                        </div>
                        <div v-else class="text-center py-4 text-grey">
                            No tasks
                        </div>
                    </v-col>
                </v-row>
            </v-card-text>
        </v-card>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import managerDashboardService from '@/services/managerDashboardService';
import authService from '@/services/authService';
import type { ManagerDashboardInfoDto, ManagerTaskDto } from '@/types';

const dashboardData = ref<ManagerDashboardInfoDto | null>(null);
const selectedSprintIndex = ref(0);
const loading = ref(true);
const error = ref<string | null>(null);

const currentSprintName = computed(() => {
    if (!dashboardData.value || dashboardData.value.sprints.length === 0) {
        return 'No Sprint';
    }

    const safeIndex = Math.max(0, Math.min(selectedSprintIndex.value, dashboardData.value.sprints.length - 1));
    return dashboardData.value.sprints[safeIndex]?.name || 'Unknown Sprint';
});

const currentSprintTasks = computed(() => {
    if (!dashboardData.value || dashboardData.value.sprints.length === 0) {
        return [];
    }

    const safeIndex = Math.max(0, Math.min(selectedSprintIndex.value, dashboardData.value.sprints.length - 1));
    const currentSprintId = dashboardData.value.sprints[safeIndex]?.id;

    if (!currentSprintId) {
        return [];
    }

    const tasksBySprint = dashboardData.value.tasksBySprint.find(
        task => task.sprintId === currentSprintId
    );

    return tasksBySprint?.tasks || [];
});

const teamProgress = computed(() => {
    if (!currentSprintTasks.value.length) return 0;

    const completedTasks = currentSprintTasks.value
        .filter(task => task.statusName.toLowerCase().includes('completed'))
        .reduce((sum, task) => sum + task.count, 0);

    const totalTasks = currentSprintTasks.value.reduce((sum, task) => sum + task.count, 0);

    return totalTasks > 0 ? (completedTasks / totalTasks) * 100 : 0;
});

const getTaskBorderColor = (statusName: string) => {
    const status = statusName.toLowerCase();
    if (status.includes('completed')) {
        return 'border-left-color: #4CAF50;';
    } else if (status.includes('started')) {
        return 'border-left-color: #42A5F5;';
    } else if (status.includes('paused') || status.includes('stopped')) {
        return 'border-left-color: #FFA726;';
    } else {
        return 'border-left-color: #9E9E9E;';
    }
};

const getTaskChipColor = (statusName: string) => {
    const status = statusName.toLowerCase();
    if (status.includes('completed')) {
        return 'success';
    } else if (status.includes('started')) {
        return 'primary';
    } else if (status.includes('paused') || status.includes('stopped')) {
        return 'warning';
    } else {
        return 'default';
    }
};

const getTasksByStatus = (statusCategory: string): ManagerTaskDto[] => {
    if (!currentSprintTasks.value.length) return [];

    const category = statusCategory.toLowerCase();
    return currentSprintTasks.value.filter(task => {
        const status = task.statusName.toLowerCase();

        if (category === 'to do') {
            return status.includes('created') || status.includes('assigned');
        } else if (category === 'in progress') {
            return status.includes('started') || status.includes('paused') || status.includes('stopped');
        } else if (category === 'completed') {
            return status.includes('completed');
        }

        return false;
    });
};

onMounted(async () => {
    try {
        loading.value = true;
        error.value = null;

        const user = authService.getStoredUser();
        if (!user?.id) {
            throw new Error('User not found or missing ID');
        }

        managerDashboardService.setUserId(user.id);
        dashboardData.value = await managerDashboardService.getDashboardInfo();

        if (dashboardData.value && selectedSprintIndex.value >= dashboardData.value.sprints.length) {
            selectedSprintIndex.value = 0;
        }
    } catch (err) {
        console.error('Error fetching dashboard data:', err);
        error.value = err instanceof Error ? err.message : 'Failed to fetch dashboard data';
    } finally {
        loading.value = false;
    }
});
</script>
