import { AxiosError } from 'axios';

export interface ApiErrorDetails {
    message: string;
    status?: number;
    details?: string;
}
export function extractErrorMessage(error: any): ApiErrorDetails {
    if (error?.name === 'AbortError') {
        return {
            message: 'Request was cancelled',
            status: 0
        };
    }
    if (error?.response) {
        const axiosError = error as AxiosError;
        const status = axiosError.response?.status;


        if (typeof axiosError.response?.data === 'object' && axiosError.response?.data !== null) {
            const data = axiosError.response.data as any;

            if (data.message) {
                return {
                    message: data.message,
                    status,
                    details: data.details || data.data
                };
            }

            if (Array.isArray(data) && data.length > 0) {
                return {
                    message: data[0].message || data[0],
                    status
                };
            }

            if (data.error) {
                return {
                    message: data.error,
                    status
                };
            }
        }

        if (status && axiosError.response?.statusText) {
            const messages: { [key: number]: string } = {
                400: 'Bad request - please check your input',
                401: 'Unauthorized - please log in again',
                403: 'Forbidden - you do not have permission',
                404: 'Not found - the resource does not exist',
                409: 'Conflict - the resource already exists or cannot be modified',
                422: 'Validation error - please check your input',
                429: 'Too many requests - please try again later',
                500: 'Server error - please try again later',
                502: 'Bad gateway - service temporarily unavailable',
                503: 'Service unavailable - please try again later'
            };

            return {
                message: messages[status] || `Error ${status}: ${axiosError.response?.statusText}`,
                status
            };
        }

        return {
            message: 'An error occurred while processing your request',
            status
        };
    }

    if (error instanceof Error) {
        return {
            message: error.message,
            status: undefined
        };
    }

    return {
        message: 'An unexpected error occurred',
        status: undefined
    };
}

export function throwApiError(error: any, defaultMessage: string = 'Operation failed'): never {
    const errorDetails = extractErrorMessage(error);
    const message = errorDetails.message || defaultMessage;

    const err = new Error(message);
    (err as any).status = errorDetails.status;
    (err as any).details = errorDetails.details;

    throw err;
}
