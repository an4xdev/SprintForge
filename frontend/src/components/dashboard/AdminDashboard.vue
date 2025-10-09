<template>
    <div>
        <h1 class="text-h4 mb-6">Administrator Panel</h1>

        <v-row class="mb-6">
            <v-col cols="12" md="3">
                <v-card class="text-center">
                    <v-card-text>
                        <v-icon size="48" color="primary" class="mb-2">mdi-domain</v-icon>
                        <div class="text-h4 font-weight-bold">{{ dashboardData?.companiesCount }}</div>
                        <div class="text-caption">Number of companies</div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" md="3">
                <v-card class="text-center">
                    <v-card-text>
                        <v-icon size="48" color="success" class="mb-2">mdi-folder-multiple</v-icon>
                        <div class="text-h4 font-weight-bold">{{ dashboardData?.projectsCount }}</div>
                        <div class="text-caption">Active projects</div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" md="3">
                <v-card class="text-center">
                    <v-card-text>
                        <v-icon size="48" color="info" class="mb-2">mdi-account-group</v-icon>
                        <div class="text-h4 font-weight-bold">{{ dashboardData?.usersCount }}</div>
                        <div class="text-caption">Users</div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" md="3">
                <v-card class="text-center">
                    <v-card-text>
                        <v-icon size="48" color="warning" class="mb-2">mdi-account-multiple</v-icon>
                        <div class="text-h4 font-weight-bold">{{ dashboardData?.teamsCount }}</div>
                        <div class="text-caption">Teams</div>
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row>

        <v-card>
            <v-card-title class="text-h5">
                Latest system activities
                <v-spacer></v-spacer>
                <v-chip v-if="totalAuditLogs > 0" size="small" color="primary">
                    {{ auditLogs.length }} / {{ totalAuditLogs }}
                </v-chip>
            </v-card-title>
            <v-card-text>
                <v-list v-if="auditLogs && auditLogs.length > 0">
                    <v-list-item v-for="log in auditLogs" :key="log.id" class="mb-1">
                        <template v-slot:prepend>
                            <div class="d-flex align-center position-relative mr-4">
                                <v-icon :color="getActionColor(log.action)" size="28">{{ getActionIcon(log.action)
                                    }}</v-icon>
                                <v-icon v-if="getStatusIcon(log.action)"
                                    :color="isSuccessAction(log.action) ? 'success' : 'error'" size="14"
                                    class="status-indicator">
                                    {{ getStatusIcon(log.action) }}
                                </v-icon>
                            </div>
                        </template>

                        <v-list-item-title>
                            <div class="d-flex align-center flex-wrap">
                                <span :class="{
                                    'text-success': isSuccessAction(log.action),
                                    'text-error': isFailedAction(log.action),
                                    'font-weight-medium': isSuccessAction(log.action) || isFailedAction(log.action)
                                }" class="flex-grow-1 text-truncate mr-2">
                                    {{ log.description }}
                                </span>
                                <v-chip size="x-small" variant="outlined" :color="getActionColor(log.action)"
                                    class="flex-shrink-0">
                                    {{ log.entity }}
                                </v-chip>
                            </div>
                        </v-list-item-title>

                        <v-list-item-subtitle>
                            <div class="d-flex align-center flex-wrap">
                                <div class="d-flex align-center mr-4">
                                    <v-icon size="16" class="mr-1">mdi-clock-outline</v-icon>
                                    <span>{{ formatTimestamp(log.timestamp) }}</span>
                                </div>
                                <div class="d-flex align-center">
                                    <v-icon size="16" class="mr-1">mdi-server</v-icon>
                                    <span class="font-weight-medium">{{ log.service }}</span>
                                </div>
                            </div>
                        </v-list-item-subtitle>
                    </v-list-item>
                </v-list>
                <div v-else class="text-center pa-4">
                    <v-progress-circular v-if="loadingAuditLogs" indeterminate></v-progress-circular>
                    <div v-else class="text-body-2 text-disabled">No recent activities</div>
                </div>

                <div v-if="hasMoreAuditLogs" class="text-center mt-4">
                    <v-btn @click="loadMoreAuditLogs" :loading="loadingMoreAuditLogs" variant="outlined" color="primary"
                        prepend-icon="mdi-reload">
                        Load More Activities
                    </v-btn>
                </div>
            </v-card-text>
        </v-card>
    </div>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import adminDashboardService from '@/services/adminDashboardService';
import reportsService from '@/services/reportsService';
import type { AdminDashboard, AuditLog } from '@/types';

