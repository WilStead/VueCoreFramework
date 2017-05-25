<template>
    <v-layout row wrap justify-center>
        <v-card>
            <v-card-row><v-card-title class="primary--text">Email reset</v-card-title></v-card-row>
            <v-card-row>
                <v-card-text>
                    <p>Your email has been reverted to the original address on your account, and any pending change has been cancelled.</p>
                    <p>
                        If this wasn't a mistake, you should probably <router-link to="/user/manage">change your password</router-link>
                        to prevent any further unauthorized attempts to modify your account information.
                    </p>
                    <p>Click <router-link to="/login">here</router-link> to go to the login page, where you can sign in.</p>
                </v-card-text>
            </v-card-row>
        </v-card>
    </v-layout>
</template>