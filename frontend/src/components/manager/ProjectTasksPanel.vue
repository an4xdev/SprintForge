<template>
    <v-card-text class="pt-6">
        <v-row v-if="activeLoading" class="justify-center">
            <v-col cols="12" md="4">
                <div class="text-h6 mb-3 text-orange-darken-3">To Do</div>
                <v-skeleton-loader type="card" class="mb-3" height="120"></v-skeleton-loader>
            </v-col>
            <v-col cols="12" md="4">
                <div class="text-h6 mb-3 text-primary-darken-2">In Progress</div>
                <v-skeleton-loader type="card" class="mb-3" height="120"></v-skeleton-loader>
            </v-col>
            <v-col cols="12" md="4">
                <div class="text-h6 mb-3 text-success-darken-2">Done</div>
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
                <div class="text-h6 mb-3 text-orange-darken-1 font-weight-bold">To Do</div>
                <v-card v-for="task in todoTasks" :key="task.id" variant="outlined" class="mb-3"
                    color="orange-lighten-4" elevation="2" @click="$emit('task-click', task)" style="cursor: pointer;">
                    <v-card-text>
                        <div class="font-weight-bold text-high-emphasis text-body-1">{{ task.name }}</div>
                        <div class="text-body-2 text-medium-emphasis mt-1">
                            {{ task.developerId ? `Assigned: Developer ${task.developerId}` : 'No developer assigned' }}
                        </div>
                        <div class="text-body-2 text-medium-emphasis">{{ task.description || 'No description' }}</div>
                        <v-chip size="small" color="orange-darken-1" class="mt-2 font-weight-bold">To Do</v-chip>
                    </v-card-text>
                </v-card>
                <div v-if="todoTasks.length === 0" class="text-center text-medium-emphasis">
                    No tasks in To Do
                </div>
            </v-col>
            <v-col cols="12" md="4">
                <div class="text-h6 mb-3 text-primary-darken-1 font-weight-bold">In Progress</div>
                <v-card v-for="task in inProgressTasks" :key="task.id" variant="outlined" class="mb-3"
                    color="blue-lighten-4" elevation="2" @click="$emit('task-click', task)" style="cursor: pointer;">
                    <v-card-text>
                        <div class="font-weight-bold text-high-emphasis text-body-1">{{ task.name }}</div>
                        <div class="text-body-2 text-medium-emphasis mt-1">
                            {{ task.developerId ? `Assigned: Developer ${task.developerId}` : 'No developer assigned' }}
                        </div>
                        <div class="text-body-2 text-medium-emphasis">{{ task.description || 'No description' }}</div>
                        <v-chip size="small" color="blue-darken-1" class="mt-2 font-weight-bold">In Progress</v-chip>
                    </v-card-text>
                </v-card>
                <div v-if="inProgressTasks.length === 0" class="text-center text-medium-emphasis">
                    No tasks in progress
                </div>
            </v-col>
            <v-col cols="12" md="4">
                <div class="text-h6 mb-3 text-success-darken-2 font-weight-bold">Completed</div>
                <v-card v-for="task in doneTasks" :key="task.id" variant="outlined" class="mb-3" color="green-lighten-4"
                    elevation="2" @click="$emit('task-click', task)" style="cursor: pointer;">
                    <v-card-text>
                        <div class="font-weight-bold text-high-emphasis text-body-1">{{ task.name }}</div>
                        <div class="text-body-2 text-medium-emphasis mt-1">
                            {{ task.developerId ? `Assigned: Developer ${task.developerId}` : 'No developer assigned' }}
                        </div>
                        <div class="text-body-2 text-medium-emphasis">{{ task.description || 'No description' }}</div>
                        <v-chip size="small" color="green-darken-1" class="mt-2 font-weight-bold">Completed</v-chip>
                    </v-card-text>
                </v-card>
                <div v-if="doneTasks.length === 0" class="text-center text-medium-emphasis">
                    No completed tasks
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

const emit = defineEmits<{
    'task-click': [task: Task]
}>();

const localTasks = ref<Task[]>([]);
const localLoading = ref(false);
const error = ref<string | null>(null);

const activeTasks = computed(() => props.tasks.length > 0 ? props.tasks : localTasks.value);
const activeLoading = computed(() => props.loading || localLoading.value);

const todoTasks = computed(() => {
    return activeTasks.value.filter(task => task.taskStatusId === 1 || task.taskStatusId === 2);
});

const inProgressTasks = computed(() => {
    return activeTasks.value.filter(task => task.taskStatusId === 3 || task.taskStatusId === 4 || task.taskStatusId === 5);
});

const doneTasks = computed(() => {
    return activeTasks.value.filter(task => task.taskStatusId === 6);
});


const fetchData = async () => {
    if (!props.projectId) {
        return;
    }

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
