<template>
    <v-card class="date-range-picker" elevation="2">
        <v-card-title class="text-h6">
            <v-icon class="me-2">mdi-calendar-range</v-icon>
            Select report date range
        </v-card-title>

        <v-card-text>
            <!-- Predefined periods -->
            <div class="mb-4">
                <v-chip-group v-model="selectedPreset" column @update:model-value="handlePresetChange">
                    <v-chip v-for="preset in presets" :key="preset.key" :value="preset.key" variant="outlined" filter
                        class="me-2 mb-2">
                        <v-icon start>{{ preset.icon }}</v-icon>
                        {{ preset.label }}
                    </v-chip>
                </v-chip-group>
            </div>

            <v-divider class="my-4"></v-divider>

            <!-- Custom date selection -->
            <div class="custom-date-section">
                <v-row>
                    <v-col cols="12" md="6">
                        <v-text-field v-model="startDateFormatted" label="Start Date" prepend-inner-icon="mdi-calendar"
                            readonly variant="outlined" density="comfortable" @click="startDateDialog = true" />
                    </v-col>
                    <v-col cols="12" md="6">
                        <v-text-field v-model="endDateFormatted" label="End Date" prepend-inner-icon="mdi-calendar"
                            readonly variant="outlined" density="comfortable" @click="endDateDialog = true" />
                    </v-col>
                </v-row>
            </div>

            <!-- Informations about selected period -->
            <v-alert v-if="startDate && endDate" type="info" variant="tonal" class="mt-4">
                <template #prepend>
                    <v-icon>mdi-information</v-icon>
                </template>
                <strong>Selected period:</strong> {{ formatDateRange() }}
                <br>
                <small>Number of days: {{ calculateDaysDifference() }}</small>
            </v-alert>
        </v-card-text>

        <v-card-actions>
            <v-spacer></v-spacer>
            <v-btn color="primary" variant="elevated" :disabled="!startDate || !endDate" @click="applyDateRange">
                <v-icon start>mdi-check</v-icon>
                Apply
            </v-btn>
            <v-btn variant="text" @click="resetDates">
                <v-icon start>mdi-refresh</v-icon>
                Reset
            </v-btn>
        </v-card-actions>

        <!-- Dialog for Start Date -->
        <v-dialog v-model="startDateDialog" max-width="400">
            <v-card>
                <v-card-title>Select Start Date</v-card-title>
                <v-card-text>
                    <v-date-picker v-model="startDate" :max="endDate || new Date()"
                        @update:model-value="startDateDialog = false" />
                </v-card-text>
            </v-card>
        </v-dialog>

        <!-- Dialog for End Date -->
        <v-dialog v-model="endDateDialog" max-width="400">
            <v-card>
                <v-card-title>Select End Date</v-card-title>
                <v-card-text>
                    <v-date-picker v-model="endDate" :min="startDate" :max="new Date()"
                        @update:model-value="endDateDialog = false" />
                </v-card-text>
            </v-card>
        </v-dialog>
    </v-card>
</template>

<script setup lang="ts">
import { ref, computed, watch } from 'vue';
import type { ReportFilters } from '@/types';

interface DatePreset {
    key: string;
    label: string;
    icon: string;
    startDate: Date;
    endDate: Date;
}

interface Props {
    modelValue?: ReportFilters;
}

const props = withDefaults(defineProps<Props>(), {
    modelValue: () => ({})
});

const emit = defineEmits<{
    'update:modelValue': [value: ReportFilters];
    'apply': [filters: ReportFilters];
}>();

const startDate = ref<Date | null>(props.modelValue.startDate || null);
const endDate = ref<Date | null>(props.modelValue.endDate || null);
const selectedPreset = ref<string | null>(null);
const startDateDialog = ref(false);
const endDateDialog = ref(false);

const startDateFormatted = computed(() => {
    return startDate.value ? formatDate(startDate.value) : '';
});

const endDateFormatted = computed(() => {
    return endDate.value ? formatDate(endDate.value) : '';
});

