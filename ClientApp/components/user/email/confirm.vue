<template>
    <v-layout row wrap justify-center>
        <v-card>
            <v-card-row><v-card-title class="primary--text">Email confirmed</v-card-title></v-card-row>
            <v-card-row>
                <v-card-text>
                    <p>Thank you for confirming your email.</p>
                    <p>Click <router-link to="/login">here</router-link> to go to the login page, where you can sign in.</p>
                </v-card-text>
            </v-card-row>
        </v-card>
    </v-layout>
</template>