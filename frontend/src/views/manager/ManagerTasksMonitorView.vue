<template>
    <v-layout>
        <v-main class="min-h-screen">
            <v-container fluid class="pa-6">
                <div class="d-flex justify-space-between align-center mb-6">
                    <h1 class="text-h4">Tasks Monitor</h1>
                    <div class="d-flex align-center gap-3">
                        <v-chip
                            :color="connectionStatus === 'Connected' ? 'success' : connectionStatus === 'Connecting' ? 'warning' : 'error'"
                            variant="flat">
                            <v-icon start>
                                {{ connectionStatus === 'Connected' ? 'mdi-wifi' : connectionStatus === 'Connecting' ?
                                    'mdi-wifi-strength-1' : 'mdi-wifi-off' }}
                            </v-icon>
                            {{ connectionStatus }}
                        </v-chip>
                        <v-btn v-if="connectionStatus === 'Disconnected'" @click="connectWebSocket" color="primary"
                            prepend-icon="mdi-refresh" class="ml-3">
                            RECONNECT
                        </v-btn>
                    </div>
                </div>

                <v-alert v-if="error" type="error" variant="tonal" closable @click:close="error = ''" class="mb-4">
                    {{ error }}
                </v-alert>

                <div v-if="tasks.length === 0 && !loading" class="mb-6">
                    <v-sheet class="d-flex flex-column align-center pa-8" elevation="1" rounded>
                        <v-icon size="64" color="primary">mdi-clipboard-list-outline</v-icon>
                        <h3 class="text-h6 mt-4">No tasks available</h3>
                        <p class="text-body-2 text-center mt-2">
                            {{ connectionStatus === 'Connected' ? 'There are no tasks in your current sprint.' :
                                'Connect to see your team tasks.' }}
                        </p>
                    </v-sheet>
                </div>

                <!-- Running Tasks Section -->
                <v-card v-if="tasks.length > 0" class="mb-6" elevation="2">
                    <v-card-title class="d-flex align-center pa-4 bg-success-lighten-4">
                        <v-icon class="me-3" color="success" size="large">mdi-play-circle</v-icon>
                        <span class="text-h6">Running Tasks</span>
                        <v-spacer></v-spacer>
                        <v-chip color="success" variant="flat" size="small">
                            {{ runningTasks.length }}
                        </v-chip>
                    </v-card-title>
                    <v-card-text class="pa-4">
                        <div v-if="runningTasks.length === 0" class="d-flex flex-column align-center pa-6">
                            <v-icon size="48" color="success" class="mb-3">mdi-play-circle-outline</v-icon>
                            <h4 class="text-h6 mb-2">No running tasks</h4>
                            <p class="text-body-2 text-center text-medium-emphasis">All tasks are either paused or
                                stopped.</p>
                        </div>
                        <v-row v-else>
                            <v-col v-for="task in runningTasks" :key="task.id" cols="12" md="6" lg="4">
                                <v-card class="border-success" variant="outlined" elevation="1">
                                    <v-card-title class="d-flex justify-space-between align-center">
                                        <span class="text-truncate">{{ task.name }}</span>
                                        <v-chip color="success" size="small" variant="flat">
                                            <v-icon start size="small">mdi-play</v-icon>
                                            Running
                                        </v-chip>
                                    </v-card-title>
                                    <v-card-text>
                                        <div class="mb-3">
                                            <v-icon size="small" class="me-2">mdi-account</v-icon>
                                            <span class="text-body-2">{{ task.developer_name || 'Unassigned' }}</span>
                                        </div>
                                        <div class="mb-3">
                                            <v-icon size="small" class="me-2">mdi-clock-outline</v-icon>
                                            <span class="text-h6 font-weight-bold">{{
                                                formatDuration(task.total_duration_seconds || 0) }}</span>
                                        </div>
                                        <div class="mb-2">
                                            <v-icon size="small" class="me-2" color="success">mdi-play-circle</v-icon>
                                            <span class="text-body-2 text-success">Currently running...</span>
                                        </div>
                                        <div class="text-caption text-medium-emphasis">
                                            ID: {{ task.id }}
                                        </div>
                                    </v-card-text>
                                    <v-card-actions v-if="task.start_time">
                                        <v-tooltip location="bottom">
                                            <template v-slot:activator="{ props }">
                                                <v-btn v-bind="props" variant="text" size="small"
                                                    prepend-icon="mdi-clock-start">
                                                    Started at {{ formatTime(task.start_time) }}
                                                </v-btn>
                                            </template>
                                            <span>Task start time: {{ formatDateTime(task.start_time) }}</span>
                                        </v-tooltip>
                                    </v-card-actions>
                                </v-card>
                            </v-col>
                        </v-row>
                    </v-card-text>
                </v-card>

                <!-- Paused Tasks Section -->
                <v-card v-if="tasks.length > 0" class="mb-6" elevation="2">
                    <v-card-title class="d-flex align-center pa-4 bg-warning-lighten-4">
                        <v-icon class="me-3" color="warning" size="large">mdi-pause-circle</v-icon>
                        <span class="text-h6">Paused Tasks</span>
                        <v-spacer></v-spacer>
                        <v-chip color="warning" variant="flat" size="small">
                            {{ pausedTasks.length }}
                        </v-chip>
                    </v-card-title>
                    <v-card-text class="pa-4">
                        <div v-if="pausedTasks.length === 0" class="d-flex flex-column align-center pa-6">
                            <v-icon size="48" color="warning" class="mb-3">mdi-pause-circle-outline</v-icon>
                            <h4 class="text-h6 mb-2">No paused tasks</h4>
                            <p class="text-body-2 text-center text-medium-emphasis">No tasks are currently paused.</p>
                        </div>
                        <v-row v-else>
                            <v-col v-for="task in pausedTasks" :key="task.id" cols="12" md="6" lg="4">
                                <v-card class="border-warning" color="amber-lighten-5" variant="outlined" elevation="1">
                                    <v-card-title class="d-flex justify-space-between align-center">
                                        <span class="text-truncate">{{ task.name }}</span>
                                        <v-chip color="warning" size="small" variant="flat">
                                            <v-icon start size="small">mdi-pause</v-icon>
                                            Paused
                                        </v-chip>
                                    </v-card-title>
                                    <v-card-text>
                                        <div class="mb-3">
                                            <v-icon size="small" class="me-2">mdi-account</v-icon>
                                            <span class="text-body-2">{{ task.developer_name || 'Unassigned' }}</span>
                                        </div>
                                        <div class="mb-3">
                                            <v-icon size="small" class="me-2">mdi-clock-outline</v-icon>
                                            <span class="text-h6 font-weight-bold">{{
                                                formatDuration(task.total_duration_seconds || 0)
                                            }}</span>
                                        </div>
                                        <div class="mb-2">
                                            <v-icon size="small" class="me-2" color="warning">mdi-pause-circle</v-icon>
                                            <span class="text-body-2 text-warning">Paused</span>
                                        </div>
                                        <div class="text-caption text-medium-emphasis">
                                            ID: {{ task.id }}
                                        </div>
                                    </v-card-text>
                                    <v-card-actions v-if="task.start_time">
                                        <v-tooltip location="bottom">
                                            <template v-slot:activator="{ props }">
                                                <v-btn v-bind="props" variant="text" size="small"
                                                    prepend-icon="mdi-clock-start">
                                                    Started at {{ formatTime(task.start_time) }}
                                                </v-btn>
                                            </template>
                                            <span>Task start time: {{ formatDateTime(task.start_time) }}</span>
                                        </v-tooltip>
                                    </v-card-actions>
                                </v-card>
                            </v-col>
                        </v-row>
                    </v-card-text>
                </v-card>

                <!-- Stopped Tasks Section -->
                <v-card v-if="tasks.length > 0" class="mb-6" elevation="2">
                    <v-card-title class="d-flex align-center pa-4 bg-error-lighten-4">
                        <v-icon class="me-3" color="error" size="large">mdi-stop-circle</v-icon>
                        <span class="text-h6">Stopped Tasks</span>
                        <v-spacer></v-spacer>
                        <v-chip color="error" variant="flat" size="small">
                            {{ stoppedTasks.length }}
                        </v-chip>
                    </v-card-title>
                    <v-card-text class="pa-4">
                        <div v-if="stoppedTasks.length === 0" class="d-flex flex-column align-center pa-6">
                            <v-icon size="48" color="error" class="mb-3">mdi-stop-circle-outline</v-icon>
                            <h4 class="text-h6 mb-2">No stopped tasks</h4>
                            <p class="text-body-2 text-center text-medium-emphasis">No tasks are currently stopped.</p>
                        </div>
                        <v-row v-else>
                            <v-col v-for="task in stoppedTasks" :key="task.id" cols="12" md="6" lg="4">
                                <v-card class="border-error" color="grey-lighten-4" variant="outlined" elevation="1">
                                    <v-card-title class="d-flex justify-space-between align-center">
                                        <span class="text-truncate">{{ task.name }}</span>
                                        <v-chip color="error" size="small" variant="flat">
                                            <v-icon start size="small">mdi-stop</v-icon>
                                            Stopped
                                        </v-chip>
                                    </v-card-title>
                                    <v-card-text>
                                        <div class="mb-3">
                                            <v-icon size="small" class="me-2">mdi-account</v-icon>
                                            <span class="text-body-2">{{ task.developer_name || 'Unassigned' }}</span>
                                        </div>
                                        <div class="mb-3">
                                            <v-icon size="small" class="me-2">mdi-clock-outline</v-icon>
                                            <span class="text-h6 font-weight-bold">{{
                                                formatDuration(task.total_duration_seconds || 0)
                                            }}</span>
                                        </div>
                                        <div class="text-caption text-medium-emphasis">
                                            ID: {{ task.id }}
                                        </div>
                                    </v-card-text>
                                    <v-card-actions v-if="task.start_time">
                                        <v-tooltip location="bottom">
                                            <template v-slot:activator="{ props }">
                                                <v-btn v-bind="props" variant="text" size="small"
                                                    prepend-icon="mdi-clock-start">
                                                    Started at {{ formatTime(task.start_time) }}
                                                </v-btn>
                                            </template>
                                            <span>Task start time: {{ formatDateTime(task.start_time) }}</span>
                                        </v-tooltip>
                                    </v-card-actions>
                                </v-card>
                            </v-col>
                        </v-row>
                    </v-card-text>
                </v-card>

                <v-expansion-panels class="mt-6">
                    <v-expansion-panel>
                        <v-expansion-panel-title>
                            <v-icon class="me-2">mdi-format-list-bulleted</v-icon>
                            Activity Log ({{ activityLog.length }})
                        </v-expansion-panel-title>
                        <v-expansion-panel-text>
                            <div class="activity-log" style="max-height: 300px; overflow-y: auto;">
                                <div v-for="(activity, index) in activityLog.slice().reverse()" :key="index"
                                    class="d-flex align-center py-2 border-b">
                                    <v-icon :color="getActivityColor(activity.action)" size="small" class="me-3">
                                        {{ getActivityIcon(activity.action) }}
                                    </v-icon>
                                    <div class="flex-grow-1">
                                        <div class="text-body-2">
                                            <strong>{{ activity.taskName }}</strong> - {{
                                                getActivityText(activity.action) }}
                                        </div>
                                        <div class="text-caption text-medium-emphasis">
                                            {{ formatDateTime(activity.timestamp) }}
                                        </div>
                                    </div>
                                </div>
                                <div v-if="activityLog.length === 0" class="text-center py-4 text-medium-emphasis">
                                    No activity logged yet
                                </div>
                            </div>
                        </v-expansion-panel-text>
                    </v-expansion-panel>
                </v-expansion-panels>
            </v-container>
        </v-main>
    </v-layout>
