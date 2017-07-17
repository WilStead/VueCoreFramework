<template>
    <v-layout row wrap justify-center>
        <v-card>
            <v-card-title primary-title class="primary--text headline">Sign In</v-card-title>
            <v-alert error :value="shareErrorMessage">{{ errorMessage }}</v-alert>
            <v-card-text>
                <vue-form-generator class="vfg-container" :schema="schema" :model="model" :options="formOptions" @validated="onValidated"></vue-form-generator>
            </v-card-text>
            <v-card-text v-if="submitting" class="activity-row">
                <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
            </v-card-text>
            <div v-else>
                <v-card-actions>
                    <v-btn primary ripple v-if="this.forgottenPassword" @click.native="resetPassword">Reset Password</v-btn>
                    <v-btn primary ripple v-else @click.native="onSubmit">Sign In</v-btn>
                    <router-link class="ml-5 mr-3" :to="{ path: '/register', query: { returnUrl }}">New? Register here</router-link>
                </v-card-actions>
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
                <v-card-text v-if="authProviderFacebook || authProviderGoogle || authProviderMicrosoft">
                    <span>Sign in with an external account</span>
                    <div class="auth-providers">
                        <v-btn icon ripple v-if="authProviderFacebook" @click.native="onSignInProvider('Facebook')"><v-icon fa>facebook</v-icon></v-btn>
                        <v-btn icon ripple v-if="authProviderGoogle" @click.native="onSignInProvider('Google')"><v-icon fa>google</v-icon></v-btn>
                        <v-btn icon ripple v-if="authProviderMicrosoft" @click.native="onSignInProvider('Microsoft')"><v-icon fa>windows</v-icon></v-btn>
                    </div>
                </v-card-text>
            </div>
        </v-card>
    </v-layout>
</template>

<script src="./login.ts"></script>