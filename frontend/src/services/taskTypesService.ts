import { taskTypesLogger } from '@/utils/logger';
import apiService from './apiService';
import { extractErrorMessage } from '@/utils/errorHandler';
import type { ApiResponse, TaskType } from '@/types';

class TaskTypesService {
    async getTaskTypes(signal?: AbortSignal): Promise<TaskType[]> {
        try {
            const response = await apiService.get<ApiResponse<TaskType[]>>('/taskTypes', signal);
            return response.data;
        } catch (error) {
            taskTypesLogger.error('Error fetching task types:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getTaskTypeById(id: number, signal?: AbortSignal): Promise<TaskType> {
        try {
            const response = await apiService.get<ApiResponse<TaskType>>(`/taskTypes/${id}`, signal);
            return response.data;
        } catch (error) {
            taskTypesLogger.error('Error fetching task type:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async createTaskType(taskType: Omit<TaskType, 'id'>, signal?: AbortSignal): Promise<TaskType> {
        try {
            const response = await apiService.post<ApiResponse<TaskType>>('/taskTypes', taskType, signal);
            return response.data;
        } catch (error) {
            taskTypesLogger.error('Error creating task type:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async updateTaskType(id: number, taskType: Partial<TaskType>, signal?: AbortSignal): Promise<TaskType> {
        try {
            const response = await apiService.put<ApiResponse<TaskType>>(`/taskTypes/${id}`, taskType, signal);
            return response.data;
        } catch (error) {
            taskTypesLogger.error('Error updating task type:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async deleteTaskType(id: number, signal?: AbortSignal): Promise<void> {
        try {
            await apiService.delete(`/taskTypes/${id}`, signal);
        } catch (error) {
            taskTypesLogger.error('Error deleting task type:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }
}

export default new TaskTypesService();
