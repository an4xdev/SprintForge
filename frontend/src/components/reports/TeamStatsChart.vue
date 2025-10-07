<template>
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
                        <span class="text-body-2">{{ teamData.developerCount }}</span>
                    </div>
                    <v-progress-linear :model-value="Math.min(100, teamData.developerCount * 20)" color="primary"
                        height="8" rounded />
                </div>

                <div class="stat-bar mb-4">
                    <div class="d-flex justify-space-between align-center mb-2">
                        <span class="text-body-2 font-weight-medium">Total Tasks</span>
                        <span class="text-body-2">{{ teamData.taskCount }}</span>
                    </div>
                    <v-progress-linear :model-value="Math.min(100, (teamData.taskCount / 20) * 100)" color="info"
                        height="8" rounded />
                </div>

                <div class="stat-bar mb-4">
                    <div class="d-flex justify-space-between align-center mb-2">
                        <span class="text-body-2 font-weight-medium">Completed Tasks</span>
                        <span class="text-body-2">{{ teamData.taskCountCompleted }}</span>
                    </div>
                    <v-progress-linear
                        :model-value="teamData.taskCount > 0 ? (teamData.taskCountCompleted / teamData.taskCount) * 100 : 0"
                        color="success" height="8" rounded />
                </div>

                <div class="stat-bar">
                    <div class="d-flex justify-space-between align-center mb-2">
                        <span class="text-body-2 font-weight-medium">Time Spent</span>
                        <span class="text-body-2">{{ formatDuration(teamData.totalTaskTime) }}</span>
                    </div>
                    <v-progress-linear :model-value="calculateTimeProgress(teamData.totalTaskTime)" color="warning"
                        height="8" rounded />
                </div>
            </div>

            <div v-else class="text-center d-flex flex-column justify-center align-center" style="height: 100%;">
                <v-icon size="64" class="mb-4 text-medium-emphasis">mdi-chart-bar</v-icon>
                <p class="text-body-1 text-medium-emphasis">No data to display</p>
            </div>
        </v-card-text>
    </v-card>
</template>

<script setup lang="ts">
import type { TeamReportDto } from '@/types';

interface Props {
    teamData?: TeamReportDto | null;
}

const props = withDefaults(defineProps<Props>(), {
    teamData: null
});

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

const calculateTimeProgress = (timeString: string): number => {
    const parts = timeString.split(':');
    if (parts.length >= 2) {
        const hours = parseInt(parts[0]);
        return Math.min(100, (hours / 40) * 100);
    }
    return 0;
};
</script>

<style scoped>
.stats-chart {
    padding: 16px 0;
}

.stat-bar {
    transition: all 0.3s ease;
}

.stat-bar:hover {
    transform: translateX(4px);
}
</style>