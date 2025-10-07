<template>
    <v-card elevation="2">
        <v-card-title class="text-h6">
            <v-icon class="me-2">mdi-chart-line</v-icon>
            Sprint Progress Comparison
        </v-card-title>
        <v-card-text style="min-height: 400px;">
            <div v-if="sprintData && sprintData.length > 0" class="sprint-comparison-chart">
                <!-- Chart Header -->
                <div class="chart-header mb-4">
                    <div class="d-flex justify-space-between align-center">
                        <span class="text-body-1 font-weight-medium">Sprint Name</span>
                        <span class="text-body-2 text-medium-emphasis">Progress (%)</span>
                    </div>
                </div>

                <!-- Chart Bars -->
                <div class="chart-bars">
                    <div v-for="(sprint, index) in sprintData" :key="sprint.sprintId" class="sprint-bar mb-3"
                        :style="{ animationDelay: `${index * 0.1}s` }">
                        <div class="d-flex align-center">
                            <div class="sprint-info">
                                <div class="text-body-1 font-weight-medium mb-1">{{ sprint.sprintName }}</div>
                                <div class="text-caption text-medium-emphasis">
                                    {{ sprint.taskCountCompleted }}/{{ sprint.taskCount }} tasks •
                                    {{ formatDuration(sprint.totalTaskTime) }}
                                </div>
                            </div>

                            <div class="flex-grow-1 mx-4">
                                <div class="progress-container">
                                    <v-progress-linear :model-value="sprint.completedRatio * 100"
                                        :color="getSprintProgressColor(sprint.completedRatio, index)" height="12"
                                        rounded class="sprint-progress">
                                        <template #default="{ value }">
                                            <div class="progress-label">{{ Math.round(value) }}%</div>
                                        </template>
                                    </v-progress-linear>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>

            <div v-else class="text-center d-flex flex-column justify-center align-center" style="height: 100%;">
                <v-icon size="64" class="mb-4 text-medium-emphasis">mdi-chart-line</v-icon>
                <p class="text-body-1 text-medium-emphasis">No sprint data to display</p>
                <p class="text-caption text-medium-emphasis" v-if="sprintData">
                    Debug: Data received but empty (length: {{ sprintData.length }})
                </p>
                <p class="text-caption text-medium-emphasis" v-else>
                    Debug: No data received (null/undefined)
                </p>
            </div>
        </v-card-text>
    </v-card>
</template>

<script setup lang="ts">
import type { SprintReportDto } from '@/types';

interface Props {
    sprintData?: SprintReportDto[] | null;
}

const props = withDefaults(defineProps<Props>(), {
    sprintData: null
});

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
    animation: fadeInUp 0.6s ease-out;
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
</style>