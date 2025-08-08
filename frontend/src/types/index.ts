export interface EmployeeTask {
    id: number;
    name: string;
    hours: number;
    minutes: number;
    seconds: number;
}

export interface ApiResponse<T> {
    data: T;
    message: string;
}

export interface User {
    id: string;
    username: string;
    role: 'admin' | 'manager' | 'developer';
}

export interface AuthResponse {
    accessToken: string;
    refreshToken: string;
}

export interface LoginCredentials {
    username: string;
    password: string;
}

export interface Company {
    id: number;
    name: string;
}

export interface JwtPayload {
    [key: string]: string | number | boolean;
    exp: number;
}

export interface MinimalUser {
    id: string;
    username: string;
}

export interface Team {
    id: string;
    name: string;
    manager: MinimalUser;
}

export interface TaskStatus {
    id: number;
    name: string;
}

export interface TaskType {
    id: number;
    name: string;
}

export interface Project {
    id: string;
    name: string;
    startDate: Date;
    endDate: Date;
    companyDto: Company;
}

export interface Profile {
    id: string;
    username: string;
    avatar: string | null;
}

export interface AvatarChangeResponse {
    path: string;
}