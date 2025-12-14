<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">Projects Management</h1>

                <v-snackbar v-model="showError" color="error" timeout="3000" location="top right">
                    <v-icon start>mdi-alert-circle</v-icon>
                    {{ error }}
                </v-snackbar>

                <v-snackbar v-model="showSuccess" color="success" timeout="3000" location="top right">
                    <v-icon start>mdi-check-circle</v-icon>
                    {{ successMessage }}
                </v-snackbar>

                <div class="py-1">

                    <v-sheet border rounded>
                        <v-data-table :headers="headers"
                            :hide-default-footer="projects !== null && projects.length < 11" :items="projects ?? []">
                            <template v-slot:top>
                                <v-toolbar flat>
                                    <v-toolbar-title>
                                        <v-icon color="medium-emphasis" icon="mdi-folder-multiple" size="x-small"
                                            start></v-icon>
                                        Projects
                                    </v-toolbar-title>

                                    <v-btn class="me-2" prepend-icon="mdi-plus" rounded="lg" text="Add a Project" border
                                        @click="addNewProject"></v-btn>
                                </v-toolbar>
                            </template>

                            <template v-slot:item.name="{ value }">
                                <v-chip :text="value" border="thin opacity-25" prepend-icon="mdi-folder-multiple" label>
                                    <template v-slot:prepend>
                                        <v-icon color="medium-emphasis"></v-icon>
                                    </template>
                                </v-chip>
                            </template>

                            <template v-slot:item.startDate="{ value }">
                                {{ formatDateForDisplay(value) }}
                            </template>

                            <template v-slot:item.endDate="{ value }">
                                {{ formatDateForDisplay(value) }}
                            </template>

                            <template v-slot:item.actions="{ item }">
                                <div class="d-flex ga-2 justify-end">
                                    <v-btn size="small" color="info" variant="tonal" prepend-icon="mdi-information"
                                        @click="showProjectDetails(item.id)">
                                        Details
                                    </v-btn>

                                    <v-icon color="medium-emphasis" icon="mdi-pencil" size="small"
                                        @click="editProject(item.id)"></v-icon>

                                    <v-icon color="medium-emphasis" icon="mdi-delete" size="small"
                                        @click="showDeleteConfirmation(item.id)"></v-icon>
                                </div>
                            </template>

                            <template v-slot:no-data>
                                <v-btn prepend-icon="mdi-backup-restore" rounded="lg" text="Refresh" variant="text"
                                    border @click="refreshProjects"></v-btn>
                            </template>
                        </v-data-table>
                    </v-sheet>

                    <v-dialog v-model="newEditDialog" max-width="500">
                        <v-card :title="`${isEditing ? 'Edit' : 'Add'} a Project`">
                            <template v-slot:text>
                                <v-row>
                                    <v-col cols="12">
                                        <v-text-field v-model="formModel.name" label="Name"></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="startDateStr" label="Start Date" type="date"
                                            :error="!!dateErrorMessage"
                                            :error-messages="dateErrorMessage"></v-text-field>
                                    </v-col>
                                    <v-col cols="12" md="6">
                                        <v-text-field v-model="endDateStr" label="End Date" type="date"
                                            :error="!!dateErrorMessage"
                                            :error-messages="dateErrorMessage"></v-text-field>
                                    </v-col>

                                    <v-col cols="12" md="12">
                                        <v-select :items="companies ?? []" item-title="name" item-value="id"
                                            label="Company (optional)" v-model="selectedCompanyId" dense></v-select>
                                    </v-col>
                                </v-row>
                            </template>

                            <v-divider></v-divider>

                            <v-card-actions class="bg-surface-light">
                                <v-btn text="Cancel" variant="plain" @click="newEditDialog = false"></v-btn>

                                <v-spacer></v-spacer>

                                <v-btn text="Save" @click="save" :disabled="!isValid"></v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-dialog>

                    <v-dialog v-model="detailsDialog" max-width="600">
                        <v-card v-if="selectedProjectDetails">
                            <v-card-title class="d-flex align-center">
                                <v-icon class="me-3" color="primary">mdi-folder-multiple</v-icon>
                                Project Details
                            </v-card-title>

                            <v-card-text>
                                <v-container>
                                    <v-row>
                                        <v-col cols="12">
                                            <v-chip color="primary" variant="tonal" size="large" class="mb-4">
                                                {{ selectedProjectDetails.name }}
                                            </v-chip>
                                        </v-col>

                                        <v-col cols="12" md="6">
                                            <div class="text-subtitle-2 text-medium-emphasis mb-1">Start Date</div>
                                            <div class="text-body-1">{{
                                                formatDateForDisplay(selectedProjectDetails.startDate) }}</div>
                                        </v-col>

                                        <v-col cols="12" md="6">
                                            <div class="text-subtitle-2 text-medium-emphasis mb-1">End Date</div>
                                            <div class="text-body-1">{{
                                                formatDateForDisplay(selectedProjectDetails.endDate) }}</div>
                                        </v-col>

                                        <v-col cols="12">
                                            <v-divider class="my-2"></v-divider>
                                            <div class="text-subtitle-1 mb-3">
                                                <v-icon class="me-2" size="small">mdi-domain</v-icon>
                                                Company Information
                                            </div>
                                        </v-col>

                                        <v-col cols="12" md="6">
                                            <div class="text-subtitle-2 text-medium-emphasis mb-1">Company ID</div>
                                            <div class="text-body-1">{{ selectedProjectDetails.company.id }}</div>
                                        </v-col>

                                        <v-col cols="12" md="6">
                                            <div class="text-subtitle-2 text-medium-emphasis mb-1">Company Name</div>
                                            <div class="text-body-1">
                                                <v-chip color="success" variant="tonal">
                                                    {{ selectedProjectDetails.company.name }}
                                                </v-chip>
                                            </div>
                                        </v-col>

                                        <v-col cols="12">
                                            <v-divider class="my-2"></v-divider>
                                            <div class="text-subtitle-2 text-medium-emphasis mb-2">Project Duration
                                            </div>
                                            <div class="text-body-1">
                                                {{ calculateProjectDuration(selectedProjectDetails.startDate,
                                                    selectedProjectDetails.endDate) }} days
                                            </div>
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

                        <!-- Loading state for details -->
                        <v-card v-else-if="isLoadingDetails">
                            <v-card-text class="text-center py-8">
                                <v-progress-circular indeterminate size="48" color="primary"></v-progress-circular>
                                <div class="mt-3">Loading project details...</div>
                            </v-card-text>
                        </v-card>

                        <!-- Error state for details -->
                        <v-card v-else>
                            <v-card-text class="text-center py-8">
                                <v-icon size="48" color="error" class="mb-3">mdi-alert-circle</v-icon>
                                <div class="text-h6">Failed to load project details</div>
                                <v-btn color="primary" variant="text" class="mt-3" @click="detailsDialog = false">
                                    Close
                                </v-btn>
                            </v-card-text>
                        </v-card>
                    </v-dialog>

                    <v-dialog v-model="confirmDeleteDialog" max-width="400px">
                        <v-card>
                            <v-card-title class="text-h6">Confirm Deletion</v-card-title>
                            <v-card-text>Are you sure you want to delete this project?</v-card-text>
                            <v-text-field v-model="projectNameToDelete" label="Project Name" readonly></v-text-field>
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
import projectsService from '@/services/projectsService';
import companyService from '@/services/companyService';
import { extractErrorMessage } from '@/utils/errorHandler';
import type { Project, Company, ProjectExt } from '@/types';
import { DevelopmentLogger } from '@/utils/logger';
import { formatDate } from '@/utils/dateFormatter';
import { ref, computed } from 'vue';

