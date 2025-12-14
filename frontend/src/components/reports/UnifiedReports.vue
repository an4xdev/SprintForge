<template>
    <div class="unified-reports-page">
        <v-container fluid>
            <!-- Header -->
            <v-row class="mb-6">
                <v-col cols="12">
                    <div class="d-flex align-center justify-space-between">
                        <div>
                            <h1 class="text-h4 font-weight-bold mb-2">
                                <v-icon class="me-3" size="large">mdi-chart-box-outline</v-icon>
                                {{ isAdmin ? 'Admin Reports' : 'Manager Reports' }}
                            </h1>
                            <p class="text-subtitle-1 text-medium-emphasis">
                                {{ isAdmin ? 'Browse reports' : 'Browse team and sprint reports' }}
                            </p>
                        </div>
                        <v-btn color="primary" variant="elevated" prepend-icon="mdi-refresh" :loading="isLoading"
                            @click="refreshCurrentTab">
                            Refresh
                        </v-btn>
                    </div>
                </v-col>
            </v-row>

            <!-- Date Range Picker -->
            <v-row class="mb-6">
                <v-col cols="12">
                    <DateRangePicker v-model="dateFilters" @apply="handleDateRangeApply" />
                </v-col>
            </v-row>

            <!-- Loading State -->
            <v-row v-if="isLoading" class="justify-center">
                <v-col cols="12" class="text-center">
                    <v-progress-circular indeterminate size="64" color="primary" />
                    <p class="mt-4 text-h6">Loading reports ...</p>
                </v-col>
            </v-row>

            <!-- Error State -->
            <v-row v-else-if="error" class="justify-center">
                <v-col cols="12" md="8">
                    <v-alert type="error" variant="tonal" prominent :text="error">
                        <template #prepend>
                            <v-icon>mdi-alert-circle</v-icon>
                        </template>
                        <template #append>
                            <v-btn variant="text" @click="refreshCurrentTab">
                                Try Again
                            </v-btn>
                        </template>
                    </v-alert>
                </v-col>
            </v-row>

            <v-snackbar v-model="showError" color="error" timeout="3000" location="top right">
                <v-icon start>mdi-alert-circle</v-icon>
                {{ errorMessage }}
            </v-snackbar>

            <v-snackbar v-model="showSuccess" color="success" timeout="3000" location="top right">
                <v-icon start>mdi-check-circle</v-icon>
                {{ successMessage }}
            </v-snackbar>

            <!-- Reports Tabs -->
            <template v-if="!isLoading">
                <v-card elevation="3">
                    <v-tabs v-model="activeTab" bg-color="primary" align-tabs="center">
                        <v-tab value="teams">
                            <v-icon class="me-2">mdi-account-group</v-icon>
                            Team Reports
                        </v-tab>
                        <v-tab value="sprints">
                            <v-icon class="me-2">mdi-calendar-range</v-icon>
                            Sprint Reports
                        </v-tab>
                        <v-tab value="projects">
                            <v-icon class="me-2">mdi-folder-multiple</v-icon>
                            Project Reports
                        </v-tab>
                    </v-tabs>

                    <v-card-text class="pa-0">
                        <v-tabs-window v-model="activeTab">
                            <!-- Teams Tab -->
                            <v-tabs-window-item value="teams">
                                <div class="tab-content">
                                    <div class="d-flex justify-space-between align-center pa-4 border-b">
                                        <h3 class="text-h6">Team Reports</h3>
                                    </div>

                                    <div class="pa-4">
                                        <template v-if="teamReports && teamReports.length > 0">
                                            <!-- Charts Section -->
                                            <v-row class="mb-6">
                                                <v-col cols="12" md="6">
                                                    <TeamProgressChart :teamData="teamReports[0]" />
                                                </v-col>
                                                <v-col cols="12" md="6">
                                                    <TeamStatsChart :teamData="teamReports[0]" />
                                                </v-col>
                                            </v-row>

                                            <!-- Data Table -->
                                            <v-data-table :headers="teamTableHeaders" :items="teamReportsWithIndex"
                                                item-key="index" class="elevation-0" :items-per-page="10">
                                                <template #item.teamName="{ item }">
                                                    <div class="d-flex align-center">
                                                        <v-icon class="me-3" color="primary">mdi-account-group</v-icon>
                                                        <div>
                                                            <div class="font-weight-medium">Team {{ item.index + 1 }}
                                                            </div>
                                                            <div class="text-caption text-medium-emphasis">{{
                                                                item.developerCount }} developers</div>
                                                        </div>
                                                    </div>
                                                </template>

                                                <template #item.progress="{ item }">
                                                    <div class="d-flex align-center">
                                                        <v-progress-linear
                                                            :model-value="(item.taskCountCompleted / item.taskCount) * 100"
                                                            :color="getProgressColor(item.taskCountCompleted / item.taskCount)"
                                                            height="8" rounded class="me-3" style="min-width: 100px;" />
                                                        <span class="text-body-2 font-weight-medium">
                                                            {{ Math.round((item.taskCountCompleted / item.taskCount) *
                                                                100) }}%
                                                        </span>
                                                    </div>
                                                </template>

                                                <template #item.totalTaskTime="{ item }">
                                                    <v-chip size="small" color="info" variant="tonal">
                                                        {{ formatDuration(item.totalTaskTime) }}
                                                    </v-chip>
                                                </template>

                                                <template #item.taskRatio="{ item }">
                                                    <span class="font-weight-medium">
                                                        {{ item.taskCountCompleted }}/{{ item.taskCount }}
                                                    </span>
                                                </template>

                                                <template #item.sprintsNames="{ item }">
                                                    <div v-if="item.sprintsNames && item.sprintsNames.length > 0">
                                                        <v-chip-group>
                                                            <v-chip v-for="sprint in item.sprintsNames.slice(0, 2)"
                                                                :key="sprint" size="small" variant="outlined">
                                                                {{ sprint }}
                                                            </v-chip>
                                                            <v-chip v-if="item.sprintsNames.length > 2" size="small"
                                                                variant="text" color="primary">
                                                                +{{ item.sprintsNames.length - 2 }} more
                                                            </v-chip>
                                                        </v-chip-group>
                                                    </div>
                                                    <span v-else class="text-medium-emphasis">No sprints</span>
                                                </template>
                                            </v-data-table>
                                        </template>

                                        <div v-else class="text-center py-8">
                                            <v-icon size="64"
                                                class="mb-4 text-medium-emphasis">mdi-account-group-outline</v-icon>
                                            <p class="text-h6 text-medium-emphasis">No data available for teams</p>
                                        </div>
                                    </div>
                                </div>
                            </v-tabs-window-item>

                            <!-- Sprints Tab -->
                            <v-tabs-window-item value="sprints">
                                <div class="tab-content">
                                    <div class="d-flex justify-space-between align-center pa-4 border-b">
                                        <h3 class="text-h6">Sprint Reports</h3>
                                    </div>

                                    <div class="pa-4">
                                        <template v-if="sprintReports && sprintReports.length > 0">
                                            <!-- Charts Section -->
                                            <SprintComparisonChart :sprintData="sprintReports" class="mb-6" />

                                            <!-- Data Table -->
                                            <v-data-table :headers="sprintTableHeaders" :items="sprintReports"
                                                item-key="sprintId" class="elevation-0" :items-per-page="10">
                                                <template #item.completedRatio="{ item }">
                                                    <div class="d-flex align-center">
                                                        <v-progress-linear :model-value="item.completedRatio * 100"
                                                            :color="getProgressColor(item.completedRatio)" height="8"
                                                            rounded class="me-3" style="min-width: 100px;" />
                                                        <span class="text-body-2 font-weight-medium">
                                                            {{ Math.round(item.completedRatio * 100) }}%
                                                        </span>
                                                    </div>
                                                </template>

                                                <template #item.totalTaskTime="{ item }">
                                                    <v-chip size="small" color="info" variant="tonal">
                                                        {{ formatDuration(item.totalTaskTime) }}
                                                    </v-chip>
                                                </template>

                                                <template #item.taskRatio="{ item }">
                                                    <span class="font-weight-medium">
                                                        {{ item.taskCountCompleted }}/{{ item.taskCount }}
                                                    </span>
                                                </template>
                                            </v-data-table>
                                        </template>

                                        <div v-else class="text-center py-8">
                                            <v-icon size="64"
                                                class="mb-4 text-medium-emphasis">mdi-calendar-blank</v-icon>
                                            <p class="text-h6 text-medium-emphasis">No data available for sprints</p>
                                        </div>
                                    </div>
                                </div>
                            </v-tabs-window-item>

                            <!-- Projects Tab -->
                            <v-tabs-window-item value="projects">
                                <div class="tab-content">
                                    <div class="d-flex justify-space-between align-center pa-4 border-b">
                                        <h3 class="text-h6">Project Reports</h3>
                                    </div>

                                    <div class="pa-4">
                                        <template v-if="projectReports && projectReports.length > 0">
                                            <!-- Charts Section -->
                                            <ProjectStatsChart :projectData="projectReports" class="mb-6" />

                                            <!-- Data Table -->
                                            <v-data-table :headers="projectTableHeaders" :items="projectReports"
                                                item-key="projectId" class="elevation-0" :items-per-page="10">
                                                <template #item.projectName="{ item }">
                                                    <div>
                                                        <div class="font-weight-medium">{{ item.projectName }}</div>
                                                        <div class="text-caption text-medium-emphasis">{{
                                                            item.companyName
                                                            }}</div>
                                                    </div>
                                                </template>

                                                <template #item.completedRatio="{ item }">
                                                    <div class="d-flex align-center">
                                                        <v-progress-linear :model-value="item.completedRatio * 100"
                                                            :color="getProgressColor(item.completedRatio)" height="8"
                                                            rounded class="me-3" style="min-width: 100px;" />
                                                        <span class="text-body-2 font-weight-medium">
                                                            {{ Math.round(item.completedRatio * 100) }}%
                                                        </span>
                                                    </div>
                                                </template>

                                                <template #item.totalTaskTime="{ item }">
                                                    <v-chip size="small" color="success" variant="tonal">
                                                        {{ formatDuration(item.totalTaskTime) }}
                                                    </v-chip>
                                                </template>

                                                <template #item.taskRatio="{ item }">
                                                    <span class="font-weight-medium">
                                                        {{ item.taskCountCompleted }}/{{ item.taskCount }}
                                                    </span>
                                                </template>
                                            </v-data-table>
                                        </template>

                                        <div v-else class="text-center py-8">
                                            <v-icon size="64"
                                                class="mb-4 text-medium-emphasis">mdi-folder-open-outline</v-icon>
                                            <p class="text-h6 text-medium-emphasis">No data available for projects</p>
                                        </div>
                                    </div>
                                </div>
                            </v-tabs-window-item>
                        </v-tabs-window>
                    </v-card-text>
                </v-card>
            </template>
        </v-container>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import { useAuthStore } from '@/stores/auth';
