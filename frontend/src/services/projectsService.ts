import { projectsLogger } from '@/utils/logger';
import apiService from './apiService';
import type { ApiResponse, Company, Project } from '@/types';

class ProjectsService {
    async getProjects(signal?: AbortSignal): Promise<Project[]> {
        try {
            const response = await apiService.get<ApiResponse<Project[]>>('/projects', signal);
            return response.data;
        } catch (error) {
            projectsLogger.error('Error fetching projects:', error);
            throw new Error('Failed to fetch projects');
        }
    }

    async getProjectById(id: string, signal?: AbortSignal): Promise<Project> {
        try {
            const response = await apiService.get<ApiResponse<Project>>(`/projects/${id}`, signal);
            return response.data;
        } catch (error) {
            projectsLogger.error('Error fetching project:', error);
            throw new Error('Failed to fetch project');
        }
    }

    async createProject(project: Omit<Project, 'id'>, signal?: AbortSignal): Promise<Project> {
        try {
            const response = await apiService.post<ApiResponse<Project>>('/projects', project, signal);
            return response.data;
        } catch (error) {
            projectsLogger.error('Error creating project:', error);
            throw new Error('Failed to create project');
        }
    }

    async updateProject(id: string, project: Partial<Project>, signal?: AbortSignal): Promise<Project> {
        try {
            const response = await apiService.put<ApiResponse<Project>>(`/projects/${id}`, project, signal);
            return response.data;
        } catch (error) {
            projectsLogger.error('Error updating project:', error);
            throw new Error('Failed to update project');
        }
    }

    async deleteProject(id: string, signal?: AbortSignal): Promise<void> {
        try {
            await apiService.delete(`/projects/${id}`, signal);
        } catch (error) {
            projectsLogger.error('Error deleting project:', error);
            throw new Error('Failed to delete project');
        }
    }
}

export default new ProjectsService();
