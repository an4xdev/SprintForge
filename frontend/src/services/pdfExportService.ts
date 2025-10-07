import jsPDF from 'jspdf';
import html2canvas from 'html2canvas';
import type { TeamReportDto, SprintReportDto, ProjectReportDto } from '@/types';

export class PdfExportService {
    private doc: jsPDF | null = null;
    private currentY = 20;
    private pageHeight = 297;
    private margin = 20;
    private lineHeight = 6;
    private colors = {
        primary: '#1976D2',
        secondary: '#424242',
        success: '#4CAF50',
        warning: '#FF9800',
        error: '#F44336',
        text: '#212121',
        lightGray: '#F5F5F5'
    };

    private initDocument(title: string): void {
        this.doc = new jsPDF();
        this.currentY = 20;

        this.doc.setFillColor(23, 118, 210);
        this.doc.rect(0, 0, 210, 40, 'F');

        this.doc.setFontSize(18);
        this.doc.setFont('helvetica', 'bold');
        this.doc.setTextColor(255, 255, 255);
        this.doc.text(title, this.margin, 25);

        this.doc.setFontSize(10);
        this.doc.setFont('helvetica', 'normal');
        const dateStr = `Generated: ${new Date().toLocaleDateString('en-GB')} ${new Date().toLocaleTimeString('en-GB')}`;
        this.doc.text(dateStr, this.margin, 35);
        this.doc.setTextColor(33, 33, 33);
        this.currentY = 50;
    }

    private checkNewPage(requiredHeight: number): void {
        if (!this.doc) return;

        if (this.currentY + requiredHeight > this.pageHeight - this.margin) {
            this.doc.addPage();
            this.currentY = this.margin;
        }
    }

    private addSectionHeader(title: string): void {
        if (!this.doc) return;

        this.checkNewPage(15);
        this.currentY += 5;

        this.doc.setFillColor(245, 245, 245);
        this.doc.rect(this.margin, this.currentY - 3, 170, 10, 'F');

        this.doc.setFontSize(12);
        this.doc.setFont('helvetica', 'bold');
        this.doc.setTextColor(33, 33, 33);
        this.doc.text(title, this.margin + 2, this.currentY + 4);
        this.currentY += 15;
    }

    private addText(text: string, fontSize: number = 10, style: 'normal' | 'bold' = 'normal'): void {
        if (!this.doc) return;

        this.checkNewPage(this.lineHeight);
        this.doc.setFontSize(fontSize);
        this.doc.setFont('helvetica', style);
        this.doc.text(text, this.margin, this.currentY);
        this.currentY += this.lineHeight;
    }

    private addKeyValue(key: string, value: string | number): void {
        if (!this.doc) return;

        this.checkNewPage(this.lineHeight);
        this.doc.setFontSize(10);
        this.doc.setFont('helvetica', 'bold');
        this.doc.text(`${key}:`, this.margin, this.currentY);

        this.doc.setFont('helvetica', 'normal');
        this.doc.text(String(value), this.margin + 60, this.currentY);
        this.currentY += this.lineHeight;
    }

    private addTable(headers: string[], rows: string[][], title?: string): void {
        if (!this.doc || headers.length === 0) return;

        if (title) {
            this.addSectionHeader(title);
        }

        const tableWidth = 170;
        const colWidth = tableWidth / headers.length;
        const rowHeight = 8;

        this.checkNewPage((rows.length + 2) * rowHeight);

        this.doc.setFillColor(23, 118, 210);
        this.doc.rect(this.margin, this.currentY, tableWidth, rowHeight, 'F');

        this.doc.setFontSize(10);
        this.doc.setFont('helvetica', 'bold');
        this.doc.setTextColor(255, 255, 255);

        headers.forEach((header, index) => {
            const x = this.margin + (index * colWidth) + 2;
            this.doc!.text(header, x, this.currentY + 5);
        });

        this.currentY += rowHeight;

        this.doc.setTextColor(33, 33, 33);
        this.doc.setFont('helvetica', 'normal');

        rows.forEach((row, rowIndex) => {
            if (rowIndex % 2 === 0) {
                this.doc!.setFillColor(250, 250, 250);
                this.doc!.rect(this.margin, this.currentY, tableWidth, rowHeight, 'F');
            }

            row.forEach((cell, colIndex) => {
                const x = this.margin + (colIndex * colWidth) + 2;
                this.doc!.text(cell.toString(), x, this.currentY + 5);
            });

            this.currentY += rowHeight;
        });

        this.doc.setDrawColor(200, 200, 200);
        this.doc.rect(this.margin, this.currentY - (rows.length + 1) * rowHeight, tableWidth, (rows.length + 1) * rowHeight);

        this.currentY += 10;
    }

