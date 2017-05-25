<template>
    <v-layout row wrap justify-center>
        <v-card>
            <v-card-row><v-card-title>Reset your password</v-card-title></v-card-row>
            <v-alert error :value="model.errors.length > 0">
                <ul>
                    <li v-for="error in model.errors" class="error--text">{{ error }}</li>
                </ul>
            </v-alert>
            <v-alert success v-model="changeSuccess">
                Success! Your password has been reset. Please <router-link to="/login">click here</router-link>
                to return to the login page, where you can sign in.
            </v-alert>
            <v-card-row>
                <vue-form-generator class="vfg-container" :schema="schema" :model="model" :options="formOptions" @validated="onValidated"></vue-form-generator>
            </v-card-row>
            <v-card-row v-if="submitting" class="activity-row">
                <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
            </v-card-row>
            <v-card-row v-if="!submitting && !changeSuccess" class="submit-row">
                <v-btn default @click.native.stop.prevent="cancelChange">Cancel</v-btn>
                <v-btn primary @click.native.stop.prevent="onSubmit">Submit</v-btn>
            </v-card-row>
        </v-card>
    </v-layout>
</template>

<script src="./reset.ts"></script>