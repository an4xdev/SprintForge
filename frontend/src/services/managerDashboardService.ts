import { managerDashboardLogger } from '@/utils/logger';
import apiService from './apiService';
import type { ApiResponse, ManagerDashboardInfoDto } from '@/types';

class ManagerDashboardService {

    private userId: string | null = null;

    setUserId(id: string) {
        this.userId = id;
    }

    async getDashboardInfo(signal?: AbortSignal): Promise<ManagerDashboardInfoDto> {
        try {
            const response = await apiService.get<ApiResponse<ManagerDashboardInfoDto>>(`/managerDashboard/${this.userId}`, signal);
            return response.data;
        } catch (error) {
            managerDashboardLogger.error('Error fetching dashboard info:', error);
            throw new Error('Failed to fetch dashboard info');
        }
    }
}

export default new ManagerDashboardService();
