<template>
    <div>
        <h1 class="text-h4 mb-6">Developer Panel</h1>

        <v-card class="mb-6" elevation="4" color="primary" variant="tonal">
            <v-card-title class="text-h5 d-flex align-center">
                <v-icon class="mr-2">{{ currentTask ? 'mdi-star' : 'mdi-star-outline' }}</v-icon>
                Current Task
            </v-card-title>
            <v-card-text v-if="currentTask">
                <div class="text-h6 font-weight-bold mb-2">{{ currentTask.name }}</div>
                <div class="text-body-1 mb-2">{{ currentTask.description }}</div>
                <div class="text-caption text-medium-emphasis mb-4">Project: {{ currentTask.project }}</div>

                <div class="d-flex align-center mb-4">
                    <span class="text-medium-emphasis mr-4">Work time:</span>
                    <span class="text-h5 font-mono text-primary">{{ formatTime(currentTask) }}</span>
                </div>

                <div class="d-flex gap-2">
                    <v-btn v-if="['NONE', 'STOPPED'].includes(currentTask.status)" color="success"
                        @click="startTask(currentTask)" prepend-icon="mdi-play">
                        Start
                    </v-btn>
                    <v-btn v-if="currentTask.status === 'STARTED'" color="warning" @click="pauseTask(currentTask)"
                        prepend-icon="mdi-pause">
                        Pause
                    </v-btn>
                    <v-btn v-if="currentTask.status === 'PAUSED'" color="primary" @click="resumeTask(currentTask)"
                        prepend-icon="mdi-play">
                        Resume
                    </v-btn>
                    <v-btn v-if="['STARTED', 'PAUSED'].includes(currentTask.status)" color="error"
                        @click="stopTask(currentTask)" prepend-icon="mdi-stop">
                        Stop
                    </v-btn>
                </div>
            </v-card-text>
            <v-card-text v-else class="text-center py-8">
                <v-icon size="64" color="grey-lighten-2" class="mb-4">mdi-clipboard-check-outline</v-icon>
                <div class="text-h6 mb-2">No active task</div>
                <div class="text-body-2 text-medium-emphasis">Select a task from below to start working</div>
            </v-card-text>
        </v-card>

        <v-card>
            <v-card-title class="text-h5">My Tasks</v-card-title>
            <v-card-text>
                <v-row v-if="tasks.length > 0">
                    <v-col cols="12" md="6" lg="4" v-for="task in tasks" :key="task.id">
                        <v-card :elevation="task.status === 'STARTED' ? 8 : 2" class="task-card position-relative"
                            :class="{ 'task-active': task.status === 'STARTED' }">
                            <div class="status-bar-left" :class="getStatusBarClass(task)"></div>

                            <v-card-title class="text-h6 d-flex align-center">
                                <v-icon :color="getTaskStatusIcon(task).color" class="mr-2">
                                    {{ getTaskStatusIcon(task).icon }}
                                </v-icon>
                                {{ task.name }}
                            </v-card-title>
                            <v-card-text>
                                <div class="text-body-2 mb-3">{{ task.description }}</div>
                                <div class="text-caption text-medium-emphasis mb-3">Project: {{ task.project }}</div>

                                <div class="d-flex align-center mb-3">
                                    <v-chip :color="getStatusChipColor(task.status)" size="small" class="mr-2">
                                        {{ task.status }}
                                    </v-chip>
                                    <span class="text-body-2 font-mono">{{ formatTime(task) }}</span>
                                </div>

                                <div class="d-flex gap-2 flex-wrap">
                                    <v-btn v-if="['NONE', 'STOPPED'].includes(task.status)" color="success" size="small"
                                        @click="startTask(task)" prepend-icon="mdi-play">
                                        Start
                                    </v-btn>
                                    <v-btn v-if="task.status === 'STARTED'" color="warning" size="small"
                                        @click="pauseTask(task)" prepend-icon="mdi-pause">
                                        Pause
                                    </v-btn>
                                    <v-btn v-if="task.status === 'PAUSED'" color="primary" size="small"
                                        @click="resumeTask(task)" prepend-icon="mdi-play">
                                        Resume
                                    </v-btn>
                                    <v-btn v-if="['STARTED', 'PAUSED'].includes(task.status)" color="error" size="small"
                                        @click="stopTask(task)" prepend-icon="mdi-stop">
                                        Stop
                                    </v-btn>
                                </div>
                            </v-card-text>
                        </v-card>
                    </v-col>
                </v-row>
                <div v-else class="text-center py-8">
                    <v-icon size="64" color="grey-lighten-1" class="mb-4">mdi-clipboard-text-outline</v-icon>
                    <div class="text-h6 mb-2">No tasks assigned</div>
                    <div class="text-body-2 text-medium-emphasis">Contact your manager to get tasks assigned</div>
                </div>
            </v-card-text>
        </v-card>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, onUnmounted } from 'vue'
