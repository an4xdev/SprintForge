<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <h1 class="text-h4 mb-6">Tasks Management</h1>

                <div class="py-1">
                    <v-sheet border rounded>
                        <v-data-table :headers="headers" :hide-default-footer="tasks !== null && tasks.length < 11"
                            :items="tasks ?? []" :loading="loading">
                            <template v-slot:top>
                                <v-toolbar flat>
                                    <v-toolbar-title>
                                        <v-icon color="medium-emphasis" icon="mdi-format-list-bulleted" size="x-small"
                                            start></v-icon>
                                        Tasks
                                    </v-toolbar-title>

                                    <v-btn class="me-2" prepend-icon="mdi-plus" rounded="lg" text="Add a Task" border
                                        @click="addNewTask"></v-btn>
                                </v-toolbar>
                            </template>

                            <template v-slot:item.name="{ value, item }">
                                <v-chip :text="value" border="thin opacity-25" prepend-icon="mdi-format-list-bulleted"
                                    label @click="showTaskDetails(item)">
                                    <template v-slot:prepend>
                                        <v-icon color="medium-emphasis"></v-icon>
                                    </template>
                                </v-chip>
                            </template>

                            <template v-slot:item.developer="{ value }">
                                <span v-if="value">
                                    {{ value.firstName }} {{ value.lastName }} ({{ value.username }})
                                </span>
                                <span v-else class="text-medium-emphasis">Not assigned</span>
                            </template>

                            <template v-slot:item.sprint="{ value }">
                                <span v-if="value">
                                    {{ value.name }}
                                </span>
                                <span v-else class="text-medium-emphasis">No sprint</span>
                            </template>

                            <template v-slot:item.taskStatus="{ value }">
                                <v-chip v-if="value" :color="getTaskStatusColor(value.name)" size="small">
                                    {{ value.name }}
                                </v-chip>
                                <span v-else class="text-medium-emphasis">No status</span>
                            </template>

                            <template v-slot:item.taskType="{ value }">
                                <v-chip v-if="value" :color="getTaskTypeColor(value.name)" size="small">
                                    {{ value.name }}
                                </v-chip>
                                <span v-else class="text-medium-emphasis">No type</span>
                            </template>

                            <template v-slot:item.actions="{ item }">
                                <div class="d-flex ga-2 justify-end">
                                    <v-icon color="medium-emphasis" icon="mdi-eye" size="small"
                                        @click="showTaskDetails(item)" title="View Details" />

                                    <v-icon color="medium-emphasis" icon="mdi-pencil" size="small"
                                        @click="editTask(item.id)" />

                                    <v-icon color="medium-emphasis" icon="mdi-account-switch" size="small"
                                        @click="showAssignDeveloperDialog(item.id)" title="Assign Developer" />

                                    <v-icon color="medium-emphasis" icon="mdi-swap-horizontal" size="small"
                                        @click="showMoveToSprintDialog(item.id)" title="Move to Sprint" />

                                    <v-icon color="medium-emphasis" icon="mdi-delete" size="small"
                                        @click="showDeleteConfirmation(item.id)" />
                                </div>
                            </template>

                            <template v-slot:no-data>
                                <v-btn prepend-icon="mdi-backup-restore" rounded="lg" text="Refresh" variant="text"
                                    border @click="refreshTasks"></v-btn>
                            </template>
                        </v-data-table>
                    </v-sheet>

                    <v-dialog v-model="newEditDialog" max-width="500">
                        <v-card :title="`${isEditing ? 'Edit' : 'Add'} a Task`">
                            <template v-slot:text>
                                <v-row>
                                    <v-col cols="12">
                                        <v-text-field v-model="formModel.name" label="Task Name" required />
                                    </v-col>
                                    <v-col cols="12">
                                        <v-textarea v-model="formModel.description" label="Description" rows="3" />
                                    </v-col>
                                    <v-col cols="12" v-if="!isEditing">
                                        <v-select v-model="formModel.developerId" :items="developers"
                                            item-title="username" item-value="id" label="Developer" clearable />
                                    </v-col>
                                    <v-col cols="12" v-if="!isEditing">
                                        <v-select v-model="formModel.sprintId" :items="sprints" item-title="name"
                                            item-value="id" label="Sprint" clearable />
                                    </v-col>
                                    <v-col cols="12">
                                        <v-select v-model="formModel.taskTypeId" :items="taskTypes" item-title="name"
                                            item-value="id" label="Task Type" clearable />
                                    </v-col>
                                </v-row>
                            </template>

                            <v-divider></v-divider>

                            <v-card-actions class="bg-surface-light">
                                <v-btn text="Cancel" variant="plain" @click="newEditDialog = false"></v-btn>

                                <v-spacer></v-spacer>

                                <v-btn text="Save" @click="save" :loading="saving"></v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-dialog>

                    <v-dialog v-model="confirmDeleteDialog" max-width="400px">
                        <v-card>
                            <v-card-title class="text-h6">Confirm Deletion</v-card-title>
                            <v-card-text>Are you sure you want to delete this task?</v-card-text>
                            <v-text-field v-model="taskNameToDelete" label="Task Name" readonly></v-text-field>
                            <v-divider></v-divider>
                            <v-card-actions class="bg-surface-light">
                                <v-btn text="Cancel" color="success" @click="cancelDelete"></v-btn>
                                <v-spacer></v-spacer>
                                <v-btn text="Confirm" color="error" @click="confirmDelete" :loading="deleting"></v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-dialog>

                    <v-dialog v-model="assignDeveloperDialog" max-width="400px">
                        <v-card title="Assign Developer">
                            <template v-slot:text>
                                <v-select v-model="selectedDeveloperId" :items="developers" item-title="username"
                                    item-value="id" label="Developer" clearable />
                            </template>
                            <v-divider></v-divider>
                            <v-card-actions class="bg-surface-light">
                                <v-btn text="Cancel" @click="assignDeveloperDialog = false"></v-btn>
                                <v-spacer></v-spacer>
                                <v-btn text="Assign" @click="assignDeveloper" :loading="assigning"></v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-dialog>

                    <v-dialog v-model="moveToSprintDialog" max-width="400px">
                        <v-card title="Move to Sprint">
                            <template v-slot:text>
                                <v-select v-model="selectedSprintId" :items="sprints" item-title="name" item-value="id"
                                    label="Sprint" clearable />
                            </template>
                            <v-divider></v-divider>
                            <v-card-actions class="bg-surface-light">
                                <v-btn text="Cancel" @click="moveToSprintDialog = false"></v-btn>
                                <v-spacer></v-spacer>
                                <v-btn text="Move" @click="moveTaskToSprint" :loading="moving"></v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-dialog>
                </div>

                <!-- Task Details Modal -->
                <TaskDetailsModal v-model="taskDetailsDialog" :task="selectedTaskDetails" />
            </v-container>
        </v-main>
    </v-layout>
