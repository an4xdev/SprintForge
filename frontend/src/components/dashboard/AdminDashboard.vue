<template>
    <div>
        <h1 class="text-h4 mb-6">Administrator Panel</h1>

        <v-row class="mb-6">
            <v-col cols="12" md="3">
                <v-card class="text-center">
                    <v-card-text>
                        <v-icon size="48" color="primary" class="mb-2">mdi-domain</v-icon>
                        <div class="text-h4 font-weight-bold">12</div>
                        <div class="text-caption">Number of companies</div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" md="3">
                <v-card class="text-center">
                    <v-card-text>
                        <v-icon size="48" color="success" class="mb-2">mdi-folder-multiple</v-icon>
                        <div class="text-h4 font-weight-bold">45</div>
                        <div class="text-caption">Active projects</div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" md="3">
                <v-card class="text-center">
                    <v-card-text>
                        <v-icon size="48" color="info" class="mb-2">mdi-account-group</v-icon>
                        <div class="text-h4 font-weight-bold">312</div>
                        <div class="text-caption">Users</div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" md="3">
                <v-card class="text-center">
                    <v-card-text>
                        <v-icon size="48" color="warning" class="mb-2">mdi-account-multiple</v-icon>
                        <div class="text-h4 font-weight-bold">24</div>
                        <div class="text-caption">Teams</div>
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row>

        <v-card>
            <v-card-title class="text-h5">Latest system activities</v-card-title>
            <v-card-text>
                <v-list>
                    <v-list-item>
                        <template v-slot:prepend>
                            <v-icon color="success">mdi-plus-circle</v-icon>
                        </template>
                        <v-list-item-title>New project "Mobile Application" was created</v-list-item-title>
                        <v-list-item-subtitle>2 hours ago</v-list-item-subtitle>
                    </v-list-item>
                    <v-list-item>
                        <template v-slot:prepend>
                            <v-icon color="info">mdi-account-plus</v-icon>
                        </template>
                        <v-list-item-title>User Marek was added to the team</v-list-item-title>
                        <v-list-item-subtitle>4 hours ago</v-list-item-subtitle>
                    </v-list-item>
                    <v-list-item>
                        <template v-slot:prepend>
                            <v-icon color="warning">mdi-pencil</v-icon>
                        </template>
                        <v-list-item-title>Company "TechSolutions" was updated</v-list-item-title>
                        <v-list-item-subtitle>6 hours ago</v-list-item-subtitle>
                    </v-list-item>
                </v-list>
            </v-card-text>
        </v-card>
    </div>
</template>

<script setup lang="ts">
</script>
