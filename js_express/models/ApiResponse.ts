class ApiResponse<T> {
    message: string;
    data: T | null;

    constructor(message: string, data: T | null,) {
        this.message = message;
        this.data = data;
    }
    static Success<T>(message: string, data: T): ApiResponse<T> {
        return new ApiResponse<T>(message, data,);
    }
    static Created<T>(message: string, data: T): ApiResponse<T> {
        return new ApiResponse<T>(message, data);
    }
    static NotFound<T>(message: string): ApiResponse<T> {
        return new ApiResponse<T>(message, null);
    }
    static BadRequest<T>(message: string): ApiResponse<T> {
        return new ApiResponse<T>(message, null);
    }
    static InternalError<T>(message: string): ApiResponse<T> {
        return new ApiResponse<T>(message, null);
    }
}

export default ApiResponse;