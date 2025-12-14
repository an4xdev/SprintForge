<template>
    <v-card-text class="pt-6">
        <v-row v-if="loading">
            <v-col cols="12" md="6">
                <div class="text-h6 mb-3 text-error-darken-2">Unassigned Tasks</div>
                <v-skeleton-loader type="card" class="mb-3" height="120"></v-skeleton-loader>
            </v-col>
            <v-col cols="12" md="6">
                <div class="text-h6 mb-3 text-purple-darken-2">Available Developers</div>
                <v-skeleton-loader type="card" class="mb-3" height="120"></v-skeleton-loader>
            </v-col>
        </v-row>

        <v-row v-else>
            <v-col cols="12" md="6">
                <div class="text-h6 mb-3 text-error-darken-2 font-weight-bold">Unassigned Tasks</div>
                <v-card v-for="task in tasks" :key="task.id" variant="outlined" class="mb-3" color="red-lighten-4"
                    elevation="2" @click="$emit('task-click', task)" style="cursor: pointer;">
                    <v-card-text>
                        <div class="font-weight-bold text-high-emphasis text-body-1">{{ task.name }}</div>
                        <div class="text-body-2 text-error-darken-3 mt-1">No developer assigned</div>
                        <div class="text-body-2 text-red-darken-2 font-weight-medium">{{ task.description }}</div>
                        <v-chip size="small" color="red-darken-1" class="mt-2 font-weight-bold">Unassigned</v-chip>
                    </v-card-text>
                </v-card>
                <div v-if="tasks.length === 0" class="text-center text-medium-emphasis">
                    No unassigned tasks in sprint
                </div>
            </v-col>
            <v-col cols="12" md="6">
                <div class="text-h6 mb-3 text-purple-darken-2 font-weight-bold">Available Developers</div>
                <v-list v-if="developers.length > 0" bg-color="transparent">
                    <v-list-item v-for="developer in developers" :key="developer.id" class="mb-2" rounded="lg"
                        variant="tonal">
                        <template v-slot:prepend>
                            <v-avatar color="green-darken-1">
                                <v-icon color="white">mdi-account</v-icon>
                            </v-avatar>
                        </template>
                        <v-list-item-title class="text-high-emphasis font-weight-medium">
                            {{ developer.firstName }} {{ developer.lastName }}
                        </v-list-item-title>
                        <v-list-item-subtitle class="text-medium-emphasis">
                            {{ developer.username }}
                        </v-list-item-subtitle>
                    </v-list-item>
                </v-list>
                <div v-else class="text-center text-medium-emphasis">
                    No developers in team
                </div>
            </v-col>
        </v-row>
    </v-card-text>
</template>

<script setup lang="ts">
import type { Task, User } from '@/types'

interface Props {
    loading?: boolean
    tasks?: Task[]
    developers?: User[]
}

const props = withDefaults(defineProps<Props>(), {
    loading: false,
    tasks: () => [],
    developers: () => []
});

const emit = defineEmits<{
    'task-click': [task: Task]
}>();
</script>

<style scoped></style>
