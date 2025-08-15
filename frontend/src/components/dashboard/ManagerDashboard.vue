<template>
    <div>
        <h1 class="text-h4 mb-6">Manager Panel</h1>

        <v-alert v-if="dashboardData && dashboardData.sprint && !dashboardData.sprint.active" type="warning"
            variant="outlined" class="mb-6" prominent>
            <template v-slot:prepend>
                <v-icon>mdi-alert-circle</v-icon>
            </template>
            <v-alert-title>No Active Sprint</v-alert-title>
            Current sprint has ended and no new sprint has been created yet. The data below shows the last known sprint
            information.
        </v-alert>

        <v-card class="mb-6">
            <v-card-text>
                <v-row>
                    <v-col cols="12" md="6">
                        <div class="d-flex align-center">
                            <v-icon size="48" :color="dashboardData?.sprint?.active ? 'primary' : 'warning'"
                                class="mr-4">
                                mdi-rocket-launch
                            </v-icon>
                            <div>
                                <div class="text-h5 font-weight-bold">
                                    {{ currentSprintName }}
                                </div>
                                <div class="text-caption">
                                    {{ dashboardData?.sprint?.active ? 'Active sprint' : 'Inactive sprint' }}
                                </div>
                                <div v-if="dashboardData?.sprint" class="text-caption text-medium-emphasis">
                                    {{ formatDateRange(dashboardData.sprint.startDate, dashboardData.sprint.endDate) }}
                                </div>
                                <div v-if="dashboardData?.sprint" class="text-caption text-medium-emphasis">
                                    {{ getSprintDuration(dashboardData.sprint.startDate, dashboardData.sprint.endDate)
                                    }}
                                </div>
                                <div v-if="dashboardData?.sprint && !dashboardData.sprint.active"
                                    class="text-caption text-warning">
                                    Sprint ended {{ getSprintStatusInfo(dashboardData.sprint.endDate,
                                        dashboardData.sprint.active) }}
                                </div>
                            </div>
                        </div>
                    </v-col>
                    <v-col cols="12" md="6">
                        <div class="d-flex flex-column justify-center h-100">
                            <div class="text-caption mb-2">Team progress</div>
                            <v-progress-linear :model-value="teamProgress" height="20"
                                :color="dashboardData?.sprint?.active ? 'success' : 'warning'">
                                <template v-slot:default="{ value }">
                                    <span class="text-white font-weight-bold">{{ Math.round(value) }}%</span>
                                </template>
                            </v-progress-linear>
                        </div>
                    </v-col>
                </v-row>
            </v-card-text>
        </v-card>

        <v-card>
            <v-card-title class="text-h5 d-flex align-center">
                Team task board
                <v-chip v-if="dashboardData && dashboardData.sprint && !dashboardData.sprint.active" color="warning"
                    size="small" class="ml-3">
                    Historical Data
                </v-chip>
            </v-card-title>
            <v-card-text>
                <v-row>
                    <v-col cols="12" md="4">
                        <div class="text-h6 mb-3">To Do</div>
                        <div v-if="getTasksByStatus('To Do').length > 0">
                            <v-card v-for="task in getTasksByStatus('To Do')" :key="task.statusId" variant="outlined"
                                class="mb-3" :class="!dashboardData?.sprint?.active ? 'opacity-75' : ''">
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
                                variant="outlined" class="mb-3"
                                :class="!dashboardData?.sprint?.active ? 'opacity-75' : ''">
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
import { formatDateRange, calculateDuration, getRelativeTime } from '@/utils/dateFormatter';
import type { ManagerDashboardInfoDto, ManagerTaskDto } from '@/types';
import { managerDashboardLogger } from '@/utils/logger';

const dashboardData = ref<ManagerDashboardInfoDto | null>(null);

const currentSprintName = computed(() => {
    if (!dashboardData.value || !dashboardData.value.sprint) {
        return 'No Sprint';
    }
    return dashboardData.value.sprint.name || 'Unknown Sprint';
});

const currentSprintTasks = computed(() => {
    return dashboardData.value?.tasks || [];
});

const teamProgress = computed(() => {
    if (!currentSprintTasks.value.length) return 0;

    const completedTasks = currentSprintTasks.value
        .filter((task: ManagerTaskDto) => task.statusName.toLowerCase().includes('completed'))
        .reduce((sum: number, task: ManagerTaskDto) => sum + task.count, 0);

    const totalTasks = currentSprintTasks.value.reduce((sum: number, task: ManagerTaskDto) => sum + task.count, 0);

    return totalTasks > 0 ? (completedTasks / totalTasks) * 100 : 0;
});

const getTasksByStatus = (statusCategory: string): ManagerTaskDto[] => {
    if (!currentSprintTasks.value.length) return [];

    const category = statusCategory.toLowerCase();
    return currentSprintTasks.value.filter((task: ManagerTaskDto) => {
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

const getSprintDuration = (startDate: Date, endDate: Date): string => {
    const duration = calculateDuration(startDate, endDate);
    return `${duration.displayText} duration`;
};

const getSprintStatusInfo = (endDate: Date, isActive: boolean): string => {
    if (isActive) {
        return '';
    }
    return getRelativeTime(endDate);
};

onMounted(async () => {
    try {
        const user = authService.getStoredUser();
        if (!user?.id) {
            throw new Error('User not found or missing ID');
        }
        managerDashboardService.setUserId(user.id);
        dashboardData.value = await managerDashboardService.getDashboardInfo();
    } catch (err) {
        managerDashboardLogger.error('Error fetching dashboard data:', err);
    }
});
</script>
