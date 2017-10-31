<template>
    <v-app light id='app-root'>
        <v-navigation-drawer app persistent disable-route-watcher v-model="sideNav">
            <v-list>
                <menu-item-component v-for="menuItem in $store.state.uiState.menuItems" :menuItem="menuItem" :submenu="menuItem.submenu" :key="menuItem.key" />
            </v-list>
        </v-navigation-drawer>
        <v-navigation-drawer right persistent disable-route-watcher v-model="$store.state.uiState.messaging.messagingShown">
            <v-card v-if="$store.state.uiState.messaging.chatShown" style="display: flex; flex-flow: column nowrap; min-height: 100%;">
                <v-toolbar card class="primary" style="flex-grow: 0;">
                    <v-btn icon @click="onHideChat"><v-icon>arrow_back</v-icon></v-btn>
                    <v-toolbar-title>
                        <span v-if="$store.state.uiState.messaging.groupChat">{{ $store.state.uiState.messaging.groupChat }}</span>
                        <span v-else>{{ $store.state.uiState.messaging.interlocutor }}</span>
                        <v-subheader v-if="$store.state.uiState.messaging.proxySender">{{ $store.state.uiState.messaging.proxySender }}</v-subheader>
                    </v-toolbar-title>
                </v-toolbar>
                <v-alert color="error" :value="chatErrorMessage">{{ chatErrorMessage }}</v-alert>
                <v-card-text class="chat-row" id="chat-row">
                    <div class="chat-container pa-0">
                        <v-list dense>
                            <v-list-tile avatar
                                         v-for="message in $store.state.uiState.messaging.messages"
                                         :key="message.timestamp">
                                <v-tooltip bottom>
                                    <v-list-tile-content v-if="message.isSystemMessage" slot="activator" class="grey--text text--darken-1">[***SYSTEM***]:</v-list-tile-content>
                                    <v-list-tile-content v-else slot="activator" :class="getMessageClass(message)">[{{ message.username }}]:</v-list-tile-content>
                                    <span>{{ formatTimestamp(message.timestamp) }}</span>
                                </v-tooltip>
                                <v-list-tile-content>
                                    <v-list-tile-title>
                                        <vue-markdown :source="message.content"></vue-markdown>
                                    </v-list-tile-title>
                                </v-list-tile-content>
                            </v-list-tile>
                        </v-list>
                    </div>
                </v-card-text>
                <v-divider style="flex-grow: 0; flex-basis: 1px;"></v-divider>
                <v-card-text v-if="!$store.state.uiState.messaging.proxySender && ($store.state.uiState.messaging.groupChat || $store.state.uiState.messaging.interlocutor)"
                             style="flex-grow: 0;"
                             @keypress.stop="onMessageTextKeypress($event)">
                    <v-text-field v-model="messageText"
                                  max="125"
                                  :counter="messageText.length > 125"
                                  label="Send a message"
                                  hint="Accepts markdown"
                                  append-icon="send"
                                  :append-icon-cb="sendMessage"
                                  single-line
                                  style="margin: 0;"></v-text-field>
                </v-card-text>
            </v-card>
            <v-list v-else two-line>
                <v-list-tile v-if="$store.state.uiState.messaging.systemMessages.length" avatar>
                    <v-list-tile-avatar>
                        <v-badge v-if="$store.state.uiState.messaging.systemMessages.filter(m => !m.received).length" overlap>
                            <span slot="badge">{{ $store.state.uiState.messaging.systemMessages.filter(m => !m.received).length }}</span>
                            <v-icon>settings</v-icon>
                        </v-badge>
                        <v-icon v-else>settings</v-icon>
                    </v-list-tile-avatar>
                    <v-list-tile-content>
                        <v-list-tile-title>System Messages</v-list-tile-title>
                    </v-list-tile-content>
                    <v-list-tile-action>
                        <v-btn icon class="info--text" @click="onSystemChat"><v-icon>chat</v-icon></v-btn>
                    </v-list-tile-action>
                </v-list-tile>
                <v-divider v-if="$store.state.uiState.messaging.systemMessages.length"></v-divider>
                <v-subheader v-if="groups.length > 0">Groups</v-subheader>
                <v-list-tile v-for="group in groups" :key="group.name" avatar>
                    <v-list-tile-avatar><v-icon class="primary--text">group</v-icon></v-list-tile-avatar>
                    <v-list-tile-content>
                        <v-list-tile-title>{{ group.name }}</v-list-tile-title>
                        <v-list-tile-sub-title>{{ describeMembers(group) }}</v-list-tile-sub-title>
                    </v-list-tile-content>
                    <v-list-tile-action>
                        <v-btn icon class="info--text" @click="onGroupChat(group)"><v-icon>chat</v-icon></v-btn>
                    </v-list-tile-action>
                </v-list-tile>
                <v-divider v-if="groups.length > 0 && $store.state.uiState.messaging.conversations.length > 0"></v-divider>
                <v-list-tile v-for="conversation in $store.state.uiState.messaging.conversations.filter(c => c.unreadCount > 0)"
                             :key="conversation.interlocutor"
                             avatar>
                    <v-list-tile-avatar>
                        <v-badge overlap color="info">
                            <span slot="badge">{{ conversation.unreadCount }}</span>
                            <v-icon class="blue-grey lighten-4 primary--text">person</v-icon>
                        </v-badge>
                    </v-list-tile-avatar>
                    <v-list-tile-content>
                        <v-list-tile-title>{{ conversation.interlocutor }}</v-list-tile-title>
                    </v-list-tile-content>
                    <v-list-tile-action>
                        <v-btn icon class="info--text" @click="onUserChat(conversation.interlocutor)"><v-icon>chat</v-icon></v-btn>
                    </v-list-tile-action>
                    <v-list-tile-action>
                        <v-btn icon class="error--text" @click="onDeleteChat(conversation.interlocutor)"><v-icon>delete</v-icon></v-btn>
                    </v-list-tile-action>
                </v-list-tile>
                <v-list-tile v-for="conversation in $store.state.uiState.messaging.conversations.filter(c => c.unreadCount === 0)"
                             :key="conversation.interlocutor"
                             avatar>
                    <v-list-tile-avatar><v-icon class="primary--text">person</v-icon></v-list-tile-avatar>
                    <v-list-tile-content>
                        <v-list-tile-title>{{ conversation.interlocutor }}</v-list-tile-title>
                    </v-list-tile-content>
                    <v-list-tile-action>
                        <v-btn icon class="info--text" @click="onUserChat(conversation.interlocutor)"><v-icon>chat</v-icon></v-btn>
                    </v-list-tile-action>
                    <v-list-tile-action>
                        <v-btn icon class="error--text" @click="onDeleteChat(conversation.interlocutor)"><v-icon>delete</v-icon></v-btn>
                    </v-list-tile-action>
                </v-list-tile>
                <v-list-tile>
                    <v-list-tile-content>
                        <v-list-tile-title @keypress.stop="onSearchUsernameKeypress($event)">
                            <v-text-field label="Username"
                                          v-model="searchUsername"
                                          @input="onSearchUsernameChange"
                                          :hint="searchUsernameSuggestion"
                                          append-icon="search"
                                          :append-icon-cb="onUsernameSearch"></v-text-field>
                        </v-list-tile-title>
                    </v-list-tile-content>
                </v-list-tile>
                <v-list-group v-if="foundUser">
                    <v-list-tile avatar slot="item">
                        <v-list-tile-avatar>
                            <v-btn icon class="info--text" @click="onUserChat(foundUser.username)"><v-icon>chat</v-icon></v-btn>
                        </v-list-tile-avatar>
                        <v-list-tile-content>
                            <v-list-tile-title>
                                <v-tooltip top>
                                    <span slot="activator">{{ foundUser.username }}</span>
                                    <span>{{ foundUser.email }}</span>
                                </v-tooltip>
                            </v-list-tile-title>
                        </v-list-tile-content>
                        <v-list-tile-action>
                            <v-icon>keyboard_arrow_down</v-icon>
                        </v-list-tile-action>
                    </v-list-tile>
                    <v-list-subheader v-if="foundUserConversations.length">Conversations</v-list-subheader>
                    <v-list-tile v-for="conversation in foundUserConversations" :key="conversation.interlocutor" avatar>
                        <v-list-tile-avatar><v-icon class="primary--text">person</v-icon></v-list-tile-avatar>
                        <v-list-tile-content>
                            <v-list-tile-title>{{ conversation.interlocutor }}</v-list-tile-title>
                        </v-list-tile-content>
                        <v-list-tile-action>
                            <v-btn icon class="info--text" @click="onAdminChatProxy(conversation.interlocutor)"><v-icon>chat</v-icon></v-btn>
                        </v-list-tile-action>
                    </v-list-tile>
                    <v-list-tile v-if="foundUser.isLocked" class="primary">
                        <v-list-tile-content>
                            <v-list-tile-title>Unlock user account</v-list-tile-title>
                        </v-list-tile-content>
                        <v-list-tile-action>
                            <v-btn icon class="white success--text" @click="onUnlockAccount()"><v-icon>lock_open</v-icon></v-btn>
                        </v-list-tile-action>
                    </v-list-tile>
                    <v-list-tile v-else class="error">
                        <v-list-tile-content>
                            <v-list-tile-title>Lock user account</v-list-tile-title>
                        </v-list-tile-content>
                        <v-list-tile-action>
                            <v-btn icon class="white error--text" @click="onLockAccount()"><v-icon>lock</v-icon></v-btn>
                        </v-list-tile-action>
                    </v-list-tile>
                </v-list-group>
            </v-list>
        </v-navigation-drawer>
        <v-toolbar app fixed class="primary topnav">
            <v-toolbar-side-icon @click.stop="sideNav = !sideNav" />
            <div class="logo-container">
                <router-link to="/"><span class="logo-link"></span></router-link>
            </div>
            <v-spacer></v-spacer>
            <div class="menu-container hidden-sm-and-down">
                <topbar-component class="topbar-header" />
            </div>
            <v-menu bottom left origin="top right" transition="v-scale-transition" class="hidden-md-and-up">
                <v-btn icon slot="activator">
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
                        <v-card-title primary-title class="error white--text">Error!</v-card-title>
                        <v-card-text class="error--text text--darken-4 mt-4">{{ $store.state.error.message }}</v-card-text>
                        <v-card-actions>
                            <v-btn default @click="$store.state.error.dialogShown = false">Close</v-btn>
                        </v-card-actions>
                    </v-card>
                </v-dialog>
            </v-container>
        </main>
        <v-footer app fixed class="primary">
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
<style src="./app.scss" lang="scss"></style>