const logger = new DevelopmentLogger({ prefix: '[ProjectsView]' });

const headers = [
    { title: 'Name', key: 'name', align: "start" as const },
    { title: 'Start date', key: 'startDate' },
    { title: 'End date', key: 'endDate' },
    {
        title: 'Company', key: 'companyDto', children: [
            { title: 'ID', key: 'companyDto.id' },
            { title: 'Name', key: 'companyDto.name' },
        ]
    },
    { title: 'Actions', key: 'actions', align: 'end' as const, sortable: false }
];

const {
    data: projects,
    load: refreshProjects
} = useAsyncData<Project[]>({
    fetchFunction: (signal) => projectsService.getProjects(signal),
    loggerPrefix: '[ProjectsView]'
});

const {
    data: companies,
    load: refreshCompanies
} = useAsyncData<Company[]>({
    fetchFunction: (signal) => companyService.getCompanies(signal),
    loggerPrefix: '[ProjectsView][companies]'
});

const newEditDialog = ref(false);
const confirmDeleteDialog = ref(false);
const detailsDialog = ref(false);
const formModel = ref(createNewRecord());
const projectNameToDelete = ref('');
const projectIdToDelete = ref('');
const selectedCompanyId = ref<number | null>(null);
const selectedProjectDetails = ref<ProjectExt | null>(null);
const isLoadingDetails = ref(false);
const error = ref('');
const showError = ref(false);
const successMessage = ref('');
const showSuccess = ref(false);

