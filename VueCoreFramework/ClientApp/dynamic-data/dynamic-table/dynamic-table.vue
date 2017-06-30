<template>
    <v-card>
        <v-alert error :value="errorMessage">{{ errorMessage }}</v-alert>
        <v-card-row>
            <v-card-title class="pt-0 pb-0">
                <v-spacer></v-spacer>
                <v-dialog v-if="canShare" v-model="shareDialog" fullscreen>
                    <v-btn v-tooltip:top="{ html: 'share' }" icon class="info--text" slot="activator"><v-icon light>visibility</v-icon></v-btn>
                    <v-card>
                        <v-card-row class="info">
                            <v-btn icon @click.native="shareDialog = false" light><v-icon>close</v-icon></v-btn>
                            <v-card-title><span class="white--text">Sharing</span></v-card-title>
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
        <dynamic-data-table :childProp="childProp"
                            :dataType="routeName"
                            :pagination="pagination"
                            :parentId="parentId"
                            :parentProp="parentProp"
                            :parentType="parentType"
                            :search="$route.query.search"
                            :selected.sync="selected"
                            :tableType="operation"
                            @onError="onError"
                            @onPagination="onPagination"></dynamic-data-table>
        <v-container fluid v-if="operation === 'multiselect'">
            <v-layout row justify-space-around>
                <v-btn dark success :class="{ 'btn--disabled': !selected.length }" @click.native="onAddSelect">Add<v-icon right dark>arrow_downward</v-icon></v-btn>
                <v-btn dark error :class="{ 'btn--disabled': !selectedChildren.length }" @click.native="onRemoveSelect">Remove<v-icon right dark>arrow_upward</v-icon></v-btn>
            </v-layout>
        </v-container>
        <dynamic-data-table v-if="operation === 'multiselect'"
                            :childProp="childProp"
                            :dataType="routeName"
                            :pagination="childPagination"
                            :parentId="parentId"
                            :parentProp="parentProp"
                            :parentType="parentType"
                            :search="$route.query.childSearch"
                            :selected.sync="selectedChildren"
                            :tableType="child"
                            @onError="onError"
                            @onPagination="onChildPagination"></dynamic-data-table>
        <v-card-row v-if="activity" class="activity-row">
            <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
        </v-card-row>
        <v-card-row v-else-if="operation === 'select'" class="submit-row condensed">
            <v-btn dark default @click.native="onCancel">Cancel</v-btn>
            <v-btn dark primary @click.native.stop="onSelectItem">Submit</v-btn>
        </v-card-row>
        <v-card-row v-else-if="operation === 'multiselect' || operation === 'collection'" class="submit-row condensed">
            <v-btn dark primary @click.native="onCancel">Finish</v-btn>
        </v-card-row>
        <v-dialog v-model="selectErrorDialogShown">
            <v-card>
                <v-card-row>
                    <v-card-title class="warning--text">{{ selectErrorDialogMessage }}</v-card-title>
                </v-card-row>
                <v-card-row actions>
                    <v-btn dark default flat @click.native="selectErrorDialogShown = false">OK</v-btn>
                </v-card-row>
            </v-card>
        </v-dialog>
    </v-card>
</template>

<script src="./dynamic-table.ts"></script>