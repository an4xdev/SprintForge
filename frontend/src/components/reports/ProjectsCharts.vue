<template>
    <div class="projects-charts">
        <v-row>
            <!-- Projects Overview Chart -->
            <v-col cols="12" md="6">
                <v-card elevation="2">
                    <v-card-title class="text-h6">
                        <v-icon class="me-2">mdi-chart-pie</v-icon>
                        Przegląd projektów
                    </v-card-title>
                    <v-card-text class="d-flex justify-center align-center" style="min-height: 350px;">
                        <div v-if="projectData && projectData.length > 0" class="text-center">
                            <div class="position-relative d-inline-block">
                                <!-- Donut Chart using SVG -->
                                <svg width="250" height="250" class="donut-chart">
                                    <g v-for="(segment, index) in chartSegments" :key="index">
                                        <path :d="segment.path" :fill="segment.color" :stroke="'white'" stroke-width="2"
                                            class="chart-segment" @mouseover="hoveredSegment = index"
                                            @mouseleave="hoveredSegment = null" />
                                    </g>

                                    <!-- Center text -->
                                    <text x="125" y="115" text-anchor="middle" class="chart-center-text">
                                        <tspan class="chart-total">{{ projectData.length }}</tspan>
                                    </text>
                                    <text x="125" y="135" text-anchor="middle" class="chart-center-label">
                                        <tspan>Projektów</tspan>
                                    </text>
                                </svg>
                            </div>

                            <!-- Legend -->
                            <div class="mt-4">
                                <div class="legend-container">
                                    <div v-for="(category, index) in completionCategories" :key="category.label"
                                        class="legend-item"
                                        :class="{ 'legend-item-hovered': hoveredSegment === index }">
                                        <div class="legend-dot" :style="{ backgroundColor: category.color }"></div>
                                        <span class="text-body-2">{{ category.label }} ({{ category.count }})</span>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <div v-else class="text-center">
                            <v-icon size="64" class="mb-4 text-medium-emphasis">mdi-chart-pie</v-icon>
                            <p class="text-body-1 text-medium-emphasis">Brak danych do wyświetlenia</p>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>

            <!-- Top Projects by Completion -->
            <v-col cols="12" md="6">
                <v-card elevation="2">
                    <v-card-title class="text-h6">
                        <v-icon class="me-2">mdi-trophy</v-icon>
                        Najlepsze projekty
                    </v-card-title>
                    <v-card-text style="min-height: 350px;">
                        <div v-if="topProjects && topProjects.length > 0" class="top-projects">
                            <div v-for="(project, index) in topProjects" :key="project.projectId"
                                class="project-item mb-3">
                                <div class="d-flex align-center mb-2">
                                    <v-avatar :color="getRankColor(index)" size="32" class="me-3">
                                        <span class="text-white font-weight-bold">{{ index + 1 }}</span>
                                    </v-avatar>

                                    <div class="flex-grow-1">
                                        <div class="font-weight-medium">{{ project.projectName }}</div>
                                        <div class="text-caption text-medium-emphasis">{{ project.companyName }}</div>
                                    </div>

                                    <div class="text-right">
                                        <div class="text-h6 font-weight-bold">{{ Math.round(project.completedRatio *
                                            100) }}%</div>
                                        <div class="text-caption text-medium-emphasis">
                                            {{ project.taskCountCompleted }}/{{ project.taskCount }} zadań
                                        </div>
                                    </div>
                                </div>

                                <v-progress-linear :model-value="project.completedRatio * 100"
                                    :color="getProgressColor(project.completedRatio)" height="8" rounded class="mb-2" />
                            </div>
                        </div>

                        <div v-else class="text-center d-flex flex-column justify-center align-center"
                            style="height: 100%;">
                            <v-icon size="64" class="mb-4 text-medium-emphasis">mdi-trophy-outline</v-icon>
                            <p class="text-body-1 text-medium-emphasis">Brak danych do wyświetlenia</p>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row>

        <!-- Company Performance Chart -->
        <v-row class="mt-6">
            <v-col cols="12">
                <v-card elevation="2">
                    <v-card-title class="text-h6">
                        <v-icon class="me-2">mdi-domain</v-icon>
                        Wydajność firm
                    </v-card-title>
                    <v-card-text style="min-height: 300px;">
                        <div v-if="companyStats && companyStats.length > 0" class="company-performance">
                            <div v-for="company in companyStats" :key="company.name" class="company-bar mb-4">
                                <div class="d-flex justify-space-between align-center mb-2">
                                    <div class="company-info">
                                        <span class="text-body-1 font-weight-medium">{{ company.name }}</span>
                                        <div class="text-caption text-medium-emphasis">
                                            {{ company.projectCount }} projektów
                                        </div>
                                    </div>
                                    <div class="text-right">
                                        <span class="text-body-1 font-weight-bold">
                                            {{ Math.round(company.averageCompletion) }}%
                                        </span>
                                        <div class="text-caption text-medium-emphasis">
                                            {{ company.totalTasks }} zadań
                                        </div>
                                    </div>
                                </div>

                                <div class="progress-container">
                                    <v-progress-linear :model-value="company.averageCompletion"
                                        :color="getCompanyProgressColor(company.averageCompletion)" height="16" rounded
                                        class="company-progress">
                                        <template #default>
                                            <div class="progress-label">
                                                {{ Math.round(company.averageCompletion) }}%
                                            </div>
                                        </template>
                                    </v-progress-linear>
                                </div>
                            </div>
                        </div>

                        <div v-else class="text-center d-flex flex-column justify-center align-center"
                            style="height: 100%;">
                            <v-icon size="64" class="mb-4 text-medium-emphasis">mdi-domain</v-icon>
                            <p class="text-body-1 text-medium-emphasis">Brak danych do wyświetlenia</p>
                        </div>
                    </v-card-text>
                </v-card>
            </v-col>
        </v-row>
    </div>