</template>

<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed } from 'vue'
import authService from '@/services/authService'

interface TaskInfo {
    id: string
    name: string
    total_duration: number
    total_duration_seconds?: number
    in_progress: boolean
    developer_name?: string
    status?: string
    start_time?: string
    update_time?: string
    is_started?: boolean
    is_paused?: boolean
    is_stopped?: boolean
}

interface ActivityLogEntry {
    timestamp: Date
    action: string
    taskName: string
    taskId: string
}

const tasks = ref<TaskInfo[]>([])
const connectionStatus = ref<'Disconnected' | 'Connecting' | 'Connected' | 'Auth Failed'>('Disconnected')
const error = ref('')
const loading = ref(false)
const activityLog = ref<ActivityLogEntry[]>([])

let socket: WebSocket | null = null
let timers: { [key: string]: number } = {}

const runningTasks = computed(() =>
    tasks.value.filter(task => task.is_started === true)
)

const pausedTasks = computed(() =>
    tasks.value.filter(task => task.is_paused === true)
)

const stoppedTasks = computed(() =>
    tasks.value.filter(task => task.is_stopped === true)
)

const connectWebSocket = async () => {
    if (socket?.readyState === WebSocket.OPEN) {
        return
    }

    const token = authService.getStoredToken()
    if (!token) {
        error.value = 'No authentication token found'
        return
    }

    connectionStatus.value = 'Connecting'
    error.value = ''

    try {
        const wsUrl = import.meta.env.VITE_WEBSOCKET_URL || 'ws://localhost:8081'
        socket = new WebSocket(`${wsUrl}/ws`)

        socket.onopen = () => {
            console.log('WebSocket connection opened')

            const authMessage = {
                action: 'authenticate',
                token: token
            }
            socket?.send(JSON.stringify(authMessage))
        }

        socket.onmessage = (event) => {
            try {
                const message = JSON.parse(event.data)
                handleWebSocketMessage(message)
            } catch (err) {
                console.error('Error parsing WebSocket message:', err)
            }
        }

        socket.onclose = (event) => {
            connectionStatus.value = 'Disconnected'
            console.log('WebSocket connection closed:', event.code, event.reason)

            Object.values(timers).forEach(timer => clearInterval(timer))
            timers = {}

            if (event.code !== 1000) {
                setTimeout(connectWebSocket, 5000)
            }
        }

        socket.onerror = (error) => {
            console.error('WebSocket error:', error)
            connectionStatus.value = 'Disconnected'
        }

    } catch (err) {
        error.value = 'Failed to connect to WebSocket server'
        connectionStatus.value = 'Disconnected'
        console.error('WebSocket connection error:', err)
    }
}

