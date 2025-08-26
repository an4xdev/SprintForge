<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">Task types Management</h1>

                <div class="py-1">
                    <v-sheet border rounded>
                        <v-data-table :headers="headers"
                            :hide-default-footer="taskTypes !== null && taskTypes.length < 11" :items="taskTypes ?? []">
                            <template v-slot:top>
                                <v-toolbar flat>
                                    <v-toolbar-title>
                                        <v-icon color="medium-emphasis" icon="mdi-shape" size="x-small" start></v-icon>
                                        Task Types
                                    </v-toolbar-title>

                                    <v-btn class="me-2" prepend-icon="mdi-plus" rounded="lg" text="Add a Task Type"
                                        border @click="addNewTaskType"></v-btn>
                                </v-toolbar>
                            </template>

                            <template v-slot:item.name="{ value }">
                                <v-chip :text="value" border="thin opacity-25" prepend-icon="mdi-shape" label>
                                    <template v-slot:prepend>
                                        <v-icon color="medium-emphasis"></v-icon>
                                    </template>
                                </v-chip>
                            </template>

                            <template v-slot:item.actions="{ item }">
                                <div class="d-flex ga-2 justify-end">
                                    <v-icon color="medium-emphasis" icon="mdi-pencil" size="small"
                                        @click="editTaskType(item.id)"></v-icon>

                                    <v-icon color="medium-emphasis" icon="mdi-delete" size="small"
                                        @click="showDeleteConfirmation(item.id)"></v-icon>
                                </div>
                            </template>

                            <template v-slot:no-data>
                                <v-btn prepend-icon="mdi-backup-restore" rounded="lg" text="Refresh" variant="text"
                                    border @click="refreshTaskTypes"></v-btn>
                            </template>
                        </v-data-table>
                    </v-sheet>

                    <v-dialog v-model="newEditDialog" max-width="500px">
                        <v-card :title="isEditing ? 'Edit Task Type' : 'Create New Task Type'">
                            <template v-slot:text>
                                <v-row>
                                    <v-col cols="12">
                                        <v-text-field v-model="formModel.name" label="Task Type Name"
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
                            <v-card-text>Are you sure you want to delete this task type?</v-card-text>
                            <v-text-field v-model="taskTypeNameToDelete" label="Task Type Name" readonly></v-text-field>
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
import type { TaskType } from '@/types';
import { useAsyncData } from '@/composables/useAsyncData';
import { DevelopmentLogger } from '@/utils/logger';
import { ref, computed } from 'vue';
import taskTypesService from '@/services/taskTypesService';

const logger = new DevelopmentLogger({ prefix: '[TaskTypesView]' });

const headers = [
    { title: 'Name', key: 'name', align: "start" as const },
    { title: 'ID', key: 'id' },
    { title: 'Actions', key: 'actions', align: 'end' as const, sortable: false }
];

const {
    data: taskTypes,
    load: refreshTaskTypes
} = useAsyncData<TaskType[]>({
    fetchFunction: (signal) => taskTypesService.getTaskTypes(signal),
    loggerPrefix: '[TaskTypesView]'
});

const newEditDialog = ref(false);
const confirmDeleteDialog = ref(false);
const formModel = ref(createNewRecord());
const taskTypeNameToDelete = ref('');
const taskTypeIdToDelete = ref<number | null>(null);
const saving = ref(false);
const deleting = ref(false);

function createNewRecord() {
    return {
        id: 0,
        name: '',
    } as TaskType;
}

const isEditing = computed(() => !!formModel.value.id && formModel.value.id !== 0)

function addNewTaskType() {
    formModel.value = createNewRecord();
    newEditDialog.value = true;
}

function editTaskType(id: number) {
    try {
        const taskType = taskTypes.value?.find(tt => tt.id === id);
        if (taskType) {
            formModel.value = { ...taskType };
            newEditDialog.value = true;
        } else {
            logger.error(`Task Type with ID ${id} not found (API returned empty).`);
        }
    } catch (err) {
        logger.error(`Failed to load task type id=${id}:`, err);
    }
}

function showDeleteConfirmation(id: number) {
    try {
        const taskType = taskTypes.value?.find(tt => tt.id === id);
        if (taskType) {
            logger.log('Delete task type:', taskType);
            taskTypeNameToDelete.value = taskType.name;
            taskTypeIdToDelete.value = taskType.id;
            confirmDeleteDialog.value = true;
        } else {
            logger.error(`Task Type with ID ${id} not found (API returned empty).`);
        }
    } catch (err) {
        logger.error(`Failed to load task type for deletion id=${id}:`, err);
    }
}

async function confirmDelete() {
    if (taskTypeIdToDelete.value == null) {
        logger.error('No task type selected for deletion.');
        return;
    }

    deleting.value = true;
    try {
        await taskTypesService.deleteTaskType(taskTypeIdToDelete.value);
        await refreshTaskTypes();
        logger.log(`Deleted task type id=${taskTypeIdToDelete.value}`);
    } catch (err) {
        logger.error('Failed to delete task type:', err);
    } finally {
        deleting.value = false;
        taskTypeIdToDelete.value = null;
        taskTypeNameToDelete.value = '';
        confirmDeleteDialog.value = false;
    }
}

function cancelDelete() {
    confirmDeleteDialog.value = false;
    taskTypeNameToDelete.value = '';
}

async function save() {
    if (!formModel.value.name) {
        logger.error('Task Type name is required.');
        return;
    }

    saving.value = true;
    try {
        if (isEditing.value) {
            await taskTypesService.updateTaskType(formModel.value.id, { name: formModel.value.name });
            logger.log('Updated task type', formModel.value);
        } else {
            await taskTypesService.createTaskType({ name: formModel.value.name });
            logger.log('Created task type', formModel.value.name);
        }

        await refreshTaskTypes();
        newEditDialog.value = false;
    } catch (err) {
        logger.error('Failed to save task type:', err);
    } finally {
        saving.value = false;
    }
}

</script>
<style scoped></style>
