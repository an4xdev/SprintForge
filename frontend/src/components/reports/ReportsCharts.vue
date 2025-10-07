<template>
    <div class="charts-container">
        <!-- Team Progress Chart -->
        <v-row class="mb-6">
            <v-col cols="12" md="6">
                <v-card elevation="2">
                    <v-card-title class="text-h6">
                        <v-icon class="me-2">mdi-chart-donut</v-icon>
                        Team Task Progress
                    </v-card-title>
                    <v-card-text class="d-flex justify-center align-center" style="min-height: 300px;">
                        <div v-if="teamData" class="text-center">
                            <div class="position-relative d-inline-block">
                                <!-- Progress Circle -->
                                <svg width="200" height="200" class="progress-circle">
                                    <circle cx="100" cy="100" r="80" stroke="#e0e0e0" stroke-width="12" fill="none" />
                                    <circle cx="100" cy="100" r="80" :stroke="getProgressColor(completionPercentage)"
                                        stroke-width="12" fill="none" stroke-linecap="round"
                                        :stroke-dasharray="circumference"
                                        :stroke-dashoffset="circumference - (completionPercentage / 100) * circumference"
                                        class="progress-bar" />
                                </svg>

                                <!-- Center Text -->
                                <div class="progress-text">
                                    <div class="text-h4 font-weight-bold">{{ completionPercentage }}%</div>
                                    <div class="text-body-2 text-medium-emphasis">Completed</div>
                                </div>
                            </div>

                            <!-- Legend -->
                            <div class="mt-4">
                                <div class="d-flex justify-center gap-4">
                                    <div class="d-flex align-center">
                                        <div class="legend-dot"
                                            :style="{ backgroundColor: getProgressColor(completionPercentage) }"></div>
                                        <span class="text-body-2">{{ teamData.taskCountCompleted }} completed</span>
                                    </div>
                                    <div class="d-flex align-center">
                                        <div class="legend-dot" style="background-color: #e0e0e0;"></div>
                                        <span class="text-body-2">{{ teamData.taskCount - teamData.taskCountCompleted }}
                                            remaining</span>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div v-else class="text-center">
                            <v-icon size="64" class="mb-4 text-medium-emphasis">mdi-chart-donut</v-icon>
                            <p class="text-body-1 text-medium-emphasis">No data to display</p>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>

            <!-- Team Stats Chart -->
            <v-col cols="12" md="6">
                <v-card elevation="2">
                    <v-card-title class="text-h6">
                        <v-icon class="me-2">mdi-chart-bar</v-icon>
                        Team Statistics
                    </v-card-title>
                    <v-card-text style="min-height: 300px;">
                        <div v-if="teamData" class="stats-chart">
                            <div class="stat-bar mb-4">
                                <div class="d-flex justify-space-between align-center mb-2">
                                    <span class="text-body-2 font-weight-medium">Developers</span>
                                    <span class="text-body-2 font-weight-bold">{{ teamData.developerCount }}</span>
                                </div>
                                <v-progress-linear
                                    :model-value="(teamData.developerCount / Math.max(teamData.developerCount, 10)) * 100"
                                    color="info" height="12" rounded />
                            </div>

                            <div class="stat-bar mb-4">
                                <div class="d-flex justify-space-between align-center mb-2">
                                    <span class="text-body-2 font-weight-medium">All Tasks</span>
                                    <span class="text-body-2 font-weight-bold">{{ teamData.taskCount }}</span>
                                </div>
                                <v-progress-linear
                                    :model-value="(teamData.taskCount / Math.max(teamData.taskCount, 50)) * 100"
                                    color="primary" height="12" rounded />
                            </div>

                            <div class="stat-bar mb-4">
                                <div class="d-flex justify-space-between align-center mb-2">
                                    <span class="text-body-2 font-weight-medium">Completed Tasks</span>
                                    <span class="text-body-2 font-weight-bold">{{ teamData.taskCountCompleted }}</span>
                                </div>
                                <v-progress-linear
                                    :model-value="teamData.taskCount > 0 ? (teamData.taskCountCompleted / teamData.taskCount) * 100 : 0"
                                    color="success" height="12" rounded />
                            </div>

                            <div class="stat-bar">
                                <div class="d-flex justify-space-between align-center mb-2">
                                    <span class="text-body-2 font-weight-medium">Active Sprints</span>
                                    <span class="text-body-2 font-weight-bold">{{ teamData.sprintsNames.length }}</span>
                                </div>
                                <v-progress-linear
                                    :model-value="(teamData.sprintsNames.length / Math.max(teamData.sprintsNames.length, 5)) * 100"
                                    color="warning" height="12" rounded />
                            </div>
                        </div>

                        <div v-else class="text-center d-flex flex-column justify-center align-center"
                            style="height: 100%;">
                            <v-icon size="64" class="mb-4 text-medium-emphasis">mdi-chart-bar</v-icon>
                            <p class="text-body-1 text-medium-emphasis">No data to display</p>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row>

        <!-- Sprint Comparison Chart -->
        <v-row v-if="sprintData && sprintData.length > 0">
            <v-col cols="12">
                <v-card elevation="2">
                    <v-card-title class="text-h6">
                        <v-icon class="me-2">mdi-chart-line</v-icon>
                        Sprint Progress Comparison
                    </v-card-title>
                    <v-card-text style="min-height: 400px;">
                        <div class="sprint-comparison-chart">
                            <!-- Chart Header -->
                            <div class="chart-header mb-4">
                                <div class="d-flex justify-space-between align-center">
                                    <span class="text-body-2 text-medium-emphasis">Sprints</span>
                                    <span class="text-body-2 text-medium-emphasis">Progress (%)</span>
                                </div>
                            </div>

                            <!-- Chart Bars -->
                            <div class="chart-bars">
                                <div v-for="(sprint, index) in sprintData" :key="sprint.sprintId"
                                    class="sprint-bar mb-4">
                                    <div class="d-flex justify-space-between align-center mb-2">
                                        <div class="sprint-info">
                                            <span class="text-body-2 font-weight-medium">{{ sprint.sprintName }}</span>
                                            <div class="text-caption text-medium-emphasis">
                                                {{ sprint.taskCountCompleted }}/{{ sprint.taskCount }} tasks
                                            </div>
                                        </div>
                                        <div class="text-right">
                                            <span class="text-body-2 font-weight-bold">
                                                {{ Math.round(sprint.completedRatio * 100) }}%
                                            </span>
                                            <div class="text-caption text-medium-emphasis">
                                                {{ formatDuration(sprint.totalTaskTime) }}
                                            </div>
                                        </div>
                                    </div>

                                    <div class="progress-container">
                                        <v-progress-linear :model-value="sprint.completedRatio * 100"
                                            :color="getSprintProgressColor(sprint.completedRatio, index)" height="16"
                                            rounded class="sprint-progress">
                                            <template #default>
                                                <div class="progress-label">
                                                    {{ Math.round(sprint.completedRatio * 100) }}%
                                                </div>
                                            </template>
                                        </v-progress-linear>
                                    </div>
                                </div>
                            </div>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row>
    </div>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { TeamReportDto, SprintReportDto } from '@/types';

