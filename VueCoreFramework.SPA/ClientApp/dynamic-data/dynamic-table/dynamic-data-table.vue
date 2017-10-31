<template>
    <v-card flat>
        <v-card-text v-if="totalItems > 5" class="pb-0">
            <v-spacer></v-spacer>
            <v-text-field append-icon="search"
                          label="Search"
                          single-line
                          hide-details
                          v-model="internalSearch"></v-text-field>
        </v-card-text>
        <v-card-text v-if="allowEdit" class="pa-0">
            <v-container fluid class="pa-0">
                <v-layout justify-end>
                    <v-tooltip top>
                        <v-btn v-if="canAdd" icon slot="activator" @click="onNew"><v-icon class="success--text">add_circle</v-icon></v-btn>
                        <span>new</span>
                    </v-tooltip>
                    <v-tooltip top>
                        <v-btn icon slot="activator" @click.native.stop="deleteDialogShown = true"><v-icon :class="{ 'error--text': selected.length > 0 }">remove_circle</v-icon></v-btn>
                        <span>delete</span>
                    </v-tooltip>
                    <v-dialog v-if="canDelete" v-model="deleteDialogShown">
                        <v-card>
                            <v-card-title primary-title class="headline">Are you sure you want to delete {{ selected.length > 1 ? 'these' : 'this' }} item{{ selected.length > 1 ? 's' : '' }}?</v-card-title>
                            <v-card-actions>
                                <v-btn class="success--text" flat @click="deleteDialogShown = false">Cancel</v-btn>
                                <v-btn class="error--text" flat @click="onDelete">Delete</v-btn>
                            </v-card-actions>
                        </v-card>
                    </v-dialog>
                </v-layout>
            </v-container>
        </v-card-text>
        <v-data-table v-model="selected"
                      select-all
                      :headers="headers"
                      :hide-actions="totalItems <= 5"
                      :items="items"
                      :loading="loading"
                      :pagination.sync="internalPagination"
                      :total-items="totalItems"
                      :search="internalSearch">
            <template slot="items" slot-scope="props">
                <td><v-checkbox hide-details color="primary" v-model="props.selected" v-if="deletePendingItems.indexOf(props.item[props.item.primaryKeyProperty]) === -1"></v-checkbox></td>
                <td v-for="field in headers" :class="{ 'text-xs-right': field.text !== 'Name' }">
                    <span v-if="field.cultural">{{ getCulturalValue(props.item[field.value]) }}</span>
                    <span v-else>{{ props.item[field.value] }}</span>
                </td>
                <td v-if="deletePendingItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1" colspan="3">Deleting...</td>
                <td v-else>
                    <span v-if="deleteAskingItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1">Are you sure?</span>
                    <v-tooltip top v-else>
                        <v-btn icon slot="activator" @click="onViewItem(props.item[props.item.primaryKeyProperty])"><v-icon class="info--text">edit</v-icon></v-btn>
                        <span>view/edit</span>
                    </v-tooltip>
                </td>
                <td v-if="allowEdit && deletePendingItems.indexOf(props.item[props.item.primaryKeyProperty]) === -1 && (canAdd || deleteAskingItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1)">
                    <v-tooltip top v-if="deleteAskingItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1">
                        <v-btn icon slot="activator" @click="cancelDelete(props.item[props.item.primaryKeyProperty])"><v-icon class="success--text">undo</v-icon></v-btn>
                        <span>cancel delete</span>
                    </v-tooltip>
                    <v-tooltip top v-else-if="allowEdit && canAdd">
                        <v-btn icon slot="activator" @click="onDuplicate(props.item[props.item.primaryKeyProperty])"><v-icon class="info--text">content_copy</v-icon></v-btn>
                        <span>copy</span>
                    </v-tooltip>
                </td>
                <td v-if="allowEdit && deletePendingItems.indexOf(props.item[props.item.primaryKeyProperty]) === -1 && (canDelete || deleteAskingItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1)">
                    <v-tooltip top v-if="deleteAskingItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1">
                        <v-btn icon slot="activator" @click="onDeleteItem(props.item[props.item.primaryKeyProperty])"><v-icon class="error--text">delete</v-icon></v-btn>
                        <span>confirm delete</span>
                    </v-tooltip>
                    <v-tooltip top v-else-if="deletePermissions[props.item[props.item.primaryKeyProperty]]">
                        <v-btn icon slot="activator" @click="deleteAskingItems.push(props.item[props.item.primaryKeyProperty])"><v-icon class="error--text">remove_circle</v-icon></v-btn>
                        <span>delete</span>
                    </v-tooltip>
                </td>
            </template>
        </v-data-table>
    </v-card>
</template>

<script src="./dynamic-data-table.ts"></script>