    private addProgressChart(label: string, percentage: number, color: string = '#1976D2'): void {
        if (!this.doc) return;

        this.checkNewPage(20);

        this.doc.setFontSize(10);
        this.doc.setFont('helvetica', 'normal');
        this.doc.setTextColor(33, 33, 33);
        this.doc.text(label, this.margin, this.currentY);

        this.doc.setFillColor(230, 230, 230);
        this.doc.rect(this.margin + 60, this.currentY - 4, 100, 8, 'F');

        const rgb = this.hexToRgb(color);
        this.doc.setFillColor(rgb.r, rgb.g, rgb.b);
        this.doc.rect(this.margin + 60, this.currentY - 4, (100 * percentage) / 100, 8, 'F');

        this.doc.setFontSize(9);
        this.doc.text(`${percentage}%`, this.margin + 165, this.currentY);

        this.currentY += 12;
    }

    private hexToRgb(hex: string): { r: number; g: number; b: number } {
        const result = /^#?([a-f\d]{2})([a-f\d]{2})([a-f\d]{2})$/i.exec(hex);
        return result ? {
            r: parseInt(result[1], 16),
            g: parseInt(result[2], 16),
            b: parseInt(result[3], 16)
        } : { r: 0, g: 0, b: 0 };
    }

    private formatDuration(timeString: string): string {
        if (!timeString || timeString === '00:00:00') {
            return '0h 0m';
        }

        const parts = timeString.split(':');
        if (parts.length >= 2) {
            const hours = parseInt(parts[0]);
            const minutes = parseInt(parts[1]);

            if (hours > 0) {
                return `${hours}h ${minutes}m`;
            }
            return `${minutes}m`;
        }

        return timeString;
    }

    async exportTeamReport(data: TeamReportDto[], title: string): Promise<void> {
        this.initDocument(title);

        if (!this.doc) return;

        if (data.length === 0) {
            this.addText('No team data available to display.');
            this.doc.save(`${title.replace(/[^a-zA-Z0-9]/g, '_')}.pdf`);
            return;
        }

        let totalDevelopers = 0;
        let totalTasks = 0;
        let totalCompleted = 0;
        const allSprints = new Set<string>();

        data.forEach(team => {
            totalDevelopers += team.developerCount;
            totalTasks += team.taskCount;
            totalCompleted += team.taskCountCompleted;
            if (team.sprintsNames) {
                team.sprintsNames.forEach(sprint => allSprints.add(sprint));
            }
        });

        this.addSectionHeader('Team Overview');

        const overallProgress = totalTasks > 0 ? Math.round((totalCompleted / totalTasks) * 100) : 0;
        this.addProgressChart('Overall Progress', overallProgress, this.colors.success);
        this.addProgressChart('Development Capacity', Math.min(100, (totalDevelopers * 10)), this.colors.primary);

        const headers = ['Team', 'Developers', 'Total Tasks', 'Completed', 'Progress', 'Time Spent'];
        const rows = data.map((team, index) => [
            `Team ${index + 1}`,
            team.developerCount.toString(),
            team.taskCount.toString(),
            team.taskCountCompleted.toString(),
            `${Math.round((team.taskCountCompleted / team.taskCount) * 100)}%`,
            this.formatDuration(team.totalTaskTime)
        ]);

        this.addTable(headers, rows, 'Team Details');

        if (allSprints.size > 0) {
            const sprintHeaders = ['Sprint Name', 'Teams Involved'];
            const sprintRows = Array.from(allSprints).map(sprint => [
                sprint,
                data.filter(team => team.sprintsNames?.includes(sprint)).length.toString()
            ]);

            this.addTable(sprintHeaders, sprintRows, 'Active Sprints');
        }

        this.addSectionHeader('Summary Statistics');
        this.addKeyValue('Total Developers', totalDevelopers);
        this.addKeyValue('Total Tasks', totalTasks);
        this.addKeyValue('Completed Tasks', totalCompleted);
        this.addKeyValue('Overall Progress', `${overallProgress}%`);
        this.addKeyValue('Active Sprints', allSprints.size);

        this.doc.save(`${title.replace(/[^a-zA-Z0-9]/g, '_')}.pdf`);
    }

    async exportSprintReports(data: SprintReportDto[], title: string): Promise<void> {
        this.initDocument(title);

        if (!this.doc) return;

        if (data.length === 0) {
            this.addText('No sprint data available.');
            this.doc.save(`${title.replace(/[^a-zA-Z0-9]/g, '_')}.pdf`);
            return;
        }

        this.addSectionHeader('Sprint Overview');

        const totalTasks = data.reduce((sum, sprint) => sum + sprint.taskCount, 0);
        const totalCompleted = data.reduce((sum, sprint) => sum + sprint.taskCountCompleted, 0);
        const avgProgress = totalTasks > 0 ? Math.round((totalCompleted / totalTasks) * 100) : 0;

        this.addProgressChart('Overall Sprint Progress', avgProgress, this.colors.success);

        data.slice(0, 3).forEach(sprint => {
            const progress = Math.round(sprint.completedRatio * 100);
            this.addProgressChart(`${sprint.sprintName} Progress`, progress, this.colors.primary);
        });

        const headers = ['Sprint Name', 'Total Tasks', 'Completed', 'Progress', 'Time Spent'];
        const rows = data.map(sprint => [
            sprint.sprintName,
            sprint.taskCount.toString(),
            sprint.taskCountCompleted.toString(),
            `${Math.round(sprint.completedRatio * 100)}%`,
            this.formatDuration(sprint.totalTaskTime)
        ]);

        this.addTable(headers, rows, 'Sprint Details');

        this.addSectionHeader('Performance Analysis');
        const highPerformingSprints = data.filter(s => s.completedRatio >= 0.8).length;
        const lowPerformingSprints = data.filter(s => s.completedRatio < 0.5).length;

        this.addKeyValue('Total Sprints', data.length);
        this.addKeyValue('High Performing Sprints (≥80%)', highPerformingSprints);
        this.addKeyValue('Low Performing Sprints (<50%)', lowPerformingSprints);
        this.addKeyValue('Average Progress', `${avgProgress}%`);

        this.doc.save(`${title.replace(/[^a-zA-Z0-9]/g, '_')}.pdf`);
    }

