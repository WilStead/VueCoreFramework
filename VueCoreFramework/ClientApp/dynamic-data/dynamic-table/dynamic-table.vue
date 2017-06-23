<template>
    <v-card>
        <v-alert error :value="errorMessage">{{ errorMessage }}</v-alert>
        <v-card-row>
            <v-card-title class="pt-0 pb-0">
                <v-spacer></v-spacer>
                <v-dialog v-if="canShare" v-model="shareDialog" fullscreen>
                    <v-btn v-tooltip:left="{ html: 'share' }" icon class="info--text" slot="activator"><v-icon light>visibility</v-icon></v-btn>
                    <v-card>
                        <v-card-row class="info">
                            <v-btn icon @click.native="shareDialog = false" light><v-icon>close</v-icon></v-btn>
                            <v-card-title><span class="white--text">Sharing</span></v-card-title>
                        </v-card-row>
                        <v-alert error :value="shareErrorMessage">{{ shareErrorMessage }}</v-alert>
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
                                                    <v-btn v-tooltip:left="{ html: 'hide' }" icon class="info--text" @click.native="onHide(share)"><v-icon light>visibility_off</v-icon></v-btn>
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
                                <v-text-field label="Username" v-model="shareUsername" @input="onShareUsernameChange"></v-text-field>
                                <small v-if="shareUsernameSuggestion && shareUsernameSuggestion !== shareUsername" class="grey--text">{{ shareUsernameSuggestion }}</small>
                                <v-select label="Group members" v-model="selectedShareUsername" :items="groupMembers" dark single-line auto></v-select>
                            </v-card-text>
                        </v-card-row>
                        <v-card-row>
                            <v-card-text>
                                <v-text-field label="Group name" v-model="shareGroup" @input="onShareGroupChange" hint="You may type a group name or select from the dropdown below"></v-text-field>
                                <small v-if="shareGroupSuggestion && shareGroupSuggestion !== shareGroup" class="grey--text">{{ shareGroupSuggestion }}</small>
                                <v-select label="Groups" v-model="selectedShareGroup" :items="shareGroups" dark single-line auto></v-select>
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
                                            <v-btn v-tooltip:left="{ html: 'share' }" dark primary @click.native="onShare()">Share</v-btn>
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
        <v-card-row>
            <v-spacer></v-spacer>
            <v-text-field append-icon="search"
                          label="Search"
                          single-line
                          hide-details
                          v-model="search"></v-text-field>
        </v-card-row>
        <v-card-row>
            <v-spacer></v-spacer>
            <v-btn v-if="canAdd" icon v-tooltip:left="{ html: 'new' }" @click.native="onNew"><v-icon class="success--text">add_circle</v-icon></v-btn>
            <v-dialog v-if="canDelete" v-model="deleteDialogShown">
                <v-btn icon slot="activator" v-tooltip:left="{ html: 'delete' }"><v-icon :class="{ 'error--text': selected.length > 0 }">remove_circle</v-icon></v-btn>
                <v-card>
                    <v-card-row>
                        <v-card-title>Are you sure you want to delete {{ selected.length > 1 ? 'these' : 'this' }} item{{ selected.length > 1 ? 's' : '' }}?</v-card-title>
                    </v-card-row>
                    <v-card-row actions>
                        <v-btn class="success--text" flat @click.native="deleteDialogShown = false">Cancel</v-btn>
                        <v-btn class="error--text" flat @click.native="onDelete">Delete</v-btn>
                    </v-card-row>
                </v-card>
            </v-dialog>
        </v-card-row>
        <v-data-table :headers="headers"
                      :items="items"
                      v-model="selected"
                      :pagination.sync="pagination"
                      :total-items="totalItems"
                      :loading="loading"
                      :search="search"
                      select-all>
            <template slot="items" scope="props">
                <td><v-checkbox hide-details primary v-model="props.selected" v-if="deletePendingItems.indexOf(props.item[props.item.primaryKeyProperty]) === -1"></v-checkbox></td>
                <td v-for="field in headers" :class="{ 'text-xs-right': field.text !== 'Name' }">{{ props.item[field.value] }}</td>
                <td v-if="deletePendingItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1" colspan="3">Deleting...</td>
                <td v-else>
                    <span v-if="deleteAskingItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1">Are you sure?</span>
                    <v-btn v-else icon v-tooltip:left="{ html: 'view/edit' }" @click.native="onViewItem(props.item[props.item.primaryKeyProperty])"><v-icon class="info--text">edit</v-icon></v-btn>
                </td>
                <td v-if="deletePendingItems.indexOf(props.item[props.item.primaryKeyProperty]) === -1 && (canAdd || deleteAskingItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1)">
                    <v-btn v-if="deleteAskingItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1" icon v-tooltip:left="{ html: 'cancel delete' }" @click.native="cancelDelete(props.item[props.item.primaryKeyProperty])"><v-icon class="success--text">undo</v-icon></v-btn>
                    <v-btn v-else-if="canAdd" icon v-tooltip:left="{ html: 'copy' }" @click.native="onDuplicate(props.item[props.item.primaryKeyProperty])"><v-icon class="info--text">content_copy</v-icon></v-btn>
                </td>
                <td v-if="deletePendingItems.indexOf(props.item[props.item.primaryKeyProperty]) === -1 && (canDelete || deleteAskingItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1)">
                    <v-btn v-if="deleteAskingItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1" icon v-tooltip:left="{ html: 'confirm delete' }" @click.native="onDeleteItem(props.item[props.item.primaryKeyProperty])"><v-icon class="error--text">delete</v-icon></v-btn>
                    <v-btn v-else-if="deletePermissions[props.item[props.item.primaryKeyProperty]]" icon v-tooltip:left="{ html: 'delete' }" @click.native="deleteAskingItems.push(props.item[props.item.primaryKeyProperty])"><v-icon class="error--text">remove_circle</v-icon></v-btn>
                </td>
            </template>
        </v-data-table>
        <v-container fluid v-if="operation === 'multiselect'">
            <v-layout row justify-space-around>
                <v-btn dark success :class="{ 'btn--disabled': !selected.length }" @click.native="onAddSelect">Add<v-icon right dark>arrow_downward</v-icon></v-btn>
                <v-btn dark error :class="{ 'btn--disabled': !selectedChildren.length }" @click.native="onRemoveSelect">Remove<v-icon right dark>arrow_upward</v-icon></v-btn>
            </v-layout>
        </v-container>
        <v-card-row v-if="operation === 'multiselect'">
            <v-spacer></v-spacer>
            <v-text-field append-icon="search"
                          label="Search"
                          single-line
                          hide-details
                          v-model="childSearch"></v-text-field>
        </v-card-row>
        <v-data-table v-if="operation === 'multiselect'"
                      :headers="headers"
                      :items="childItems"
                      v-model="selectedChildren"
                      :pagination.sync="childPagination"
                      :total-items="totalChildItems"
                      :loading="childLoading"
                      :search="childSearch"
                      select-all>
            <template slot="items" scope="props">
                <td><v-checkbox hide-details primary v-model="props.selected" v-if="deletePendingChildItems.indexOf(props.item[props.item.primaryKeyProperty]) === -1"></v-checkbox></td>
                <td v-for="field in headers" :class="{ 'text-xs-right': field.text !== 'Name' }">{{ props.item[field.value] }}</td>
                <td v-if="deletePendingChildItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1" colspan="3">Deleting...</td>
                <td v-else>
                    <span v-if="deleteAskingChildItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1">Are you sure?</span>
                    <v-btn v-else icon v-tooltip:left="{ html: 'view/edit' }" @click.native="onViewChildItem(props.item[props.item.primaryKeyProperty])"><v-icon class="info--text">edit</v-icon></v-btn>
                </td>
                <td v-if="deletePendingChildItems.indexOf(props.item[props.item.primaryKeyProperty]) === -1">
                </td>
                <td v-if="deletePendingChildItems.indexOf(props.item[props.item.primaryKeyProperty]) === -1 && (canAdd || deleteAskingChildItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1)">
                    <v-btn v-if="deleteAskingChildItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1" icon v-tooltip:left="{ html: 'cancel delete' }" @click.native="cancelDeleteChild(props.item[props.item.primaryKeyProperty])"><v-icon class="success--text">undo</v-icon></v-btn>
                    <v-btn v-else-if="canAdd" icon v-tooltip:left="{ html: 'copy' }" @click.native="onDuplicate(props.item[props.item.primaryKeyProperty])"><v-icon class="info--text">content_copy</v-icon></v-btn>
                </td>
                <td v-if="deletePendingChildItems.indexOf(props.item[props.item.primaryKeyProperty]) === -1 && (canDeleteChildren || deleteAskingChildItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1)">
                    <v-btn v-if="deleteAskingChildItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1" icon v-tooltip:left="{ html: 'confirm delete' }" @click.native="onDeleteChildItem(props.item[props.item.primaryKeyProperty])"><v-icon class="error--text">delete</v-icon></v-btn>
                    <v-btn v-else-if="deleteChildPermissions[props.item[props.item.primaryKeyProperty]]" icon v-tooltip:left="{ html: 'delete' }" @click.native="deleteAskingChildItems.push(props.item[props.item.primaryKeyProperty])"><v-icon class="error--text">remove_circle</v-icon></v-btn>
                </td>
            </template>
        </v-data-table>
        <v-card-row v-if="activity" class="activity-row">
            <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
        </v-card-row>
        <v-card-row v-else>
            <div v-if="operation === 'select'" class="submit-row condensed">
                <v-btn dark default @click.native="onCancel">Cancel</v-btn>
                <v-btn dark primary @click.native.stop="onSelectItem">Submit</v-btn>
            </div>
            <div v-else-if="operation === 'multiselect' || operation === 'collection'" class="submit-row condensed">
                <v-btn dark primary @click.native="onCancel">Finish</v-btn>
            </div>
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