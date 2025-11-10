<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">{{ pageTitle }}</h1>

                <div class="py-1">
                    <v-sheet border rounded>
                        <v-data-table :headers="headers" :hide-default-footer="sprints !== null && sprints.length < 11"
                            :items="sprints ?? []">
                            <template v-slot:top>
                                <v-toolbar flat>
                                    <v-toolbar-title>
                                        <v-icon color="medium-emphasis" icon="mdi-rocket-launch" size="x-small"
                                            start></v-icon>
                                        {{ tableTitle }}
                                    </v-toolbar-title>

                                    <v-btn class="me-2" prepend-icon="mdi-plus" rounded="lg" text="Add a Sprint" border
                                        @click="addNewSprint"></v-btn>
                                </v-toolbar>
                            </template>

                            <template v-slot:item.name="{ value }">
                                <v-chip :text="value" border="thin opacity-25" prepend-icon="mdi-rocket-launch" label>
                                    <template v-slot:prepend>
                                        <v-icon color="medium-emphasis"></v-icon>
                                    </template>
                                </v-chip>
                            </template>

                            <template v-slot:item.team.name="{ item }">
                                <v-chip color="success" variant="tonal" size="small">
                                    <v-icon start size="small">mdi-account-group</v-icon>
                                    {{ item.team.name }}
                                </v-chip>
                            </template>

                            <template v-slot:item.project.name="{ item }">
                                <v-chip color="info" variant="tonal" size="small">
                                    <v-icon start size="small">mdi-folder</v-icon>
                                    {{ item.project.name }}
                                </v-chip>
                            </template>

                            <template v-slot:item.startDate="{ value }">
                                <v-chip color="primary" variant="tonal" size="small">
                                    {{ formatDate(value) }}
                                </v-chip>
                            </template>

                            <template v-slot:item.endDate="{ value }">
                                <v-chip color="warning" variant="tonal" size="small">
                                    {{ formatDate(value) }}
                                </v-chip>
                            </template>

                            <template v-if="isAdmin" v-slot:item.manager.username="{ item }">
                                <div class="d-flex align-center">
                                    <v-avatar size="24" color="primary" class="me-2">
                                        <span class="text-caption">{{ item.manager.firstName.charAt(0) }}{{
                                            item.manager.lastName.charAt(0) }}</span>
                                    </v-avatar>
                                    <div>
                                        <div class="text-body-2 font-weight-medium">{{ item.manager.username }}</div>
                                        <div class="text-caption text-medium-emphasis">{{ item.manager.firstName }} {{
                                            item.manager.lastName }}</div>
                                    </div>
                                </div>
                            </template>

                            <template v-slot:item.actions="{ item }">
                                <div class="d-flex ga-2 justify-end">
                                    <v-btn size="small" color="info" variant="tonal" prepend-icon="mdi-information"
                                        @click="showSprintDetails(item.id)">
                                        Details
                                    </v-btn>

                                    <v-icon color="medium-emphasis" icon="mdi-pencil" size="small"
                                        @click="editSprint(item.id)"></v-icon>

                                    <v-icon v-if="isAdmin" color="medium-emphasis" icon="mdi-delete" size="small"
                                        @click="showDeleteConfirmation(item.id)"></v-icon>
                                </div>
                            </template>

                            <template v-slot:no-data>
                                <v-btn prepend-icon="mdi-backup-restore" rounded="lg" text="Refresh" variant="text"
                                    border @click="refreshSprints"></v-btn>
                            </template>
                        </v-data-table>
                    </v-sheet>

                    <v-dialog v-model="newEditDialog" max-width="500">
                        <v-card :title="`${isEditing ? 'Edit' : 'Add'} a Sprint`">
                            <template v-slot:text>
                                <v-row>
                                    <v-col cols="12">
                                        <v-text-field v-model="formModel.name" label="Sprint Name"
                                            required></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.startDate" label="Start Date" type="date"
                                            required></v-text-field>
                                    </v-col>
                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.endDate" label="End Date" type="date"
                                            required></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.teamId" label="Team ID"
                                            required></v-text-field>
                                    </v-col>

                                    <v-col v-if="isAdmin" cols="12" md="6">
                                        <v-text-field v-model="formModel.managerId" label="Manager ID" required
                                            hint="Select which manager will oversee this sprint"></v-text-field>
                                    </v-col>

                                    <v-col v-if="isManager" cols="12" md="6">
                                        <v-text-field :model-value="currentUser?.id" label="Manager ID" readonly
                                            hint="Automatically assigned to you" persistent-hint></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="formModel.projectId" label="Project ID"
                                            required></v-text-field>
                                    </v-col>

                                    <v-col v-if="isManager && !isEditing" cols="12">
                                        <v-alert type="info" variant="tonal">
                                            <div class="text-subtitle-2 mb-1">Auto Assignment</div>
                                            <div class="text-body-2">
                                                This sprint will be automatically assigned to you as the manager.
                                            </div>
                                        </v-alert>
                                    </v-col>
                                </v-row>
                            </template>

                            <v-divider></v-divider>

                            <v-card-actions class="bg-surface-light">
                                <v-btn text="Cancel" variant="plain" @click="newEditDialog = false"></v-btn>

                                <v-spacer></v-spacer>

                                <v-btn text="Save" color="primary" @click="save"></v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-dialog>

                    <!-- Sprint Details Modal -->
                    <v-dialog v-model="detailsDialog" max-width="700">
                        <v-card v-if="selectedSprintDetails">
                            <v-card-title class="d-flex align-center">
                                <v-icon class="me-3" color="primary">mdi-rocket-launch</v-icon>
                                Sprint Details
                            </v-card-title>

                            <v-card-text>
                                <v-container>
                                    <v-row>
                                        <v-col cols="12">
                                            <div class="text-h5 mb-3 d-flex align-center">
                                                <v-icon class="me-2" color="primary">mdi-rocket</v-icon>
                                                {{ selectedSprintDetails.name }}
                                            </div>
                                        </v-col>

                                        <v-col cols="12" md="6">
                                            <v-card variant="outlined" class="pa-3 mb-3">
                                                <div class="text-subtitle-2 text-medium-emphasis mb-2">
                                                    <v-icon size="small" class="me-1">mdi-calendar-start</v-icon>
                                                    Start Date
                                                </div>
                                                <div class="text-body-1 font-weight-medium">{{
                                                    formatDate(selectedSprintDetails.startDate)
                                                }}</div>
                                            </v-card>
                                        </v-col>

                                        <v-col cols="12" md="6">
                                            <v-card variant="outlined" class="pa-3 mb-3">
                                                <div class="text-subtitle-2 text-medium-emphasis mb-2">
                                                    <v-icon size="small" class="me-1">mdi-calendar-end</v-icon>
                                                    End Date
                                                </div>
                                                <div class="text-body-1 font-weight-medium">{{
                                                    formatDate(selectedSprintDetails.endDate) }}
                                                </div>
                                            </v-card>
                                        </v-col>

                                        <v-col cols="12">
                                            <v-card variant="outlined" class="pa-3 mb-3">
                                                <div class="text-subtitle-2 text-medium-emphasis mb-2">
                                                    <v-icon size="small" class="me-1">mdi-timer-outline</v-icon>
                                                    Duration
                                                </div>
                                                <div class="text-body-1 font-weight-medium">
                                                    {{ calculateSprintDuration(selectedSprintDetails.startDate,
                                                        selectedSprintDetails.endDate) }} days
                                                </div>
                                            </v-card>
                                        </v-col>

                                        <v-col cols="12">
                                            <v-divider class="mb-3"></v-divider>
                                            <div class="text-subtitle-1 mb-3 d-flex align-center">
                                                <v-icon class="me-2" color="primary">mdi-account-tie</v-icon>
                                                Manager Information
                                            </div>
                                        </v-col>

                                        <v-col cols="12" md="6">
                                            <v-card variant="outlined" class="pa-3">
                                                <div class="text-subtitle-2 text-medium-emphasis mb-2">Username</div>
                                                <div class="text-body-1 font-weight-medium">{{
                                                    selectedSprintDetails.manager.username }}
                                                </div>
                                            </v-card>
                                        </v-col>

                                        <v-col cols="12" md="6">
                                            <v-card variant="outlined" class="pa-3">
                                                <div class="text-subtitle-2 text-medium-emphasis mb-2">Full Name</div>
                                                <div class="text-body-1 font-weight-medium">
                                                    {{ selectedSprintDetails.manager.firstName }} {{
                                                        selectedSprintDetails.manager.lastName
                                                    }}
                                                </div>
                                            </v-card>
                                        </v-col>

                                        <v-col cols="12">
                                            <v-divider class="mb-3"></v-divider>
                                            <div class="text-subtitle-1 mb-3 d-flex align-center">
                                                <v-icon class="me-2" color="primary">mdi-account-group</v-icon>
                                                Team Information
                                            </div>
                                        </v-col>

                                        <v-col cols="12">
                                            <v-card variant="outlined" class="pa-3">
                                                <div class="text-subtitle-2 text-medium-emphasis mb-2">Team Name</div>
                                                <v-chip color="success" variant="tonal" size="large">
                                                    <v-icon start>mdi-account-group</v-icon>
                                                    {{ selectedSprintDetails.team.name }}
                                                </v-chip>
                                            </v-card>
                                        </v-col>

                                        <v-col cols="12">
                                            <v-divider class="mb-3"></v-divider>
                                            <div class="text-subtitle-1 mb-3 d-flex align-center">
                                                <v-icon class="me-2" color="primary">mdi-folder-multiple</v-icon>
                                                Project Information
                                            </div>
                                        </v-col>

                                        <v-col cols="12">
                                            <v-card variant="outlined" class="pa-3 mb-3">
                                                <div class="text-subtitle-2 text-medium-emphasis mb-2">Project Name
                                                </div>
                                                <v-chip color="info" variant="tonal" size="large">
                                                    <v-icon start>mdi-folder</v-icon>
                                                    {{ selectedSprintDetails.project.name }}
                                                </v-chip>
                                            </v-card>
                                        </v-col>

                                        <v-col cols="12" md="6">
                                            <v-card variant="outlined" class="pa-3">
                                                <div class="text-subtitle-2 text-medium-emphasis mb-2">Project Start
                                                </div>
                                                <div class="text-body-1">{{
                                                    formatDate(selectedSprintDetails.project.startDate) }}</div>
                                            </v-card>
                                        </v-col>

                                        <v-col cols="12" md="6">
                                            <v-card variant="outlined" class="pa-3">
                                                <div class="text-subtitle-2 text-medium-emphasis mb-2">Project End</div>
                                                <div class="text-body-1">{{
                                                    formatDate(selectedSprintDetails.project.endDate) }}</div>
                                            </v-card>
                                        </v-col>
                                    </v-row>
                                </v-container>
                            </v-card-text>

                            <v-divider></v-divider>

                            <v-card-actions>
                                <v-spacer></v-spacer>
                                <v-btn color="primary" text="Close" @click="detailsDialog = false"></v-btn>
                            </v-card-actions>
                        </v-card>

                        <v-card v-else-if="isLoadingDetails">
                            <v-card-text class="text-center py-8">
                                <v-progress-circular indeterminate size="48" color="primary"></v-progress-circular>
                                <div class="mt-3">Loading sprint details...</div>
                            </v-card-text>
                        </v-card>

                        <v-card v-else>
                            <v-card-text class="text-center py-8">
                                <v-icon size="48" color="error" class="mb-3">mdi-alert-circle</v-icon>
                                <div class="text-h6">Failed to load sprint details</div>
                                <v-btn color="primary" variant="text" class="mt-3" @click="detailsDialog = false">
                                    Close
                                </v-btn>
                            </v-card-text>
                        </v-card>
                    </v-dialog>

                    <v-dialog v-if="isAdmin" v-model="confirmDeleteDialog" max-width="400px">
                        <v-card>
                            <v-card-title class="text-h6">Confirm Deletion</v-card-title>
                            <v-card-text>Are you sure you want to delete this sprint?</v-card-text>
                            <v-text-field v-model="sprintNameToDelete" label="Sprint Name" readonly></v-text-field>
                            <v-divider></v-divider>
                            <v-card-actions class="bg-surface-light">
                                <v-btn text="Cancel" color="success" @click="cancelDelete"></v-btn>
                                <v-spacer></v-spacer>
                                <v-btn text="Confirm" color="error" @click="confirmDelete"></v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-dialog>
                </div>
            </v-container>
        </v-main>
    </v-layout>
