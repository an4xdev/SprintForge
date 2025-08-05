<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">Company Management</h1>

                <v-card>
                    <v-card-title class="text-h5">
                        Company List
                        <v-spacer></v-spacer>
                        <v-btn color="primary" @click="loadCompanies" :loading="loading" prepend-icon="mdi-refresh">
                            Refresh List
                        </v-btn>
                    </v-card-title>

                    <v-card-text>
                        <v-alert v-if="error" type="error" class="mb-4" closable @click:close="error = ''">
                            {{ error }}
                        </v-alert>

                        <div v-if="(!companies || !companies.length) && !loading && !error" class="text-center py-12">
                            <v-icon size="64" color="primary" class="mb-4">mdi-domain</v-icon>
                            <div class="text-h6 mb-2">No companies</div>
                            <div class="text-body-2 text-medium-emphasis mb-4">
                                Click the "Refresh List" button to fetch companies from the server
                            </div>
                        </div>

                        <v-progress-linear v-if="loading" indeterminate class="mb-4"></v-progress-linear>

                        <v-list v-if="companies && companies.length > 0">
                            <v-list-item v-for="company in companies" :key="company.id" :title="company.name"
                                :subtitle="`ID: ${company.id}`" prepend-icon="mdi-domain">
                                <template v-slot:append>
                                    <v-btn icon="mdi-pencil" size="small" variant="text"
                                        @click="editCompany(company)"></v-btn>
                                </template>
                            </v-list-item>
                        </v-list>
                    </v-card-text>
                </v-card>
            </v-container>
        </v-main>
    </v-layout>
</template>

<script setup lang="ts">
import companyService from '@/services/companyService';
import type { Company } from '@/types';
import { useAsyncData } from '@/composables/useAsyncData';
import { DevelopmentLogger } from '@/utils/logger';

const logger = new DevelopmentLogger({ prefix: '[CompaniesView]' });

const {
    data: companies,
    loading,
    error,
    load: loadCompanies
} = useAsyncData<Company[]>({
    fetchFunction: (signal) => companyService.getCompanies(signal),
    loggerPrefix: '[CompaniesView]'
});

const editCompany = (company: Company) => {
    logger.log('Edit company:', company);
};
</script>
<style scoped></style>
