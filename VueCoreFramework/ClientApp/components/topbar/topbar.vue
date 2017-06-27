<template>
    <v-list class="topbar">
        <router-link title="sign in" v-if="!signedIn" :to="{ path: '/login', query: { returnUrl: getReturnUrl() } }"><v-icon class="user-icon">account_circle</v-icon></router-link>
        <div v-if="signedIn">
            <v-btn dark icon v-tooltip:left="{ html: 'chat' }" @click.native="onToggleChat">
                <v-icon v-if="totalUnread"
                        v-badge="{ value: totalUnread, overlap: true }"
                        class="blue-grey darken-1 info--after user-icon">chat</v-icon>
                <v-icon v-else class="user-icon">chat</v-icon>
            </v-btn>
            <router-link v-tooltip:left="{ html: 'profile' }" title="profile" to="/user/manage"><v-icon class="user-icon">account_circle</v-icon></router-link>
            <v-btn dark icon v-tooltip:left="{ html: 'sign out' }" @click.native="logout"><v-icon class="user-icon">exit_to_app</v-icon></v-btn>
        </div>
        <div>
            <a href="http://twitter.com" title="Twitter"><v-icon fa>twitter</v-icon></a>
            <a href="http://facebook.com" title="Facebook"><v-icon fa>facebook-official</v-icon></a>
            <a href="http://plus.google.com" title="Google+"><v-icon fa>google-plus-official</v-icon></a>
            <a href="http://reddit.com" title="Reddit"><v-icon fa>reddit-alien</v-icon></a>
        </div>
    </v-list>
</template>

<script src="./topbar.ts"></script>

<style src="./topbar.scss" lang="scss"></style>