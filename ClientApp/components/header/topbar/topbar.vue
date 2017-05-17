<template>
    <div class="topbar">
        <div class="social-icons-container">
            <a href="http://twitter.com" title="Twitter"><i class="fa fa-twitter" aria-hidden="true"></i></a>
            <a href="http://facebook.com" title="Facebook"><i class="fa fa-facebook-official" aria-hidden="true"></i></a>
            <a href="http://plus.google.com" title="Google+"><i class="fa fa-google-plus-official" aria-hidden="true"></i></a>
            <a href="http://reddit.com" title="Reddit"><i class="fa fa-reddit-alien" aria-hidden="true"></i></a>
        </div>
        <router-link v-if="!signedIn" class="user-action" :to="{ path: '/login', query: { returnUrl: getReturnUrl() } }">sign in</router-link>
        <div v-if="signedIn" class="user-controls-container">
            <router-link to="/user/manage">{{ $store.state.email }}</router-link>
            <a href="#" @click.stop.prevent="logout" class="user-action">sign out</a>
        </div>
    </div>
</template>

<script src="./topbar.ts"></script>

<style src="./topbar.scss" lang="scss"></style>