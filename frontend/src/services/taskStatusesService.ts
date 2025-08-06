import { companyLogger } from '@/utils/logger';
import apiService from './apiService';
import type { ApiResponse, TaskStatus } from '@/types';

class TaskStatusesService {
    async getTaskStatuses(signal?: AbortSignal): Promise<TaskStatus[]> {
        try {
            const response = await apiService.get<ApiResponse<TaskStatus[]>>('/taskStatuses', signal);
            return response.data;
        } catch (error) {
            companyLogger.error('Error fetching task statuses:', error);
            throw new Error('Failed to fetch task statuses');
        }
    }

    async getTaskStatusById(id: number, signal?: AbortSignal): Promise<TaskStatus> {
        try {
            const response = await apiService.get<ApiResponse<TaskStatus>>(`/taskStatuses/${id}`, signal);
            return response.data;
        } catch (error) {
            companyLogger.error('Error fetching task status:', error);
            throw new Error('Failed to fetch task status');
        }
    }

    async createTaskStatus(taskStatus: Omit<TaskStatus, 'id'>, signal?: AbortSignal): Promise<TaskStatus> {
        try {
            const response = await apiService.post<ApiResponse<TaskStatus>>('/taskStatuses', taskStatus, signal);
            return response.data;
        } catch (error) {
            companyLogger.error('Error creating task status:', error);
            throw new Error('Failed to create task status');
        }
    }

    async updateTaskStatus(id: number, taskStatus: Partial<TaskStatus>, signal?: AbortSignal): Promise<TaskStatus> {
        try {
            const response = await apiService.put<ApiResponse<TaskStatus>>(`/taskStatuses/${id}`, taskStatus, signal);
            return response.data;
        } catch (error) {
            companyLogger.error('Error updating task status:', error);
            throw new Error('Failed to update task status');
        }
    }

    async deleteTaskStatus(id: number, signal?: AbortSignal): Promise<void> {
        try {
            await apiService.delete(`/taskStatusesK/${id}`, signal);
        } catch (error) {
            companyLogger.error('Error deleting task status:', error);
            throw new Error('Failed to delete task status');
        }
    }
}

export default new TaskStatusesService();
