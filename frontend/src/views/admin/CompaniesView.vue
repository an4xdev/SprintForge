<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">Company Management</h1>

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
                            :hide-default-footer="companies !== null && companies.length < 11" :items="companies ?? []">
                            <template v-slot:top>
                                <v-toolbar flat>
                                    <v-toolbar-title>
                                        <v-icon color="medium-emphasis" icon="mdi-domain" size="x-small" start></v-icon>
                                        Companies
                                    </v-toolbar-title>

                                    <v-btn class="me-2" prepend-icon="mdi-plus" rounded="lg" text="Add a Company" border
                                        @click="addNewCompany"></v-btn>
                                </v-toolbar>
                            </template>

                            <template v-slot:item.name="{ value }">
                                <v-chip :text="value" border="thin opacity-25" prepend-icon="mdi-domain" label>
                                    <template v-slot:prepend>
                                        <v-icon color="medium-emphasis"></v-icon>
                                    </template>
                                </v-chip>
                            </template>

                            <template v-slot:item.actions="{ item }">
                                <div class="d-flex ga-2 justify-end">
                                    <v-icon color="medium-emphasis" icon="mdi-pencil" size="small"
                                        @click="editCompany(item.id)"></v-icon>

                                    <v-icon color="medium-emphasis" icon="mdi-delete" size="small"
                                        @click="showDeleteConfirmation(item.id)"></v-icon>
                                </div>
                            </template>

                            <template v-slot:no-data>
                                <v-btn prepend-icon="mdi-backup-restore" rounded="lg" text="Refresh" variant="text"
                                    border @click="refreshCompanies"></v-btn>
                            </template>
                        </v-data-table>
                    </v-sheet>

                    <v-dialog v-model="newEditDialog" max-width="500px">
                        <v-card :title="isEditing ? 'Edit Company' : 'Create New Company'">
                            <template v-slot:text>
                                <v-row>
                                    <v-col cols="12">
                                        <v-text-field v-model="formModel.name" label="Company Name"
                                            required></v-text-field>
                                    </v-col>
                                </v-row>
                            </template>
                            <v-divider></v-divider>
                            <v-card-actions class="bg-surface-light">
                                <v-btn text="Cancel" color="danger" @click="newEditDialog = false"></v-btn>
                                <v-spacer></v-spacer>
                                <v-btn :prepend-icon="isEditing ? 'mdi-pencil' : 'mdi-plus'" color="primary"
                                    @click="save">
                                    {{ isEditing ? 'Update' : 'Create' }}
                                </v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-dialog>

                    <v-dialog v-model="confirmDeleteDialog" max-width="400px">
                        <v-card>
                            <v-card-title class="text-h6">Confirm Deletion</v-card-title>
                            <v-card-text>Are you sure you want to delete this company?</v-card-text>
                            <v-text-field v-model="companyNameToDelete" label="Company Name" readonly></v-text-field>
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
import companyService from '@/services/companyService';
import { extractErrorMessage } from '@/utils/errorHandler';
import type { Company } from '@/types';
import { useAsyncData } from '@/composables/useAsyncData';
import { DevelopmentLogger } from '@/utils/logger';
import { ref, computed } from 'vue';

const logger = new DevelopmentLogger({ prefix: '[CompaniesView]' });

const headers = [
    { title: 'Name', key: 'name', align: "start" as const },
    { title: 'ID', key: 'id' },
    { title: 'Actions', key: 'actions', align: 'end' as const, sortable: false }
];

const {
    data: companies,
    load: refreshCompanies
} = useAsyncData<Company[]>({
    fetchFunction: (signal) => companyService.getCompanies(signal),
    loggerPrefix: '[CompaniesView]'
});

const newEditDialog = ref(false);
const confirmDeleteDialog = ref(false);
const formModel = ref(createNewRecord());
const companyNameToDelete = ref('');
const companyIdToDelete = ref<number | null>(null);
const error = ref('');
const showError = ref(false);
const successMessage = ref('');
const showSuccess = ref(false);

function createNewRecord() {
    return {
        id: 0,
        name: '',
    } as Company;
}

const isEditing = computed(() => !!formModel.value.id);

function addNewCompany() {
    formModel.value = createNewRecord();
    newEditDialog.value = true;
}

function editCompany(id: number) {
    const company = companies.value?.find(c => c.id === id);
    if (company) {
        formModel.value = { ...company };
        newEditDialog.value = true;
    } else {
        logger.error(`Company with ID ${id} not found.`);
    }
}

function showDeleteConfirmation(id: number) {
    const company = companies.value?.find(c => c.id === id);
    if (company) {
        logger.log('Delete company:', company);
        companyNameToDelete.value = company.name;
        companyIdToDelete.value = company.id;
        confirmDeleteDialog.value = true;
    } else {
        logger.error(`Company with ID ${id} not found.`);
    }
}

async function confirmDelete() {
    if (companyIdToDelete.value == null) {
        logger.error('No company selected for deletion.');
        error.value = 'No company selected for deletion';
        return;
    }

    try {
        await companyService.deleteCompany(companyIdToDelete.value);
        await refreshCompanies();
        logger.log(`Confirmed deletion of company id: ${companyIdToDelete.value}`);
        error.value = '';
    } catch (err) {
        logger.error('Failed to delete company:', err);
        error.value = (err instanceof Error ? err.message : 'Failed to delete company');
    } finally {
        companyIdToDelete.value = null;
        companyNameToDelete.value = '';
        confirmDeleteDialog.value = false;
    }
}

function cancelDelete() {
    confirmDeleteDialog.value = false;
    companyNameToDelete.value = '';
    companyIdToDelete.value = null;
}

async function save() {
    if (!formModel.value.name) {
        logger.error('Company name is required.');
        error.value = 'Company name is required';
        return;
    }

    try {
        if (isEditing.value) {
            await companyService.updateCompany(formModel.value.id, { name: formModel.value.name });
            await refreshCompanies();
        } else {
            await companyService.createCompany({ name: formModel.value.name });
            await refreshCompanies();
        }

        newEditDialog.value = false;
        error.value = '';
    } catch (err) {
        logger.error('Failed to save company:', err);
        error.value = (err instanceof Error ? err.message : 'Failed to save company');
    }
}

</script>
<style scoped></style>
