<template>
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
                            stroke-width="12" fill="none" stroke-linecap="round" :stroke-dasharray="circumference"
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
                            <span class="text-body-2 ms-2">{{ teamData.taskCountCompleted }} completed</span>
                        </div>
                        <div class="d-flex align-center">
                            <div class="legend-dot" style="background-color: #e0e0e0;"></div>
                            <span class="text-body-2 ms-2">{{ teamData.taskCount - teamData.taskCountCompleted }}
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
</template>

<script setup lang="ts">
import { computed } from 'vue';
import type { TeamReportDto } from '@/types';

interface Props {
    teamData?: TeamReportDto | null;
}

const props = withDefaults(defineProps<Props>(), {
    teamData: null
});

const completionPercentage = computed(() => {
    if (!props.teamData || props.teamData.taskCount === 0) return 0;
    return Math.round((props.teamData.taskCountCompleted / props.teamData.taskCount) * 100);
});

const circumference = computed(() => 2 * Math.PI * 80); // radius = 80

// Methods
const getProgressColor = (percentage: number): string => {
    if (percentage >= 80) return '#4CAF50'; // green
    if (percentage >= 60) return '#FF9800'; // orange
    if (percentage >= 40) return '#FFC107'; // amber
    return '#F44336'; // red
};
</script>

<style scoped>
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
    flex-shrink: 0;
}
</style>