const dashboardData = ref<AdminDashboard | null>(null);
const auditLogs = ref<AuditLog[]>([]);
const loadingAuditLogs = ref(false);
const loadingMoreAuditLogs = ref(false);
const totalAuditLogs = ref(0);
const currentOffset = ref(0);
const logsPerPage = ref(10);

const hasMoreAuditLogs = computed(() => {
    return auditLogs.value.length < totalAuditLogs.value;
});

const getActionColor = (action: string) => {
    const actionUpper = action.toUpperCase();

    if (actionUpper.includes('FAILED')) return 'error';
    if (actionUpper.includes('SUCCESS')) return 'success';
    switch (true) {
        case actionUpper.includes('CREATE'): return 'success';
        case actionUpper.includes('UPDATE'): return 'warning';
        case actionUpper.includes('DELETE'): return 'error';
        case actionUpper.includes('LOGIN'): return 'info';
        case actionUpper.includes('CHANGE_PASSWORD'): return 'warning';
        default: return 'primary';
    }
};

const getActionIcon = (action: string) => {
    const actionUpper = action.toUpperCase();

    switch (true) {
        case actionUpper.includes('CREATE'): return 'mdi-plus-circle';
        case actionUpper.includes('UPDATE'): return 'mdi-pencil';
        case actionUpper.includes('DELETE'): return 'mdi-delete';
        case actionUpper.includes('LOGIN'): return 'mdi-login';
        case actionUpper.includes('CHANGE_PASSWORD'): return 'mdi-key-change';
        default: return 'mdi-information';
    }
};

const getStatusIcon = (action: string) => {
    const actionUpper = action.toUpperCase();
    if (actionUpper.includes('FAILED')) return 'mdi-close-circle';
    if (actionUpper.includes('SUCCESS')) return 'mdi-check-circle';
    return null;
};

const isSuccessAction = (action: string) => {
    return action.toUpperCase().includes('SUCCESS');
};

const isFailedAction = (action: string) => {
    return action.toUpperCase().includes('FAILED');
};

const formatTimestamp = (timestamp: string) => {
    const date = new Date(timestamp);
    const now = new Date();
    const diffMs = now.getTime() - date.getTime();
    const diffHours = Math.floor(diffMs / (1000 * 60 * 60));
    const diffMinutes = Math.floor(diffMs / (1000 * 60));

    if (diffHours > 24) {
        return date.toLocaleDateString();
    } else if (diffHours > 0) {
        return `${diffHours}h ago`;
    } else if (diffMinutes > 0) {
        return `${diffMinutes}m ago`;
    } else {
        return 'Just now';
    }
};

const fetchAuditLogs = async () => {
    try {
        loadingAuditLogs.value = true;
        const response = await reportsService.getAuditLogs(logsPerPage.value, 0);
        auditLogs.value = response.data.logs;
        totalAuditLogs.value = response.data.totalCount;
        currentOffset.value = logsPerPage.value;
    } catch (error) {
        console.error('Error fetching audit logs:', error);
    } finally {
        loadingAuditLogs.value = false;
    }
};

const loadMoreAuditLogs = async () => {
    try {
        loadingMoreAuditLogs.value = true;
        const response = await reportsService.getAuditLogs(logsPerPage.value, currentOffset.value);
        auditLogs.value = [...auditLogs.value, ...response.data.logs];
        currentOffset.value += logsPerPage.value;
    } catch (error) {
        console.error('Error loading more audit logs:', error);
    } finally {
        loadingMoreAuditLogs.value = false;
    }
};

onMounted(async () => {
    try {
        await Promise.all([
            adminDashboardService.getDashboardInfo().then(data => dashboardData.value = data),
            fetchAuditLogs()
        ]);
    } catch (error) {
        console.error('Error fetching dashboard data:', error);
    }
});
</script>

<style scoped>
.status-indicator {
    position: absolute;
    top: -4px;
    right: -4px;
    background: white;
    border-radius: 50%;
    padding: 1px;
}

.v-list-item {
    border-left: 3px solid transparent;
    overflow: hidden;
}

.v-list-item:has(.text-success) {
    border-left-color: rgb(var(--v-theme-success));
    background: rgba(var(--v-theme-success), 0.05);
}

.v-list-item:has(.text-error) {
    border-left-color: rgb(var(--v-theme-error));
    background: rgba(var(--v-theme-error), 0.05);
}

.text-truncate {
    overflow: hidden;
    text-overflow: ellipsis;
    white-space: nowrap;
}

.flex-grow-1 {
    flex-grow: 1;
    min-width: 0;
}

.flex-shrink-0 {
    flex-shrink: 0;
}
</style>
