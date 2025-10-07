<template>
    <v-row>
        <v-col cols="12" md="6">
            <v-card elevation="2">
                <v-card-title class="text-h6">
                    <v-icon class="me-2">mdi-chart-bar</v-icon>
                    Project Progress Overview
                </v-card-title>
                <v-card-text style="min-height: 300px;">
                    <div v-if="projectData && projectData.length > 0" class="project-progress-chart">
                        <div v-for="(project, index) in projectData.slice(0, 5)" :key="project.projectId"
                            class="mb-3 project-bar" :style="{ animationDelay: `${index * 0.1}s` }">
                            <div class="d-flex justify-space-between align-center mb-1">
                                <span class="text-body-2 font-weight-medium">{{
                                    project.projectName }}</span>
                                <span class="text-body-2">{{
                                    Math.round(project.completedRatio * 100)
                                    }}%</span>
                            </div>
                            <v-progress-linear :model-value="project.completedRatio * 100"
                                :color="getProgressColor(project.completedRatio)" height="10" rounded
                                class="progress-bar-hover" />
                        </div>
                    </div>
                    <div v-else class="d-flex flex-column align-center justify-center" style="height: 250px;">
                        <v-icon size="64" color="grey-lighten-1">mdi-chart-bar-stacked</v-icon>
                        <p class="text-body-1 text-medium-emphasis mt-4">No project data available</p>
                    </div>
                </v-card-text>
            </v-card>
        </v-col>
        <v-col cols="12" md="6">
            <v-card elevation="2">
                <v-card-title class="text-h6">
                    <v-icon class="me-2">mdi-chart-donut</v-icon>
                    Summary Statistics
                </v-card-title>
                <v-card-text class="d-flex flex-column justify-center" style="min-height: 300px;">
                    <div v-if="projectData && projectData.length > 0" class="text-center">
                        <div class="stats-item mb-4">
                            <div class="text-h3 font-weight-bold mb-2 text-primary">
                                {{ projectData.length }}
                            </div>
                            <div class="text-body-1">Total Projects</div>
                        </div>

                        <div class="stats-item mb-4">
                            <div class="text-h4 font-weight-bold mb-2 text-success">
                                {{ Math.round(averageProgress) }}%
                            </div>
                            <div class="text-body-1">Average Progress</div>
                        </div>

                        <div class="stats-item">
                            <div class="text-h5 font-weight-bold mb-2 text-info">
                                {{ totalTasks }}
                            </div>
                            <div class="text-body-1">Total Tasks</div>
                        </div>
                    </div>
                    <div v-else class="d-flex flex-column align-center justify-center" style="height: 250px;">
                        <v-icon size="64" color="grey-lighten-1">mdi-chart-donut</v-icon>
                        <p class="text-body-1 text-medium-emphasis mt-4">No statistics available</p>
                    </div>
                </v-card-text>
            </v-card>
        </v-col>
    </v-row>
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { ProjectReportDto } from '@/types';

interface Props {
    projectData?: ProjectReportDto[] | null;
}

const props = withDefaults(defineProps<Props>(), {
    projectData: null
});

const averageProgress = computed(() => {
    if (!props.projectData || props.projectData.length === 0) return 0;

    const totalCompleted = props.projectData.reduce((sum, p) => sum + p.taskCountCompleted, 0);
    const totalTasks = props.projectData.reduce((sum, p) => sum + p.taskCount, 0);

    return totalTasks > 0 ? (totalCompleted / totalTasks) * 100 : 0;
});

const totalTasks = computed(() => {
    if (!props.projectData || props.projectData.length === 0) return 0;
    return props.projectData.reduce((sum, p) => sum + p.taskCount, 0);
});

const getProgressColor = (ratio: number): string => {
    if (ratio >= 0.8) return 'success';
    if (ratio >= 0.6) return 'warning';
    if (ratio >= 0.3) return 'primary';
    return 'error';
};
</script>

<style scoped>
.project-bar {
    opacity: 0;
    animation: slideInUp 0.6s ease-out forwards;
}

@keyframes slideInUp {
    from {
        opacity: 0;
        transform: translateY(20px);
    }

    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.progress-bar-hover {
    transition: all 0.3s ease;
}

.progress-bar-hover:hover {
    transform: scaleY(1.2);
}

.stats-item {
    opacity: 0;
    animation: fadeInScale 0.6s ease-out forwards;
}

.stats-item:nth-child(1) {
    animation-delay: 0.1s;
}

.stats-item:nth-child(2) {
    animation-delay: 0.2s;
}

.stats-item:nth-child(3) {
    animation-delay: 0.3s;
}

@keyframes fadeInScale {
    from {
        opacity: 0;
        transform: scale(0.8);
    }

    to {
        opacity: 1;
        transform: scale(1);
    }
}
</style>