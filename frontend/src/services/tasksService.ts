import { tasksLogger } from '@/utils/logger';
import apiService from './apiService';
import type { ApiResponse, Task, DeveloperTask, CreateTask } from '@/types';

class TasksService {
    async getTasks(signal?: AbortSignal): Promise<Task[]> {
        try {
            const response = await apiService.get<ApiResponse<Task[]>>('/tasks', signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching tasks:', error);
            throw new Error('Failed to fetch tasks');
        }
    }

    async getTaskById(id: string, signal?: AbortSignal): Promise<Task> {
        try {
            const response = await apiService.get<ApiResponse<Task>>(`/tasks/${id}`, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching task:', error);
            throw new Error('Failed to fetch task');
        }
    }

    async createTask(task: CreateTask, signal?: AbortSignal): Promise<Task> {
        try {
            const response = await apiService.post<ApiResponse<Task>>('/tasks', task, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error creating task:', error);
            throw new Error('Failed to create task');
        }
    }

    async updateTask(id: string, task: Partial<Task>, signal?: AbortSignal): Promise<Task> {
        try {
            const response = await apiService.put<ApiResponse<Task>>(`/tasks/${id}`, task, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error updating task:', error);
            throw new Error('Failed to update task');
        }
    }

    async deleteTask(id: string, signal?: AbortSignal): Promise<void> {
        try {
            await apiService.delete(`/tasks/${id}`, signal);
        } catch (error) {
            tasksLogger.error('Error deleting task:', error);
            throw new Error('Failed to delete task');
        }
    }

    async getUnassignedTasksByProject(projectId: string, signal?: AbortSignal): Promise<Task[]> {
        try {
            const response = await apiService.get<ApiResponse<Task[]>>(`/tasks/unassigned/project/${projectId}`, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching unassigned tasks by project:', error);
            throw new Error('Failed to fetch unassigned tasks by project');
        }
    }

    async getUnassignedTasksBySprint(sprintId: string, signal?: AbortSignal): Promise<Task[]> {
        try {
            const response = await apiService.get<ApiResponse<Task[]>>(`/tasks/unassigned/sprint/${sprintId}`, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching unassigned tasks by sprint:', error);
            throw new Error('Failed to fetch unassigned tasks by sprint');
        }
    }

    async getAssignedTasksBySprint(sprintId: string, signal?: AbortSignal): Promise<Task[]> {
        try {
            const response = await apiService.get<ApiResponse<Task[]>>(`/tasks/assigned/sprint/${sprintId}`, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching assigned tasks by sprint:', error);
            throw new Error('Failed to fetch assigned tasks by sprint');
        }
    }

    async getTasksByDeveloper(developerId: string, signal?: AbortSignal): Promise<Task[]> {
        try {
            const response = await apiService.get<ApiResponse<Task[]>>(`/tasks/developer/${developerId}`, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error fetching tasks by developer:', error);
            throw new Error('Failed to fetch tasks by developer');
        }
    }

    async assignDeveloper(taskId: string, developerId: string, signal?: AbortSignal): Promise<Task> {
        try {
            const response = await apiService.patch<ApiResponse<Task>>(`/tasks/${taskId}/assign-developer`, { developerId }, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error assigning developer to task:', error);
            throw new Error('Failed to assign developer to task');
        }
    }

    async moveToSprint(taskId: string, sprintId: string, signal?: AbortSignal): Promise<Task> {
        try {
            const response = await apiService.patch<ApiResponse<Task>>(`/tasks/${taskId}/move-to-sprint`, { sprintId }, signal);
            return response.data;
        } catch (error) {
            tasksLogger.error('Error moving task to sprint:', error);
            throw new Error('Failed to move task to sprint');
        }
    }

    mapTaskToDeveloperTask(task: Task, projectName: string = 'Unknown Project'): DeveloperTask {
        return {
            id: task.id,
            name: task.name,
            description: task.description,
            project: projectName,
            status: 'NONE',
            hours: 0,
            minutes: 0,
            seconds: 0
        };
    }
}

export default new TasksService();
