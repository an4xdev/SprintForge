<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <div class="d-flex justify-space-between align-center mb-6">
                    <h1 class="text-h4">Team Tasks Management</h1>
                    <div class="d-flex ga-2">
                        <v-btn color="primary" prepend-icon="mdi-plus" @click="openCreateTaskDialog">
                            Add Task
                        </v-btn>
                        <v-btn-toggle v-model="viewMode" mandatory>
                            <v-btn value="tabs" prepend-icon="mdi-tab">
                                Tabs
                            </v-btn>
                            <v-btn value="cards" prepend-icon="mdi-view-sequential">
                                Cards
                            </v-btn>
                        </v-btn-toggle>
                    </div>
                </div>

                <v-snackbar v-model="showError" color="error" timeout="3000" location="top right">
                    <v-icon start>mdi-alert-circle</v-icon>
                    {{ error }}
                </v-snackbar>

                <v-snackbar v-model="showSuccess" color="success" timeout="3000" location="top right">
                    <v-icon start>mdi-check-circle</v-icon>
                    {{ successMessage }}
                </v-snackbar>

                <v-card v-if="viewMode === 'tabs'">
                    <v-tabs v-model="activeTab">
                        <v-tab value="sprint-with-dev" class="bg-success text-white">Task in
                            sprint</v-tab>
                        <v-tab value="sprint-no-dev" class="bg-primary text-white">Unassigned tasks in
                            sprint</v-tab>
                        <v-tab value="project-no-sprint" class="bg-amber-darken-3 text-white">Unassigned
                            tasks in
                            project</v-tab>
                    </v-tabs>

                    <v-tabs-window v-model="activeTab">
                        <v-tabs-window-item value="sprint-with-dev">
                            <div v-if="sprintWithDevLoading" class="pa-6">
                                <v-progress-linear indeterminate color="success" />
                            </div>

                            <div v-else-if="noSprintAssigned" class="pa-6">
                                <v-sheet class="d-flex flex-column align-center pa-8" elevation="1">
                                    <v-icon size="48" color="primary">mdi-alert-circle-outline</v-icon>
                                    <h3 class="text-h6 mt-4">No sprint assigned</h3>
                                    <p class="text-body-2 text-center mt-2">You don't have any active sprint assigned.
                                    </p>
                                </v-sheet>
                            </div>

                            <div v-else-if="sprintWithDevTasks.length === 0" class="pa-6">
                                <v-sheet class="d-flex flex-column align-center pa-8" elevation="1">
                                    <v-icon size="48" color="success">mdi-clipboard-check-outline</v-icon>
                                    <h3 class="text-h6 mt-4">No tasks in sprint</h3>
                                    <p class="text-body-2 text-center mt-2">There are currently no tasks assigned to
                                        developers in the active sprint.</p>
                                </v-sheet>
                            </div>

                            <div v-else>
                                <SprintWithDevPanel :loading="sprintWithDevLoading" :tasks="sprintWithDevTasks"
                                    @task-click="openEditTaskDialog" />
                            </div>
                        </v-tabs-window-item>

                        <v-tabs-window-item value="sprint-no-dev">
                            <div v-if="sprintWithoutDevLoading" class="pa-6">
                                <v-progress-linear indeterminate color="primary" />
                            </div>
                            <div v-else-if="noSprintAssigned" class="pa-6">
                                <v-sheet class="d-flex flex-column align-center pa-8" elevation="1">
                                    <v-icon size="48" color="primary">mdi-alert-circle-outline</v-icon>
                                    <h3 class="text-h6 mt-4">No sprint assigned</h3>
                                    <p class="text-body-2 text-center mt-2">You don't have any active sprint assigned.
                                    </p>
                                </v-sheet>
                            </div>

                            <div v-else-if="sprintWithoutDevTasks.length === 0" class="pa-6">
                                <v-sheet class="d-flex flex-column align-center pa-8" elevation="1">
                                    <v-icon size="48" color="primary">mdi-clipboard-check-outline</v-icon>
                                    <h3 class="text-h6 mt-4">No unassigned sprint tasks</h3>
                                    <p class="text-body-2 text-center mt-2">There are currently no unassigned tasks in
                                        the active sprint. All tasks are assigned.</p>
                                    <v-btn color="primary" class="mt-4"
                                        @click="loadSprintWithoutDevTasks">Refresh</v-btn>
                                </v-sheet>
                            </div>

                            <div v-else>
                                <SprintWithoutDevPanel :loading="sprintWithoutDevLoading" :tasks="sprintWithoutDevTasks"
                                    :developers="teamDevelopers" @task-click="openEditTaskDialog" />
                            </div>
                        </v-tabs-window-item>

                        <v-tabs-window-item value="project-no-sprint">
                            <div v-if="noProjectAssigned" class="pa-6">
                                <v-alert type="info" variant="tonal" border="start">
                                    You don't have an assigned project. Please contact your administrator to be
                                    assigned to a project.
                                </v-alert>
                            </div>

                            <div v-else>
                                <ProjectTasksPanel ref="projectTasksPanel" :tasks="projectTasks"
                                    :loading="projectTasksLoading" :project-id="projectId || ''" />
                            </div>
                        </v-tabs-window-item>
                    </v-tabs-window>
                </v-card>

                <div v-if="viewMode === 'cards'" class="cards-view">
                    <v-card class="mb-6">
                        <v-card-title class="text-h5 bg-success text-white">
                            Tasks in Sprint
                        </v-card-title>

                        <div v-if="sprintWithDevLoading" class="pa-6">
                            <v-progress-linear indeterminate color="success" />
                        </div>

                        <div v-else-if="noSprintAssigned" class="pa-6">
                            <v-sheet class="d-flex flex-column align-center pa-8" elevation="1">
                                <v-icon size="48" color="primary">mdi-alert-circle-outline</v-icon>
                                <h3 class="text-h6 mt-4">No sprint assigned</h3>
                                <p class="text-body-2 text-center mt-2">You don't have any active sprint assigned.</p>
                            </v-sheet>
                        </div>

                        <div v-else-if="sprintWithDevTasks.length === 0" class="pa-6">
                            <v-sheet class="d-flex flex-column align-center pa-8" elevation="1">
                                <v-icon size="48" color="success">mdi-clipboard-check-outline</v-icon>
                                <h3 class="text-h6 mt-4">No tasks in sprint</h3>
                                <p class="text-body-2 text-center mt-2">There are currently no tasks assigned to
                                    developers in the active sprint.</p>
                            </v-sheet>
                        </div>

                        <div v-else>
                            <SprintWithDevPanel :loading="sprintWithDevLoading" :tasks="sprintWithDevTasks"
                                @task-click="openEditTaskDialog" />
                        </div>
                    </v-card>

                    <v-card class="mb-6">
                        <v-card-title class="text-h5 bg-primary text-white">
                            Unassigned Tasks in Sprint
                        </v-card-title>

                        <div v-if="sprintWithoutDevLoading" class="pa-6">
                            <v-progress-linear indeterminate color="primary" />
                        </div>

                        <div v-else-if="noSprintAssigned" class="pa-6">
                            <v-sheet class="d-flex flex-column align-center pa-8" elevation="1">
                                <v-icon size="48" color="primary">mdi-alert-circle-outline</v-icon>
                                <h3 class="text-h6 mt-4">No sprint assigned</h3>
                                <p class="text-body-2 text-center mt-2">You don't have any active sprint assigned.
                                </p>
                            </v-sheet>
                        </div>

                        <div v-else-if="sprintWithoutDevTasks.length === 0" class="pa-6">
                            <v-sheet class="d-flex flex-column align-center pa-8" elevation="1">
                                <v-icon size="48" color="primary">mdi-clipboard-check-outline</v-icon>
                                <h3 class="text-h6 mt-4">No unassigned sprint tasks</h3>
                                <p class="text-body-2 text-center mt-2">There are currently no unassigned tasks in the
                                    active sprint. All tasks are assigned.</p>
                                <v-btn color="primary" class="mt-4" @click="loadSprintWithoutDevTasks">Refresh</v-btn>
                            </v-sheet>
                        </div>

                        <div v-else>
                            <SprintWithoutDevPanel :loading="sprintWithoutDevLoading" :tasks="sprintWithoutDevTasks"
                                :developers="teamDevelopers" @task-click="openEditTaskDialog" />
                        </div>
                    </v-card>

                    <v-card class="mb-6">
                        <v-card-title class="text-h5 bg-amber-darken-3 text-white">
                            Unassigned Tasks in Project
                        </v-card-title>

                        <div v-if="noProjectAssigned" class="pa-6">
                            <v-alert type="info" variant="tonal" border="start">
                                You don't have an assigned project. Please contact your administrator to be
                                assigned to a project.
                            </v-alert>
                        </div>

                        <div v-else>
                            <ProjectTasksPanel ref="projectTasksPanel" :tasks="projectTasks"
                                :loading="projectTasksLoading" :project-id="projectId || ''"
                                @task-click="openEditTaskDialog" />
                        </div>
                    </v-card>
                </div>

                <v-dialog v-model="createTaskDialog" max-width="600">
                    <v-card title="Create New Task">
                        <v-card-text>
                            <v-form ref="taskForm">
                                <v-text-field v-model="taskFormData.name" label="Task Name*" required
                                    :rules="[v => !!v || 'Task name is required']"></v-text-field>

                                <v-textarea v-model="taskFormData.description" label="Description"
                                    rows="3"></v-textarea>

                                <v-select :items="taskTypes" item-title="name" item-value="id" label="Task Type*"
                                    v-model="taskFormData.taskTypeId" required
                                    :rules="[v => !!v || 'Task type is required']"></v-select>

                                <v-select :items="teamDevelopers" item-title="username" item-value="id"
                                    label="Assign Developer (Optional)" v-model="taskFormData.developerId"
                                    clearable></v-select>

                                <v-select :items="availableSprints" item-title="name" item-value="id"
                                    label="Assign to Sprint (Optional)" v-model="taskFormData.sprintId"
                                    clearable></v-select>
                            </v-form>
                        </v-card-text>

                        <v-divider></v-divider>

                        <v-card-actions class="bg-surface-light">
                            <v-btn text="Cancel" @click="createTaskDialog = false"></v-btn>
                            <v-spacer></v-spacer>
                            <v-btn text="Create" color="primary" @click="createTask"></v-btn>
                        </v-card-actions>
                    </v-card>
                </v-dialog>

                <v-dialog v-model="editTaskDialog" max-width="600">
                    <v-card title="Edit Task">
                        <v-card-text>
                            <v-form ref="editTaskForm">
                                <v-text-field v-model="editTaskFormData.name" label="Task Name*" required
                                    :rules="[v => !!v || 'Task name is required']"></v-text-field>

                                <v-textarea v-model="editTaskFormData.description" label="Description"
                                    rows="3"></v-textarea>

                                <v-select :items="taskTypes" item-title="name" item-value="id" label="Task Type*"
                                    v-model="editTaskFormData.taskTypeId" required
                                    :rules="[v => !!v || 'Task type is required']"></v-select>

                                <v-select :items="teamDevelopers" item-title="username" item-value="id"
                                    label="Assign Developer (Optional)" v-model="editTaskFormData.developerId"
                                    clearable></v-select>

                                <v-select :items="availableSprints" item-title="name" item-value="id"
                                    label="Assign to Sprint (Optional)" v-model="editTaskFormData.sprintId"
                                    clearable></v-select>
                            </v-form>
                        </v-card-text>

                        <v-divider></v-divider>

                        <v-card-actions class="bg-surface-light">
                            <v-btn text="Delete" color="error" @click="confirmDeleteTask"></v-btn>
                            <v-spacer></v-spacer>
                            <v-btn text="Cancel" @click="editTaskDialog = false"></v-btn>
                            <v-btn text="Save" color="primary" @click="updateTask"></v-btn>
                        </v-card-actions>
                    </v-card>
                </v-dialog>

                <v-dialog v-model="deleteTaskDialog" max-width="400">
                    <v-card>
                        <v-card-title class="text-h6">Confirm Deletion</v-card-title>
                        <v-card-text>
                            Are you sure you want to delete this task?
                            <div class="mt-3 font-weight-bold">{{ taskToDelete?.name }}</div>
                        </v-card-text>

                        <v-divider></v-divider>

                        <v-card-actions class="bg-surface-light">
                            <v-btn text="Cancel" @click="deleteTaskDialog = false"></v-btn>
                            <v-spacer></v-spacer>
                            <v-btn text="Delete" color="error" @click="deleteTask"></v-btn>
                        </v-card-actions>
                    </v-card>
                </v-dialog>
            </v-container>
        </v-main>
    </v-layout>