</template>

<script setup lang="ts">
import { useAsyncData } from '@/composables/useAsyncData';
import authService from '@/services/authService';
import sprintsService from '@/services/sprintsService';
import type { SprintExt } from '@/types';
import { DevelopmentLogger } from '@/utils/logger';
import { ref, toRef, computed } from 'vue';

const logger = new DevelopmentLogger({ prefix: '[SprintsView]' });

const currentUser = authService.getStoredUser();
const isAdmin = computed(() => currentUser?.role === 'admin');
const isManager = computed(() => currentUser?.role === 'manager');

const pageTitle = computed(() => {
    if (isAdmin.value) return 'Sprint Management';
    if (isManager.value) return 'My Sprints';
    return 'Sprints';
});

const tableTitle = computed(() => {
    if (isAdmin.value) return 'All Sprints';
    if (isManager.value) return 'My Sprints';
    return 'Sprints';
});

const headers = computed(() => {
    const baseHeaders: any[] = [
        { title: 'Sprint Name', key: 'name', align: "start" as const },
        { title: 'Team', key: 'team.name' },
        { title: 'Project', key: 'project.name' },
        { title: 'Start Date', key: 'startDate' },
        { title: 'End Date', key: 'endDate' },
    ];

    if (isAdmin.value) {
        baseHeaders.push({ title: 'Manager', key: 'manager.username' });
    }

    baseHeaders.push({ title: 'Actions', key: 'actions', align: 'start' as const, sortable: false });

    return baseHeaders;
});

