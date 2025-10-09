import { reportsLogger } from '@/utils/logger';
import apiService from './apiService';
import type {
    TeamReportDto,
    SprintReportDto,
    ProjectReportDto,
    ReportFilters,
    ApiResponse,
    AuditLog
} from '@/types';

class ReportsService {
    private readonly baseUrl = '/reports';

    async getTeamReports(managerId?: string | null, filters?: ReportFilters, signal?: AbortSignal): Promise<TeamReportDto[]> {
        try {
            reportsLogger.info('Fetching team reports', { managerId, filters });

            const params = new URLSearchParams();

            if (managerId) {
                params.append('managerId', managerId);
            }

            if (filters?.startDate) {
                params.append('startDate', filters.startDate.toISOString());
            }
            if (filters?.endDate) {
                params.append('endDate', filters.endDate.toISOString());
            }

            const response = await apiService.get<ApiResponse<TeamReportDto[]>>(
                `${this.baseUrl}/teams?${params.toString()}`, signal
            );

            reportsLogger.info('Team reports fetched successfully');
            return response.data;
        } catch (error) {
            reportsLogger.error('Failed to fetch team reports', error);
            throw error;
        }
    }

    async getSprintReports(managerId?: string | null, filters?: ReportFilters, signal?: AbortSignal): Promise<SprintReportDto[]> {
        try {
            reportsLogger.info('Fetching sprint reports', { managerId, filters });

            const params = new URLSearchParams();

            if (managerId) {
                params.append('managerId', managerId);
            }

            if (filters?.startDate) {
                params.append('startDate', filters.startDate.toISOString());
            }
            if (filters?.endDate) {
                params.append('endDate', filters.endDate.toISOString());
            }

            const response = await apiService.get<ApiResponse<SprintReportDto[]>>(
                `${this.baseUrl}/sprints?${params.toString()}`, signal
            );

            reportsLogger.info('Sprint reports fetched successfully');
            return response.data;
        } catch (error) {
            reportsLogger.error('Failed to fetch sprint reports', error);
            throw error;
        }
    }

    async getProjectReports(managerId?: string | null, filters?: ReportFilters, signal?: AbortSignal): Promise<ProjectReportDto[]> {
        try {
            reportsLogger.info('Fetching project reports', { managerId, filters });

            const params = new URLSearchParams();

            if (managerId) {
                params.append('managerId', managerId);
            }

            if (filters?.startDate) {
                params.append('startDate', filters.startDate.toISOString());
            }
            if (filters?.endDate) {
                params.append('endDate', filters.endDate.toISOString());
            }

            const response = await apiService.get<ApiResponse<ProjectReportDto[]>>(
                `${this.baseUrl}/projects?${params.toString()}`, signal
            );

            reportsLogger.info('Project reports fetched successfully');
            return response.data;
        } catch (error) {
            reportsLogger.error('Failed to fetch project reports', error);
            throw error;
        }
    }

    async exportToPdf(reportType: 'team' | 'sprint' | 'project', data: any, title: string): Promise<void> {
        try {
            reportsLogger.info('Exporting report to PDF (client-side)', { reportType, title });

            const { default: pdfExportService } = await import('./pdfExportService');

            switch (reportType) {
                case 'team':
                    await pdfExportService.exportTeamReport(data, title);
                    break;
                case 'sprint':
                    await pdfExportService.exportSprintReports(data, title);
                    break;
                case 'project':
                    await pdfExportService.exportProjectReports(data, title);
                    break;
                default:
                    throw new Error(`Unsupported report type: ${reportType}`);
            }

            reportsLogger.info('Report exported to PDF successfully (client-side)');
        } catch (error) {
            reportsLogger.error('Failed to export report to PDF (client-side)', error);
            throw error;
        }
    }

    async exportChartToPdf(chartElement: HTMLElement, title: string): Promise<void> {
        try {
            reportsLogger.info('Exporting chart to PDF', { title });

            const { default: pdfExportService } = await import('./pdfExportService');
            await pdfExportService.exportChartToPdf(chartElement, title);

            reportsLogger.info('Chart exported to PDF successfully');
        } catch (error) {
            reportsLogger.error('Failed to export chart to PDF', error);
            throw error;
        }
    }

    formatDuration(timeString: string): string {
        if (!timeString || timeString === '00:00:00') {
            return '0h 0m';
        }

        const parts = timeString.split(':');
        if (parts.length >= 2) {
            const hours = parseInt(parts[0]);
            const minutes = parseInt(parts[1]);

            if (hours > 0) {
                return `${hours}h ${minutes}m`;
            }
            return `${minutes}m`;
        }

        return timeString;
    }

    calculateCompletionPercentage(completed: number, total: number): number {
        if (total === 0) return 0;
        return Math.round((completed / total) * 100);
    }

    async getAuditLogs(limit: number = 10, offset: number = 0, signal?: AbortSignal): Promise<ApiResponse<{ logs: AuditLog[], totalCount: number, limit: number, offset: number }>> {
        try {
            reportsLogger.info('Fetching audit logs', { limit, offset });

            const params = new URLSearchParams();
            params.append('limit', limit.toString());
            params.append('offset', offset.toString());

            const response = await apiService.get<ApiResponse<{ logs: AuditLog[], totalCount: number, limit: number, offset: number }>>(
                `${this.baseUrl}/audit?${params.toString()}`, signal
            );

            reportsLogger.info('Audit logs fetched successfully', { count: response.data.logs?.length });
            return response;
        } catch (error) {
            reportsLogger.error('Failed to fetch audit logs', error);
            throw error;
        }
    }
}

export default new ReportsService();