</template>

<script setup lang="ts">
import { useAsyncData } from '@/composables/useAsyncData';
import tasksService from '@/services/tasksService';
import usersService from '@/services/usersService';
import sprintsService from '@/services/sprintsService';
import taskStatusesService from '@/services/taskStatusesService';
import taskTypesService from '@/services/taskTypesService';
import type { Task, CreateTask, User, Sprint, TaskStatus, TaskType, TaskExt } from '@/types';
import { DevelopmentLogger } from '@/utils/logger';
import { ref, toRef, computed, onMounted } from 'vue';
import TaskDetailsModal from '@/components/modals/TaskDetailsModal.vue';
import { getTaskStatusColor, getTaskTypeColor } from '@/utils/taskColors';

const logger = new DevelopmentLogger({ prefix: '[AdminTasksView]' });

const headers = [
    { title: 'Name', key: 'name', align: "start" as const },
    { title: 'Description', key: 'description' },
    { title: 'Developer', key: 'developer' },
    { title: 'Sprint', key: 'sprint' },
    { title: 'Status', key: 'taskStatus' },
    { title: 'Type', key: 'taskType' },
    { title: 'Actions', key: 'actions', align: 'end' as const, sortable: false }
];

const {
    data: tasks,
    load: refreshTasks,
    loading
} = useAsyncData<TaskExt[]>({
    fetchFunction: (signal) => tasksService.getAllTasksExt(signal),
    loggerPrefix: '[AdminTasksView]'
});

const developers = ref<User[]>([]);
const sprints = ref<Sprint[]>([]);
const taskStatuses = ref<TaskStatus[]>([]);
const taskTypes = ref<TaskType[]>([]);

const newEditDialog = ref(false);
const confirmDeleteDialog = ref(false);
const assignDeveloperDialog = ref(false);
const moveToSprintDialog = ref(false);
const taskDetailsDialog = ref(false);
const saving = ref(false);
const deleting = ref(false);
const assigning = ref(false);
const moving = ref(false);

const formModel = ref<CreateTask & { id?: string }>(createNewRecord());
const taskToDelete = ref<TaskExt | null>(null);
const taskNameToDelete = computed(() => taskToDelete.value?.name || '');
const selectedTaskDetails = ref<TaskExt | null>(null);
const taskToAssign = ref<string | null>(null);
const taskToMove = ref<string | null>(null);
const selectedDeveloperId = ref<string | null>(null);
const selectedSprintId = ref<string | null>(null);

function createNewRecord(): CreateTask & { id?: string } {
    return {
        name: '',
        description: '',
        developerId: null,
        sprintId: null,
        taskTypeId: null
    };
}

