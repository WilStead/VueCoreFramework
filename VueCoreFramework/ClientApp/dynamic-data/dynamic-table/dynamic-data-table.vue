<template>
    <v-card flat>
        <v-card-row>
            <v-spacer></v-spacer>
            <v-text-field append-icon="search"
                          label="Search"
                          single-line
                          hide-details
                          v-model="internalSearch"></v-text-field>
        </v-card-row>
        <v-card-row>
            <v-spacer></v-spacer>
            <v-btn v-if="canAdd" icon v-tooltip:top="{ html: 'new' }" @click.native="onNew"><v-icon class="success--text">add_circle</v-icon></v-btn>
            <v-dialog v-if="canDelete" v-model="deleteDialogShown">
                <v-btn icon slot="activator" v-tooltip:top="{ html: 'delete' }"><v-icon :class="{ 'error--text': selected.length > 0 }">remove_circle</v-icon></v-btn>
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
                      :pagination.sync="internalPagination"
                      :total-items="totalItems"
                      :loading="loading"
                      :search="internalSearch"
                      select-all>
            <template slot="items" scope="props">
                <td><v-checkbox hide-details primary v-model="props.selected" v-if="deletePendingItems.indexOf(props.item[props.item.primaryKeyProperty]) === -1"></v-checkbox></td>
                <td v-for="field in headers" :class="{ 'text-xs-right': field.text !== 'Name' }">{{ props.item[field.value] }}</td>
                <td v-if="deletePendingItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1" colspan="3">Deleting...</td>
                <td v-else>
                    <span v-if="deleteAskingItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1">Are you sure?</span>
                    <v-btn v-else icon v-tooltip:top="{ html: 'view/edit' }" @click.native="onViewItem(props.item[props.item.primaryKeyProperty])"><v-icon class="info--text">edit</v-icon></v-btn>
                </td>
                <td v-if="deletePendingItems.indexOf(props.item[props.item.primaryKeyProperty]) === -1 && (canAdd || deleteAskingItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1)">
                    <v-btn v-if="deleteAskingItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1" icon v-tooltip:top="{ html: 'cancel delete' }" @click.native="cancelDelete(props.item[props.item.primaryKeyProperty])"><v-icon class="success--text">undo</v-icon></v-btn>
                    <v-btn v-else-if="canAdd" icon v-tooltip:top="{ html: 'copy' }" @click.native="onDuplicate(props.item[props.item.primaryKeyProperty])"><v-icon class="info--text">content_copy</v-icon></v-btn>
                </td>
                <td v-if="deletePendingItems.indexOf(props.item[props.item.primaryKeyProperty]) === -1 && (canDelete || deleteAskingItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1)">
                    <v-btn v-if="deleteAskingItems.indexOf(props.item[props.item.primaryKeyProperty]) !== -1" icon v-tooltip:top="{ html: 'confirm delete' }" @click.native="onDeleteItem(props.item[props.item.primaryKeyProperty])"><v-icon class="error--text">delete</v-icon></v-btn>
                    <v-btn v-else-if="deletePermissions[props.item[props.item.primaryKeyProperty]]" icon v-tooltip:top="{ html: 'delete' }" @click.native="deleteAskingItems.push(props.item[props.item.primaryKeyProperty])"><v-icon class="error--text">remove_circle</v-icon></v-btn>
                </td>
            </template>
        </v-data-table>
    </v-card>
</template>

<script src="./dynamic-data-table.ts"></script>