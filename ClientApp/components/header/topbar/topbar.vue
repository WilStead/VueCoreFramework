<template>
    <div class="topbar">
        <div class="social-icons-container">
            <a href="http://twitter.com" title="Twitter"><i class="fa fa-twitter" aria-hidden="true"></i></a>
            <a href="http://facebook.com" title="Facebook"><i class="fa fa-facebook-official" aria-hidden="true"></i></a>
            <a href="http://plus.google.com" title="Google+"><i class="fa fa-google-plus-official" aria-hidden="true"></i></a>
            <a href="http://reddit.com" title="Reddit"><i class="fa fa-reddit-alien" aria-hidden="true"></i></a>
        </div>
        <div class="user-controls-container">
            <router-link :to="{ path: '/login', query: { returnUrl: getReturnUrl() } }">{{ userActivity }}</router-link>
        </div>
    </div>
</template>

<script src="./topbar.ts"></script>

<style src="./topbar.scss" lang="scss"></style>