const startDateStr = computed({
    get: () => {
        const raw = formModel.value.startDate;
        if (!raw) return '';

        let date: Date;
        if (typeof raw === 'string') {
            date = new Date(raw);
        } else {
            date = raw as Date;
        }

        if (isNaN(date.getTime())) return '';

        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    },
    set: (val: string) => {
        if (val) {
            formModel.value.startDate = new Date(val + 'T00:00:00.000Z');
        } else {
            formModel.value.startDate = new Date();
        }
    }
});

const endDateStr = computed({
    get: () => {
        const raw = formModel.value.endDate;
        if (!raw) return '';

        let date: Date;
        if (typeof raw === 'string') {
            date = new Date(raw);
        } else {
            date = raw as Date;
        }

        if (isNaN(date.getTime())) return '';

        const year = date.getFullYear();
        const month = String(date.getMonth() + 1).padStart(2, '0');
        const day = String(date.getDate()).padStart(2, '0');
        return `${year}-${month}-${day}`;
    },
    set: (val: string) => {
        if (val) {
            formModel.value.endDate = new Date(val + 'T00:00:00.000Z');
        } else {
            formModel.value.endDate = new Date();
        }
    }
});

const isValid = computed(() => {
    if (!formModel.value.name || !formModel.value.name.trim()) return false;

    const rawS = formModel.value.startDate;
    const rawE = formModel.value.endDate;
    if (!rawS || !rawE) return false;

    let startDate: Date;
    let endDate: Date;

    if (typeof rawS === 'string') {
        startDate = new Date(rawS);
    } else {
        startDate = rawS as Date;
    }

    if (typeof rawE === 'string') {
        endDate = new Date(rawE);
    } else {
        endDate = rawE as Date;
    }

    if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) return false;
    return startDate.getTime() <= endDate.getTime();
});

const dateErrorMessage = computed(() => {
    const rawS = formModel.value.startDate;
    const rawE = formModel.value.endDate;
    if (!rawS || !rawE) return '';

    let startDate: Date;
    let endDate: Date;

    if (typeof rawS === 'string') {
        startDate = new Date(rawS);
    } else {
        startDate = rawS as Date;
    }

    if (typeof rawE === 'string') {
        endDate = new Date(rawE);
    } else {
        endDate = rawE as Date;
    }

    if (isNaN(startDate.getTime()) || isNaN(endDate.getTime())) return 'Invalid date';
    return startDate.getTime() <= endDate.getTime() ? '' : 'Start date must be before or equal to End date';
});