</template>

<script setup lang="ts">
import { ref, watch, onMounted } from 'vue'
import { useRoute, useRouter } from 'vue-router'
import SprintWithDevPanel from '@/components/manager/SprintWithDevPanel.vue'
import SprintWithoutDevPanel from '@/components/manager/SprintWithoutDevPanel.vue'
import ProjectTasksPanel from '@/components/manager/ProjectTasksPanel.vue'
import tasksService from '@/services/tasksService'
import projectsService from '@/services/projectsService'
import sprintsService from '@/services/sprintsService'
import authService from '@/services/authService'
import usersService from '@/services/usersService'
import { extractErrorMessage } from '@/utils/errorHandler'
import type { Task, CreateTask, User, TaskType, Sprint } from '@/types'
import { DevelopmentLogger } from '@/utils/logger'

const route = useRoute();
const router = useRouter();

const activeTab = ref(route.query.tab as string || 'sprint-with-dev');
const viewMode = ref('tabs');

const sprintWithDevTasks = ref<Task[]>([]);
const sprintWithoutDevTasks = ref<Task[]>([]);
const projectTasks = ref<Task[]>([]);

const sprintWithDevLoading = ref(false);
const sprintWithoutDevLoading = ref(false);
const projectTasksLoading = ref(false);
const error = ref('');
const showError = ref(false);
const successMessage = ref('');
const showSuccess = ref(false);