const {
    data: sprints,
    load: refreshSprints
} = useAsyncData<SprintExt[]>({
    fetchFunction: (signal) => {
        if (isAdmin.value) {
            return sprintsService.getSprintsExt(signal);
        } else if (isManager.value && currentUser?.id) {
            return sprintsService.getByManagerExt(currentUser.id, signal);
        } else {
            throw new Error('User not authorized to view sprints');
        }
    },
    loggerPrefix: '[SprintsView]'
});

const newEditDialog = ref(false);
const confirmDeleteDialog = ref(false);
const detailsDialog = ref(false);
const formModel = ref<any>(createNewRecord());
const sprintNameToDelete = ref('');
const selectedSprintDetails = ref<SprintExt | null>(null);
const isLoadingDetails = ref(false);

function createNewRecord(): any {
    return {
        id: '',
        name: '',
        startDate: new Date().toISOString().split('T')[0],
        endDate: new Date().toISOString().split('T')[0],
        teamId: '',
        managerId: isManager.value ? currentUser?.id || '' : '',
        projectId: ''
    };
}

const isEditing = toRef(() => !!formModel.value.id);

function addNewSprint() {
    formModel.value = createNewRecord();
    newEditDialog.value = true;
}

function editSprint(id: string) {
    const sprint = sprints.value?.find(s => s.id === id);
    if (sprint) {
        formModel.value = {
            id: sprint.id,
            name: sprint.name,
            startDate: new Date(sprint.startDate).toISOString().split('T')[0],
            endDate: new Date(sprint.endDate).toISOString().split('T')[0],
            teamId: sprint.team.id,
            managerId: sprint.manager.id,
            projectId: sprint.project.id
        };
        newEditDialog.value = true;
    } else {
        logger.error(`Sprint with ID ${id} not found.`);
    }
}