function createNewRecord() {
    return {
        id: '',
        name: '',
        startDate: new Date(),
        endDate: new Date(),
        companyDto: { id: 0, name: '' }
    } as Project;
}
const isEditing = computed(() => !!formModel.value.id)

function addNewProject() {
    formModel.value = createNewRecord();
    selectedCompanyId.value = null;
    newEditDialog.value = true;
}

function editProject(id: string) {
    const project = projects.value?.find(p => p.id === id);
    if (project) {
        formModel.value = {
            ...project,
            startDate: typeof project.startDate === 'string' ? new Date(project.startDate) : project.startDate,
            endDate: typeof project.endDate === 'string' ? new Date(project.endDate) : project.endDate
        };
        selectedCompanyId.value = project.companyDto?.id ?? null;
        newEditDialog.value = true;
    } else {
        logger.error(`Project with ID ${id} not found.`);
    }
}

function showDeleteConfirmation(id: string) {
    const project = projects.value?.find(p => p.id === id);
    if (project) {
        logger.log('Delete project:', project);
        projectNameToDelete.value = project.name;
        projectIdToDelete.value = project.id;
        confirmDeleteDialog.value = true;
    } else {
        logger.error(`Project with ID ${id} not found.`);
    }
}

function confirmDelete() {
    if (!projectIdToDelete.value) {
        logger.error('No project selected for deletion.');
        error.value = 'No project selected for deletion';
        showError.value = true;
        return;
    }

    projectsService.deleteProject(projectIdToDelete.value)
        .then(() => {
            logger.log(`Deleted project id=${projectIdToDelete.value}`);
            successMessage.value = 'Project deleted successfully!';
            showSuccess.value = true;
            refreshProjects();
            error.value = '';
        })
        .catch(err => {
            logger.error('Failed to delete project:', err);
            const errorDetails = extractErrorMessage(err);
            error.value = errorDetails.message;
            showError.value = true;
            error.value = (err instanceof Error ? err.message : 'Failed to delete project');
        })
        .finally(() => {
            projectNameToDelete.value = '';
            projectIdToDelete.value = '';
            confirmDeleteDialog.value = false;
        });
}

function cancelDelete() {
    confirmDeleteDialog.value = false;
    projectNameToDelete.value = '';
    projectIdToDelete.value = '';
}

function formatDateForDisplay(date: Date | string): string {
    if (!date) return '';
    return formatDate(date, { format: 'numeric', includeYear: true });
}

async function save() {
    if (!formModel.value.name) {
        logger.error('Project name is required.');
        error.value = 'Project name is required';
        return;
    }

    const payload = {
        name: formModel.value.name,
        startDate: startDateStr.value,
        endDate: endDateStr.value,
        companyId: selectedCompanyId.value ?? null
    };

    try {
        if (isEditing.value) {
            await projectsService.updateProject(formModel.value.id, payload as any);
            logger.log('Updated project', formModel.value.id);
        } else {
            const created = await projectsService.createProject(payload as any);
            logger.log('Created project', created.id);
        }

        await refreshProjects();
        newEditDialog.value = false;
        error.value = '';
    } catch (err) {
        logger.error('Failed to save project:', err);
        error.value = (err instanceof Error ? err.message : 'Failed to save project');
    }
}

async function showProjectDetails(id: string) {
    detailsDialog.value = true;
    isLoadingDetails.value = true;
    selectedProjectDetails.value = null;

    try {
        selectedProjectDetails.value = await projectsService.getProjectExtById(id);
        logger.log('Loaded extended project details:', selectedProjectDetails.value);
    } catch (err) {
        logger.error('Failed to load project details:', err);
    } finally {
        isLoadingDetails.value = false;
    }
}

function calculateProjectDuration(startDate: Date | string, endDate: Date | string): number {
    const start = typeof startDate === 'string' ? new Date(startDate) : startDate;
    const end = typeof endDate === 'string' ? new Date(endDate) : endDate;

    const diffTime = Math.abs(end.getTime() - start.getTime());
    const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

    return diffDays;
}
</script>

<style scoped></style>
