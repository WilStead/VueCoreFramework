<template>
    <v-layout row wrap justify-center>
        <v-card flat style="width: 100%;">
            <v-card-row class="primary--text">
                <v-card-title>
                    <v-icon :fa="fontAwesome" class="primary--text">{{ iconClass }}</v-icon>
                    <span style="margin-left: 0.5em;">{{ title }}</span>
                    <v-spacer></v-spacer>
                    <v-dialog v-if="$store.state.userState.isAdmin" v-model="shareDialog" fullscreen :overlay="false">
                        <v-btn v-tooltip:top="{ html: 'type sharing' }" icon class="info--text" slot="activator"><v-icon light>visibility</v-icon></v-btn>
                        <v-card>
                            <v-card-row class="info">
                                <v-btn icon @click.native="shareDialog = false" light><v-icon>close</v-icon></v-btn>
                                <v-card-title><span class="white--text">Share {{routeName}} Data Type</span></v-card-title>
                            </v-card-row>
                            <v-alert error :value="shareErrorMessage">{{ shareErrorMessage }}</v-alert>
                            <v-alert success :value="shareSuccessMessage">{{ shareSuccessMessage }}</v-alert>
                            <v-card-row>
                                <v-card-text>
                                    <v-list>
                                        <v-list-group>
                                            <v-list-tile slot="item">
                                                <v-list-tile-content>
                                                    <v-list-tile-title>Currently Shared With</v-list-tile-title>
                                                </v-list-tile-content>
                                                <v-list-tile-action>
                                                    <v-icon>keyboard_arrow_down</v-icon>
                                                </v-list-tile-action>
                                            </v-list-tile>
                                            <v-list-item v-for="share in shares" :key="share.id">
                                                <v-list-tile>
                                                    <v-list-tile-content>
                                                        <v-list-tile-title>{{ share.name }}</v-list-tile-title>
                                                        <v-list-tile-sub-title>{{ share.shortLevel }}</v-list-tile-sub-title>
                                                    </v-list-tile-content>
                                                    <v-list-tile-action>
                                                        <v-btn v-tooltip:top="{ html: 'hide' }" icon class="info--text" @click.native="onHide(share)"><v-icon light>visibility_off</v-icon></v-btn>
                                                    </v-list-tile-action>
                                                </v-list-tile>
                                            </v-list-item>
                                        </v-list-group>
                                    </v-list>
                                </v-card-text>
                            </v-card-row>
                            <v-divider></v-divider>
                            <v-card-row>
                                <v-card-text>
                                    <v-checkbox label="All Users" v-model="shareWithAll" dark></v-checkbox>
                                </v-card-text>
                            </v-card-row>
                            <v-card-row>
                                <v-card-text>
                                    <v-text-field label="Username" v-model="shareUsername" @input="onShareUsernameChange" :hint="shareUsernameSuggestion"></v-text-field>
                                    <v-select v-if="groupMembers.length > 0"
                                              label="My Group members"
                                              v-model="selectedShareUsername"
                                              :items="groupMembers"
                                              @input="onSelectedShareUsernameChange"
                                              dark
                                              single-line
                                              auto></v-select>
                                </v-card-text>
                            </v-card-row>
                            <v-card-row>
                                <v-card-text>
                                    <v-text-field label="Group name" v-model="shareGroup" @input="onShareGroupChange" :hint="shareGroupSuggestion"></v-text-field>
                                    <v-select v-if="shareGroups.length > 0"
                                              label="My Groups"
                                              v-model="selectedShareGroup"
                                              :items="shareGroups"
                                              @input="onSelectedShareGroupChange"
                                              dark
                                              single-line
                                              auto></v-select>
                                </v-card-text>
                            </v-card-row>
                            <v-card-row>
                                <v-card-text>
                                    <v-container fluid class="pa-0">
                                        <v-layout row wrap>
                                            <v-flex xs9>
                                                <v-select label="Permission" v-model="selectedPermission" :items="permissionOptions" dark auto></v-select>
                                            </v-flex>
                                            <v-flex xs3>
                                                <v-btn v-tooltip:top="{ html: 'share' }" dark primary @click.native="onShare()">Share</v-btn>
                                            </v-flex>
                                        </v-layout>
                                    </v-container>
                                </v-card-text>
                            </v-card-row>
                            <v-card-row v-if="shareActivity" class="activity-row">
                                <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
                            </v-card-row>
                            <v-card-row actions>
                                <v-btn dark @click.native="shareDialog = false">Done</v-btn>
                            </v-card-row>
                        </v-card>
                    </v-dialog>
                </v-card-title>
            </v-card-row>
            <router-view name="content"></router-view>
            <router-view name="data"></router-view>
        </v-card>
    </v-layout>
</template>

<script src="./dashboard.ts"></script>