<template>
    <v-layout row wrap justify-center>
        <v-card>
            <v-card-title primary-title class="primary--text headline">Register for a New Account</v-card-title>
            <v-alert error :value="errors.length > 0">
                <ul>
                    <li v-for="error in errors" class="white--text">{{ error }}</li>
                </ul>
            </v-alert>
            <v-alert success v-model="success">Success! Check your email for a confirmation message and a link that will allow you to access your new account.</v-alert>
            <v-card-text>
                <vue-form-generator class="vfg-container" :schema="schema" :model="model" :options="formOptions" @validated="onValidated"></vue-form-generator>
            </v-card-text>
            <v-card-text v-if="submitting" class="activity-row">
                <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
            </v-card-text>
            <v-card-actions v-else>
                <router-link tag="button" class="btn btn--dark btn--raised btn--default" :to="{ path: '/login', query: { returnUrl }}">Cancel</router-link>
                <v-btn primary ripple @click.native.stop.prevent="onSubmit">Register</v-btn>
            </v-card-actions>
        </v-card>
    </v-layout>
</template>

<script src="./register.ts"></script>