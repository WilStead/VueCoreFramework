<template>
    <v-app id='app-root' left-fixed-sidebar top-toolbar footer>
        <v-toolbar class="topnav">
            <v-toolbar-side-icon @click.native.stop="sideNav = !sideNav" />
            <v-toolbar-logo class="logo-container">
                <router-link to="/"><span class="logo-link"></span></router-link>
            </v-toolbar-logo>
            <v-spacer></v-spacer>
            <v-toolbar-items class="menu-container hidden-md-and-down">
                <topbar-component class="topbar-header" />
            </v-toolbar-items>
            <v-menu bottom left origin="top right" transition="v-scale-transition" class="hidden-lg-and-up">
                <v-btn icon dark slot="activator">
                    <v-icon>more_vert</v-icon>
                </v-btn>
                <topbar-component />
            </v-menu>
        </v-toolbar>
        <main>
            <v-sidebar left fixed drawer v-model="sideNav" class="navmenu-v-container">
                <v-list>
                    <template v-for="menuItem in $store.state.uiState.menuItems">
                        <menu-item-component :menuItem="menuItem" />
                    </template>
                </v-list>
            </v-sidebar>
            <v-content>
                <v-container fluid class="main-container">
                    <router-view></router-view>
                </v-container>
            </v-content>
        </main>
        <v-footer>
            <v-spacer></v-spacer>
            <a :href="'mailto:' + $store.state.contact" class="copy-text">&copy; {{ $store.state.author }}, {{ $store.state.year }}</a>
        </v-footer>
    </v-app>
</template>

<script src="./app.ts"></script>

<style lang="stylus">
    @import '../../stylus/main';
</style>
<style src="../../vue-form-generator-custom.scss" lang="scss"></style>
<style src="../common.scss" lang="scss"></style>
<style src="./app.scss" lang="scss"></style>