const presets = computed<DatePreset[]>(() => {
    const today = new Date();
    const yesterday = new Date(today);
    yesterday.setDate(today.getDate() - 1);

    const weekStart = new Date(today);
    weekStart.setDate(today.getDate() - today.getDay());

    const lastWeekStart = new Date(weekStart);
    lastWeekStart.setDate(weekStart.getDate() - 7);
    const lastWeekEnd = new Date(weekStart);
    lastWeekEnd.setDate(weekStart.getDate() - 1);

    const monthStart = new Date(today.getFullYear(), today.getMonth(), 1);

    const lastMonthStart = new Date(today.getFullYear(), today.getMonth() - 1, 1);
    const lastMonthEnd = new Date(today.getFullYear(), today.getMonth(), 0);

    const quarterStart = new Date(today.getFullYear(), Math.floor(today.getMonth() / 3) * 3, 1);

    return [
        {
            key: 'today',
            label: 'Today',
            icon: 'mdi-calendar-today',
            startDate: new Date(today),
            endDate: new Date(today)
        },
        {
            key: 'yesterday',
            label: 'Yesterday',
            icon: 'mdi-calendar-minus',
            startDate: yesterday,
            endDate: yesterday
        },
        {
            key: 'thisWeek',
            label: 'This Week',
            icon: 'mdi-calendar-week',
            startDate: weekStart,
            endDate: today
        },
        {
            key: 'lastWeek',
            label: 'Last Week',
            icon: 'mdi-calendar-week-begin',
            startDate: lastWeekStart,
            endDate: lastWeekEnd
        },
        {
            key: 'thisMonth',
            label: 'This Month',
            icon: 'mdi-calendar-month',
            startDate: monthStart,
            endDate: today
        },
        {
            key: 'lastMonth',
            label: 'Last Month',
            icon: 'mdi-calendar-month-outline',
            startDate: lastMonthStart,
            endDate: lastMonthEnd
        },
        {
            key: 'thisQuarter',
            label: 'This Quarter',
            icon: 'mdi-calendar-range',
            startDate: quarterStart,
            endDate: today
        },
        {
            key: 'last30Days',
            label: 'Last 30 Days',
            icon: 'mdi-calendar-clock',
            startDate: new Date(today.getTime() - 30 * 24 * 60 * 60 * 1000),
            endDate: today
        }
    ];
});

const formatDate = (date: Date): string => {
    return date.toLocaleDateString('pl-PL', {
        year: 'numeric',
        month: '2-digit',
        day: '2-digit'
    });
};

const formatDateRange = (): string => {
    if (!startDate.value || !endDate.value) return '';

    if (startDate.value.getTime() === endDate.value.getTime()) {
        return formatDate(startDate.value);
    }

    return `${formatDate(startDate.value)} - ${formatDate(endDate.value)}`;
};

const calculateDaysDifference = (): number => {
    if (!startDate.value || !endDate.value) return 0;

    const diffTime = Math.abs(endDate.value.getTime() - startDate.value.getTime());
    return Math.ceil(diffTime / (1000 * 60 * 60 * 24)) + 1; // +1 to include both start and end dates
};

const handlePresetChange = (presetKey: string | null) => {
    if (!presetKey) return;

    const preset = presets.value.find(p => p.key === presetKey);
    if (preset) {
        startDate.value = new Date(preset.startDate);
        endDate.value = new Date(preset.endDate);
    }
};

const applyDateRange = () => {
    if (!startDate.value || !endDate.value) return;

    const filters: ReportFilters = {
        startDate: startDate.value,
        endDate: endDate.value
    };

    emit('update:modelValue', filters);
    emit('apply', filters);
};

const resetDates = () => {
    startDate.value = null;
    endDate.value = null;
    selectedPreset.value = null;

    const filters: ReportFilters = {};
    emit('update:modelValue', filters);
    emit('apply', filters);
};

watch(() => props.modelValue, (newValue) => {
    if (newValue.startDate) {
        startDate.value = new Date(newValue.startDate);
    }
    if (newValue.endDate) {
        endDate.value = new Date(newValue.endDate);
    }
}, { deep: true });

watch([startDate, endDate], () => {
    const matchingPreset = presets.value.find(preset => {
        return startDate.value?.getTime() === preset.startDate.getTime() &&
            endDate.value?.getTime() === preset.endDate.getTime();
    });

    selectedPreset.value = matchingPreset?.key || null;
});
</script>

<style scoped>
.date-range-picker {
    max-width: 600px;
}

.custom-date-section {
    background-color: rgba(var(--v-theme-surface-variant), 0.1);
    border-radius: 8px;
    padding: 16px;
}

.v-chip-group {
    justify-content: flex-start;
}
</style>