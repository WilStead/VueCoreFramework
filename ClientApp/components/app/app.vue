<template>
    <v-app id='app-root'>
        <v-navigation-drawer persistent clipped disable-route-watcher v-model="sideNav">
            <v-list>
                <template v-for="menuItem in $store.state.uiState.menuItems">
                    <menu-item-component :menuItem="menuItem" :submenu="menuItem.submenu" />
                </template>
            </v-list>
        </v-navigation-drawer>
        <v-toolbar fixed class="topnav">
            <v-toolbar-side-icon light @click.native.stop="sideNav = !sideNav" />
            <v-toolbar-logo class="logo-container">
                <router-link to="/"><span class="logo-link"></span></router-link>
            </v-toolbar-logo>
            <v-spacer></v-spacer>
            <v-toolbar-items class="menu-container hidden-sm-and-down">
                <topbar-component class="topbar-header" />
            </v-toolbar-items>
            <v-menu bottom left origin="top right" transition="v-scale-transition" class="hidden-md-and-up">
                <v-btn icon dark slot="activator">
                    <v-icon>more_vert</v-icon>
                </v-btn>
                <topbar-component />
            </v-menu>
        </v-toolbar>
        <main>
            <v-container fluid>
                <router-view></router-view>
                <v-dialog v-model="$store.state.error.dialogShown">
                    <v-card>
                        <v-card-row class="error">
                            <v-card-title class="white--text">Error!</v-card-title>
                        </v-card-row>
                        <v-card-row>
                            <v-card-text class="error--text text--darken-4 mt-4">{{ $store.state.error.message }}</v-card-text>
                        </v-card-row>
                        <v-card-row actions>
                            <v-btn dark default @click.native="$store.state.error.dialogShown = false">Close</v-btn>
                        </v-card-row>
                    </v-card>
                </v-dialog>
            </v-container>
        </main>
        <v-footer fixed>
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
