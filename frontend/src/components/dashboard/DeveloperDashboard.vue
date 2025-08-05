<template>
    <div>
        <h1 class="text-h4 mb-6">Developer Panel</h1>

        <v-card class="mb-6" v-if="currentTask">
            <v-card-title class="text-h5">Current Task</v-card-title>
            <v-card-text>
                <div class="text-h6 font-weight-bold mb-2">{{ currentTask.title }}</div>
                <div class="text-body-1 mb-4">{{ currentTask.description }}</div>
                <div class="text-caption text-medium-emphasis mb-4">Project: {{ currentTask.project }}</div>

                <div class="d-flex align-center mb-4">
                    <span class="text-medium-emphasis mr-4">Work time:</span>
                    <span class="text-h5 font-mono text-primary">{{ timeDisplay }}</span>
                </div>

                <div class="d-flex gap-2">
                    <v-btn v-if="!isTimerRunning && !isPaused" color="success" @click="startTimer"
                        prepend-icon="mdi-play">
                        Start
                    </v-btn>
                    <v-btn v-if="isTimerRunning" color="warning" @click="pauseTimer" prepend-icon="mdi-pause">
                        Pause
                    </v-btn>
                    <v-btn v-if="isPaused" color="primary" @click="resumeTimer" prepend-icon="mdi-play">
                        Resume
                    </v-btn>
                    <v-btn v-if="isTimerRunning || isPaused" color="error" @click="stopTimer" prepend-icon="mdi-stop">
                        Stop
                    </v-btn>
                </div>
            </v-card-text>
        </v-card>

        <v-card class="mb-6" v-else>
            <v-card-text class="text-center py-8">
                <v-icon size="64" color="primary" class="mb-4">mdi-clipboard-check-outline</v-icon>
                <div class="text-h6 mb-2">No active task</div>
                <div class="text-body-2 text-medium-emphasis">Go to "All Tasks" section to select a task to work on
                </div>
            </v-card-text>
        </v-card>

        <v-row class="mb-6">
            <v-col cols="12" md="4">
                <v-card class="text-center">
                    <v-card-text>
                        <v-icon size="48" color="warning" class="mb-2">mdi-clock-outline</v-icon>
                        <div class="text-h4 font-weight-bold">24h 30m</div>
                        <div class="text-caption">Time this week</div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" md="4">
                <v-card class="text-center">
                    <v-card-text>
                        <v-icon size="48" color="success" class="mb-2">mdi-check-circle</v-icon>
                        <div class="text-h4 font-weight-bold">8</div>
                        <div class="text-caption">Completed tasks</div>
                    </v-card-text>
                </v-card>
            </v-col>
            <v-col cols="12" md="4">
                <v-card class="text-center">
                    <v-card-text>
                        <v-icon size="48" color="primary" class="mb-2">mdi-code-tags</v-icon>
                        <div class="text-h4 font-weight-bold">3</div>
                        <div class="text-caption">Tasks in progress</div>
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row>

        <v-card>
            <v-card-title class="text-h5">Recent Activities</v-card-title>
            <v-card-text>
                <v-list>
                    <v-list-item>
                        <template v-slot:prepend>
                            <v-icon color="success">mdi-check-circle</v-icon>
                        </template>
                        <v-list-item-title>Completed task "Database design"</v-list-item-title>
                        <v-list-item-subtitle>Today, 2:30 PM</v-list-item-subtitle>
                    </v-list-item>
                    <v-list-item>
                        <template v-slot:prepend>
                            <v-icon color="primary">mdi-play</v-icon>
                        </template>
                        <v-list-item-title>Started work on "OAuth login implementation"</v-list-item-title>
                        <v-list-item-subtitle>Today, 9:15 AM</v-list-item-subtitle>
                    </v-list-item>
                    <v-list-item>
                        <template v-slot:prepend>
                            <v-icon color="info">mdi-comment</v-icon>
                        </template>
                        <v-list-item-title>Added comment to task "API endpoints"</v-list-item-title>
                        <v-list-item-subtitle>Yesterday, 4:45 PM</v-list-item-subtitle>
                    </v-list-item>
                </v-list>
            </v-card-text>
        </v-card>
    </div>
</template>

<script setup lang="ts">
import { ref, computed, onUnmounted } from 'vue'

const currentTask = ref({
    title: 'OAuth Login Implementation',
    description: 'Need to implement Google and Facebook login in the web application. Requires API integration and proper token handling.',
    project: 'Task Management System'
})

const seconds = ref(0)
const isTimerRunning = ref(false)
const isPaused = ref(false)
let timerInterval: number | null = null

const timeDisplay = computed(() => {
    const hours = Math.floor(seconds.value / 3600).toString().padStart(2, '0')
    const minutes = Math.floor((seconds.value % 3600) / 60).toString().padStart(2, '0')
    const secs = (seconds.value % 60).toString().padStart(2, '0')
    return `${hours}:${minutes}:${secs}`
})

const startTimer = () => {
    isTimerRunning.value = true
    isPaused.value = false
    timerInterval = setInterval(() => {
        seconds.value++
    }, 1000)
}

const pauseTimer = () => {
    isTimerRunning.value = false
    isPaused.value = true
    if (timerInterval) {
        clearInterval(timerInterval)
        timerInterval = null
    }
}

const resumeTimer = () => {
    startTimer()
}

const stopTimer = () => {
    isTimerRunning.value = false
    isPaused.value = false
    if (timerInterval) {
        clearInterval(timerInterval)
        timerInterval = null
    }
    seconds.value = 0
    alert('Task completed. Work time has been saved.')
}

onUnmounted(() => {
    if (timerInterval) {
        clearInterval(timerInterval)
    }
})
</script>
