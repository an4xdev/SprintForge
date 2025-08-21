<template>
    <v-card-text class="pt-6">
        <v-row v-if="loading">
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

        <v-row v-else>
            <v-col cols="12" md="4">
                <div class="text-h6 mb-3 text-orange-darken-1 font-weight-bold">To Do</div>
                <v-card v-for="task in todoTasks" :key="task.id" variant="outlined" class="mb-3"
                    color="orange-lighten-4" elevation="2">
                    <v-card-text>
                        <div class="font-weight-bold text-high-emphasis text-body-1">{{ task.name }}</div>
                        <div class="text-body-2 text-medium-emphasis mt-1">{{ task.description }}</div>
                        <div class="text-body-2 text-orange-darken-2 font-weight-medium">Task ID: {{ task.id }}</div>
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
                    color="blue-lighten-4" elevation="2">
                    <v-card-text>
                        <div class="font-weight-bold text-high-emphasis text-body-1">{{ task.name }}</div>
                        <div class="text-body-2 text-medium-emphasis mt-1">{{ task.description }}</div>
                        <div class="text-body-2 text-blue-darken-2 font-weight-medium">Task ID: {{ task.id }}</div>
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
                    elevation="2">
                    <v-card-text>
                        <div class="font-weight-bold text-high-emphasis text-body-1">{{ task.name }}</div>
                        <div class="text-body-2 text-medium-emphasis mt-1">{{ task.description }}</div>
                        <div class="text-body-2 text-green-darken-2 font-weight-medium">Task ID: {{ task.id }}</div>
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
import { computed } from 'vue'
import type { Task } from '@/types'

interface Props {
    loading?: boolean
    tasks?: Task[]
}

const props = withDefaults(defineProps<Props>(), {
    loading: false,
    tasks: () => []
});

const todoTasks = computed(() =>
    props.tasks.filter(task => task.taskStatusId === 1)
);

const inProgressTasks = computed(() =>
    props.tasks.filter(task => task.taskStatusId === 2)
);

const doneTasks = computed(() =>
    props.tasks.filter(task => task.taskStatusId === 3)
);
</script>

<style scoped></style>
