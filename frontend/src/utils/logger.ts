type LogLevel = 'log' | 'info' | 'warn' | 'error' | 'debug';

interface LoggerOptions {
    prefix?: string;
    enableInProduction?: boolean;
}

class DevelopmentLogger {
    private isDevelopment: boolean;
    private prefix: string;
    private enableInProduction: boolean;

    constructor(options: LoggerOptions = {}) {
        this.isDevelopment = import.meta.env.MODE === 'development' || import.meta.env.DEV;
        this.prefix = options.prefix || '[App]';
        this.enableInProduction = options.enableInProduction || false;
    }

    private shouldLog(): boolean {
        return this.isDevelopment || this.enableInProduction;
    }

    private formatMessage(message: string, data?: any): [string, ...any[]] {
        const timestamp = new Date().toISOString().split('T')[1].split('.')[0];
        const formattedMessage = `${this.prefix} [${timestamp}] ${message}`;

        if (data !== undefined) {
            return [formattedMessage, data];
        }
        return [formattedMessage];
    }

    log(message: string, data?: any): void {
        if (!this.shouldLog()) return;
        const args = this.formatMessage(message, data);
        console.log(...args);
    }

    info(message: string, data?: any): void {
        if (!this.shouldLog()) return;
        const args = this.formatMessage(message, data);
        console.info(...args);
    }

    warn(message: string, data?: any): void {
        if (!this.shouldLog()) return;
        const args = this.formatMessage(message, data);
        console.warn(...args);
    }

    error(message: string, data?: any): void {
        if (!this.shouldLog()) return;
        const args = this.formatMessage(message, data);
        console.error(...args);
    }

    debug(message: string, data?: any): void {
        if (!this.shouldLog()) return;
        const args = this.formatMessage(message, data);
        console.debug(...args);
    }

    group(label: string): void {
        if (!this.shouldLog()) return;
        console.group(`${this.prefix} ${label}`);
    }

    groupEnd(): void {
        if (!this.shouldLog()) return;
        console.groupEnd();
    }

    table(data: any): void {
        if (!this.shouldLog()) return;
        console.table(data);
    }

    time(label: string): void {
        if (!this.shouldLog()) return;
        console.time(`${this.prefix} ${label}`);
    }

    timeEnd(label: string): void {
        if (!this.shouldLog()) return;
        console.timeEnd(`${this.prefix} ${label}`);
    }
}

export { DevelopmentLogger };

export const authLogger = new DevelopmentLogger({ prefix: '[AuthService]' });
export const apiLogger = new DevelopmentLogger({ prefix: '[ApiService]' });
export const appLogger = new DevelopmentLogger({ prefix: '[App]' });
export const companyLogger = new DevelopmentLogger({ prefix: '[CompanyService]' });
export const teamLogger = new DevelopmentLogger({ prefix: '[TeamService]' });
export const routerLogger = new DevelopmentLogger({ prefix: '[Router]' });
export const viteLogger = new DevelopmentLogger({ prefix: '[Vite]' });

export default new DevelopmentLogger();
