import { adminDashboardLogger } from '@/utils/logger';
import apiService from './apiService';
import type { AdminDashboard, ApiResponse } from '@/types';

class AdminDashboardService {
    async getDashboardInfo(signal?: AbortSignal): Promise<AdminDashboard> {
        try {
            const response = await apiService.get<ApiResponse<AdminDashboard>>('/adminDashboard', signal);
            return response.data;
        } catch (error) {
            adminDashboardLogger.error('Error fetching dashboard info:', error);
            throw new Error('Failed to fetch dashboard info');
        }
    }
}

export default new AdminDashboardService();
