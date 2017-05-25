<template>
    <v-layout row wrap justify-center>
        <v-card>
            <v-card-row><v-card-title>Manage your account</v-card-title></v-card-row>
            <v-alert error :value="model.errors.length > 0">
                <ul>
                    <li v-for="error in model.errors" class="error--text">{{ error }}</li>
                </ul>
            </v-alert>
            <v-card-row>
                <vue-form-generator class="vfg-container" :schema="schema" :model="model" :options="formOptions" @validated="onValidated"></vue-form-generator>
            </v-card-row>
            <v-card-row v-if="changingEmail || changingPassword || settingPassword" class="submit-row">
                <v-btn default @click.native.stop.prevent="cancelChange">Cancel</v-btn>
                <v-btn primary @click.native.stop.prevent="onSubmit">Submit</v-btn>
            </v-card-row>
            <v-card-row v-else>
                <v-card-text>
                    <a href="#" @click.stop.prevent="changeEmail">Change email</a>
                    <div v-if="hasPassword">
                        <a href="#" @click.stop.prevent="changePassword">Change password</a>
                    </div>
                    <div v-if="!hasPassword">
                        <a href="#" @click.stop.prevent="setPassword">Add a local password</a>
                    </div>
                </v-card-text>
            </v-card-row>
            <v-card-row v-if="false">
                <!--TODO: manage external logins-->
            </v-card-row>
        </v-card>
    </v-layout>
</template>

<script src="./manage.ts"></script>