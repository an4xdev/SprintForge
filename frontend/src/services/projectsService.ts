import { projectsLogger } from '@/utils/logger';
import apiService from './apiService';
import type { ApiResponse, Project, ProjectExt } from '@/types';

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

    async getCurrentProjectByManagerId(id: string, signal?: AbortSignal): Promise<string | null> {
        try {
            const response = await apiService.get<ApiResponse<string>>(`/projects/current/${id}`, signal);
            return response.data || null;
        } catch (error) {
            projectsLogger.error('Error fetching current project by manager ID:', error);
            throw new Error('Failed to fetch current project');
        }
    }

    async getProjectsExt(signal?: AbortSignal): Promise<ProjectExt[]> {
        try {
            const response = await apiService.get<ApiResponse<ProjectExt[]>>('/projects/all-ext', signal);
            return response.data;
        } catch (error) {
            projectsLogger.error('Error fetching extended projects:', error);
            throw new Error('Failed to fetch extended projects');
        }
    }

    async getProjectExtById(id: string, signal?: AbortSignal): Promise<ProjectExt> {
        try {
            const response = await apiService.get<ApiResponse<ProjectExt>>(`/projects/${id}-ext`, signal);
            return response.data;
        } catch (error) {
            projectsLogger.error('Error fetching extended project:', error);
            throw new Error('Failed to fetch extended project');
        }
    }
}

export default new ProjectsService();
