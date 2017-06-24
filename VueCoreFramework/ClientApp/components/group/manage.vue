<template>
    <div class="main-content">
        <v-card>
            <v-card-row class="primary">
                <v-card-title>Groups</v-card-title>
            </v-card-row>
            <v-alert error :value="errorMessage">{{ errorMessage }}</v-alert>
            <v-list two-line>
                <v-subheader v-if="managedGroups.length > 0">Groups you manage</v-subheader>
                <v-list-group v-for="group in managedGroups" :key="group.name">
                    <v-list-tile slot="item">
                        <v-list-tile-content>
                            <v-list-tile-title>{{ group.name }}</v-list-tile-title>
                            <v-list-tile-sub-title>{{ describeMembers(group) }}</v-list-tile-sub-title>
                        </v-list-tile-content>
                        <v-list-tile-action>
                            <v-icon>keyboard_arrow_down</v-icon>
                        </v-list-tile-action>
                    </v-list-tile>
                    <v-list-item v-for="member in group.members.filter(m => m !== $store.state.userState.username)" :key="member">
                        <v-list-tile avatar>
                            <v-list-tile-avatar>
                                <v-btn v-tooltip:top="{ html: 'contact' }" dark icon class="info--text" @click.native="onContactGroupMember(group, member)"><v-icon>person</v-icon></v-btn>
                            </v-list-tile-avatar>
                            <v-list-tile-content>
                                <v-list-tile-title>{{ member }}</v-list-tile-title>
                            </v-list-tile-content>
                            <v-list-tile-action>
                                <v-btn v-tooltip:top="{ html: 'remove from group' }" dark icon class="error--text" @click.native="onRemoveGroupMember(group, member)"><v-icon>remove_circle</v-icon></v-btn>
                            </v-list-tile-action>
                        </v-list-tile>
                    </v-list-item>
                </v-list-group>
                <v-list-item>
                    <v-list-tile class="error white--text">
                        <v-list-tile-content>
                            <v-list-tile-title>Leave this group</v-list-tile-title>
                        </v-list-tile-content>
                        <v-list-tile-action>
                            <v-dialog v-model="leaveGroupDialog">
                                <v-btn slot="activator" light icon class="white--text"><v-icon>remove_circle</v-icon></v-btn>
                                <v-card>
                                    <v-card-row class="error white--text">
                                        <v-card-title>Are you sure?</v-card-title>
                                    </v-card-row>
                                    <v-card-row>
                                        <v-card-text>
                                            <p>Are you sure you want to leave the {{ group }} group?</p>
                                            <p>This action cannot be undone. Only the group manager will be able to invite you to join the group again.</p>
                                        </v-card-text>
                                    </v-card-row>
                                    <v-card-row actions>
                                        <v-btn flat @click.native="leaveGroupDialog = false">Cancel</v-btn>
                                        <v-btn error @click.native="onLeaveGroup(group)">Leave Group</v-btn>
                                    </v-card-row>
                                </v-card>
                            </v-dialog>
                        </v-list-tile-action>
                    </v-list-tile>
                </v-list-item>
                <v-divider v-if="managedGroups.length > 0 && joinedGroups.length > 0"></v-divider>
                <v-subheader v-if="joinedGroups.length > 0">Groups you belong to</v-subheader>
                <v-list-group v-for="group in joinedGroups" :key="group.name">
                    <v-list-tile slot="item">
                        <v-list-tile-content>
                            <v-list-tile-title>{{ group.name }}</v-list-tile-title>
                            <v-list-tile-sub-title>{{ describeMembers(group) }}</v-list-tile-sub-title>
                        </v-list-tile-content>
                        <v-list-tile-action>
                            <v-icon>keyboard_arrow_down</v-icon>
                        </v-list-tile-action>
                    </v-list-tile>
                    <v-list-item v-for="member in group.members.filter(m => m !== $store.state.userState.username)" :key="member">
                        <v-list-tile avatar>
                            <v-list-tile-avatar>
                                <v-btn v-tooltip:top="{ html: 'contact' }" dark icon class="info--text" @click.native="onContactGroupMember(group, member)"><v-icon>person</v-icon></v-btn>
                            </v-list-tile-avatar>
                            <v-list-tile-content>
                                <v-list-tile-title>{{ member }}</v-list-tile-title>
                                <v-list-tile-sub-title v-if="member === group.manager">Manager</v-list-tile-sub-title>
                            </v-list-tile-content>
                        </v-list-tile>
                    </v-list-item>
                </v-list-group>
            </v-list>
            <v-divider v-if="managedGroups.length > 0 && joinedGroups.length > 0"></v-divider>
            <v-card-row v-if="activity" class="activity-row">
                <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
            </v-card-row>
            <v-card-row v-else>
                <v-card-text>
                    <v-dialog v-model="createGroupDialog">
                        <v-btn slot="activator" light icon class="white--text"><v-icon>remove_circle</v-icon></v-btn>
                        <v-card>
                            <v-card-row class="primary">
                                <v-card-title>Create a Group</v-card-title>
                            </v-card-row>
                            <v-alert error :value="createErrorMessage">{{ createErrorMessage }}</v-alert>
                            <v-card-row>
                                <v-card-text>
                                    <v-text-field label="Group name" v-model="newGroupName" rules="[validateGroupName]"></v-text-field>
                                </v-card-text>
                            </v-card-row>
                            <v-card-row actions>
                                <v-btn flat @click.native="createGroupDialog = false">Cancel</v-btn>
                                <v-btn primary @click.native="onCreateGroup" :disabled="!newGroupName">Create Group</v-btn>
                            </v-card-row>
                        </v-card>
                    </v-dialog>
                    <v-btn dark primary @click.native="onCreateGroup">Start a New Group</v-btn>
                </v-card-text>
            </v-card-row>
        </v-card>
    </div>
</template>

<script src="./manage.ts"></script>