interface Props {
    teamData?: TeamReportDto | null;
    sprintData?: SprintReportDto[] | null;
}

const props = withDefaults(defineProps<Props>(), {
    teamData: null,
    sprintData: null
});

const completionPercentage = computed(() => {
    if (!props.teamData || props.teamData.taskCount === 0) return 0;
    return Math.round((props.teamData.taskCountCompleted / props.teamData.taskCount) * 100);
});

const circumference = computed(() => 2 * Math.PI * 80); // radius = 80

const getProgressColor = (percentage: number): string => {
    if (percentage >= 80) return '#4CAF50'; // green
    if (percentage >= 60) return '#FF9800'; // orange
    if (percentage >= 40) return '#FFC107'; // amber
    return '#F44336'; // red
};

const getSprintProgressColor = (ratio: number, index: number): string => {
    const colors = ['primary', 'secondary', 'success', 'warning', 'info', 'error'];
    if (ratio >= 0.8) return 'success';
    if (ratio >= 0.6) return 'warning';
    if (ratio >= 0.3) return colors[index % colors.length];
    return 'error';
};

const formatDuration = (timeString: string): string => {
    if (!timeString || timeString === '00:00:00') {
        return '0h 0m';
    }

    const parts = timeString.split(':');
    if (parts.length >= 2) {
        const hours = parseInt(parts[0]);
        const minutes = parseInt(parts[1]);

        if (hours > 0) {
            return `${hours}h ${minutes}m`;
        }
        return `${minutes}m`;
    }

    return timeString;
};
</script>

<style scoped>
.charts-container {
    width: 100%;
}

.progress-circle {
    transform: rotate(-90deg);
}

.progress-bar {
    transition: stroke-dashoffset 1s ease-in-out;
}

.progress-text {
    position: absolute;
    top: 50%;
    left: 50%;
    transform: translate(-50%, -50%);
}

.legend-dot {
    width: 12px;
    height: 12px;
    border-radius: 50%;
    margin-right: 8px;
    flex-shrink: 0;
}

.stats-chart {
    padding: 16px 0;
}

.stat-bar {
    transition: all 0.3s ease;
}

.stat-bar:hover {
    transform: translateX(4px);
}

.sprint-comparison-chart {
    padding: 16px 0;
}

.chart-header {
    border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
    padding-bottom: 8px;
}

.sprint-bar {
    padding: 12px;
    border-radius: 8px;
    background-color: rgba(var(--v-theme-surface-variant), 0.1);
    transition: all 0.3s ease;
}

.sprint-bar:hover {
    background-color: rgba(var(--v-theme-surface-variant), 0.2);
    transform: translateY(-2px);
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}

.progress-container {
    position: relative;
}

.sprint-progress {
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.progress-label {
    color: white;
    font-size: 11px;
    font-weight: 600;
    text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.5);
}

.sprint-info {
    max-width: 60%;
}

@keyframes fadeInUp {
    from {
        opacity: 0;
        transform: translateY(20px);
    }

    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.sprint-bar {
    animation: fadeInUp 0.6s ease-out;
}

.sprint-bar:nth-child(1) {
    animation-delay: 0.1s;
}

.sprint-bar:nth-child(2) {
    animation-delay: 0.2s;
}

.sprint-bar:nth-child(3) {
    animation-delay: 0.3s;
}

.sprint-bar:nth-child(4) {
    animation-delay: 0.4s;
}

.sprint-bar:nth-child(5) {
    animation-delay: 0.5s;
}
</style>