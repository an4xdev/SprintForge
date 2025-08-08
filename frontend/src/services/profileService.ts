import { profileLogger } from '@/utils/logger';
import apiService from './apiService';
import type { ApiResponse, AvatarChangeResponse, Profile } from '@/types';
import axios from 'axios';

class ProfileService {
    async getProfile(id: string, signal?: AbortSignal): Promise<Profile> {
        try {
            const response = await apiService.get<ApiResponse<Profile>>(`/profile/${id}`, signal);
            return response.data;
        } catch (error) {
            profileLogger.error('Error fetching profile:', error);
            throw new Error('Failed to fetch profile');
        }
    }

    async updateAvatar(file: File, userId: string, signal?: AbortSignal): Promise<AvatarChangeResponse | object> {
        try {
            const formData = new FormData();
            formData.append('file', file);
            formData.append('userId', userId);

            const baseURL = import.meta.env.VITE_API_BASE_URL || '/api';
            const token = localStorage.getItem('token');

            const response = await axios.post<ApiResponse<AvatarChangeResponse>>(
                `${baseURL}/profile/avatar`,
                formData,
                {
                    headers: {
                        'Content-Type': 'multipart/form-data',
                        ...(token && { Authorization: `Bearer ${token}` })
                    },
                    signal
                }
            );

            return { path: response.data.data.path };
        } catch (error) {
            profileLogger.error('Error updating avatar:', error);
            throw new Error('Failed to update avatar');
        }
    }
}

export default new ProfileService();
