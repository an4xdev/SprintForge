import { ref, onMounted, onBeforeUnmount, watch } from 'vue'
import { useAuthStore } from '@/stores/auth'
import { DevelopmentLogger } from '@/utils/logger'

export interface UseAsyncDataOptions<T> {
    fetchFunction: (signal: AbortSignal) => Promise<T>
    loggerPrefix?: string
    autoLoad?: boolean
    resetOnLogout?: boolean
}

export function useAsyncData<T>(options: UseAsyncDataOptions<T>) {
    const {
        fetchFunction,
        loggerPrefix = '[AsyncData]',
        autoLoad = true,
        resetOnLogout = true
    } = options;

    const authStore = useAuthStore();
    const logger = new DevelopmentLogger({ prefix: loggerPrefix });

    const data = ref<T | null>(null);
    const loading = ref(false);
    const error = ref('');
    const isComponentMounted = ref(true);
    const abortController = ref<AbortController | null>(null);

    const load = async () => {
        if (!authStore.isAuthenticated || !isComponentMounted.value || !localStorage.getItem('token')) {
            logger.log('Skipping load: not authenticated or component not mounted');
            return;
        }

        logger.log('Starting to load data');

        if (abortController.value) {
            abortController.value.abort();
        }

        abortController.value = new AbortController();
        loading.value = true;
        error.value = '';

        try {
            const result = await fetchFunction(abortController.value.signal);

            if (isComponentMounted.value && !abortController.value.signal.aborted && authStore.isAuthenticated) {
                logger.log('Successfully loaded data');
                data.value = result;
            }
        } catch (err) {
            if (err instanceof Error && (err.name === 'AbortError' || !authStore.isAuthenticated)) {
                logger.log('Request canceled or user logged out');
                return;
            }

            logger.error('Error loading data:', err);
            if (isComponentMounted.value && !abortController.value.signal.aborted && authStore.isAuthenticated) {
                error.value = err instanceof Error ? err.message : 'An unexpected error occurred';
            }
        } finally {
            if (isComponentMounted.value && !abortController.value.signal.aborted && authStore.isAuthenticated) {
                loading.value = false;
            }
        }
    }

    const reset = () => {
        if (abortController.value) {
            abortController.value.abort();
        }
        loading.value = false;
        error.value = '';
        data.value = null;
    }

    watch(() => authStore.isAuthenticated, (newValue) => {
        logger.log('Authentication changed:', newValue);
        if (!newValue && resetOnLogout) {
            logger.log('User logged out - resetting state');
            reset();
        }
    })

    onMounted(() => {
        if (autoLoad) {
            load();
        }
    })

    onBeforeUnmount(() => {
        isComponentMounted.value = false;
        if (abortController.value) {
            abortController.value.abort();
        }
    })

    return {
        data,
        loading,
        error,
        load,
        reset
    }
}