import type { DeveloperTask, DeveloperTaskStatus } from '@/types'
import tasksService from '@/services/tasksService'
import { useAuthStore } from '@/stores/auth'
import { DevelopmentLogger } from '@/utils/logger';

const logger = new DevelopmentLogger({ prefix: '[DeveloperDashboard]' });

const authStore = useAuthStore();

const tasks = ref<DeveloperTask[]>([]);
const loading = ref(false);
let timerIntervals: Map<string, number> = new Map();

onMounted(async () => {
    await loadTasks();

    tasks.value.forEach(task => {
        if (task.status === 'STARTED') {
            startTimer(task);
        }
    })
})

const loadTasks = async () => {
    if (!authStore.user?.id) return;

    loading.value = true;
    try {
        const userTasks = await tasksService.getTasksByDeveloper(authStore.user.id)
        const taskTimes = await tasksService.getDeveloperTaskTimes(authStore.user.id)

        tasks.value = userTasks.map(task => {
            const timeData = taskTimes.find(t => t.taskId === task.id);
            const timeInfo = timeData ? tasksService.formatSecondsToTime(timeData.totalSeconds) : { hours: 0, minutes: 0, seconds: 0 };

            const developerTask: DeveloperTask = {
                ...tasksService.mapTaskToDeveloperTask(task, 'Project Name'), // TODO: get actual project name
                ...timeInfo
            };

            if (timeData) {
                if (timeData.isRunning) {
                    developerTask.status = 'STARTED';
                } else if (timeData.currentStatus === 'Paused') {
                    developerTask.status = 'PAUSED';
                } else if (timeData.currentStatus === 'Stopped') {
                    developerTask.status = 'STOPPED';
                } else {
                    developerTask.status = 'NONE';
                }
            }

            return developerTask;
        });
    } catch (error) {
        logger.error('Error loading tasks:', error);
    } finally {
        loading.value = false;
    }
}

const currentTask = computed(() => {
    return tasks.value.find(task => ['STARTED', 'PAUSED'].includes(task.status)) || null;
})


const formatTime = (task: DeveloperTask): string => {
    const hours = task.hours.toString().padStart(2, '0');
    const minutes = task.minutes.toString().padStart(2, '0');
    const seconds = task.seconds.toString().padStart(2, '0');
    return `${hours}:${minutes}:${seconds}`;
}

const getStatusBarClass = (task: DeveloperTask): string => {
    switch (task.status) {
        case 'STARTED': return 'status-bar--started';
        case 'PAUSED': return 'status-bar--paused';
        case 'STOPPED': return 'status-bar--stopped';
        default: return 'status-bar--none';
    }
}

const getTaskStatusIcon = (task: DeveloperTask) => {
    switch (task.status) {
        case 'STARTED':
            return { icon: 'mdi-play-circle', color: 'success' };
        case 'PAUSED':
            return { icon: 'mdi-pause-circle', color: 'warning' };
        case 'STOPPED':
            return { icon: 'mdi-check-circle', color: 'success' };
        default:
            return { icon: 'mdi-circle-outline', color: 'grey' };
    }
}

const getStatusChipColor = (status: DeveloperTaskStatus): string => {
    switch (status) {
        case 'STARTED': return 'success';
        case 'PAUSED': return 'warning';
        case 'STOPPED': return 'primary';
        default: return 'grey';
    }
}

