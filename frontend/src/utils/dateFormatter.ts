export interface DateFormatOptions {
    locale?: string;
    includeYear?: boolean;
    format?: 'short' | 'long' | 'numeric' | 'iso';
    timeZone?: string;
}

export class DateFormatter {
    private static defaultLocale = 'en-US';

    static formatDate(date: Date | string, options: DateFormatOptions = {}): string {
        const {
            locale = this.defaultLocale,
            includeYear = true,
            format = 'short',
            timeZone
        } = options;

        const dateObj = new Date(date);

        if (isNaN(dateObj.getTime())) {
            return 'Invalid Date';
        }

        const formatOptions: Intl.DateTimeFormatOptions = {
            timeZone
        };

        switch (format) {
            case 'short':
                formatOptions.month = 'short';
                formatOptions.day = 'numeric';
                if (includeYear) formatOptions.year = 'numeric';
                break;
            case 'long':
                formatOptions.month = 'long';
                formatOptions.day = 'numeric';
                if (includeYear) formatOptions.year = 'numeric';
                break;
            case 'numeric':
                formatOptions.month = '2-digit';
                formatOptions.day = '2-digit';
                if (includeYear) formatOptions.year = 'numeric';
                break;
            case 'iso':
                return dateObj.toISOString().split('T')[0];
        }

        return dateObj.toLocaleDateString(locale, formatOptions);
    }

    static formatDateRange(
        startDate: Date | string,
        endDate: Date | string,
        separator: string = ' - ',
        options: DateFormatOptions = {}
    ): string {
        const formattedStart = this.formatDate(startDate, options);
        const formattedEnd = this.formatDate(endDate, options);

        return `${formattedStart}${separator}${formattedEnd}`;
    }

    static calculateDuration(startDate: Date | string, endDate: Date | string): {
        days: number;
        weeks: number;
        months: number;
        displayText: string;
    } {
        const start = new Date(startDate);
        const end = new Date(endDate);

        if (isNaN(start.getTime()) || isNaN(end.getTime())) {
            return { days: 0, weeks: 0, months: 0, displayText: 'Invalid dates' };
        }

        const diffTime = Math.abs(end.getTime() - start.getTime());
        const days = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
        const weeks = Math.floor(days / 7);
        const months = Math.floor(days / 30);

        let displayText: string;
        if (days === 1) {
            displayText = '1 day';
        } else if (days < 7) {
            displayText = `${days} days`;
        } else if (days < 30) {
            displayText = `${weeks} week${weeks !== 1 ? 's' : ''}`;
        } else {
            displayText = `${months} month${months !== 1 ? 's' : ''}`;
        }

        return { days, weeks, months, displayText };
    }

    static getRelativeTime(date: Date | string, locale: string = this.defaultLocale): string {
        const dateObj = new Date(date);
        const now = new Date();

        if (isNaN(dateObj.getTime())) {
            return 'Invalid Date';
        }

        const diffTime = dateObj.getTime() - now.getTime();
        const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));

        if (diffDays === 0) {
            return 'Today';
        } else if (diffDays === 1) {
            return 'Tomorrow';
        } else if (diffDays === -1) {
            return 'Yesterday';
        } else if (diffDays > 0) {
            return `In ${diffDays} day${diffDays !== 1 ? 's' : ''}`;
        } else {
            return `${Math.abs(diffDays)} day${Math.abs(diffDays) !== 1 ? 's' : ''} ago`;
        }
    }

    static isToday(date: Date | string): boolean {
        const dateObj = new Date(date);
        const today = new Date();

        return dateObj.toDateString() === today.toDateString();
    }

    static isPast(date: Date | string): boolean {
        const dateObj = new Date(date);
        const now = new Date();

        return dateObj < now;
    }

    static isFuture(date: Date | string): boolean {
        const dateObj = new Date(date);
        const now = new Date();

        return dateObj > now;
    }

    static formatDateTime(
        date: Date | string,
        options: DateFormatOptions & { includeTime?: boolean } = {}
    ): string {
        const {
            locale = this.defaultLocale,
            includeYear = true,
            format = 'short',
            includeTime = false,
            timeZone
        } = options;

        const dateObj = new Date(date);

        if (isNaN(dateObj.getTime())) {
            return 'Invalid Date';
        }

        const formatOptions: Intl.DateTimeFormatOptions = {
            timeZone
        };

        switch (format) {
            case 'short':
                formatOptions.month = 'short';
                formatOptions.day = 'numeric';
                if (includeYear) formatOptions.year = 'numeric';
                break;
            case 'long':
                formatOptions.month = 'long';
                formatOptions.day = 'numeric';
                if (includeYear) formatOptions.year = 'numeric';
                break;
            case 'numeric':
                formatOptions.month = '2-digit';
                formatOptions.day = '2-digit';
                if (includeYear) formatOptions.year = 'numeric';
                break;
        }

        if (includeTime) {
            formatOptions.hour = '2-digit';
            formatOptions.minute = '2-digit';
        }

        return dateObj.toLocaleDateString(locale, formatOptions);
    }
}

export const formatDate = (date: Date | string, options: DateFormatOptions = {}) =>
    DateFormatter.formatDate(date, options);

export const formatDateRange = (
    startDate: Date | string,
    endDate: Date | string,
    separator: string = ' - ',
    options: DateFormatOptions = {}
) => DateFormatter.formatDateRange(startDate, endDate, separator, options);

export const calculateDuration = (startDate: Date | string, endDate: Date | string) =>
    DateFormatter.calculateDuration(startDate, endDate);

export const getRelativeTime = (date: Date | string, locale?: string) =>
    DateFormatter.getRelativeTime(date, locale);

export const isToday = (date: Date | string) =>
    DateFormatter.isToday(date);

export const isPast = (date: Date | string) =>
    DateFormatter.isPast(date);

export const isFuture = (date: Date | string) =>
    DateFormatter.isFuture(date);

export const formatDateTime = (
    date: Date | string,
    options: DateFormatOptions & { includeTime?: boolean } = {}
) => DateFormatter.formatDateTime(date, options);