const handleWebSocketMessage = (message: any) => {
    console.log('Received WebSocket message:', message)

    if (message.action === 'auth_success') {
        connectionStatus.value = 'Connected'
        return
    }

    if (message.action === 'auth_error') {
        connectionStatus.value = 'Auth Failed'
        error.value = `Authentication failed: ${message.error}`
        return
    }

    if (message.action === 'initial_tasks') {
        return
    }

    if (message.task_data) {
        updateTask(message.task_data)

        activityLog.value.push({
            timestamp: new Date(),
            action: message.action,
            taskName: message.task_data.name,
            taskId: message.task_data.id
        })

        if (activityLog.value.length > 50) {
            activityLog.value = activityLog.value.slice(-50)
        }
    }
}

const updateTask = (taskData: TaskInfo) => {
    if (!taskData.is_started && !taskData.is_paused && !taskData.is_stopped) {
        const existingIndex = tasks.value.findIndex(t => t.id === taskData.id)
        if (existingIndex >= 0) {
            tasks.value.splice(existingIndex, 1)

            if (timers[taskData.id]) {
                clearInterval(timers[taskData.id])
                delete timers[taskData.id]
            }

            console.log(`Removed task ${taskData.name} (${taskData.id}) from display - no active flags`)
        }
        return
    }

    if (taskData.total_duration && taskData.total_duration > 1000000000) {
        taskData.total_duration_seconds = Math.floor(taskData.total_duration / 1000000000)
    } else {
        taskData.total_duration_seconds = taskData.total_duration || 0
    }

    const existingIndex = tasks.value.findIndex(t => t.id === taskData.id)
    if (existingIndex >= 0) {
        tasks.value[existingIndex] = taskData
    } else {
        tasks.value.push(taskData)
    }

    if (timers[taskData.id]) {
        clearInterval(timers[taskData.id])
        delete timers[taskData.id]
    }

    if (taskData.is_started === true) {
        startTaskTimer(taskData.id)
    }
}

