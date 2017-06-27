<template>
    <div class="main-content">
        <v-card>
            <v-card-row class="primary">
                <v-card-title>Groups</v-card-title>
            </v-card-row>
            <v-alert error :value="errorMessage">{{ errorMessage }}</v-alert>
            <v-alert success :value="successMessage">{{ successMessage }}</v-alert>
            <v-list two-line>
                <v-subheader v-if="$store.state.userState.managedGroups.length > 0">Groups you manage</v-subheader>
                <v-list-group v-for="group in $store.state.userState.managedGroups" :key="group.name">
                    <v-list-tile avatar slot="item">
                        <v-list-tile-avatar>
                            <v-btn v-tooltip:top="{ html: 'group chat' }" dark icon class="info--text" @click.native="onGroupChat(group)"><v-icon>group</v-icon></v-btn>
                        </v-list-tile-avatar>
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
                                <v-btn v-tooltip:top="{ html: 'chat' }" dark icon class="info--text" @click.native="onContactGroupMember(member)"><v-icon>person</v-icon></v-btn>
                            </v-list-tile-avatar>
                            <v-list-tile-content>
                                <v-list-tile-title>{{ member }}</v-list-tile-title>
                            </v-list-tile-content>
                            <v-list-tile-action>
                                <v-btn v-tooltip:top="{ html: 'remove from group' }" dark icon class="error--text" @click.native="onRemoveGroupMember(group, member)"><v-icon>remove_circle</v-icon></v-btn>
                            </v-list-tile-action>
                        </v-list-tile>
                    </v-list-item>
                    <v-list-item>
                        <v-list-tile>
                            <v-list-tile-content>
                                <v-list-tile-title>Invite a new member</v-list-tile-title>
                            </v-list-tile-content>
                            <v-list-tile-action>
                                <v-btn light icon class="success--text" @click.native.stop="onInviteConfirm(group)"><v-icon>person_add</v-icon></v-btn>
                            </v-list-tile-action>
                        </v-list-tile>
                    </v-list-item>
                    <v-list-item v-if="group.name !== 'Admin' && group.members.length > 1">
                        <v-list-tile>
                            <v-list-tile-content>
                                <v-list-tile-title>Transfer management</v-list-tile-title>
                            </v-list-tile-content>
                            <v-list-tile-action>
                                <v-btn light icon class="primary--text" @click.native.stop="onXferGroupConfirm(group)"><v-icon>transfer_within_a_station</v-icon></v-btn>
                            </v-list-tile-action>
                        </v-list-tile>
                    </v-list-item>
                    <v-list-item v-if="group.name !== 'Admin'">
                        <v-list-tile class="error white--text">
                            <v-list-tile-content>
                                <v-list-tile-title>Delete this group</v-list-tile-title>
                            </v-list-tile-content>
                            <v-list-tile-action>
                                <v-btn light icon class="white--text" @click.native="onDeleteConfirm(group)"><v-icon>remove_circle</v-icon></v-btn>
                            </v-list-tile-action>
                        </v-list-tile>
                    </v-list-item>
                </v-list-group>
                <v-divider v-if="$store.state.userState.managedGroups.length > 0 && $store.state.userState.joinedGroups.length > 0"></v-divider>
                <v-subheader v-if="$store.state.userState.joinedGroups.length > 0">Groups you belong to</v-subheader>
                <v-list-group v-for="group in $store.state.userState.joinedGroups" :key="group.name">
                    <v-list-tile slot="item">
                        <v-list-tile-avatar>
                            <v-btn v-tooltip:top="{ html: 'group chat' }" dark icon class="info--text" @click.native="onGroupChat(group)"><v-icon>group</v-icon></v-btn>
                        </v-list-tile-avatar>
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
                                <v-btn v-tooltip:top="{ html: 'contact' }" dark icon class="info--text" @click.native="onContactGroupMember(member)"><v-icon>person</v-icon></v-btn>
                            </v-list-tile-avatar>
                            <v-list-tile-content>
                                <v-list-tile-title>{{ member }}</v-list-tile-title>
                                <v-list-tile-sub-title v-if="member === group.manager">Manager</v-list-tile-sub-title>
                            </v-list-tile-content>
                        </v-list-tile>
                    </v-list-item>
                    <v-list-item>
                        <v-list-tile class="error white--text">
                            <v-list-tile-content>
                                <v-list-tile-title>Leave this group</v-list-tile-title>
                            </v-list-tile-content>
                            <v-list-tile-action>
                                <v-btn slot="activator" light icon class="white--text" @click.native.stop="onLeaveGroupConfirm(group)"><v-icon>remove_circle</v-icon></v-btn>
                            </v-list-tile-action>
                        </v-list-tile>
                    </v-list-item>
                </v-list-group>
            </v-list>
            <v-divider v-if="$store.state.userState.managedGroups.length > 0 && $store.state.userState.joinedGroups.length > 0"></v-divider>
            <v-card-row v-if="activity" class="activity-row">
                <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
            </v-card-row>
            <v-card-row v-else>
                <v-card-text>
                    <v-dialog v-model="createGroupDialog">
                        <v-btn slot="activator" dark primary>Start a New Group</v-btn>
                        <v-card>
                            <v-card-row class="primary">
                                <v-card-title>Create a Group</v-card-title>
                            </v-card-row>
                            <v-alert error :value="createErrorMessage">{{ createErrorMessage }}</v-alert>
                            <v-card-row>
                                <v-card-text>
                                    <v-text-field label="Group name" v-model="newGroupName" :rules="[validateGroupName]"></v-text-field>
                                </v-card-text>
                            </v-card-row>
                            <v-card-row actions>
                                <v-btn flat @click.native="createGroupDialog = false">Cancel</v-btn>
                                <v-btn primary @click.native="onCreateGroup" :disabled="!newGroupName">Create Group</v-btn>
                            </v-card-row>
                        </v-card>
                    </v-dialog>
                </v-card-text>
            </v-card-row>
            <v-divider v-if="$store.state.userState.isAdmin"></v-divider>
            <v-card-row v-if="$store.state.userState.isAdmin">
                <v-card-text><h5>Find a Group</h5></v-card-text>
            </v-card-row>
            <v-card-row v-if="$store.state.userState.isAdmin">
                <v-card-text @keypress.stop="onSearchGroupKeypress($event)">
                    <v-text-field label="Group name"
                                  v-model="searchGroup"
                                  @input="onSearchGroupChange"
                                  :hint="searchGroupSuggestion"
                                  append-icon="search"
                                  :append-icon-cb="onGroupSearch"></v-text-field>
                </v-card-text>
            </v-card-row>
            <v-list two-line v-if="$store.state.userState.isAdmin && foundGroup">
                <v-list-group>
                    <v-list-tile slot="item">
                        <v-list-tile-avatar>
                            <v-btn v-tooltip:top="{ html: 'group chat' }" dark icon class="info--text" @click.native="onGroupChat(foundGroup)"><v-icon>group</v-icon></v-btn>
                        </v-list-tile-avatar>
                        <v-list-tile-content>
                            <v-list-tile-title>{{ foundGroup.name }}</v-list-tile-title>
                            <v-list-tile-sub-title>{{ describeMembers(foundGroup) }}</v-list-tile-sub-title>
                        </v-list-tile-content>
                        <v-list-tile-action>
                            <v-icon>keyboard_arrow_down</v-icon>
                        </v-list-tile-action>
                    </v-list-tile>
                    <v-list-item v-for="member in foundGroup.members.filter(m => m !== $store.state.userState.username)" :key="member">
                        <v-list-tile avatar>
                            <v-list-tile-avatar>
                                <v-btn v-tooltip:top="{ html: 'contact' }" dark icon class="info--text" @click.native="onContactGroupMember(member)"><v-icon>person</v-icon></v-btn>
                            </v-list-tile-avatar>
                            <v-list-tile-content>
                                <v-list-tile-title>{{ member }}</v-list-tile-title>
                            </v-list-tile-content>
                            <v-list-tile-action>
                                <v-btn v-tooltip:top="{ html: 'remove from group' }" dark icon class="error--text" @click.native="onRemoveGroupMember(foundGroup, member)"><v-icon>remove_circle</v-icon></v-btn>
                            </v-list-tile-action>
                        </v-list-tile>
                    </v-list-item>
                    <v-list-item>
                        <v-list-tile>
                            <v-list-tile-content>
                                <v-list-tile-title>Invite a new member</v-list-tile-title>
                            </v-list-tile-content>
                            <v-list-tile-action>
                                <v-btn light icon class="success--text" @click.native="onInviteConfirm(foundGroup)"><v-icon>person_add</v-icon></v-btn>
                            </v-list-tile-action>
                        </v-list-tile>
                    </v-list-item>
                    <v-list-item>
                        <v-list-tile>
                            <v-list-tile-content>
                                <v-list-tile-title>Transfer management</v-list-tile-title>
                            </v-list-tile-content>
                            <v-list-tile-action>
                                <v-btn light icon class="primary--text" @click.native="onXferGroupConfirm(foundGroup)"><v-icon>transfer_within_a_station</v-icon></v-btn>
                            </v-list-tile-action>
                        </v-list-tile>
                    </v-list-item>
                    <v-list-item v-if="foundGroup.name !== 'Admin'">
                        <v-list-tile class="error white--text">
                            <v-list-tile-content>
                                <v-list-tile-title>Delete this group</v-list-tile-title>
                            </v-list-tile-content>
                            <v-list-tile-action>
                                <v-btn light icon class="white--text" @click.native="onDeleteConfirm(foundGroup)"><v-icon>remove_circle</v-icon></v-btn>
                            </v-list-tile-action>
                        </v-list-tile>
                    </v-list-item>
                </v-list-group>
            </v-list>
        </v-card>
        <v-dialog v-model="inviteDialog">
            <v-card>
                <v-card-row class="primary">
                    <v-card-title>Invite a new member</v-card-title>
                </v-card-row>
                <v-card-row>
                    <v-card-text>Find a user</v-card-text>
                </v-card-row>
                <v-card-row>
                    <v-card-text>
                        <v-text-field label="Username" v-model="searchUsername" @input="onSearchUsernameChange" :hint="searchUsernameSuggestion"></v-text-field>
                    </v-card-text>
                </v-card-row>
                <v-card-row actions>
                    <v-btn flat @click.native="inviteDialog = false">Cancel</v-btn>
                    <v-btn primary @click.native="onInvite">Invite</v-btn>
                </v-card-row>
            </v-card>
        </v-dialog>
        <v-dialog v-if="xferGroup" v-model="xferGroupDialog">
            <v-card>
                <v-card-row class="warning">
                    <v-card-title>Transfer management of {{ xferGroup.name }}</v-card-title>
                </v-card-row>
                <v-card-row v-if="xferGroup.name === 'Admin'">
                    <v-card-text>
                        <p>Select an admin to be the new site administrator.</p>
                        <p>Note that this aciton cannot be undone. Only the new site admin will be able to transfer the role back to you if you change your mind.</p>
                    </v-card-text>
                </v-card-row>
                <v-card-row v-else>
                    <v-card-text>
                        <p>Select a group member to be the new manager of {{ xferGroup.name }}.</p>
                        <p>Note that this aciton cannot be undone. Only the new manager of {{ xferGroup.name }} will be able to transfer management back to you if you change your mind.</p>
                    </v-card-text>
                </v-card-row>
                <v-card-row>
                    <v-card-text>
                        <v-select label="New manager" v-model="newManager" :items="xferGroup.members.filter(m => m !== xferGroup.manager)" dark single-line auto></v-select>
                    </v-card-text>
                </v-card-row>
                <v-card-row actions>
                    <v-btn flat @click.native="xferGroupDialog = false">Cancel</v-btn>
                    <v-btn warning @click.native="onXferGroup" :disabled="!newManager">Transfer</v-btn>
                </v-card-row>
            </v-card>
        </v-dialog>
        <v-dialog v-model="leaveGroupDialog">
            <v-card>
                <v-card-row class="error white--text">
                    <v-card-title>Are you sure?</v-card-title>
                </v-card-row>
                <v-card-row>
                    <v-card-text>
                        <p>Are you sure you want to leave the {{ leaveGroup }} group?</p>
                        <p>This action cannot be undone. Only the group manager will be able to invite you to join the group again.</p>
                    </v-card-text>
                </v-card-row>
                <v-card-row actions>
                    <v-btn flat @click.native="leaveGroupDialog = false">Cancel</v-btn>
                    <v-btn error @click.native="onLeaveGroup">Leave Group</v-btn>
                </v-card-row>
            </v-card>
        </v-dialog>
        <v-dialog v-model="deleteGroupDialog">
            <v-card>
                <v-card-row class="error white--text">
                    <v-card-title>Are you sure?</v-card-title>
                </v-card-row>
                <v-card-row>
                    <v-card-text>
                        <p>Are you sure you want to delete the {{ deleteGroup }} group?</p>
                        <p>This action cannot be undone. You can re-form the group later, but all members will need to be re-invited to join the new group, and all data associated with the original group will be permanently lost.</p>
                    </v-card-text>
                </v-card-row>
                <v-card-row actions>
                    <v-btn flat @click.native="deleteGroupDialog = false">Cancel</v-btn>
                    <v-btn error @click.native="onDeleteGroup">Delete Group</v-btn>
                </v-card-row>
            </v-card>
        </v-dialog>
    </div>
</template>

<script src="./manage.ts"></script>