    async exportProjectReports(data: ProjectReportDto[], title: string): Promise<void> {
        this.initDocument(title);

        if (!this.doc) return;

        if (data.length === 0) {
            this.addText('No project data available.');
            this.doc.save(`${title.replace(/[^a-zA-Z0-9]/g, '_')}.pdf`);
            return;
        }

        const totalProjects = data.length;
        const totalTasks = data.reduce((sum, project) => sum + project.taskCount, 0);
        const totalCompletedTasks = data.reduce((sum, project) => sum + project.taskCountCompleted, 0);
        const averageCompletion = totalTasks > 0 ? Math.round((totalCompletedTasks / totalTasks) * 100) : 0;

        this.addSectionHeader('Project Overview');

        this.addProgressChart('Overall Project Progress', averageCompletion, this.colors.success);

        const topProjects = [...data]
            .sort((a, b) => b.completedRatio - a.completedRatio)
            .slice(0, 3);

        topProjects.forEach(project => {
            const progress = Math.round(project.completedRatio * 100);
            this.addProgressChart(`${project.projectName}`, progress, this.colors.primary);
        });

        const headers = ['Project Name', 'Company', 'Total Tasks', 'Completed', 'Progress', 'Time Spent'];
        const rows = data.map(project => [
            project.projectName,
            project.companyName,
            project.taskCount.toString(),
            project.taskCountCompleted.toString(),
            `${Math.round(project.completedRatio * 100)}%`,
            this.formatDuration(project.totalTaskTime)
        ]);

        this.addTable(headers, rows, 'Project Details');


        this.addSectionHeader('Performance Analysis');
        const highPerformingProjects = data.filter(p => p.completedRatio >= 0.8).length;
        const lowPerformingProjects = data.filter(p => p.completedRatio < 0.5).length;

        this.addKeyValue('Total Projects', totalProjects);
        this.addKeyValue('High Performing Projects (≥80%)', highPerformingProjects);
        this.addKeyValue('Low Performing Projects (<50%)', lowPerformingProjects);
        this.addKeyValue('Average Project Progress', `${averageCompletion}%`);
        this.addKeyValue('Total Tasks Across All Projects', totalTasks);
        this.addKeyValue('Total Completed Tasks', totalCompletedTasks);

        const companies = [...new Set(data.map(p => p.companyName))];
        if (companies.length > 1) {
            const companyHeaders = ['Company', 'Projects', 'Total Tasks', 'Avg Progress'];
            const companyRows = companies.map(company => {
                const companyProjects = data.filter(p => p.companyName === company);
                const companyTasks = companyProjects.reduce((sum, p) => sum + p.taskCount, 0);
                const companyCompleted = companyProjects.reduce((sum, p) => sum + p.taskCountCompleted, 0);
                const companyProgress = companyTasks > 0 ? Math.round((companyCompleted / companyTasks) * 100) : 0;

                return [
                    company,
                    companyProjects.length.toString(),
                    companyTasks.toString(),
                    `${companyProgress}%`
                ];
            });

            this.addTable(companyHeaders, companyRows, 'Company Breakdown');
        }

        this.doc.save(`${title.replace(/[^a-zA-Z0-9]/g, '_')}.pdf`);
    }

    async exportChartToPdf(chartElement: HTMLElement, title: string): Promise<void> {
        try {
            const canvas = await html2canvas(chartElement, {
                backgroundColor: '#ffffff',
                scale: 2,
                useCORS: true
            });

            this.initDocument(title);

            if (!this.doc) return;

            const imgData = canvas.toDataURL('image/png');
            const imgWidth = 170;
            const imgHeight = (canvas.height * imgWidth) / canvas.width;

            this.checkNewPage(imgHeight);
            this.doc.addImage(imgData, 'PNG', this.margin, this.currentY, imgWidth, imgHeight);

            this.doc.save(`${title.replace(/[^a-zA-Z0-9]/g, '_')}.pdf`);
        } catch (error) {
            console.error('Failed to export chart to PDF:', error);
            throw error;
        }
    }
}

export default new PdfExportService();