function showDeleteConfirmation(id: string) {
    if (!isAdmin.value) {
        logger.error('Only administrators can delete sprints');
        return;
    }

    const sprint = sprints.value?.find(s => s.id === id);
    if (sprint) {
        logger.log('Delete sprint:', sprint);
        sprintNameToDelete.value = sprint.name;
        confirmDeleteDialog.value = true;
    } else {
        logger.error(`Sprint with ID ${id} not found.`);
    }
}

function confirmDelete() {
    if (!isAdmin.value) {
        logger.error('Only administrators can delete sprints');
        return;
    }

    if (sprints.value == null) {
        logger.error('Sprints data is not loaded yet.');
        return;
    }

    if (!sprintNameToDelete.value) {
        logger.error('No sprint selected for deletion.');
        return;
    }

    sprints.value = sprints.value.filter(s => s.name !== sprintNameToDelete.value);
    sprintNameToDelete.value = '';

    logger.log(`Confirmed deletion of sprint: ${sprintNameToDelete.value}`);
    confirmDeleteDialog.value = false;
}

function cancelDelete() {
    confirmDeleteDialog.value = false;
    sprintNameToDelete.value = '';
}

function save() {
    if (sprints.value === null) {
        logger.error('Sprints data is not loaded yet.');
        return;
    }

    if (!formModel.value.name) {
        logger.error('Sprint name is required.');
        return;
    }

    if (!formModel.value.startDate || !formModel.value.endDate) {
        logger.error('Start and end dates are required.');
        return;
    }

    if (!formModel.value.teamId || !formModel.value.projectId) {
        logger.error('Team ID and Project ID are required.');
        return;
    }
    if (isManager.value && currentUser?.id) {
        formModel.value.managerId = currentUser.id;
    }

    if (!formModel.value.managerId) {
        logger.error('Manager ID is required.');
        return;
    }

    const startDate = new Date(formModel.value.startDate);
    const endDate = new Date(formModel.value.endDate);

    if (endDate <= startDate) {
        logger.error('End date must be after start date.');
        return;
    }

    if (isEditing.value) {
        const index = sprints.value.findIndex(sprint => sprint.id === formModel.value.id);
        if (index !== -1) {
            sprints.value[index] = { ...formModel.value };
        }
    } else {
        formModel.value.id = `${Date.now()}`; // fake ID for demo
        sprints.value.push({ ...formModel.value });
    }

    logger.log(`Sprint ${isEditing.value ? 'updated' : 'created'}:`, formModel.value);
    newEditDialog.value = false;
}

async function showSprintDetails(id: string) {
    detailsDialog.value = true;
    isLoadingDetails.value = true;
    selectedSprintDetails.value = null;

    try {
        selectedSprintDetails.value = await sprintsService.getSprintExtById(id);
        logger.log('Loaded extended sprint details:', selectedSprintDetails.value);
    } catch (err) {
        logger.error('Failed to load sprint details:', err);
    } finally {
        isLoadingDetails.value = false;
    }
}

function formatDate(date: Date | string): string {
    if (!date) return '';

    const d = typeof date === 'string' ? new Date(date) : date;
    if (isNaN(d.getTime())) return '';

    return d.toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'short',
        day: 'numeric'
    });
}

function calculateSprintDuration(startDate: Date | string, endDate: Date | string): number {
    const start = typeof startDate === 'string' ? new Date(startDate) : startDate;
    const end = typeof endDate === 'string' ? new Date(endDate) : endDate;

    const diffTime = Math.abs(end.getTime() - start.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    return diffDays;
}
</script>

<style scoped></style>