import reportsService from '@/services/reportsService';
import DateRangePicker from './DateRangePicker.vue';
import TeamProgressChart from './TeamProgressChart.vue';
import TeamStatsChart from './TeamStatsChart.vue';
import SprintComparisonChart from './SprintComparisonChart.vue';
import ProjectStatsChart from './ProjectStatsChart.vue';
import type {
    TeamReportDto,
    SprintReportDto,
    ProjectReportDto,
    ReportFilters
} from '@/types';
import { DevelopmentLogger } from '@/utils/logger';

const logger = new DevelopmentLogger({ prefix: '[UnifiedReports]' });

const authStore = useAuthStore();

const isLoading = ref(false);
const error = ref<string | null>(null);
const errorMessage = ref('');
const showError = ref(false);
const successMessage = ref('');
const showSuccess = ref(false);
const activeTab = ref('teams');
const dateFilters = ref<ReportFilters>({});

const teamReports = ref<TeamReportDto[]>([]);
const sprintReports = ref<SprintReportDto[]>([]);
const projectReports = ref<ProjectReportDto[]>([]);

const isAdmin = computed(() => authStore.hasRole('admin'));
const managerId = computed(() => isAdmin.value ? null : authStore.user?.id || null);

const teamReportsWithIndex = computed(() => {
    return teamReports.value.map((team, index) => ({
        ...team,
        index
    }));
});

