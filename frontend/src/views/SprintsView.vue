<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">{{ pageTitle }}</h1>

                <v-snackbar v-model="showError" color="error" timeout="3000" location="top right">
                    <v-icon start>mdi-alert-circle</v-icon>
                    {{ error }}
                </v-snackbar>

                <v-snackbar v-model="showSuccess" color="success" timeout="3000" location="top right">
                    <v-icon start>mdi-check-circle</v-icon>
                    {{ successMessage }}
                </v-snackbar>

                <div class="py-1\">
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
                                        <v-select v-model="formModel.teamId" :items="teams" item-title="name"
                                            item-value="id" label="Team" prepend-inner-icon="mdi-account-group"
                                            :loading="isLoadingOptions" required variant="outlined"
                                            density="comfortable" :disabled="isManager"
                                            :hint="isManager ? 'Automatically assigned to your team' : ''"
                                            :persistent-hint="isManager">
                                            <template v-slot:item="{ props, item }">
                                                <v-list-item v-bind="props">
                                                    <template v-slot:prepend>
                                                        <v-icon color="success">mdi-account-group</v-icon>
                                                    </template>
                                                </v-list-item>
                                            </template>
                                        </v-select>
                                    </v-col>

                                    <v-col v-if="isAdmin" cols="12" md="6">
                                        <v-select v-model="formModel.managerId" :items="managers" item-title="username"
                                            item-value="id" label="Manager" prepend-inner-icon="mdi-account-tie"
                                            :loading="isLoadingOptions" required variant="outlined"
                                            density="comfortable" hint="Select which manager will oversee this sprint"
                                            persistent-hint>
                                            <template v-slot:item="{ props, item }">
                                                <v-list-item v-bind="props">
                                                    <template v-slot:prepend>
                                                        <v-avatar color="primary" size="32">
                                                            <span class="text-caption">{{ item.raw.firstName?.charAt(0)
                                                                }}{{ item.raw.lastName?.charAt(0) }}</span>
                                                        </v-avatar>
                                                    </template>
                                                    <template v-slot:subtitle>
                                                        {{ item.raw.firstName }} {{ item.raw.lastName }}
                                                    </template>
                                                </v-list-item>
                                            </template>
                                        </v-select>
                                    </v-col>

                                    <v-col v-if="isManager" cols="12" md="6">
                                        <v-select :model-value="currentUser?.id" :items="[currentUser]"
                                            item-title="username" item-value="id" label="Manager"
                                            prepend-inner-icon="mdi-account-tie" disabled variant="outlined"
                                            density="comfortable" hint="Automatically assigned to you" persistent-hint>
                                            <template v-slot:selection="{ item }">
                                                <div class="d-flex align-center">
                                                    <v-avatar color="primary" size="24" class="me-2">
                                                        <span class="text-caption">{{ currentUser?.firstName?.charAt(0)
                                                            }}{{ currentUser?.lastName?.charAt(0) }}</span>
                                                    </v-avatar>
                                                    <span>{{ currentUser?.username }}</span>
                                                </div>
                                            </template>
                                        </v-select>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-select v-model="formModel.projectId" :items="projects" item-title="name"
                                            item-value="id" label="Project" prepend-inner-icon="mdi-folder"
                                            :loading="isLoadingOptions" required variant="outlined"
                                            density="comfortable" :disabled="isManager"
                                            :hint="isManager ? 'Automatically assigned to your project' : ''"
                                            :persistent-hint="isManager">
                                            <template v-slot:item="{ props, item }">
                                                <v-list-item v-bind="props">
                                                    <template v-slot:prepend>
                                                        <v-icon color="info">mdi-folder</v-icon>
                                                    </template>
                                                    <template v-slot:subtitle>
                                                        {{ formatDate(item.raw.startDate) }} - {{
                                                            formatDate(item.raw.endDate) }}
                                                    </template>
                                                </v-list-item>
                                            </template>
                                        </v-select>
                                    </v-col>

                                    <v-col v-if="isManager && !isEditing" cols="12">
                                        <v-alert type="info" variant="tonal">
                                            <div class="text-subtitle-2 mb-1">Auto Assignment</div>
                                            <div class="text-body-2">
                                                This sprint will be automatically assigned to you as the manager, your
                                                team, and your project.
                                            </div>
                                        </v-alert>
                                    </v-col>
                                </v-row>
                            </template>

                            <v-divider></v-divider>

                            <v-card-actions class="bg-surface-light">
                                <v-btn text="Cancel" variant="plain" @click="newEditDialog = false"
                                    :disabled="isSaving"></v-btn>

                                <v-spacer></v-spacer>

                                <v-btn text="Save" color="primary" @click="save" :loading="isSaving"
                                    :disabled="isSaving"></v-btn>
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
                                <v-btn text="Confirm" color="error" @click="confirmDelete"
                                    :loading="isDeleting"></v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-dialog>

                    <v-snackbar v-model="showSnackbar" :color="snackbarColor" :timeout="3000">
                        {{ snackbarMessage }}
                    </v-snackbar>
                </div>
            </v-container>
        </v-main>
    </v-layout>
