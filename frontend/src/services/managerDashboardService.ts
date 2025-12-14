import { managerDashboardLogger } from '@/utils/logger';
import apiService from './apiService';
import { extractErrorMessage } from '@/utils/errorHandler';
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
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }
}

export default new ManagerDashboardService();
