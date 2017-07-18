<template>
    <v-layout row wrap justify-center>
        <v-card>
            <v-card-title primary-title class="primary headline">
                <span>Welcome, {{ $store.state.userState.username }}</span>
                <v-subheader>Manage your account</v-subheader>
            </v-card-title>
            <v-alert error :value="errors.length > 0">
                <ul>
                    <li v-for="error in errors" class="white--text">{{ error }}</li>
                </ul>
            </v-alert>
            <v-alert success v-model="success">{{ successMessage }}</v-alert>
            <v-card-text>
                <vue-form-generator class="vfg-container" :schema="schema" :model="model" :options="formOptions" @validated="onValidated"></vue-form-generator>
            </v-card-text>
            <v-card-text v-if="!submitting && (authProviderFacebook || authProviderGoogle || authProviderMicrosoft) && (!authProviderFacebookUser || !authProviderGoogleUser || !authProviderMicrosoftUser)">
                <span>Add external login</span>
                <div class="auth-providers">
                    <v-btn icon ripple v-if="authProviderFacebook && !authProviderFacebookUser" @click.native="onSignInProviderAdd('Facebook')"><v-icon fa>facebook</v-icon></v-btn>
                    <v-btn icon ripple v-if="authProviderGoogle && !authProviderGoogleUser" @click.native="onSignInProviderAdd('Google')"><v-icon fa>google</v-icon></v-btn>
                    <v-btn icon ripple v-if="authProviderMicrosoft && !authProviderMicrosoftUser" @click.native="onSignInProviderAdd('Microsoft')"><v-icon fa>windows</v-icon></v-btn>
                </div>
            </v-card-text>
            <v-card-text v-if="!submitting && (authProviderFacebookUser || authProviderGoogleUser || authProviderMicrosoftUser)">
                <span>Remove external login</span>
                <div class="auth-providers">
                    <v-btn icon ripple v-if="authProviderFacebook && authProviderFacebookUser" @click.native="onSignInProviderRemove('Facebook')"><v-icon fa>facebook</v-icon></v-btn>
                    <v-btn icon ripple v-if="authProviderGoogle && authProviderGoogleUser" @click.native="onSignInProviderRemove('Google')"><v-icon fa>google</v-icon></v-btn>
                    <v-btn icon ripple v-if="authProviderMicrosoft && authProviderMicrosoftUser" @click.native="onSignInProviderRemove('Microsoft')"><v-icon fa>windows</v-icon></v-btn>
                </div>
            </v-card-text>
            <v-card-text v-if="submitting" class="activity-row">
                <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
            </v-card-text>
            <v-card-actions v-else-if="changingUsername || changingEmail || changingPassword || settingPassword">
                <v-btn default @click.native.stop.prevent="cancelChange">Cancel</v-btn>
                <v-btn primary @click.native.stop.prevent="onSubmit">Submit</v-btn>
            </v-card-actions>
            <v-card-text v-else>
                <dl class="dl-horizontal">
                    <dt>Username: {{ $store.state.username }}</dt>
                    <dd><a href="#" @click.stop.prevent="changeUsername">Change username</a></dd>
                    <dt>Email: {{ $store.state.email }}</dt>
                    <dd><a href="#" @click.stop.prevent="changeEmail">Change email</a></dd>
                    <dt>Password</dt>
                    <dd>
                        <a v-if="hasPassword" href="#" @click.stop.prevent="changePassword">Change password</a>
                        <a v-if="!hasPassword" href="#" @click.stop.prevent="setPassword">Add a local password</a>
                    </dd>
                </dl>
                <v-select :items="cultures"
                          v-model="selectedCulture"
                          @input="onCultureChange"
                          label="Culture"
                          prepend-icon="language"
                          single-line></v-select>
            </v-card-text>
            <v-card-text v-if="!submitting && !changingUsername && !changingEmail && !changingPassword && !settingPassword">
                <v-dialog v-model="deleteAccountDialog" fullscreen :overlay="false">
                    <v-btn error slot="activator">Delete Account</v-btn>
                    <v-card>
                        <v-card-title primary-title class="error white--text headline">Are you sure you want to delete your account?</v-card-title>
                        <v-card-text class="error--text">This will delete all data associated with your account that isn't being shared with others (unless you choose to transfer it, below). Even if you sign up again later with the same account information, any deleted data will remain lost.</v-card-text>
                        <v-card-text class="warning--text">Any data you have shared will remain available, and ownership (full control over the data) will be automatically transferred to one of the users with whom it is currently shared, unless you explicitly choose a user who will receive ownership of all your data below.</v-card-text>
                        <v-card-text class="warning--text">If there is any data you have previously shared which you do not with to be transferred to other users after your account has been deleted, you should delete that data before deleting your account.</v-card-text>
                        <v-divider></v-divider>
                        <v-card-text>
                            <v-checkbox label="Transfer my data to another user" v-model="xferData"></v-checkbox>
                        </v-card-text>
                        <v-card-text v-if="xferData">
                            <v-text-field label="Username"
                                            v-model="selectedXferUsername"
                                            :rules="[usernameValidation]"></v-text-field>
                        </v-card-text>
                        <v-card-text v-if="xferData && xferUsernames.length > 0">
                            <v-select label="Known users" single-line auto :items="xferUsernames.filter(u => u !== $store.state.userState.username)" v-model="selectedXferUsername"></v-select>
                        </v-card-text>
                        <v-card-text v-if="xferLoading" class="activity-row">
                            <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
                        </v-card-text>
                        <v-card-actions v-else>
                            <v-btn flat class="success--text" @click.native="deleteAccountDialog = false">Cancel</v-btn>
                            <v-btn :disabled="xferData && !validXferUsername" error @click.native="onDeleteAccount">Delete Account</v-btn>
                        </v-card-actions>
                    </v-card>
                </v-dialog>
            </v-card-text>
        </v-card>
    </v-layout>
</template>

<script src="./manage.ts"></script>