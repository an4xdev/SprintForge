<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <div class="d-flex justify-space-between align-center mb-6">
                    <h1 class="text-h4">Team Tasks Management</h1>
                    <v-btn-toggle v-model="viewMode" mandatory>
                        <v-btn value="tabs" prepend-icon="mdi-tab">
                            Tabs
                        </v-btn>
                        <v-btn value="cards" prepend-icon="mdi-view-sequential">
                            Cards
                        </v-btn>
                    </v-btn-toggle>
                </div>

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
                            <SprintWithDevPanel :loading="sprintWithDevLoading" :tasks="sprintWithDevTasks" />
                        </v-tabs-window-item>

                        <v-tabs-window-item value="sprint-no-dev">
                            <div v-if="sprintWithoutDevLoading" class="pa-6">
                                <v-progress-linear indeterminate color="primary" />
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
                                <SprintWithoutDevPanel :loading="sprintWithoutDevLoading"
                                    :tasks="sprintWithoutDevTasks" />
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
                        <SprintWithDevPanel :loading="sprintWithDevLoading" :tasks="sprintWithDevTasks" />
                    </v-card>

                    <v-card class="mb-6">
                        <v-card-title class="text-h5 bg-primary text-white">
                            Unassigned Tasks in Sprint
                        </v-card-title>

                        <div v-if="sprintWithoutDevLoading" class="pa-6">
                            <v-progress-linear indeterminate color="primary" />
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
                            <SprintWithoutDevPanel :loading="sprintWithoutDevLoading" :tasks="sprintWithoutDevTasks" />
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
                            <ProjectTasksPanel ref="projectTasksPanelCards" :tasks="projectTasks"
                                :loading="projectTasksLoading" :project-id="projectId || ''" />
                        </div>
                    </v-card>
                </div>
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
import authService from '@/services/authService'
import type { Task } from '@/types'

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

const projectTasksPanel = ref<InstanceType<typeof ProjectTasksPanel> | null>(null);
const projectTasksPanelCards = ref<InstanceType<typeof ProjectTasksPanel> | null>(null);

const projectId = ref<string | null>(null);
const noProjectAssigned = ref(false);
const tempSprintId = '6407002c-e35a-4b18-a5a1-8a886d0c3b78';

const loadSprintWithDevTasks = async () => {
    if (sprintWithDevTasks.value.length > 0) return;

    sprintWithDevLoading.value = true;
    try {
        console.log('Loading sprint with dev tasks...');
        sprintWithDevTasks.value = await tasksService.getAssignedTasksBySprint(tempSprintId);
    } catch (error) {
        console.error('Error loading sprint with dev tasks:', error);
    } finally {
        sprintWithDevLoading.value = false;
    }
}

const loadSprintWithoutDevTasks = async () => {
    if (sprintWithoutDevTasks.value.length > 0) return;

    sprintWithoutDevLoading.value = true;
    try {
        console.log('Loading sprint without dev tasks...');
        sprintWithoutDevTasks.value = await tasksService.getUnassignedTasksBySprint(tempSprintId);
    } catch (error) {
        console.error('Error loading sprint without dev tasks:', error);
    } finally {
        sprintWithoutDevLoading.value = false;
    }
}

const loadProjectTasks = async () => {
    if (projectTasks.value.length > 0 || !projectId.value) return;

    projectTasksLoading.value = true;
    try {
        console.log('Loading project tasks...');
        projectTasks.value = await tasksService.getUnassignedTasksByProject(projectId.value);
    } catch (error) {
        console.error('Error loading project tasks:', error);
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


onMounted(async () => {
    if (route.query.tab && typeof route.query.tab === 'string') {
        activeTab.value = route.query.tab;
    }
    try {
        const currentUser = authService.getStoredUser();

        const currentProject = await projectsService.getCurrentProjectByManagerId(currentUser!.id);
        if (!currentProject || !(currentProject as any).id) {
            noProjectAssigned.value = true;
        } else {
            projectId.value = (currentProject as any).id;
        }
    } catch (error) {
        console.error('Error loading current project:', error);
        noProjectAssigned.value = true;
    }

    await loadSprintWithDevTasks();
})
</script>

<style scoped></style>
