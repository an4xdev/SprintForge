import { tasksLogger } from '@/utils/logger';
import apiService from './apiService';
import { extractErrorMessage } from '@/utils/errorHandler';
import type { ApiResponse, Task, DeveloperTask, CreateTask, TaskExt, TaskHistory, TaskHistoryExt } from '@/types';

class TasksService {
    async getTasks(signal?: AbortSignal): Promise<Task[]> {
        try {
            const response = await apiService.get<ApiResponse<Task[]>>('/tasks', signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching tasks:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getTaskById(id: string, signal?: AbortSignal): Promise<Task> {
        try {
            const response = await apiService.get<ApiResponse<Task>>(`/tasks/${id}`, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching task:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async createTask(task: CreateTask, managerId: string, signal?: AbortSignal): Promise<Task> {
        try {
            const response = await apiService.post<ApiResponse<Task>>(`/tasks/manager/${managerId}`, task, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error creating task:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async updateTask(id: string, task: Partial<Task>, signal?: AbortSignal): Promise<Task> {
        try {
            const response = await apiService.put<ApiResponse<Task>>(`/tasks/${id}`, task, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error updating task:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async deleteTask(id: string, signal?: AbortSignal): Promise<void> {
        try {
            await apiService.delete(`/tasks/${id}`, signal);
        } catch (error) {
            tasksLogger.error('Error deleting task:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getUnassignedTasksByProject(projectId: string, signal?: AbortSignal): Promise<Task[]> {
        try {
            const response = await apiService.get<ApiResponse<Task[]>>(`/tasks/unassigned/project/${projectId}`, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching unassigned tasks by project:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getUnassignedTasksBySprint(sprintId: string, signal?: AbortSignal): Promise<Task[]> {
        try {
            const response = await apiService.get<ApiResponse<Task[]>>(`/tasks/unassigned/sprint/${sprintId}`, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching unassigned tasks by sprint:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getAssignedTasksBySprint(sprintId: string, signal?: AbortSignal): Promise<Task[]> {
        try {
            const response = await apiService.get<ApiResponse<Task[]>>(`/tasks/assigned/sprint/${sprintId}`, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching assigned tasks by sprint:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getTasksByDeveloper(developerId: string, signal?: AbortSignal): Promise<Task[]> {
        try {
            const response = await apiService.get<ApiResponse<Task[]>>(`/tasks/developer/${developerId}`, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching tasks by developer:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getTasksByDeveloperExt(developerId: string, signal?: AbortSignal): Promise<TaskExt[]> {
        try {
            const response = await apiService.get<ApiResponse<TaskExt[]>>(`/tasks/developer/${developerId}-ext`, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching extended tasks by developer:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async assignDeveloper(taskId: string, developerId: string, signal?: AbortSignal): Promise<Task> {
        try {
            const response = await apiService.patch<ApiResponse<Task>>(`/tasks/${taskId}/assign-developer`, { developerId }, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error assigning developer to task:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async moveToSprint(taskId: string, sprintId: string, signal?: AbortSignal): Promise<Task> {
        try {
            const response = await apiService.patch<ApiResponse<Task>>(`/tasks/${taskId}/move-to-sprint`, { sprintId }, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error moving task to sprint:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    mapTaskToDeveloperTask(task: Task, projectName: string = 'Unknown Project'): DeveloperTask {
        return {
            id: task.id,
            name: task.name,
            description: task.description,
            project: projectName,
            status: 'NONE',
            hours: 0,
            minutes: 0,
            seconds: 0
        };
    }

    async getTaskTime(taskId: string, signal?: AbortSignal): Promise<any> {
        try {
            const response = await apiService.get<ApiResponse<any>>(`/tasks/${taskId}/time`, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching task time:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getDeveloperTaskTimes(developerId: string, signal?: AbortSignal): Promise<any[]> {
        try {
            const response = await apiService.get<ApiResponse<any[]>>(`/tasks/developer/${developerId}/times`, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching developer task times:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    formatSecondsToTime(totalSeconds: number): { hours: number, minutes: number, seconds: number } {
        const hours = Math.floor(totalSeconds / 3600);
        const minutes = Math.floor((totalSeconds % 3600) / 60);
        const seconds = totalSeconds % 60;

        return { hours, minutes, seconds };
    }

    calculateCurrentSessionTime(sessionStart: string): number {
        const start = new Date(sessionStart);
        const now = new Date();
        return Math.floor((now.getTime() - start.getTime()) / 1000);
    }

    async startTask(taskId: string, signal?: AbortSignal): Promise<any> {
        try {
            const response = await apiService.put<ApiResponse<any>>(`/tasks/${taskId}/start`, {}, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error starting task:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async pauseTask(taskId: string, signal?: AbortSignal): Promise<any> {
        try {
            const response = await apiService.put<ApiResponse<any>>(`/tasks/${taskId}/pause`, {}, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error pausing task:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async stopTask(taskId: string, signal?: AbortSignal): Promise<any> {
        try {
            const response = await apiService.put<ApiResponse<any>>(`/tasks/${taskId}/stop`, {}, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error stopping task:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getAllTasksExt(signal?: AbortSignal): Promise<TaskExt[]> {
        try {
            const response = await apiService.get<ApiResponse<TaskExt[]>>('/tasks/all-ext', signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching extended tasks:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getTaskExtById(id: string, signal?: AbortSignal): Promise<TaskExt> {
        try {
            const response = await apiService.get<ApiResponse<TaskExt>>(`/tasks/${id}-ext`, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching extended task:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }
    async getAllTaskHistories(signal?: AbortSignal): Promise<TaskHistory[]> {
        try {
            const response = await apiService.get<ApiResponse<TaskHistory[]>>('/taskHistories', signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching task histories:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getAllTaskHistoriesExt(signal?: AbortSignal): Promise<TaskHistoryExt[]> {
        try {
            const response = await apiService.get<ApiResponse<TaskHistoryExt[]>>('/taskHistories/ext', signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching extended task histories:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getTaskHistoriesByTask(taskId: string, signal?: AbortSignal): Promise<TaskHistory[]> {
        try {
            const response = await apiService.get<ApiResponse<TaskHistory[]>>(`/taskHistories/task/${taskId}`, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching task histories by task:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getTaskHistoriesByTaskExt(taskId: string, signal?: AbortSignal): Promise<TaskHistoryExt[]> {
        try {
            const response = await apiService.get<ApiResponse<TaskHistoryExt[]>>(`/taskHistories/task/${taskId}/ext`, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching extended task histories by task:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getTaskHistoriesByManagerExt(managerId: string, signal?: AbortSignal): Promise<TaskHistoryExt[]> {
        try {
            const response = await apiService.get<ApiResponse<TaskHistoryExt[]>>(`/taskHistories/manager/${managerId}/ext`, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching extended task histories by manager:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }
}

export default new TasksService();
