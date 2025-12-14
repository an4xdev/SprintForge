import { projectsLogger } from '@/utils/logger';
import apiService from './apiService';
import { extractErrorMessage } from '@/utils/errorHandler';
import type { ApiResponse, Project, ProjectExt } from '@/types';

class ProjectsService {
    async getProjects(signal?: AbortSignal): Promise<Project[]> {
        try {
            const response = await apiService.get<ApiResponse<Project[]>>('/projects', signal);
            return response.data;
        } catch (error) {
            projectsLogger.error('Error fetching projects:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getProjectById(id: string, signal?: AbortSignal): Promise<Project> {
        try {
            const response = await apiService.get<ApiResponse<Project>>(`/projects/${id}`, signal);
            return response.data;
        } catch (error) {
            projectsLogger.error('Error fetching project:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async createProject(project: Omit<Project, 'id'>, signal?: AbortSignal): Promise<Project> {
        try {
            const response = await apiService.post<ApiResponse<Project>>('/projects', project, signal);
            return response.data;
        } catch (error) {
            projectsLogger.error('Error creating project:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async updateProject(id: string, project: Partial<Project>, signal?: AbortSignal): Promise<Project> {
        try {
            const response = await apiService.put<ApiResponse<Project>>(`/projects/${id}`, project, signal);
            return response.data;
        } catch (error) {
            projectsLogger.error('Error updating project:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async deleteProject(id: string, signal?: AbortSignal): Promise<void> {
        try {
            await apiService.delete(`/projects/${id}`, signal);
        } catch (error) {
            projectsLogger.error('Error deleting project:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getCurrentProjectByManagerId(id: string, signal?: AbortSignal): Promise<string | null> {
        try {
            const response = await apiService.get<ApiResponse<string>>(`/projects/current/${id}`, signal);
            return response.data || null;
        } catch (error) {
            projectsLogger.error('Error fetching current project by manager ID:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getProjectsExt(signal?: AbortSignal): Promise<ProjectExt[]> {
        try {
            const response = await apiService.get<ApiResponse<ProjectExt[]>>('/projects/all-ext', signal);
            return response.data;
        } catch (error) {
            projectsLogger.error('Error fetching extended projects:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getProjectExtById(id: string, signal?: AbortSignal): Promise<ProjectExt> {
        try {
            const response = await apiService.get<ApiResponse<ProjectExt>>(`/projects/${id}-ext`, signal);
            return response.data;
        } catch (error) {
            projectsLogger.error('Error fetching extended project:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }
}

export default new ProjectsService();
