<template>
    <v-list class="topbar">
        <v-tooltip left v-if="!signedIn">
            <v-btn slot="activator" icon @click="onLogin"><v-icon class="user-icon">account_circle</v-icon></v-btn>
            <span>sign in</span>
        </v-tooltip>
        <div v-else>
            <v-tooltip left>
                <v-btn icon slot="activator" @click="onToggleChat">
                    <v-badge overlap v-model="totalUnread" color="info">
                        <span slot="badge">{{ totalUnread }}</span>
                        <v-icon class="user-icon">chat</v-icon>
                    </v-badge>
                </v-btn>
                <span>chat</span>
            </v-tooltip>
            <v-tooltip left>
                <router-link class="btn btn--icon btn--raised" slot="activator" title="profile" to="/user/manage"><v-icon class="user-icon" dark>account_circle</v-icon></router-link>
                <span>profile</span>
            </v-tooltip>
            <v-tooltip left>
                <v-btn icon slot="activator" @click="onLogout"><v-icon class="user-icon">exit_to_app</v-icon></v-btn>
                <span>sign out</span>
            </v-tooltip>
        </div>
        <div>
            <a class="btn btn--icon btn--raised" href="http://twitter.com" title="Twitter"><v-icon dark>fa-twitter</v-icon></a>
            <a class="btn btn--icon btn--raised" href="http://facebook.com" title="Facebook"><v-icon dark>fa-facebook-official</v-icon></a>
            <a class="btn btn--icon btn--raised" href="http://plus.google.com" title="Google+"><v-icon dark>fa-google-plus-official</v-icon></a>
            <a class="btn btn--icon btn--raised" href="http://reddit.com" title="Reddit"><v-icon dark>fa-reddit-alien</v-icon></a>
        </div>
    </v-list>
</template>

<script src="./topbar.ts"></script>

<style src="./topbar.scss" lang="scss"></style>