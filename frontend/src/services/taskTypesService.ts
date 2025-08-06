import { companyLogger } from '@/utils/logger';
import apiService from './apiService';
import type { ApiResponse, TaskType } from '@/types';

class TaskTypesService {
    async getTaskTypes(signal?: AbortSignal): Promise<TaskType[]> {
        try {
            const response = await apiService.get<ApiResponse<TaskType[]>>('/taskTypes', signal);
            return response.data;
        } catch (error) {
            companyLogger.error('Error fetching task types:', error);
            throw new Error('Failed to fetch task types');
        }
    }

    async getTaskTypeById(id: number, signal?: AbortSignal): Promise<TaskType> {
        try {
            const response = await apiService.get<ApiResponse<TaskType>>(`/taskTypes/${id}`, signal);
            return response.data;
        } catch (error) {
            companyLogger.error('Error fetching task type:', error);
            throw new Error('Failed to fetch task type');
        }
    }

    async createTaskType(taskType: Omit<TaskType, 'id'>, signal?: AbortSignal): Promise<TaskType> {
        try {
            const response = await apiService.post<ApiResponse<TaskType>>('/taskTypes', taskType, signal);
            return response.data;
        } catch (error) {
            companyLogger.error('Error creating task type:', error);
            throw new Error('Failed to create task type');
        }
    }

    async updateTaskType(id: number, taskType: Partial<TaskType>, signal?: AbortSignal): Promise<TaskType> {
        try {
            const response = await apiService.put<ApiResponse<TaskType>>(`/taskTypes/${id}`, taskType, signal);
            return response.data;
        } catch (error) {
            companyLogger.error('Error updating task type:', error);
            throw new Error('Failed to update task type');
        }
    }

    async deleteTaskType(id: number, signal?: AbortSignal): Promise<void> {
        try {
            await apiService.delete(`/taskTypes/${id}`, signal);
        } catch (error) {
            companyLogger.error('Error deleting task type:', error);
            throw new Error('Failed to delete task type');
        }
    }
}

export default new TaskTypesService();