const projectTasksPanel = ref<InstanceType<typeof ProjectTasksPanel> | null>(null);
const projectTasksPanelCards = ref<InstanceType<typeof ProjectTasksPanel> | null>(null);

const projectId = ref<string | null>(null);
const noProjectAssigned = ref(false);
const currentSprintId = ref<string | null>(null);
const noSprintAssigned = ref(false);

const logger = new DevelopmentLogger({ prefix: '[ManagerTasksView]' });

const createTaskDialog = ref(false);
const editTaskDialog = ref(false);
const deleteTaskDialog = ref(false);
const taskFormData = ref<CreateTask>({
    name: '',
    description: '',
    taskTypeId: null,
    developerId: null,
    sprintId: null
});
const editTaskFormData = ref<any>({
    id: '',
    name: '',
    description: '',
    taskTypeId: null,
    developerId: null,
    sprintId: null
});
const taskToDelete = ref<Task | null>(null);
const taskForm = ref<any>(null);
const editTaskForm = ref<any>(null);

const taskTypes = ref<TaskType[]>([]);
const teamDevelopers = ref<User[]>([]);
const availableSprints = ref<Sprint[]>([]);

const loadSprintWithDevTasks = async () => {
    logger.log("No sprint assigned:", noSprintAssigned.value);
    logger.log("Current sprint ID:", currentSprintId.value);
    logger.log("Sprint with dev tasks:", sprintWithDevTasks.value);
    if (noSprintAssigned.value) return;
    if (!currentSprintId.value) return;

    sprintWithDevLoading.value = true;
    try {
        console.log('Loading sprint with dev tasks...');
        sprintWithDevTasks.value = await tasksService.getAssignedTasksBySprint(currentSprintId.value);
        error.value = '';
    } catch (err) {
        const errorDetails = extractErrorMessage(err);
        error.value = errorDetails.message;
        showError.value = true;
        console.error('Error loading sprint with dev tasks:', err);
    } finally {
        sprintWithDevLoading.value = false;
    }
}