const isEditing = toRef(() => !!formModel.value.id);

async function loadReferenceData() {
    try {
        const [developersData, sprintsData, statusesData, typesData] = await Promise.all([
            usersService.getUsersByRole('developer'),
            sprintsService.getSprints(),
            taskStatusesService.getTaskStatuses(),
            taskTypesService.getTaskTypes()
        ]);

        developers.value = developersData;
        sprints.value = sprintsData;
        taskStatuses.value = statusesData;
        taskTypes.value = typesData;
    } catch (error) {
        logger.error('Error loading reference data:', error);
    }
}

function addNewTask() {
    formModel.value = createNewRecord();
    newEditDialog.value = true;
}

function editTask(id: string) {
    const task = tasks.value?.find(t => t.id === id);
    if (task) {
        formModel.value = {
            id: task.id,
            name: task.name,
            description: task.description || undefined,
            developerId: task.developer?.id || null,
            sprintId: task.sprint?.id || null,
            taskTypeId: task.taskType.id
        };
        newEditDialog.value = true;
    } else {
        logger.error(`Task with ID ${id} not found.`);
    }
}

function showTaskDetails(task: TaskExt) {
    selectedTaskDetails.value = task;
    taskDetailsDialog.value = true;
}

function showDeleteConfirmation(id: string) {
    const task = tasks.value?.find(t => t.id === id);
    if (task) {
        taskToDelete.value = task;
        confirmDeleteDialog.value = true;
    } else {
        logger.error(`Task with ID ${id} not found.`);
    }
}

async function save() {
    if (!formModel.value.name.trim()) {
        logger.error('Task name is required.');
        return;
    }

    saving.value = true;
    try {
        if (isEditing.value && formModel.value.id) {
            const updateData: Partial<Task> = {
                name: formModel.value.name,
                description: formModel.value.description,
                taskTypeId: formModel.value.taskTypeId || 0
            };

            await tasksService.updateTask(formModel.value.id, updateData);
            logger.log('Task updated successfully');
        } else {
            const createData: CreateTask = {
                name: formModel.value.name,
                description: formModel.value.description,
                developerId: formModel.value.developerId,
                sprintId: formModel.value.sprintId,
                taskTypeId: formModel.value.taskTypeId
            };

            await tasksService.createTask(createData);
            logger.log('Task created successfully');
        }

        await refreshTasks();
        newEditDialog.value = false;
    } catch (error) {
        logger.error('Error saving task:', error);
    } finally {
        saving.value = false;
    }
}

async function confirmDelete() {
    if (!taskToDelete.value) {
        logger.error('No task selected for deletion.');
        return;
    }

    deleting.value = true;
    try {
        await tasksService.deleteTask(taskToDelete.value.id);
        logger.log(`Task "${taskToDelete.value.name}" deleted successfully`);

        await refreshTasks();
        confirmDeleteDialog.value = false;
        taskToDelete.value = null;
    } catch (error) {
        logger.error('Error deleting task:', error);
    } finally {
        deleting.value = false;
    }
}

function cancelDelete() {
    confirmDeleteDialog.value = false;
    taskToDelete.value = null;
}

function showAssignDeveloperDialog(id: string) {
    taskToAssign.value = id;
    selectedDeveloperId.value = null;
    assignDeveloperDialog.value = true;
}

async function assignDeveloper() {
    if (!taskToAssign.value || !selectedDeveloperId.value) {
        logger.error('Task ID and Developer ID are required for assignment.');
        return;
    }

    assigning.value = true;
    try {
        await tasksService.assignDeveloper(taskToAssign.value, selectedDeveloperId.value);
        logger.log('Developer assigned to task successfully');

        await refreshTasks();
        assignDeveloperDialog.value = false;
        taskToAssign.value = null;
        selectedDeveloperId.value = null;
    } catch (error) {
        logger.error('Error assigning developer to task:', error);
    } finally {
        assigning.value = false;
    }
}

function showMoveToSprintDialog(id: string) {
    taskToMove.value = id;
    selectedSprintId.value = null;
    moveToSprintDialog.value = true;
}

async function moveTaskToSprint() {
    if (!taskToMove.value || !selectedSprintId.value) {
        logger.error('Task ID and Sprint ID are required for moving task.');
        return;
    }

    moving.value = true;
    try {
        await tasksService.moveToSprint(taskToMove.value, selectedSprintId.value);
        logger.log('Task moved to sprint successfully');

        await refreshTasks();
        moveToSprintDialog.value = false;
        taskToMove.value = null;
        selectedSprintId.value = null;
    } catch (error) {
        logger.error('Error moving task to sprint:', error);
    } finally {
        moving.value = false;
    }
}

onMounted(() => {
    loadReferenceData();
});
</script>

<style scoped></style>