const teamTableHeaders = [
    { title: 'Team', key: 'teamName', sortable: false },
    { title: 'Tasks', key: 'taskRatio', sortable: false },
    { title: 'Progress', key: 'progress', sortable: true },
    { title: 'Time', key: 'totalTaskTime', sortable: true },
    { title: 'Sprints', key: 'sprintsNames', sortable: false }
];

const sprintTableHeaders = [
    { title: 'Sprint Name', key: 'sprintName', sortable: true },
    { title: 'Tasks', key: 'taskRatio', sortable: false },
    { title: 'Progress', key: 'completedRatio', sortable: true },
    { title: 'Time', key: 'totalTaskTime', sortable: true }
];

const projectTableHeaders = [
    { title: 'Project', key: 'projectName', sortable: true },
    { title: 'Tasks', key: 'taskRatio', sortable: false },
    { title: 'Progress', key: 'completedRatio', sortable: true },
    { title: 'Time', key: 'totalTaskTime', sortable: true }
];

const loadTeamReports = async () => {
    try {
        teamReports.value = await reportsService.getTeamReports(managerId.value, dateFilters.value);
    } catch (err) {
        logger.error('Failed to load team reports:', err);
        throw err;
    }
};

const loadSprintReports = async () => {
    try {
        sprintReports.value = await reportsService.getSprintReports(managerId.value, dateFilters.value);
    } catch (err) {
        logger.error('Failed to load sprint reports:', err);
        throw err;
    }
};

