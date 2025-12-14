import { teamLogger } from '@/utils/logger';
import apiService from './apiService';
import { extractErrorMessage } from '@/utils/errorHandler';
import type { ApiResponse, CreateTeam, Team, UpdateTeam } from '@/types';

class TeamsService {
    async getTeams(signal?: AbortSignal): Promise<Team[]> {
        try {
            const response = await apiService.get<ApiResponse<Team[]>>('/teams', signal);
            return response.data;
        } catch (error) {
            teamLogger.error('Error fetching teams:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getTeamById(id: string): Promise<Team> {
        try {
            const response = await apiService.get<ApiResponse<Team>>(`/teams/${id}`);
            return response.data;
        } catch (error) {
            teamLogger.error('Error fetching team:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async createTeam(team: CreateTeam): Promise<Team> {
        try {
            const response = await apiService.post<ApiResponse<Team>>('/teams', team);
            return response.data;
        } catch (error) {
            teamLogger.error('Error creating team:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async updateTeam(id: string, team: UpdateTeam): Promise<Team> {
        try {
            const response = await apiService.put<ApiResponse<Team>>(`/teams/${id}`, team);
            return response.data;
        } catch (error) {
            teamLogger.error('Error updating team:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async deleteTeam(id: string): Promise<void> {
        try {
            await apiService.delete(`/teams/${id}`);
        } catch (error) {
            teamLogger.error('Error deleting team:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getTeamsByManager(managerId: string, signal?: AbortSignal): Promise<Team> {
        try {
            const response = await apiService.get<ApiResponse<Team>>(`/teams/manager/${managerId}`, signal);
            return response.data;
        } catch (error) {
            teamLogger.error('Error fetching teams by manager:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async getTeamByManager(managerId: string, signal?: AbortSignal): Promise<Team> {
        try {
            const response = await apiService.get<ApiResponse<Team>>(`/teams/manager/${managerId}`, signal);
            return response.data;
        } catch (error) {
            teamLogger.error('Error fetching team by manager:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async addDeveloperToTeam(teamId: string, developerId: string): Promise<void> {
        try {
            await apiService.post(`/teams/${teamId}/developers/${developerId}`, {});
            teamLogger.log(`Developer ${developerId} added to team ${teamId}`);
        } catch (error) {
            teamLogger.error('Error adding developer to team:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    async removeDeveloperFromTeam(teamId: string, developerId: string): Promise<void> {
        try {
            await apiService.delete(`/teams/${teamId}/developers/${developerId}`);
            teamLogger.log(`Developer ${developerId} removed from team ${teamId}`);
        } catch (error) {
            teamLogger.error('Error removing developer from team:', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }
}

export default new TeamsService();