</template>

<script setup lang="ts">
import { useAsyncData } from '@/composables/useAsyncData';
import authService from '@/services/authService';
import sprintsService from '@/services/sprintsService';
import teamsService from '@/services/teamsService';
import usersService from '@/services/usersService';
import projectsService from '@/services/projectsService';
import type { SprintExt, Team, User, Project } from '@/types';
import { DevelopmentLogger } from '@/utils/logger';
import { ref, toRef, computed, onMounted } from 'vue';

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
    error: asyncError,
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

// Dane dla list wyboru (muszą być zadeklarowane przed createNewRecord)
const teams = ref<Team[]>([]);
const managers = ref<User[]>([]);
const projects = ref<Project[]>([]);
const managerProjectId = ref<string>('');
const managerTeamId = ref<string>('');
const isLoadingOptions = ref(false);

function createNewRecord(): any {
    return {
        id: '',
        name: '',
        startDate: new Date().toISOString().split('T')[0],
        endDate: new Date().toISOString().split('T')[0],
        teamId: isManager.value ? managerTeamId.value : '',
        managerId: isManager.value ? currentUser?.id || '' : '',
        projectId: isManager.value ? managerProjectId.value : ''
    };
}

const newEditDialog = ref(false);
const confirmDeleteDialog = ref(false);
const detailsDialog = ref(false);
const formModel = ref<any>(createNewRecord());
const sprintNameToDelete = ref('');
const sprintIdToDelete = ref('');
const selectedSprintDetails = ref<SprintExt | null>(null);
const isLoadingDetails = ref(false);
const isSaving = ref(false);
const isDeleting = ref(false);
const showSnackbar = ref(false);
const snackbarMessage = ref('');
const error = ref('');
const snackbarColor = ref<'success' | 'error' | 'info'>('success');
const showError = ref(false);
const successMessage = ref('');
const showSuccess = ref(false);

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
        showNotification('Only administrators can delete sprints', 'error');
        return;
    }

    const sprint = sprints.value?.find(s => s.id === id);
    if (sprint) {
        logger.log('Delete sprint:', sprint);
        sprintNameToDelete.value = sprint.name;
        sprintIdToDelete.value = sprint.id;
        confirmDeleteDialog.value = true;
    } else {
        logger.error(`Sprint with ID ${id} not found.`);
        showNotification('Sprint not found', 'error');
    }
}

async function confirmDelete() {
    if (!isAdmin.value) {
        logger.error('Only administrators can delete sprints');
        showNotification('Only administrators can delete sprints', 'error');
        return;
    }

    if (!sprintIdToDelete.value) {
        logger.error('No sprint selected for deletion.');
        showNotification('No sprint selected for deletion', 'error');
        return;
    }

    isDeleting.value = true;
    try {
        await sprintsService.deleteSprint(sprintIdToDelete.value);
        logger.log(`Successfully deleted sprint: ${sprintNameToDelete.value}`);
        showNotification(`Sprint "${sprintNameToDelete.value}" deleted successfully`, 'success');

        await refreshSprints();

        confirmDeleteDialog.value = false;
        sprintNameToDelete.value = '';
        sprintIdToDelete.value = '';
    } catch (error) {
        logger.error('Failed to delete sprint:', error);
        showNotification('Failed to delete sprint. Please try again.', 'error');
    } finally {
        isDeleting.value = false;
    }
}

