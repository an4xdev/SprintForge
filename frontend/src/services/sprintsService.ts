import { sprintsLogger } from '@/utils/logger';
import apiService from './apiService';
import type { ApiResponse, Sprint, SprintExt } from '@/types';

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

    async getByManagerLast(managerId: string, signal?: AbortSignal): Promise<string> {
        try {
            const response = await apiService.get<ApiResponse<string>>(`/sprints/manager/${managerId}/last`, signal);
            return response.data;
        } catch (error) {
            sprintsLogger.error('Error fetching last sprint by manager:', error);
            throw new Error('Failed to fetch last sprint by manager');
        }
    }

    async getSprintsExt(signal?: AbortSignal): Promise<SprintExt[]> {
        try {
            const response = await apiService.get<ApiResponse<SprintExt[]>>('/sprints/all-ext', signal);
            return response.data;
        } catch (error) {
            sprintsLogger.error('Error fetching extended sprints:', error);
            throw new Error('Failed to fetch extended sprints');
        }
    }

    async getSprintExtById(id: string, signal?: AbortSignal): Promise<SprintExt> {
        try {
            const response = await apiService.get<ApiResponse<SprintExt>>(`/sprints/${id}-ext`, signal);
            return response.data;
        } catch (error) {
            sprintsLogger.error('Error fetching extended sprint:', error);
            throw new Error('Failed to fetch extended sprint');
        }
    }

    async getByManagerExt(managerId: string, signal?: AbortSignal): Promise<SprintExt[]> {
        try {
            const response = await apiService.get<ApiResponse<SprintExt[]>>(`/sprints/manager/${managerId}-ext`, signal);
            return response.data;
        } catch (error) {
            sprintsLogger.error('Error fetching extended sprints by manager:', error);
            throw new Error('Failed to fetch extended sprints by manager');
        }
    }
}

export default new SprintsService();
