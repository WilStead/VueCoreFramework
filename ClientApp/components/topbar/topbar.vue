<template>
    <v-list class="topbar">
        <router-link v-tooltip:bottom="{ html: 'sign in' }" v-if="!signedIn" :to="{ path: '/login', query: { returnUrl: getReturnUrl() } }"><span class="sr-only">sign in</span><v-icon class="user-icon">account_circle</v-icon></router-link>
        <div v-if="signedIn" class="user-controls-container">
            <router-link v-tooltip:bottom="{ html: 'profile' }" to="/user/manage"><span class="sr-only">manage account</span><v-icon class="user-icon">account_circle</v-icon></router-link>
            <a v-tooltip:bottom="{ html: 'sign out' }" href="#" @click.stop.prevent="logout"><span class="sr-only">sign out</span><v-icon class="user-icon">exit_to_app</v-icon></a>
        </div>
        <div>
            <a href="http://twitter.com" title="Twitter"><i class="fa fa-twitter" aria-hidden="true"></i></a>
            <a href="http://facebook.com" title="Facebook"><i class="fa fa-facebook-official" aria-hidden="true"></i></a>
            <a href="http://plus.google.com" title="Google+"><i class="fa fa-google-plus-official" aria-hidden="true"></i></a>
            <a href="http://reddit.com" title="Reddit"><i class="fa fa-reddit-alien" aria-hidden="true"></i></a>
        </div>
    </v-list>
</template>

<script src="./topbar.ts"></script>

<style src="./topbar.scss" lang="scss"></style>