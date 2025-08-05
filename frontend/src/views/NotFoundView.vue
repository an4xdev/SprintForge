<template>
    <v-container fluid class="fill-height">
        <v-row align="center" justify="center" class="text-center">
            <v-col cols="12" md="6">
                <div class="error-content">
                    <h1 class="display-1 font-weight-bold text-primary mb-4">404</h1>
                    <h2 class="text-h4 mb-4">Page not found</h2>
                    <p class="text-body-1 text-medium-emphasis mb-6">
                        Sorry, but the page you are looking for does not exist or has been moved.
                    </p>

                    <div class="mb-6">
                        <v-icon size="120" color="grey-lighten-2">mdi-emoticon-sad-outline</v-icon>
                    </div>

                    <div class="d-flex flex-column flex-sm-row gap-4 justify-center">
                        <v-btn color="primary" size="large" variant="flat" prepend-icon="mdi-home" @click="goHome">
                            Home Page
                        </v-btn>

                        <v-btn color="secondary" size="large" variant="outlined" prepend-icon="mdi-arrow-left"
                            @click="goBack">
                            Go Back
                        </v-btn>
                    </div>
                </div>
            </v-col>
        </v-row>
    </v-container>
</template>

<script setup lang="ts">
import { useRouter } from 'vue-router'

const router = useRouter()

const goHome = () => {
    router.push('/dashboard')
}

const goBack = () => {
    router.go(-1)
}
</script>

<style scoped>
.error-content {
    max-width: 500px;
    margin: 0 auto;
}

.display-1 {
    font-size: 8rem !important;
    line-height: 1;
}

@media (max-width: 600px) {
    .display-1 {
        font-size: 6rem !important;
    }

    .d-flex.flex-column.flex-sm-row {
        flex-direction: column;
    }
}
</style>