const loadSprintWithoutDevTasks = async () => {
    if (noSprintAssigned.value) return;
    if (!currentSprintId.value) return;

    sprintWithoutDevLoading.value = true;
    try {
        console.log('Loading sprint without dev tasks...');
        sprintWithoutDevTasks.value = await tasksService.getUnassignedTasksBySprint(currentSprintId.value);
        error.value = '';
    } catch (err) {
        const errorDetails = extractErrorMessage(err);
        error.value = errorDetails.message;
        showError.value = true;
        console.error('Error loading sprint without dev tasks:', err);
    } finally {
        sprintWithoutDevLoading.value = false;
    }
}

const loadProjectTasks = async () => {
    if (!projectId.value) return;

    projectTasksLoading.value = true;
    try {
        console.log('Loading project tasks...');
        projectTasks.value = await tasksService.getUnassignedTasksByProject(projectId.value);
        error.value = '';
    } catch (err) {
        const errorDetails = extractErrorMessage(err);
        error.value = errorDetails.message;
        showError.value = true;
        console.error('Error loading project tasks:', err);
    } finally {
        projectTasksLoading.value = false;
    }
}


watch(activeTab, async (newTab) => {
    router.replace({ query: { ...route.query, tab: newTab } });
    switch (newTab) {
        case 'sprint-with-dev':
            await loadSprintWithDevTasks();
            break;
        case 'sprint-no-dev':
            await loadSprintWithoutDevTasks();
            break;
        case 'project-no-sprint':
            await loadProjectTasks();
            break;
    }
})

