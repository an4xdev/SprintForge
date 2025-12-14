import axios, { type AxiosResponse } from 'axios';
import { jwtDecode } from 'jwt-decode';
import type { ApiResponse, AuthResponse, JwtPayload, LoginCredentials, MinimalUser, User } from '@/types';
import { authLogger } from '@/utils/logger';
import { extractErrorMessage } from '@/utils/errorHandler';

class AuthService {
    private baseURL = import.meta.env.VITE_API_BASE_URL || '/api';

    async login(credentials: LoginCredentials): Promise<AuthResponse> {
        try {
            authLogger.info('Starting login process', { username: credentials.username });

            const response: AxiosResponse<ApiResponse<AuthResponse>> = await axios.post(
                `${this.baseURL}/auth/login`,
                credentials
            );
            const { accessToken, refreshToken } = response.data.data;

            localStorage.setItem('token', accessToken);
            localStorage.setItem('refreshToken', refreshToken);
            localStorage.setItem('user', JSON.stringify(this.parseTokenData(accessToken)));

            authLogger.info('Login successful', { userId: this.parseTokenData(accessToken).id });

            return response.data.data;
        } catch (error) {
            authLogger.error('Login failed', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message);
        }
    }

    logout(): void {
        authLogger.info('Starting logout process');

        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');

        authLogger.info('User data removed from localStorage');
    }

    getStoredToken(): string | null {
        return localStorage.getItem('token');
    }

    getStoredUser(): User | null {
        const userStr = localStorage.getItem('user');
        return userStr ? JSON.parse(userStr) : null;
    }

    mapClaims(decoded: any): MinimalUser & { role: string } {
        const mappedUser = {
            id: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier"] || '',
            username: decoded["http://schemas.xmlsoap.org/ws/2005/05/identity/claims/name"] || '',
            role: decoded["http://schemas.microsoft.com/ws/2008/06/identity/claims/role"] || ''
        };
        authLogger.debug('Mapping claims input', decoded);
        authLogger.debug('Mapping claims output', mappedUser);
        return mappedUser;
    }

    parseTokenData(token: string): MinimalUser & { role: string } {
        try {
            const decoded = jwtDecode(token);
            authLogger.debug('Parsed user data from token', decoded);
            const mappedUser = this.mapClaims(decoded);
            authLogger.debug('Mapped user data', mappedUser);
            return mappedUser;
        } catch (error) {
            authLogger.error('Token decoding error', error);
            const errorDetails = extractErrorMessage(error);
            throw new Error(errorDetails.message || 'Invalid JWT token');
        }
    }

    isAuthenticated(): boolean {
        const token = this.getStoredToken();
        if (!token) return false;

        try {
            const decoded = jwtDecode<JwtPayload>(token);
            return decoded.exp * 1000 > Date.now();
        } catch {
            return false;
        }
    }

    hasRole(role: string): boolean {
        const user = this.getStoredUser();
        return user?.role === role;
    }
}

export default new AuthService();