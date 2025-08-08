import type { JwtPayload } from '@/types';
import { apiLogger } from '@/utils/logger';
import axios, { type AxiosInstance, type AxiosResponse } from 'axios';
import { jwtDecode } from 'jwt-decode';

class ApiService {
    private api: AxiosInstance;
    private baseURL = import.meta.env.VITE_API_BASE_URL || '/api';
    private isLoggingOut = false;
    private pendingRequests = new Map<string, AbortController>();

    constructor() {
        this.api = axios.create({
            baseURL: this.baseURL,
            timeout: 10000,
        });

        this.setupInterceptors();
    }

    private setupInterceptors() {
        this.api.interceptors.request.use(
            (config) => {
                if (this.isLoggingOut) {
                    return config;
                }

                const token = this.getStoredToken();
                if (token) {
                    config.headers.Authorization = `Bearer ${token}`;
                }
                return config;
            },
            (error) => Promise.reject(error)
        );

        this.api.interceptors.response.use(
            (response) => response,
            async (error) => {
                const originalRequest = error.config;

                if (error.response?.status === 401 && !originalRequest._retry && !this.isLoggingOut) {
                    originalRequest._retry = true;

                    try {
                        const newToken = await this.refreshToken();
                        originalRequest.headers.Authorization = `Bearer ${newToken}`;
                        return this.api(originalRequest);
                    } catch (refreshError) {
                        this.logout();
                        if (!this.isLoggingOut) {
                            window.dispatchEvent(new CustomEvent('auth-error'));
                        }
                        return Promise.reject(refreshError);
                    }
                }

                if (error.response?.status === 401 && !localStorage.getItem('refreshToken') && !this.isLoggingOut) {
                    this.logout();
                    window.dispatchEvent(new CustomEvent('auth-error'));
                }

                return Promise.reject(error);
            }
        );
    }

    private getStoredToken(): string | null {
        return localStorage.getItem('token');
    }

    private async refreshToken(): Promise<string> {
        if (this.isLoggingOut) {
            throw new Error('Logging out in progress');
        }

        const refreshToken = localStorage.getItem('refreshToken');

        if (!refreshToken) {
            throw new Error('No refresh token found');
        }

        try {
            const response: AxiosResponse<{ token: string }> = await axios.post(
                `${this.baseURL}/auth/refresh`,
                { refreshToken }
            );

            const newToken = response.data.token;
            localStorage.setItem('token', newToken);

            return newToken;
        } catch (error) {
            localStorage.removeItem('token');
            localStorage.removeItem('refreshToken');
            throw error;
        }
    }

    private logout(): void {
        localStorage.removeItem('token');
        localStorage.removeItem('refreshToken');
        localStorage.removeItem('user');
    }

    public setLoggingOut(value: boolean): void {
        apiLogger.log('setLoggingOut:', value);
        this.isLoggingOut = value;

        if (value) {
            apiLogger.log('Cancelling all pending requests');
            this.cancelAllRequests();
        }
    }

    private cancelAllRequests(): void {
        this.pendingRequests.forEach((controller) => {
            controller.abort();
        });
        this.pendingRequests.clear();
    }

    async get<T>(url: string, signal?: AbortSignal): Promise<T> {
        const config = signal ? { signal } : {};
        const response = await this.api.get(url, config);
        return response.data;
    }

    async post<T>(url: string, data?: any, signal?: AbortSignal): Promise<T> {
        const config = signal ? { signal } : {};
        const response = await this.api.post(url, data, config);
        return response.data;
    }

    async put<T>(url: string, data?: any, signal?: AbortSignal): Promise<T> {
        const config = signal ? { signal } : {};
        const response = await this.api.put(url, data, config);
        return response.data;
    }

    async delete<T>(url: string, signal?: AbortSignal): Promise<T> {
        const config = signal ? { signal } : {};
        const response = await this.api.delete(url, config);
        return response.data;
    }

    async patch<T>(url: string, data?: any, signal?: AbortSignal): Promise<T> {
        const config = signal ? { signal } : {};
        const response = await this.api.patch(url, data, config);
        return response.data;
    }

    isTokenValid(): boolean {
        const token = this.getStoredToken();
        if (!token) return false;

        try {
            const decoded = jwtDecode<JwtPayload>(token);
            return decoded.exp * 1000 > Date.now();
        } catch {
            return false;
        }
    }
}

export default new ApiService();