function cancelDelete() {
    confirmDeleteDialog.value = false;
    sprintNameToDelete.value = '';
}

async function save() {
    if (!formModel.value.name) {
        showNotification('Sprint name is required', 'error');
        return;
    }

    if (!formModel.value.startDate || !formModel.value.endDate) {
        showNotification('Start and end dates are required', 'error');
        return;
    }

    if (!formModel.value.teamId || !formModel.value.projectId) {
        showNotification('Team ID and Project ID are required', 'error');
        return;
    }

    if (isManager.value && currentUser?.id) {
        formModel.value.managerId = currentUser.id;
    }

    if (!formModel.value.managerId) {
        showNotification('Manager ID is required', 'error');
        return;
    }

    const startDate = new Date(formModel.value.startDate);
    const endDate = new Date(formModel.value.endDate);

    if (endDate <= startDate) {
        showNotification('End date must be after start date', 'error');
        return;
    }

    isSaving.value = true;
    try {
        const sprintData = {
            name: formModel.value.name,
            startDate: startDate,
            endDate: endDate,
            teamId: formModel.value.teamId,
            managerId: formModel.value.managerId,
            projectId: formModel.value.projectId
        };

        if (isEditing.value) {
            await sprintsService.updateSprint(formModel.value.id, sprintData);
            logger.log('Sprint updated successfully:', formModel.value);
            showNotification('Sprint updated successfully', 'success');
        } else {
            await sprintsService.createSprint(sprintData);
            logger.log('Sprint created successfully:', formModel.value);
            showNotification('Sprint created successfully', 'success');
        }

        // Odśwież listę sprintów
        await refreshSprints();

        newEditDialog.value = false;
    } catch (error) {
        logger.error(`Failed to ${isEditing.value ? 'update' : 'create'} sprint:`, error);
        showNotification(
            `Failed to ${isEditing.value ? 'update' : 'create'} sprint. Please try again.`,
            'error'
        );
    } finally {
        isSaving.value = false;
    }
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

function showNotification(message: string, color: 'success' | 'error' | 'info' = 'info') {
    snackbarMessage.value = message;
    snackbarColor.value = color;
    showSnackbar.value = true;
}

async function loadFormOptions() {
    isLoadingOptions.value = true;
    try {
        if (isAdmin.value) {
            const [teamsData, managersData, projectsData] = await Promise.all([
                teamsService.getTeams(),
                usersService.getUsersByRole('manager'),
                projectsService.getProjects()
            ]);

            teams.value = teamsData;
            managers.value = managersData;
            projects.value = projectsData;
            logger.log('Loaded form options:', { teams: teamsData.length, managers: managersData.length, projects: projectsData.length });
        } else if (isManager.value && currentUser?.id) {
            const [teamData, projectIdData] = await Promise.all([
                teamsService.getTeamByManager(currentUser.id),
                projectsService.getCurrentProjectByManagerId(currentUser.id)
            ]);

            managerTeamId.value = teamData?.id || '';
            managerProjectId.value = projectIdData || '';

            if (teamData) {
                teams.value = [teamData];
            }

            if (managerProjectId.value) {
                const project = await projectsService.getProjectById(managerProjectId.value);
                projects.value = [project];
            }

            logger.log('Loaded form options:', { managerTeamId: managerTeamId.value, managerProjectId: managerProjectId.value });
        }
    } catch (err) {
        logger.error('Failed to load form options:', err);
        showNotification('Failed to load form data. Please refresh the page.', 'error');
    } finally {
        isLoadingOptions.value = false;
    }
}

onMounted(() => {
    loadFormOptions();
});
</script>

<style scoped></style>