const startTask = async (task: DeveloperTask) => {
    try {
        const runningTask = tasks.value.find(t => t.status === 'STARTED' && t.id !== task.id);
        if (runningTask) {
            await stopTask(runningTask);
        }

        await tasksService.startTask(task.id);

        task.status = 'STARTED';
        startTimer(task);


        await syncTaskTimes();

    } catch (error) {
        logger.error('Error starting task:', error);
        task.status = 'NONE';
    }
}

const pauseTask = async (task: DeveloperTask) => {
    try {
        await tasksService.pauseTask(task.id);

        task.status = 'PAUSED';
        const interval = timerIntervals.get(task.id);
        if (interval) {
            clearInterval(interval);
            timerIntervals.delete(task.id);
        }

        await syncTaskTimes();

    } catch (error) {
        logger.error('Error pausing task:', error);
    }
}

const resumeTask = async (task: DeveloperTask) => {
    try {
        await tasksService.startTask(task.id);

        task.status = 'STARTED';
        startTimer(task);

        await syncTaskTimes();

    } catch (error) {
        logger.error('Error resuming task:', error);
    }
}

const stopTask = async (task: DeveloperTask) => {
    try {
        await tasksService.stopTask(task.id);

        task.status = 'STOPPED';
        const interval = timerIntervals.get(task.id);
        if (interval) {
            clearInterval(interval);
            timerIntervals.delete(task.id);
        }

        await syncTaskTimes();

    } catch (error) {
        logger.error('Error stopping task:', error);
    }
}

const syncTaskTimes = async () => {
    if (!authStore.user?.id) return;

    try {
        const taskTimes = await tasksService.getDeveloperTaskTimes(authStore.user.id);

        tasks.value.forEach(task => {
            const timeData = taskTimes.find(t => t.taskId === task.id);
            if (timeData) {
                const timeInfo = tasksService.formatSecondsToTime(timeData.totalSeconds);
                task.hours = timeInfo.hours;
                task.minutes = timeInfo.minutes;
                task.seconds = timeInfo.seconds;

                if (timeData.isRunning) {
                    task.status = 'STARTED';
                } else if (timeData.currentStatus === 'Paused') {
                    task.status = 'PAUSED';
                } else if (timeData.currentStatus === 'Stopped') {
                    task.status = 'STOPPED';
                } else {
                    task.status = 'NONE';
                }
            }
        });
    } catch (error) {
        logger.error('Error syncing task times:', error);
    }
}

const startTimer = (task: DeveloperTask) => {
    const interval = setInterval(async () => {
        task.seconds++;
        if (task.seconds >= 60) {
            task.seconds = 0;
            task.minutes++;
            if (task.minutes >= 60) {
                task.minutes = 0;
                task.hours++;
            }
        }
    }, 1000);

    timerIntervals.set(task.id, interval);
}

let syncInterval: number;

onMounted(async () => {
    await loadTasks();

    tasks.value.forEach(task => {
        if (task.status === 'STARTED') {
            startTimer(task);
        }
    })

    syncInterval = setInterval(syncTaskTimes, 30000);
})

onUnmounted(() => {
    timerIntervals.forEach(interval => clearInterval(interval));
    timerIntervals.clear();
    if (syncInterval) {
        clearInterval(syncInterval);
    }
})
</script>

<style scoped>
.task-card {
    transition: all 0.3s ease;
}

.task-card:hover {
    transform: translateY(-2px);
}

.task-active {
    box-shadow: 0 4px 12px rgba(0, 0, 0, 0.15) !important;
}

.status-bar-left {
    position: absolute;
    top: 0;
    left: 0;
    width: 4px;
    height: 100%;
    border-radius: 4px 0 0 4px;
}

.status-bar--started {
    background-color: #4CAF50;
}

.status-bar--paused {
    background-color: #FF9800;
}

.status-bar--stopped {
    background-color: #2196F3;
}

.status-bar--none {
    background-color: #E0E0E0;
}
</style>
