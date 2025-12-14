import { sprintsLogger } from '@/utils/logger';
import apiService from './apiService';
import { extractErrorMessage } from '@/utils/errorHandler';
import type { ApiResponse, Sprint, SprintExt } from '@/types';

class SprintsService {
    async getSprints(signal?: AbortSignal): Promise<Sprint[]> {
        try {
            const response = await apiService.get<ApiResponse<Sprint[]>>('/sprints', signal);
            return response.data;
        } catch (error) {
            sprintsLogger.error('Error fetching sprints:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getSprintById(id: string, signal?: AbortSignal): Promise<Sprint> {
        try {
            const response = await apiService.get<ApiResponse<Sprint>>(`/sprints/${id}`, signal);
            return response.data;
        } catch (error) {
            sprintsLogger.error('Error fetching sprint:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async createSprint(sprint: Omit<Sprint, 'id'>, signal?: AbortSignal): Promise<Sprint> {
        try {
            const response = await apiService.post<ApiResponse<Sprint>>('/sprints', sprint, signal);
            return response.data;
        } catch (error) {
            sprintsLogger.error('Error creating sprint:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async updateSprint(id: string, sprint: Partial<Sprint>, signal?: AbortSignal): Promise<Sprint> {
        try {
            const response = await apiService.put<ApiResponse<Sprint>>(`/sprints/${id}`, sprint, signal);
            return response.data;
        } catch (error) {
            sprintsLogger.error('Error updating sprint:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async deleteSprint(id: string, signal?: AbortSignal): Promise<void> {
        try {
            await apiService.delete(`/sprints/${id}`, signal);
        } catch (error) {
            sprintsLogger.error('Error deleting sprint:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getByManager(managerId: string, signal?: AbortSignal): Promise<Sprint[]> {
        try {
            const response = await apiService.get<ApiResponse<Sprint[]>>(`/sprints/manager/${managerId}`, signal);
            return response.data;
        } catch (error) {
            sprintsLogger.error('Error fetching sprints by manager:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getByManagerLast(managerId: string, signal?: AbortSignal): Promise<string> {
        try {
            const response = await apiService.get<ApiResponse<string>>(`/sprints/manager/${managerId}/last`, signal);
            return response.data;
        } catch (error) {
            sprintsLogger.error('Error fetching last sprint by manager:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getSprintsExt(signal?: AbortSignal): Promise<SprintExt[]> {
        try {
            const response = await apiService.get<ApiResponse<SprintExt[]>>('/sprints/all-ext', signal);
            return response.data;
        } catch (error) {
            sprintsLogger.error('Error fetching extended sprints:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getSprintExtById(id: string, signal?: AbortSignal): Promise<SprintExt> {
        try {
            const response = await apiService.get<ApiResponse<SprintExt>>(`/sprints/${id}-ext`, signal);
            return response.data;
        } catch (error) {
            sprintsLogger.error('Error fetching extended sprint:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getByManagerExt(managerId: string, signal?: AbortSignal): Promise<SprintExt[]> {
        try {
            const response = await apiService.get<ApiResponse<SprintExt[]>>(`/sprints/manager/${managerId}-ext`, signal);
            return response.data;
        } catch (error) {
            sprintsLogger.error('Error fetching extended sprints by manager:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }
}

export default new SprintsService();
