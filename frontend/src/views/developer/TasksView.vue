<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">All Tasks</h1>

                <v-card>
                    <v-card-title class="text-h5">My Tasks</v-card-title>
                    <v-card-text>
                        <div class="text-center py-12">
                            <v-icon size="64" color="primary" class="mb-4">mdi-format-list-bulleted</v-icon>
                            <div class="text-h6 mb-2">Task List</div>
                            <div class="text-body-2 text-medium-emphasis">No tasks to display at this time.</div>
                        </div>
                    </v-card-text>
                </v-card>
            </v-container>
        </v-main>
    </v-layout>
</template>

<script setup lang="ts">
</script>

<style scoped></style>
