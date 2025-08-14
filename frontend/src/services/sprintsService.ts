import { sprintsLogger } from '@/utils/logger';
import apiService from './apiService';
import type { ApiResponse, Sprint } from '@/types';

class SprintsService {
    async getSprints(signal?: AbortSignal): Promise<Sprint[]> {
        try {
            const response = await apiService.get<ApiResponse<Sprint[]>>('/sprints', signal);
            return response.data;
        } catch (error) {
            sprintsLogger.error('Error fetching sprints:', error);
            throw new Error('Failed to fetch sprints');
        }
    }

    async getSprintById(id: string, signal?: AbortSignal): Promise<Sprint> {
        try {
            const response = await apiService.get<ApiResponse<Sprint>>(`/sprints/${id}`, signal);
            return response.data;
        } catch (error) {
            sprintsLogger.error('Error fetching sprint:', error);
            throw new Error('Failed to fetch sprint');
        }
    }

    async createSprint(sprint: Omit<Sprint, 'id'>, signal?: AbortSignal): Promise<Sprint> {
        try {
            const response = await apiService.post<ApiResponse<Sprint>>('/sprints', sprint, signal);
            return response.data;
        } catch (error) {
            sprintsLogger.error('Error creating sprint:', error);
            throw new Error('Failed to create sprint');
        }
    }

    async updateSprint(id: string, sprint: Partial<Sprint>, signal?: AbortSignal): Promise<Sprint> {
        try {
            const response = await apiService.put<ApiResponse<Sprint>>(`/sprints/${id}`, sprint, signal);
            return response.data;
        } catch (error) {
            sprintsLogger.error('Error updating sprint:', error);
            throw new Error('Failed to update sprint');
        }
    }

    async deleteSprint(id: string, signal?: AbortSignal): Promise<void> {
        try {
            await apiService.delete(`/sprints/${id}`, signal);
        } catch (error) {
            sprintsLogger.error('Error deleting sprint:', error);
            throw new Error('Failed to delete sprint');
        }
    }

    async getByManager(managerId: string, signal?: AbortSignal): Promise<Sprint[]> {
        try {
            const response = await apiService.get<ApiResponse<Sprint[]>>(`/sprints/manager/${managerId}`, signal);
            return response.data;
        } catch (error) {
            sprintsLogger.error('Error fetching sprints by manager:', error);
            throw new Error('Failed to fetch sprints by manager');
        }
    }
}

export default new SprintsService();
