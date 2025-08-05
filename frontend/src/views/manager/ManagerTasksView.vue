<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">My team tasks</h1>

                <v-card>
                    <v-card-title class="text-h5">My team tasks</v-card-title>
                    <v-card-text>
                        <v-row>
                            <v-col cols="12" md="4">
                                <div class="text-h6 mb-3">To Do</div>
                                <v-card variant="outlined" class="mb-3">
                                    <v-card-text>
                                        <div class="font-weight-medium">Fix bug on login page</div>
                                        <div class="text-caption text-medium-emphasis">Assigned: Anna</div>
                                    </v-card-text>
                                </v-card>
                                <v-card variant="outlined">
                                    <v-card-text>
                                        <div class="font-weight-medium">Program contact form</div>
                                        <div class="text-caption text-medium-emphasis">Assigned: John</div>
                                    </v-card-text>
                                </v-card>
                            </v-col>
                            <v-col cols="12" md="4">
                                <div class="text-h6 mb-3">In Progress</div>
                                <v-card variant="outlined">
                                    <v-card-text>
                                        <div class="font-weight-medium">Payment system integration</div>
                                        <div class="text-caption text-medium-emphasis">Assigned: Peter</div>
                                    </v-card-text>
                                </v-card>
                            </v-col>
                            <v-col cols="12" md="4">
                                <div class="text-h6 mb-3">Completed</div>
                                <v-card variant="outlined" class="opacity-75">
                                    <v-card-text>
                                        <div class="font-weight-medium">Database design</div>
                                        <div class="text-caption text-medium-emphasis">Assigned: John</div>
                                    </v-card-text>
                                </v-card>
                            </v-col>
                        </v-row>
                    </v-card-text>
                </v-card>
            </v-container>
        </v-main>
    </v-layout>
</template>

<script setup lang="ts">
</script>

<style scoped></style>
