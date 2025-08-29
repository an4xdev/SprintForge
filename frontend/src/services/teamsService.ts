import { teamLogger } from '@/utils/logger';
import apiService from './apiService';
import type { ApiResponse, CreateTeam, Team, UpdateTeam } from '@/types';

class TeamsService {
    async getTeams(signal?: AbortSignal): Promise<Team[]> {
        try {
            const response = await apiService.get<ApiResponse<Team[]>>('/teams', signal);
            return response.data;
        } catch (error) {
            teamLogger.error('Error fetching teams:', error);
            throw new Error('Failed to fetch teams');
        }
    }

    async getTeamById(id: string): Promise<Team> {
        try {
            const response = await apiService.get<ApiResponse<Team>>(`/teams/${id}`);
            return response.data;
        } catch (error) {
            teamLogger.error('Error fetching team:', error);
            throw new Error('Failed to fetch team');
        }
    }

    async createTeam(team: CreateTeam): Promise<Team> {
        try {
            const response = await apiService.post<ApiResponse<Team>>('/teams', team);
            return response.data;
        } catch (error) {
            teamLogger.error('Error creating team:', error);
            throw new Error('Failed to create team');
        }
    }

    async updateTeam(id: string, team: UpdateTeam): Promise<Team> {
        try {
            const response = await apiService.put<ApiResponse<Team>>(`/teams/${id}`, team);
            return response.data;
        } catch (error) {
            teamLogger.error('Error updating team:', error);
            throw new Error('Failed to update team');
        }
    }

    async deleteTeam(id: string): Promise<void> {
        try {
            await apiService.delete(`/teams/${id}`);
        } catch (error) {
            teamLogger.error('Error deleting team:', error);
            throw new Error('Failed to delete team');
        }
    }

    async getTeamsByManager(managerId: string, signal?: AbortSignal): Promise<Team> {
        try {
            const response = await apiService.get<ApiResponse<Team>>(`/teams/manager/${managerId}`, signal);
            return response.data;
        } catch (error) {
            teamLogger.error('Error fetching teams by manager:', error);
            throw new Error('Failed to fetch teams by manager');
        }
    }
}

export default new TeamsService();
