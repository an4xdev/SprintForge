<template>
    <div>
        <h1 class="text-h4 mb-6">Manager Panel</h1>

        <v-row class="mb-6">
            <v-col cols="12" md="6">
                <v-card>
                    <v-card-text>
                        <div class="d-flex align-center">
                            <v-icon size="48" color="primary" class="mr-4">mdi-rocket-launch</v-icon>
                            <div>
                                <div class="text-h5 font-weight-bold">Sprint 3.2</div>
                                <div class="text-caption">Active sprint</div>
                            </div>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" md="6">
                <v-card>
                    <v-card-text>
                        <div class="text-caption mb-2">Team progress</div>
                        <v-progress-linear value="75" height="20" color="success" class="mb-2">
                            <template v-slot:default="{ value }">
                                <span class="text-white font-weight-bold">{{ value }}%</span>
                            </template>
                        </v-progress-linear>
                        <div class="text-caption text-center">75% completed</div>
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row>

        <v-card class="mb-6">
            <v-card-title class="text-h5">In-progress tasks from your team</v-card-title>
            <v-card-text>
                <v-row>
                    <v-col cols="12" md="6">
                        <v-card variant="outlined" class="border-s-4" style="border-left-color: #FFA726;">
                            <v-card-text>
                                <div class="text-h6 font-weight-bold">Fix bug on login page</div>
                                <div class="text-caption text-medium-emphasis">Assigned: Anna</div>
                                <v-chip color="warning" size="small" class="mt-2">Paused</v-chip>
                            </v-card-text>
                        </v-card>
                    </v-col>
                    <v-col cols="12" md="6">
                        <v-card variant="outlined" class="border-s-4" style="border-left-color: #42A5F5;">
                            <v-card-text>
                                <div class="text-h6 font-weight-bold">Notification system implementation</div>
                                <div class="text-caption text-medium-emphasis">Assigned: John</div>
                                <v-chip color="primary" size="small" class="mt-2">In Progress</v-chip>
                            </v-card-text>
                        </v-card>
                    </v-col>
                </v-row>
            </v-card-text>
        </v-card>

        <v-card>
            <v-card-title class="text-h5">Team task board</v-card-title>
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
    </div>
</template>

<script setup lang="ts">
</script>
