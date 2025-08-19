<template>
    <v-card-text class="pt-6">
        <v-row v-if="activeLoading" class="justify-center">
            <v-col cols="12" md="4">
                <div class="text-h6 mb-3 text-medium-emphasis">Backlog</div>
                <v-skeleton-loader type="card" class="mb-3" height="120"></v-skeleton-loader>
            </v-col>
            <v-col cols="12" md="4">
                <div class="text-h6 mb-3 text-info-darken-2">Future Features</div>
                <v-skeleton-loader type="card" class="mb-3" height="120"></v-skeleton-loader>
            </v-col>
            <v-col cols="12" md="4">
                <div class="text-h6 mb-3 text-brown-darken-2">Research Tasks</div>
                <v-skeleton-loader type="card" class="mb-3" height="120"></v-skeleton-loader>
            </v-col>
        </v-row>

        <v-row v-else-if="error" class="justify-center">
            <v-col cols="12">
                <v-alert type="error" :text="error"></v-alert>
            </v-col>
        </v-row>

        <v-row v-else>
            <v-col cols="12" md="4">
                <div class="text-h6 mb-3 text-medium-emphasis font-weight-bold">Backlog</div>
                <v-card v-for="task in backlogTasks" :key="task.id" variant="outlined" class="mb-3"
                    color="grey-lighten-4" elevation="2">
                    <v-card-text>
                        <div class="font-weight-bold text-high-emphasis text-body-1">{{ task.name }}</div>
                        <div class="text-body-2 text-medium-emphasis mt-1">
                            {{ task.developerId ? `Assigned: Developer ${task.developerId}` : 'No developer assigned' }}
                        </div>
                        <div class="text-body-2 text-medium-emphasis">{{ task.description || 'No description' }}</div>
                        <v-chip size="small" color="grey-darken-2" class="mt-2 font-weight-bold">Backlog</v-chip>
                    </v-card-text>
                </v-card>
                <div v-if="backlogTasks.length === 0" class="text-center text-medium-emphasis">
                    No backlog tasks
                </div>
            </v-col>
            <v-col cols="12" md="4">
                <div class="text-h6 mb-3 text-info-darken-2 font-weight-bold">Future Features</div>
                <v-card v-for="task in futureFeatureTasks" :key="task.id" variant="outlined" class="mb-3"
                    color="info-lighten-4" elevation="2">
                    <v-card-text>
                        <div class="font-weight-bold text-high-emphasis text-body-1">{{ task.name }}</div>
                        <div class="text-body-2 text-medium-emphasis mt-1">
                            {{ task.developerId ? `Assigned: Developer ${task.developerId}` : 'No developer assigned' }}
                        </div>
                        <div class="text-body-2 text-medium-emphasis">{{ task.description || 'No description' }}</div>
                        <v-chip size="small" color="info-darken-1" class="mt-2 font-weight-bold">Future Sprint</v-chip>
                    </v-card-text>
                </v-card>
                <div v-if="futureFeatureTasks.length === 0" class="text-center text-medium-emphasis">
                    No future feature tasks
                </div>
            </v-col>
            <v-col cols="12" md="4">
                <div class="text-h6 mb-3 text-brown-darken-2 font-weight-bold">Research Tasks</div>
                <v-card v-for="task in researchTasks" :key="task.id" variant="outlined" class="mb-3"
                    color="brown-lighten-4" elevation="2">
                    <v-card-text>
                        <div class="font-weight-bold text-high-emphasis text-body-1">{{ task.name }}</div>
                        <div class="text-body-2 text-medium-emphasis mt-1">
                            {{ task.developerId ? `Assigned: Developer ${task.developerId}` : 'No developer assigned' }}
                        </div>
                        <div class="text-body-2 text-medium-emphasis">{{ task.description || 'No description' }}</div>
                        <v-chip size="small" color="brown-darken-2" class="mt-2 font-weight-bold">Research</v-chip>
                    </v-card-text>
                </v-card>
                <div v-if="researchTasks.length === 0" class="text-center text-medium-emphasis">
                    No research tasks
                </div>
            </v-col>
        </v-row>
    </v-card-text>
</template>

<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import tasksService from '@/services/tasksService'
import type { Task, TaskType } from '@/types'

interface Props {
    tasks?: Task[],
    loading?: boolean,
    projectId: string
};

const props = withDefaults(defineProps<Props>(), {
    tasks: () => [],
    loading: false,
    projectId: ''
});

const localTasks = ref<Task[]>([]);
const localLoading = ref(false);
const error = ref<string | null>(null);

const activeTasks = computed(() => props.tasks.length > 0 ? props.tasks : localTasks.value);
const activeLoading = computed(() => props.loading || localLoading.value);

const backlogTasks = computed(() => {
    return activeTasks.value.filter((_, index) => index % 3 === 0)
});

const futureFeatureTasks = computed(() => {
    return activeTasks.value.filter((_, index) => index % 3 === 1)
});

const researchTasks = computed(() => {
    return activeTasks.value.filter((_, index) => index % 3 === 2)
});


const fetchData = async () => {
    try {
        localLoading.value = true;
        error.value = null;
        const tasksData = await tasksService.getUnassignedTasksByProject(props.projectId);
        localTasks.value = tasksData;
    } catch (err) {
        error.value = 'Failed to load data';
        console.error('Error fetching data:', err);
    } finally {
        localLoading.value = false;
    }
}

onMounted(() => {
    fetchData()
});

defineExpose({
    refresh: fetchData
});
</script>

<style scoped></style>