</template>

<script setup lang="ts">
import { computed, ref } from 'vue';
import type { ProjectReportDto } from '@/types';

interface Props {
    projectData?: ProjectReportDto[] | null;
}

const props = withDefaults(defineProps<Props>(), {
    projectData: null
});

const hoveredSegment = ref<number | null>(null);

const completionCategories = computed(() => {
    if (!props.projectData || props.projectData.length === 0) return [];

    const categories = [
        { label: 'Ukończone (80-100%)', color: '#4CAF50', min: 0.8, max: 1.0, count: 0 },
        { label: 'W trakcie (50-79%)', color: '#FF9800', min: 0.5, max: 0.79, count: 0 },
        { label: 'Rozpoczęte (20-49%)', color: '#FFC107', min: 0.2, max: 0.49, count: 0 },
        { label: 'Planowane (0-19%)', color: '#F44336', min: 0.0, max: 0.19, count: 0 }
    ];

    props.projectData.forEach(project => {
        const ratio = project.completedRatio;
        const category = categories.find(cat => ratio >= cat.min && ratio <= cat.max);
        if (category) {
            category.count++;
        }
    });

    return categories.filter(cat => cat.count > 0);
});

const chartSegments = computed(() => {
    if (completionCategories.value.length === 0) return [];

    const total = completionCategories.value.reduce((sum, cat) => sum + cat.count, 0);
    const centerX = 125;
    const centerY = 125;
    const outerRadius = 90;
    const innerRadius = 50;

    let currentAngle = -Math.PI / 2; // Start at top

    return completionCategories.value.map(category => {
        const percentage = category.count / total;
        const angle = percentage * 2 * Math.PI;

        const x1 = centerX + Math.cos(currentAngle) * outerRadius;
        const y1 = centerY + Math.sin(currentAngle) * outerRadius;
        const x2 = centerX + Math.cos(currentAngle) * innerRadius;
        const y2 = centerY + Math.sin(currentAngle) * innerRadius;

        currentAngle += angle;

        const x3 = centerX + Math.cos(currentAngle) * innerRadius;
        const y3 = centerY + Math.sin(currentAngle) * innerRadius;
        const x4 = centerX + Math.cos(currentAngle) * outerRadius;
        const y4 = centerY + Math.sin(currentAngle) * outerRadius;

        const largeArcFlag = angle > Math.PI ? 1 : 0;

        const path = [
            `M ${x1} ${y1}`,
            `A ${outerRadius} ${outerRadius} 0 ${largeArcFlag} 1 ${x4} ${y4}`,
            `L ${x3} ${y3}`,
            `A ${innerRadius} ${innerRadius} 0 ${largeArcFlag} 0 ${x2} ${y2}`,
            'Z'
        ].join(' ');

        return {
            path,
            color: category.color,
            percentage
        };
    });
});

const topProjects = computed(() => {
    if (!props.projectData || props.projectData.length === 0) return [];

    return [...props.projectData]
        .sort((a, b) => b.completedRatio - a.completedRatio)
        .slice(0, 5);
});

