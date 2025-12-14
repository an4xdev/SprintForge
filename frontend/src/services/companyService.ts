import { companyLogger } from '@/utils/logger';
import apiService from './apiService';
import { extractErrorMessage } from '@/utils/errorHandler';
import type { ApiResponse, Company } from '@/types';

class CompanyService {
    async getCompanies(signal?: AbortSignal): Promise<Company[]> {
        try {
            const response = await apiService.get<ApiResponse<Company[]>>('/companies', signal);
            return response.data;
        } catch (error) {
            companyLogger.error('Error fetching companies:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getCompanyById(id: number, signal?: AbortSignal): Promise<Company> {
        try {
            const response = await apiService.get<ApiResponse<Company>>(`/companies/${id}`, signal);
            return response.data;
        } catch (error) {
            companyLogger.error('Error fetching company:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async createCompany(company: Omit<Company, 'id'>, signal?: AbortSignal): Promise<Company> {
        try {
            const response = await apiService.post<ApiResponse<Company>>('/companies', company, signal);
            return response.data;
        } catch (error) {
            companyLogger.error('Error creating company:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async updateCompany(id: number, company: Partial<Company>, signal?: AbortSignal): Promise<Company> {
        try {
            const response = await apiService.put<ApiResponse<Company>>(`/companies/${id}`, company, signal);
            return response.data;
        } catch (error) {
            companyLogger.error('Error updating company:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async deleteCompany(id: number, signal?: AbortSignal): Promise<void> {
        try {
            await apiService.delete(`/companies/${id}`, signal);
        } catch (error) {
            companyLogger.error('Error deleting company:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }
}

export default new CompanyService();
