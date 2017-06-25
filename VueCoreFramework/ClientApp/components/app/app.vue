<template>
    <v-app id='app-root'>
        <v-navigation-drawer persistent clipped disable-route-watcher v-model="sideNav">
            <v-list>
                <template v-for="menuItem in $store.state.uiState.menuItems">
                    <menu-item-component :menuItem="menuItem" :submenu="menuItem.submenu" />
                </template>
            </v-list>
        </v-navigation-drawer>
        <v-navigation-drawer right persistent clipped disable-route-watcher v-model="$store.state.uiState.messaging.messagingShown">
            <v-card v-if="$store.state.uiState.messaging.chatShown">
                <v-card-row class="primary">
                    <v-card-title v-if="$store.state.uiState.messaging.groupChat">{{ $store.state.uiState.messaging.groupChat }}</v-card-title>
                    <v-card-title v-else>{{ $store.state.uiState.messaging.interlocutor }}</v-card-title>
                </v-card-row>
                <v-card-row>
                    <v-list dense>
                        <template v-for="message in $store.state.uiState.messaging.messages">

                        </template>
                    </v-list>
                </v-card-row>
                <v-divider></v-divider>
                <v-card-row>
                    <v-card-text>
                        <v-text-field v-model="messageText"
                                      max="125"
                                      :counter="messageText.length > 125"
                                      label="Send a message"
                                      hint="Accepts markdown"
                                      append-icon="send"
                                      append-icon-cb="sendMessage"
                                      single-line></v-text-field>
                    </v-card-text>
                </v-card-row>
            </v-card>
            <v-list v-else two-line>
                <v-subheader v-if="groups.length > 0">Groups</v-subheader>
                <v-list-item v-for="group in groups" :key="group.name">
                    <v-list-tile avatar>
                        <v-list-tile-avatar><v-icon>group</v-icon></v-list-tile-avatar>
                        <v-list-tile-content>
                            <v-list-tile-title>{{ group.name }}</v-list-tile-title>
                            <v-list-tile-sub-title>{{ describeMembers(group) }}</v-list-tile-sub-title>
                        </v-list-tile-content>
                        <v-list-tile-action>
                            <v-btn dark icon class="info--text" @click.native="onGroupChat(group)"><v-icon>chat</v-icon></v-btn>
                        </v-list-tile-action>
                    </v-list-tile>
                </v-list-item>
                <v-divider v-if="groups.length > 0 && conversations.length > 0"></v-divider>
                <v-list-item v-for="conversation in conversations.filter(c => c.unreadCount > 0)" :key="conversation.interlocutor">
                    <v-list-tile avatar>
                        <v-list-tile-avatar><v-icon v-badge="{ value: conversation.unreadCount }">person</v-icon></v-list-tile-avatar>
                        <v-list-tile-content>
                            <v-list-tile-title>{{ conversation.interlocutor }}</v-list-tile-title>
                        </v-list-tile-content>
                        <v-list-tile-action>
                            <v-btn dark icon class="info--text" @click.native="onUserChat(conversation.interlocutor)"><v-icon>chat</v-icon></v-btn>
                        </v-list-tile-action>
                    </v-list-tile>
                </v-list-item>
                <v-list-item v-for="conversation in conversations.filter(c => c.unreadCount === 0)" :key="conversation.interlocutor">
                    <v-list-tile avatar>
                        <v-list-tile-avatar><v-icon>person</v-icon></v-list-tile-avatar>
                        <v-list-tile-content>
                            <v-list-tile-title>{{ conversation.interlocutor }}</v-list-tile-title>
                        </v-list-tile-content>
                        <v-list-tile-action>
                            <v-btn dark icon class="info--text" @click.native="onUserChat(conversation.interlocutor)"><v-icon>chat</v-icon></v-btn>
                        </v-list-tile-action>
                    </v-list-tile>
                </v-list-item>
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
            <a :href="'mailto:' + $store.state.uiState.contact" class="copy-text">&copy; {{ $store.state.uiState.author }}, {{ $store.state.uiState.year }}</a>
        </v-footer>
    </v-app>
</template>

<script src="./app.ts"></script>

<style lang="stylus">
    @import '../../stylus/main';
</style>
<style src="../../vfg/vue-form-generator-custom.scss" lang="scss"></style>
<style src="../common.scss" lang="scss"></style>
<style src="./app.scss" lang="scss"></style>
