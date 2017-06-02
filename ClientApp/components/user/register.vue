<template>
    <v-layout row wrap justify-center>
        <v-card>
            <v-card-row><v-card-title class="primary--text">Register for a New Account</v-card-title></v-card-row>
            <v-alert error :value="model.errors.length > 0">
                <ul>
                    <li v-for="error in model.errors" class="error--text">{{ error }}</li>
                </ul>
            </v-alert>
            <v-card-row>
                <vue-form-generator class="vfg-container" :schema="schema" :model="model" :options="formOptions" @validated="onValidated"></vue-form-generator>
            </v-card-row>
            <v-card-row v-if="submitting" class="activity-row">
                <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
            </v-card-row>
            <v-card-row v-else class="submit-row">
                <router-link tag="button" class="btn btn--dark btn--raised btn--default" :to="{ path: '/login', query: { returnUrl }}">Cancel</router-link>
                <v-btn dark primary ripple @click.native.stop.prevent="onSubmit">Register</v-btn>
            </v-card-row>
        </v-card>
    </v-layout>
</template>

<script src="./register.ts"></script>