<template>
    <v-list class="topbar">
        <router-link title="sign in" v-if="!signedIn" :to="{ path: '/login', query: { returnUrl: getReturnUrl() } }"><v-icon class="user-icon">account_circle</v-icon></router-link>
        <div v-if="signedIn" class="user-controls-container">
            <router-link title="profile" to="/user/manage"><v-icon class="user-icon">account_circle</v-icon></router-link>
            <a href="#" title="sign out" @click.stop.prevent="logout"><v-icon class="user-icon">exit_to_app</v-icon></a>
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