const loadProjectReports = async () => {
    try {
        projectReports.value = await reportsService.getProjectReports(managerId.value, dateFilters.value);
    } catch (err) {
        logger.error('Failed to load project reports:', err);
        throw err;
    }
};

const loadCurrentTabData = async () => {
    isLoading.value = true;
    error.value = null;

    try {
        switch (activeTab.value) {
            case 'teams':
                await loadTeamReports();
                break;
            case 'sprints':
                await loadSprintReports();
                break;
            case 'projects':
                await loadProjectReports();
                break;
        }
    } catch (err) {
        logger.error('Failed to load reports:', err);
        error.value = 'Failed to load reports. Please try again.';
    } finally {
        isLoading.value = false;
    }
};

const refreshCurrentTab = () => {
    loadCurrentTabData();
};

const handleDateRangeApply = (filters: ReportFilters) => {
    dateFilters.value = filters;
    loadCurrentTabData();
};

const formatDuration = (timeString: string): string => {
    return reportsService.formatDuration(timeString);
};

const getProgressColor = (ratio: number): string => {
    if (ratio >= 0.8) return 'success';
    if (ratio >= 0.5) return 'warning';
    return 'error';
};

watch(activeTab, () => {
    loadCurrentTabData();
});

onMounted(() => {
    if (authStore.isAuthenticated && (authStore.hasRole('admin') || authStore.hasRole('manager'))) {
        loadCurrentTabData();
    } else {
        error.value = 'You do not have permission to view reports.';
    }
});
</script>

<style scoped>
.unified-reports-page {
    padding: 20px;
    min-height: 100vh;
}

.tab-content {
    min-height: 400px;
}

.border-b {
    border-bottom: 1px solid rgba(var(--v-border-color), var(--v-border-opacity));
}
</style>