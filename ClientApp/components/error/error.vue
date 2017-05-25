<template>
    <v-layout row wrap>
        <v-flex xs10 offset-xs1>
            <h1 class="error--text">Error {{ code }}</h1>
            <h3 class="error--text text--lighten-1">{{ errorMsg || 'An error occurred while processing your request.' }}</h3>
        </v-flex>
    </v-layout>
</template>

<script src="./error.ts"></script>