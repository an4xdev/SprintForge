<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">Users Management</h1>

                <div class="py-1">
                    <v-sheet border rounded>
                        <v-data-table :headers="headers" :hide-default-footer="users !== null && users.length < 11"
                            :items="users ?? []">
                            <template v-slot:top>
                                <v-toolbar flat>
                                    <v-toolbar-title>
                                        <v-icon color="medium-emphasis" icon="mdi-account-multiple" size="x-small"
                                            start></v-icon>
                                        Users
                                    </v-toolbar-title>

                                    <v-btn class="me-2" prepend-icon="mdi-plus" rounded="lg" text="Register New User"
                                        border @click="addNewUser"></v-btn>
                                </v-toolbar>
                            </template>

                            <template v-slot:item.username="{ value }">
                                <v-chip :text="value" border="thin opacity-25" prepend-icon="mdi-account-multiple"
                                    label>
                                    <template v-slot:prepend>
                                        <v-icon color="medium-emphasis"></v-icon>
                                    </template>
                                </v-chip>
                            </template>

                            <template v-slot:item.actions="{ item }">
                                <div class="d-flex ga-2 justify-end">
                                    <v-icon color="medium-emphasis" icon="mdi-pencil" size="small"
                                        @click="editUser(item.id)"></v-icon>

                                    <v-icon color="medium-emphasis" icon="mdi-delete" size="small"
                                        @click="showDeleteConfirmation(item.id)"></v-icon>
                                </div>
                            </template>

                            <template v-slot:no-data>
                                <v-btn prepend-icon="mdi-backup-restore" rounded="lg" text="Refresh" variant="text"
                                    border @click="refreshUsers"></v-btn>
                            </template>
                        </v-data-table>
                    </v-sheet>

                    <v-dialog v-model="newEditDialog" max-width="500">
                        <v-card :title="`${isEditing ? 'Edit User' : 'Register New User'}`">
                            <template v-slot:text>
                                <v-row v-if="!isEditing">
                                    <!-- Registration Form -->
                                    <v-col cols="12">
                                        <v-text-field v-model="registrationFormModel.username"
                                            label="Username"></v-text-field>
                                    </v-col>

                                    <v-col cols="12">
                                        <v-text-field v-model="registrationFormModel.password" label="Password"
                                            type="password"></v-text-field>
                                    </v-col>

                                    <v-col cols="12">
                                        <v-select v-model="registrationFormModel.role"
                                            :items="['admin', 'manager', 'developer']" label="Role"></v-select>
                                    </v-col>
                                </v-row>

                                <v-row v-else>
                                    <!-- Edit Form -->
                                    <v-col cols="12">
                                        <v-text-field v-model="editFormModel.username" label="Username"></v-text-field>
                                    </v-col>
                                </v-row>
                            </template>

                            <v-divider></v-divider>

                            <v-card-actions class="bg-surface-light">
                                <v-btn text="Cancel" variant="plain" @click="newEditDialog = false"></v-btn>

                                <v-spacer></v-spacer>

                                <v-btn text="Save" @click="save"></v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-dialog>

                    <v-dialog v-model="confirmDeleteDialog" max-width="400px">
                        <v-card>
                            <v-card-title class="text-h6">Confirm Deletion</v-card-title>
                            <v-card-text>Are you sure you want to delete this user?</v-card-text>
                            <v-text-field v-model="userNameToDelete" label="User Name" readonly></v-text-field>
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
import usersService from '@/services/usersService';
import type { Profile, RegisterCredentials } from '@/types';
import { DevelopmentLogger } from '@/utils/logger';
import { ref } from 'vue';

const logger = new DevelopmentLogger({ prefix: '[UsersView]' });

const headers = [
    { title: 'Username', key: 'username', align: "start" as const },
    { title: 'ID', key: 'id' },
    { title: 'Avatar', key: 'avatar' },
    { title: 'Actions', key: 'actions', align: 'end' as const, sortable: false }
];

const {
    data: users,
    load: refreshUsers
} = useAsyncData<Profile[]>({
    fetchFunction: (signal) => usersService.getUsers(signal),
    loggerPrefix: '[UsersView]'
});

const newEditDialog = ref(false);
const confirmDeleteDialog = ref(false);
const registrationFormModel = ref(createNewRegistrationRecord());
const editFormModel = ref(createNewEditRecord());
const userNameToDelete = ref('');
const isEditing = ref(false);

function createNewRegistrationRecord() {
    return {
        username: '',
        role: '' as 'admin' | 'manager' | 'developer' | '',
        password: '',
    } as RegisterCredentials;
}

function createNewEditRecord() {
    return {
        id: '',
        username: '',
        avatar: null,
    } as Profile;
}

function addNewUser() {
    registrationFormModel.value = createNewRegistrationRecord();
    isEditing.value = false;
    newEditDialog.value = true;
}

function editUser(id: string) {
    const user = users.value?.find(u => u.id === id);
    if (user) {
        editFormModel.value = { ...user };
        isEditing.value = true;
        newEditDialog.value = true;
    } else {
        logger.error(`User with ID ${id} not found.`);
    }
}

function showDeleteConfirmation(id: string) {
    const user = users.value?.find(u => u.id === id);
    if (user) {
        logger.log('Delete user:', user);
        userNameToDelete.value = user.username;
        confirmDeleteDialog.value = true;
    } else {
        logger.error(`User with ID ${id} not found.`);
    }
}

function confirmDelete() {

    if (users.value == null) {
        logger.error('Users data is not loaded yet.');
        return;
    }

    if (!userNameToDelete.value) {
        logger.error('No user selected for deletion.');
        return;
    }

    users.value = users.value.filter(u => u.username !== userNameToDelete.value);
    userNameToDelete.value = '';

    logger.log(`Confirmed deletion of user: ${userNameToDelete.value}`);
    confirmDeleteDialog.value = false;
}

function cancelDelete() {
    confirmDeleteDialog.value = false;
    userNameToDelete.value = '';
}

function save() {
    if (users.value === null) {
        logger.error('Users data is not loaded yet.');
        return;
    }

    if (isEditing.value) {
        // editing
        if (!editFormModel.value.username) {
            logger.error('User username is required.');
            return;
        }

        const index = users.value.findIndex(user => user.id === editFormModel.value.id);
        if (index !== -1) {
            users.value[index] = { ...editFormModel.value };
        }
    } else {
        // registering
        if (!registrationFormModel.value.username) {
            logger.error('User username is required.');
            return;
        }

        if (!registrationFormModel.value.password) {
            logger.error('User password is required.');
            return;
        }

        // new profile from registration data
        const newProfile: Profile = {
            id: `${Date.now()}`,
            username: registrationFormModel.value.username,
            avatar: null
        };

        users.value.push(newProfile);
    }

    newEditDialog.value = false;
}
</script>

<style scoped></style>
