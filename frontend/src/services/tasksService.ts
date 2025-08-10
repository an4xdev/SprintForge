import { tasksLogger } from '@/utils/logger';
import apiService from './apiService';
import type { ApiResponse, Task } from '@/types';

class TasksService {
    async getTasks(signal?: AbortSignal): Promise<Task[]> {
        try {
            const response = await apiService.get<ApiResponse<Task[]>>('/tasks', signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching tasks:', error);
            throw new Error('Failed to fetch tasks');
        }
    }

    async getTaskById(id: string, signal?: AbortSignal): Promise<Task> {
        try {
            const response = await apiService.get<ApiResponse<Task>>(`/tasks/${id}`, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching task:', error);
            throw new Error('Failed to fetch task');
        }
    }

    async createTask(task: Omit<Task, 'id'>, signal?: AbortSignal): Promise<Task> {
        try {
            const response = await apiService.post<ApiResponse<Task>>('/tasks', task, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error creating task:', error);
            throw new Error('Failed to create task');
        }
    }

    async updateTask(id: string, task: Partial<Task>, signal?: AbortSignal): Promise<Task> {
        try {
            const response = await apiService.put<ApiResponse<Task>>(`/tasks/${id}`, task, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error updating task:', error);
            throw new Error('Failed to update task');
        }
    }

    async deleteTask(id: string, signal?: AbortSignal): Promise<void> {
        try {
            await apiService.delete(`/tasks/${id}`, signal);
        } catch (error) {
            tasksLogger.error('Error deleting task:', error);
            throw new Error('Failed to delete task');
        }
    }
}

export default new TasksService();
