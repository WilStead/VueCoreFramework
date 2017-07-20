<template>
    <v-list class="topbar">
        <v-btn icon v-tooltip:left="{ html: 'sign in' }" v-if="!signedIn" @click="onLogin"><v-icon class="user-icon">account_circle</v-icon></v-btn>
        <div v-else>
            <v-btn icon v-tooltip:left="{ html: 'chat' }" @click="onToggleChat">
                <v-icon v-badge="{ value: totalUnread, overlap: true, visible: totalUnread > 0 }"
                        class="info--after user-icon">chat</v-icon>
            </v-btn>
            <router-link class="btn btn--icon btn--raised" v-tooltip:left="{ html: 'profile' }" title="profile" to="/user/manage"><v-icon class="user-icon">account_circle</v-icon></router-link>
            <v-btn icon v-tooltip:left="{ html: 'sign out' }" @click="onLogout"><v-icon class="user-icon">exit_to_app</v-icon></v-btn>
        </div>
        <div>
            <a class="btn btn--icon btn--raised" href="http://twitter.com" title="Twitter"><v-icon fa>twitter</v-icon></a>
            <a class="btn btn--icon btn--raised" href="http://facebook.com" title="Facebook"><v-icon fa>facebook-official</v-icon></a>
            <a class="btn btn--icon btn--raised" href="http://plus.google.com" title="Google+"><v-icon fa>google-plus-official</v-icon></a>
            <a class="btn btn--icon btn--raised" href="http://reddit.com" title="Reddit"><v-icon fa>reddit-alien</v-icon></a>
        </div>
    </v-list>
</template>

<script src="./topbar.ts"></script>

<style src="./topbar.scss" lang="scss"></style>