watch(viewMode, async (newMode) => {
    if (newMode === 'cards') {
        await Promise.all([
            loadSprintWithDevTasks(),
            loadSprintWithoutDevTasks(),
            loadProjectTasks()
        ]);
    }
})


const openCreateTaskDialog = async () => {
    taskFormData.value = {
        name: '',
        description: '',
        taskTypeId: null,
        developerId: null,
        sprintId: null
    };

    createTaskDialog.value = true;
};

const createTask = async () => {
    if (!taskForm.value) return;

    const { valid } = await taskForm.value.validate();
    if (!valid) return;

    try {
        const currentUser = authService.getStoredUser();
        if (!currentUser) {
            error.value = 'User not authenticated';
            showError.value = true;
            return;
        }

        await tasksService.createTask(taskFormData.value, currentUser.id);
        logger.log('Task created successfully');
        createTaskDialog.value = false;

        await refreshAllTasks();
        error.value = '';
        successMessage.value = 'Task created successfully!';
        showSuccess.value = true;
    } catch (err) {
        const errorDetails = extractErrorMessage(err);
        error.value = errorDetails.message;
        showError.value = true;
        logger.error('Failed to create task:', err);
    }
};

const openEditTaskDialog = (task: Task) => {
    editTaskFormData.value = {
        id: task.id,
        name: task.name,
        description: task.description,
        taskTypeId: task.taskTypeId,
        developerId: task.developerId,
        sprintId: task.sprintId
    };

    editTaskDialog.value = true;
};

