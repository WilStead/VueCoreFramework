<template>
    <v-layout row wrap justify-center>
        <v-card>
            <v-card-title primary-title class="headline">Reset your password</v-card-title>
            <v-alert color="error" :value="errors.length > 0">
                <ul>
                    <li v-for="error in errors" class="error--text">{{ error }}</li>
                </ul>
            </v-alert>
            <v-alert color="success" v-model="changeSuccess">
                Success! Your password has been reset. Please <router-link to="/login">click here</router-link>
                to return to the login page, where you can sign in.
            </v-alert>
            <v-card-text>
                <vue-form-generator class="vfg-container" :schema="schema" :model="model" :options="formOptions" @validated="onValidated"></vue-form-generator>
            </v-card-text>
            <v-card-text v-if="submitting" class="activity-row">
                <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
            </v-card-text>
            <v-card-actions v-if="!submitting && !changeSuccess">
                <v-btn default @click.stop.prevent="cancelChange">Cancel</v-btn>
                <v-btn color="primary" @click.stop.prevent="onSubmit">Submit</v-btn>
            </v-card-actions>
        </v-card>
    </v-layout>
</template>

<script src="./reset.ts"></script>