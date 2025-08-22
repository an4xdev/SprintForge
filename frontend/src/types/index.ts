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

export interface RegisterCredentials extends LoginCredentials {
    role: 'admin' | 'manager' | 'developer' | '';
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

export interface Sprint {
    id: string;
    name: string;
    startDate: Date;
    endDate: Date;
    teamId: string;
    managerId: string;
    projectId: string;
}

export interface Task {
    id: string;
    name: string;
    description: string;
    developerId: string | null;
    sprintId: string | null;
    taskStatusId: number;
    taskTypeId: number;
}

export interface AdminDashboard {
    companiesCount: number;
    usersCount: number;
    projectsCount: number;
    teamsCount: number;
}

export interface ManagerDashboardInfoDto {
    sprint: ManagerSprintDto | null;
    tasks: ManagerTaskDto[];
}

export interface ManagerSprintDto {
    id: string;
    name: string;
    active: boolean;
    startDate: Date;
    endDate: Date;
}

export interface ManagerTaskDto {
    statusId: number;
    statusName: string;
    count: number;
}

export type DeveloperTaskStatus = 'NONE' | 'STARTED' | 'PAUSED' | 'STOPPED';

export interface DeveloperTask {
    id: string;
    name: string;
    description: string;
    project: string;
    status: DeveloperTaskStatus;
    hours: number;
    minutes: number;
    seconds: number;
}