const updateTask = async () => {
    if (!editTaskForm.value) return;

    const { valid } = await editTaskForm.value.validate();
    if (!valid) return;

    try {
        const { id, ...updateData } = editTaskFormData.value;
        await tasksService.updateTask(id, updateData);
        logger.log('Task updated successfully');
        editTaskDialog.value = false;

        await refreshAllTasks();
        error.value = '';
        successMessage.value = 'Task updated successfully!';
        showSuccess.value = true;
    } catch (err) {
        const errorDetails = extractErrorMessage(err);
        error.value = errorDetails.message;
        showError.value = true;
        logger.error('Failed to update task:', err);
    }
};

const confirmDeleteTask = () => {
    taskToDelete.value = {
        id: editTaskFormData.value.id,
        name: editTaskFormData.value.name,
    } as Task;
    editTaskDialog.value = false;
    deleteTaskDialog.value = true;
};

const deleteTask = async () => {
    if (!taskToDelete.value) return;

    try {
        await tasksService.deleteTask(taskToDelete.value.id);
        logger.log('Task deleted successfully');
        deleteTaskDialog.value = false;
        taskToDelete.value = null;

        await refreshAllTasks();
        error.value = '';
        successMessage.value = 'Task deleted successfully!';
        showSuccess.value = true;
    } catch (err) {
        const errorDetails = extractErrorMessage(err);
        error.value = errorDetails.message;
        showError.value = true;
        logger.error('Failed to delete task:', err);
    }
};

const refreshAllTasks = async () => {
    sprintWithDevTasks.value = [];
    sprintWithoutDevTasks.value = [];
    projectTasks.value = [];

    await Promise.all([
        loadSprintWithDevTasks(),
        loadSprintWithoutDevTasks(),
        loadProjectTasks()
    ]);
};

onMounted(async () => {
    if (route.query.tab && typeof route.query.tab === 'string') {
        activeTab.value = route.query.tab;
    }

    try {
        const currentUser = authService.getStoredUser();

        try {
            const taskTypesService = await import('@/services/taskTypesService');
            taskTypes.value = await taskTypesService.default.getTaskTypes();
        } catch (err) {
            const errorDetails = extractErrorMessage(err);
            error.value = errorDetails.message;
            showError.value = true;
            logger.error('Error loading task types:', err);
        }

        try {
            const team = await import('@/services/teamsService');
            const teamData = await team.default.getTeamByManager(currentUser!.id);
            logger.log('Team data loaded:', teamData);
            if (teamData && teamData.id) {
                teamDevelopers.value = await usersService.getDevelopersByTeam(teamData.id);
                logger.log('Team developers loaded:', teamDevelopers.value);
            } else {
                logger.log('No team found for manager');
            }
        } catch (err) {
            const errorDetails = extractErrorMessage(err);
            error.value = errorDetails.message;
            showError.value = true;
            logger.error('Error loading team developers:', err);
        }

        try {
            availableSprints.value = await sprintsService.getByManager(currentUser!.id);
        } catch (err) {
            const errorDetails = extractErrorMessage(err);
            error.value = errorDetails.message;
            showError.value = true;
            logger.error('Error loading sprints:', err);
        }

        const currentProject = await projectsService.getCurrentProjectByManagerId(currentUser!.id);

        if (!currentProject) {
            noProjectAssigned.value = true;
        } else {
            projectId.value = currentProject;
        }
        try {
            const sprintId = await sprintsService.getByManagerLast(currentUser!.id);
            if (!sprintId) {
                noSprintAssigned.value = true;
            } else {
                currentSprintId.value = sprintId;
                noSprintAssigned.value = false;
            }
        } catch (err) {
            const errorDetails = extractErrorMessage(err);
            error.value = errorDetails.message;
            showError.value = true;
            console.error('Error loading sprints for manager:', err);
            noSprintAssigned.value = true;
        }
    } catch (err) {
        const errorDetails = extractErrorMessage(err);
        error.value = errorDetails.message;
        showError.value = true;
        console.error('Error loading current project:', err);
        noProjectAssigned.value = true;
    }

    if (!noSprintAssigned.value) {
        await loadSprintWithDevTasks();
    }
})
</script>

<style scoped></style>