const startTaskTimer = (taskId: string) => {
    timers[taskId] = setInterval(() => {
        const task = tasks.value.find(t => t.id === taskId)
        if (task && task.is_started === true) {
            task.total_duration_seconds = (task.total_duration_seconds || 0) + 1
        }
    }, 1000)
}

const formatDuration = (seconds: number): string => {
    if (!seconds || seconds < 0) return '00:00:00'

    const hours = Math.floor(seconds / 3600)
    const minutes = Math.floor((seconds % 3600) / 60)
    const secs = seconds % 60

    return `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${secs.toString().padStart(2, '0')}`
}

const formatTime = (dateString: string): string => {
    return new Date(dateString).toLocaleTimeString()
}

const formatDateTime = (dateString: string | Date): string => {
    const date = typeof dateString === 'string' ? new Date(dateString) : dateString
    return date.toLocaleString()
}


const getActivityColor = (action: string): string => {
    if (action.includes('started')) return 'success'
    if (action.includes('paused')) return 'warning'
    if (action.includes('stopped')) return 'error'
    return 'info'
}

const getActivityIcon = (action: string): string => {
    if (action.includes('started')) return 'mdi-play-circle'
    if (action.includes('paused')) return 'mdi-pause-circle'
    if (action.includes('stopped')) return 'mdi-stop-circle'
    return 'mdi-information'
}

const getActivityText = (action: string): string => {
    if (action.includes('started')) return 'was started'
    if (action.includes('paused')) return 'was paused'
    if (action.includes('stopped')) return 'was stopped'
    return action
}

const disconnectWebSocket = () => {
    if (socket) {
        socket.close(1000, 'Manual disconnect')
        socket = null
    }
}

onMounted(() => {
    connectWebSocket()
})

onUnmounted(() => {
    disconnectWebSocket()
    Object.values(timers).forEach(timer => clearInterval(timer))
})
</script>

<style scoped>
.border-success {
    border-color: rgb(76, 175, 80) !important;
    border-width: 2px !important;
}

.border-warning {
    border-color: rgb(255, 152, 0) !important;
    border-width: 2px !important;
}

.border-error {
    border-color: rgb(244, 67, 54) !important;
    border-width: 2px !important;
}

.activity-log {
    font-family: 'Roboto Mono', monospace;
}

.border-b {
    border-bottom: 1px solid rgba(0, 0, 0, 0.12);
}
</style>
