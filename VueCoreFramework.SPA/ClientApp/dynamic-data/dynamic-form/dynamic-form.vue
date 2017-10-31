<template>
    <v-card>
        <v-alert color="error" :value="errorMessage">{{ errorMessage }}</v-alert>
        <v-card-title primary-title class="pt-0 pb-0">
            <v-spacer></v-spacer>
            <v-tooltip top>
                <v-btn slot="activator" icon class="info--text" @click.native.stop="shareDialog = true"><v-icon>visibility</v-icon></v-btn>
                <span>item sharing</span>
            </v-tooltip>
            <v-dialog v-if="canShare" v-model="shareDialog" fullscreen :overlay="false">
                <v-card>
                    <v-card-title class="info">
                        <v-btn icon @click="shareDialog = false"><v-icon>close</v-icon></v-btn>
                        <span class="white--text headline">Sharing</span>
                    </v-card-title>
                    <v-alert color="error" :value="shareErrorMessage">{{ shareErrorMessage }}</v-alert>
                    <v-alert color="success" :value="shareSuccessMessage">{{ shareSuccessMessage }}</v-alert>
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
                                <v-list-tile v-for="share in shares" :key="share.id">
                                    <v-list-tile-content>
                                        <v-list-tile-title>{{ share.name }}</v-list-tile-title>
                                        <v-list-tile-sub-title>{{ share.shortLevel }}</v-list-tile-sub-title>
                                    </v-list-tile-content>
                                    <v-list-tile-action>
                                        <v-tooltip top>
                                            <v-btn slot="activator" icon class="info--text" @click="onHide(share)"><v-icon>visibility_off</v-icon></v-btn>
                                            <span>hide</span>
                                        </v-tooltip>
                                    </v-list-tile-action>
                                </v-list-tile>
                            </v-list-group>
                        </v-list>
                    </v-card-text>
                    <v-divider></v-divider>
                    <v-card-text v-if="canShareAll">
                        <v-checkbox label="All Users" v-model="shareWithAll"></v-checkbox>
                    </v-card-text>
                    <v-card-text>
                        <v-text-field label="Username" v-model="shareUsername" @input="onShareUsernameChange" :hint="shareUsernameSuggestion"></v-text-field>
                        <v-select v-if="groupMembers.length > 0"
                                  label="My group members"
                                  v-model="selectedShareUsername"
                                  :items="groupMembers"
                                  @input="onSelectedShareUsernameChange"
                                  single-line
                                  auto></v-select>
                    </v-card-text>
                    <v-card-text v-if="canShareGroup">
                        <v-text-field label="Group name" v-model="shareGroup" @input="onShareGroupChange" :hint="shareGroupSuggestion"></v-text-field>
                        <v-select v-if="shareGroups.length > 0"
                                  label="My Groups"
                                  v-model="selectedShareGroup"
                                  :items="shareGroups"
                                  @input="onSelectedShareGroupChange"
                                  single-line
                                  auto></v-select>
                    </v-card-text>
                    <v-card-text>
                        <v-container fluid class="pa-0">
                            <v-layout row wrap>
                                <v-flex xs9>
                                    <v-select label="Permission" v-model="selectedPermission" :items="permissionOptions" auto></v-select>
                                </v-flex>
                                <v-flex xs3>
                                    <v-tooltip top>
                                        <v-btn slot="activator" color="primary" @click="onShare()">Share</v-btn>
                                        <span>share</span>
                                    </v-tooltip>
                                </v-flex>
                            </v-layout>
                        </v-container>
                    </v-card-text>
                    <v-card-text v-if="shareActivity" class="activity-row">
                        <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
                    </v-card-text>
                    <v-card-actions>
                        <v-btn @click="shareDialog = false">Done</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
        </v-card-title>
        <v-card-text class="pt-0">
            <vue-form-generator class="vfg-container" :schema="schema" :model="model" :options="formOptions" @validated="onValidated"></vue-form-generator>
        </v-card-text>
        <v-card-text v-if="activity" class="activity-row">
            <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
        </v-card-text>
        <v-card-actions v-else-if="operation === 'view'">
            <v-btn default @click="onCancel">Back</v-btn>
            <v-btn v-if="canEdit" color="primary" @click="onEdit">Edit</v-btn>
        </v-card-actions>
        <v-card-actions v-else-if="operation === 'edit' || operation === 'add'">
            <v-btn default @click="onCancel">Cancel</v-btn>
            <v-btn color="primary" :class="{ 'btn--disabled': !isValid }" @click="onSave">Save</v-btn>
        </v-card-actions>
    </v-card>
</template>

<script src="./dynamic-form.ts"></script>