const companyStats = computed(() => {
    if (!props.projectData || props.projectData.length === 0) return [];

    const companies = new Map<string, {
        name: string;
        projectCount: number;
        totalCompletion: number;
        totalTasks: number;
        averageCompletion: number;
    }>();

    props.projectData.forEach(project => {
        const companyName = project.companyName;
        if (!companies.has(companyName)) {
            companies.set(companyName, {
                name: companyName,
                projectCount: 0,
                totalCompletion: 0,
                totalTasks: 0,
                averageCompletion: 0
            });
        }

        const company = companies.get(companyName)!;
        company.projectCount++;
        company.totalCompletion += project.completedRatio * 100;
        company.totalTasks += project.taskCount;
    });

    const companiesArray = Array.from(companies.values());

    companiesArray.forEach(company => {
        company.averageCompletion = company.totalCompletion / company.projectCount;
    });

    return companiesArray.sort((a, b) => b.averageCompletion - a.averageCompletion);
});

const getProgressColor = (ratio: number): string => {
    if (ratio >= 0.8) return 'success';
    if (ratio >= 0.5) return 'warning';
    return 'error';
};

const getCompanyProgressColor = (percentage: number): string => {
    if (percentage >= 80) return 'success';
    if (percentage >= 60) return 'info';
    if (percentage >= 40) return 'warning';
    return 'error';
};

const getRankColor = (index: number): string => {
    const colors = ['#FFD700', '#C0C0C0', '#CD7F32', '#4CAF50', '#2196F3'];
    return colors[index] || '#9E9E9E';
};
</script>

<style scoped>
.projects-charts {
    width: 100%;
}

.donut-chart {
    transform: rotate(0deg);
}

.chart-segment {
    transition: all 0.3s ease;
    cursor: pointer;
}

.chart-segment:hover {
    filter: brightness(1.1);
    stroke-width: 3;
}

.chart-center-text .chart-total {
    font-size: 24px;
    font-weight: bold;
    fill: var(--v-theme-on-surface);
}

.chart-center-label {
    font-size: 12px;
    fill: var(--v-theme-on-surface-variant);
}

.legend-container {
    display: flex;
    flex-wrap: wrap;
    justify-content: center;
    gap: 16px;
}

.legend-item {
    display: flex;
    align-items: center;
    transition: all 0.3s ease;
}

.legend-item-hovered {
    transform: scale(1.1);
    font-weight: 600;
}

.legend-dot {
    width: 12px;
    height: 12px;
    border-radius: 50%;
    margin-right: 8px;
    flex-shrink: 0;
}

.top-projects .project-item {
    padding: 12px;
    border-radius: 8px;
    background-color: rgba(var(--v-theme-surface-variant), 0.1);
    transition: all 0.3s ease;
}

.top-projects .project-item:hover {
    background-color: rgba(var(--v-theme-surface-variant), 0.2);
    transform: translateX(4px);
}

.company-performance .company-bar {
    padding: 16px;
    border-radius: 8px;
    background-color: rgba(var(--v-theme-surface-variant), 0.1);
    transition: all 0.3s ease;
}

.company-performance .company-bar:hover {
    background-color: rgba(var(--v-theme-surface-variant), 0.2);
    transform: translateY(-2px);
    box-shadow: 0 4px 8px rgba(0, 0, 0, 0.1);
}

.progress-container {
    position: relative;
}

.company-progress {
    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
}

.progress-label {
    color: white;
    font-size: 11px;
    font-weight: 600;
    text-shadow: 1px 1px 2px rgba(0, 0, 0, 0.5);
}

.company-info {
    max-width: 60%;
}

@keyframes fadeInUp {
    from {
        opacity: 0;
        transform: translateY(20px);
    }

    to {
        opacity: 1;
        transform: translateY(0);
    }
}

.project-item,
.company-bar {
    animation: fadeInUp 0.6s ease-out;
}

.project-item:nth-child(1) {
    animation-delay: 0.1s;
}

.project-item:nth-child(2) {
    animation-delay: 0.2s;
}

.project-item:nth-child(3) {
    animation-delay: 0.3s;
}

.project-item:nth-child(4) {
    animation-delay: 0.4s;
}

.project-item:nth-child(5) {
    animation-delay: 0.5s;
}

.company-bar:nth-child(1) {
    animation-delay: 0.1s;
}

.company-bar:nth-child(2) {
    animation-delay: 0.2s;
}

.company-bar:nth-child(3) {
    animation-delay: 0.3s;
}

.company-bar:nth-child(4) {
    animation-delay: 0.4s;
}
</style>