<template>
    <v-layout row wrap justify-center>
        <v-card>
            <v-card-row><v-card-title>Manage your account</v-card-title></v-card-row>
            <v-alert error :value="model.errors.length > 0">
                <ul>
                    <li v-for="error in model.errors" class="error--text">{{ error }}</li>
                </ul>
            </v-alert>
            <v-alert success v-model="success">Success!</v-alert>
            <v-card-row>
                <vue-form-generator class="vfg-container" :schema="schema" :model="model" :options="formOptions" @validated="onValidated"></vue-form-generator>
            </v-card-row>
            <v-card-row v-if="!submitting && (authProviderFacebook || authProviderGoogle || authProviderMicrosoft) && (!authProviderFacebookUser || !authProviderGoogleUser || !authProviderMicrosoftUser)">
                <v-card-text>Add external login</v-card-text>
                <div class="auth-providers">
                    <v-btn icon dark ripple v-if="authProviderFacebook && !authProviderFacebookUser" @click.native="onSignInProviderAdd('Facebook')"><v-icon fa>facebook</v-icon></v-btn>
                    <v-btn icon dark ripple v-if="authProviderGoogle && !authProviderGoogleUser" @click.native="onSignInProviderAdd('Google')"><v-icon fa>google</v-icon></v-btn>
                    <v-btn icon dark ripple v-if="authProviderMicrosoft && !authProviderMicrosoftUser" @click.native="onSignInProviderAdd('Microsoft')"><v-icon fa>windows</v-icon></v-btn>
                </div>
            </v-card-row>
            <v-card-row v-if="!submitting && (authProviderFacebookUser || authProviderGoogleUser || authProviderMicrosoftUser)">
                <v-card-text>Remove external login</v-card-text>
                <div class="auth-providers">
                    <v-btn icon dark ripple v-if="authProviderFacebook && authProviderFacebookUser" @click.native="onSignInProviderRemove('Facebook')"><v-icon fa>facebook</v-icon></v-btn>
                    <v-btn icon dark ripple v-if="authProviderGoogle && authProviderGoogleUser" @click.native="onSignInProviderRemove('Google')"><v-icon fa>google</v-icon></v-btn>
                    <v-btn icon dark ripple v-if="authProviderMicrosoft && authProviderMicrosoftUser" @click.native="onSignInProviderRemove('Microsoft')"><v-icon fa>windows</v-icon></v-btn>
                </div>
            </v-card-row>
            <v-card-row v-if="submitting" class="activity-row">
                <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
            </v-card-row>
            <v-card-row v-if="changingEmail || changingPassword || settingPassword" class="submit-row">
                <v-btn default @click.native.stop.prevent="cancelChange">Cancel</v-btn>
                <v-btn primary @click.native.stop.prevent="onSubmit">Submit</v-btn>
            </v-card-row>
            <v-card-row v-else>
                <v-card-text>
                    <dl class="dl-horizontal">
                        <dt>Email</dt>
                        <dd><a href="#" @click.stop.prevent="changeEmail">Change email</a></dd>
                        <dt>Password</dt>
                        <dd>
                            <a v-if="hasPassword" href="#" @click.stop.prevent="changePassword">Change password</a>
                            <a v-if="!hasPassword" href="#" @click.stop.prevent="setPassword">Add a local password</a>
                        </dd>
                    </dl>
                </v-card-text>
            </v-card-row>
        </v-card>
    </v-layout>
</template>

<script src="./manage.ts"></script>