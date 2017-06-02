<template>
    <v-layout row wrap justify-center>
        <v-card>
            <v-card-row><v-card-title class="primary--text">Sign In</v-card-title></v-card-row>
            <v-alert error :value="model.errors.length > 0">
                <ul>
                    <li v-for="error in model.errors">{{ error }}</li>
                </ul>
            </v-alert>
            <v-card-row>
                <vue-form-generator class="vfg-container" :schema="schema" :model="model" :options="formOptions" @validated="onValidated"></vue-form-generator>
            </v-card-row>
            <v-card-row v-if="submitting" class="activity-row">
                <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
            </v-card-row>
            <div v-else>
                <v-card-row class="submit-row">
                    <v-btn dark primary ripple v-if="this.forgottenPassword" @click.native="resetPassword">Reset</v-btn>
                    <v-btn dark primary ripple v-if="!this.forgottenPassword" @click.native="onSubmit">Sign In</v-btn>
                    <router-link class="ml-5 mr-3" :to="{ path: '/register', query: { returnUrl }}">New? Register here</router-link>
                </v-card-row>
                <v-card-row>
                    <v-card-text class="text-xs-right small">
                        <div v-if="!passwordReset">
                            <div v-if="!forgottenPassword">
                                <a href="#" @click.stop.prevent="forgotPassword(true)">Forgot your password?</a>
                            </div>
                            <div v-if="forgottenPassword">
                                <a href="#" @click.stop.prevent="forgotPassword(false)">Return to login</a>
                            </div>
                        </div>
                        <div v-if="passwordReset">
                            <p>Please check your email to reset your password. <a href="#" @click.stop.prevent="forgotPassword(true)">Re-try?</a></p>
                        </div>
                    </v-card-text>
                </v-card-row>
                <v-card-row v-if="authProviderFacebook || authProviderGoogle || authProviderMicrosoft">
                    <v-card-text>Sign in with an external account</v-card-text>
                    <div class="auth-providers">
                        <v-btn icon dark ripple v-if="authProviderFacebook" @click.native="onSignInProvider('Facebook')"><v-icon fa>facebook</v-icon></v-btn>
                        <v-btn icon dark ripple v-if="authProviderGoogle" @click.native="onSignInProvider('Google')"><v-icon fa>google</v-icon></v-btn>
                        <v-btn icon dark ripple v-if="authProviderMicrosoft" @click.native="onSignInProvider('Microsoft')"><v-icon fa>windows</v-icon></v-btn>
                    </div>
                </v-card-row>
            </div>
        </v-card>
    </v-layout>
</template>

<script src="./login.ts"></script>