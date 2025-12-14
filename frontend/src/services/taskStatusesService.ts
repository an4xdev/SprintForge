import { taskStatusesLogger } from '@/utils/logger';
import apiService from './apiService';
import { extractErrorMessage } from '@/utils/errorHandler';
import type { ApiResponse, TaskStatus } from '@/types';

class TaskStatusesService {
    async getTaskStatuses(signal?: AbortSignal): Promise<TaskStatus[]> {
        try {
            const response = await apiService.get<ApiResponse<TaskStatus[]>>('/taskStatuses', signal);
            return response.data;
        } catch (error) {
            taskStatusesLogger.error('Error fetching task statuses:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getTaskStatusById(id: number, signal?: AbortSignal): Promise<TaskStatus> {
        try {
            const response = await apiService.get<ApiResponse<TaskStatus>>(`/taskStatuses/${id}`, signal);
            return response.data;
        } catch (error) {
            taskStatusesLogger.error('Error fetching task status:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async createTaskStatus(taskStatus: Omit<TaskStatus, 'id'>, signal?: AbortSignal): Promise<TaskStatus> {
        try {
            const response = await apiService.post<ApiResponse<TaskStatus>>('/taskStatuses', taskStatus, signal);
            return response.data;
        } catch (error) {
            taskStatusesLogger.error('Error creating task status:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async updateTaskStatus(id: number, taskStatus: Partial<TaskStatus>, signal?: AbortSignal): Promise<TaskStatus> {
        try {
            const response = await apiService.put<ApiResponse<TaskStatus>>(`/taskStatuses/${id}`, taskStatus, signal);
            return response.data;
        } catch (error) {
            taskStatusesLogger.error('Error updating task status:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async deleteTaskStatus(id: number, signal?: AbortSignal): Promise<void> {
        try {
            await apiService.delete(`/taskStatuses/${id}`, signal);
        } catch (error) {
            taskStatusesLogger.error('Error deleting task status:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }
}

export default new TaskStatusesService();
