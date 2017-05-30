<template>
    <v-card>
        <v-alert error :value="errorMessage">{{ errorMessage }}</v-alert>
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
            <v-btn icon v-tooltip:left="{ html: 'new' }" @click.native="onNew"><v-icon class="green--text">add_circle</v-icon></v-btn>
            <v-dialog v-model="deleteDialogShown">
                <v-btn icon slot="activator" v-tooltip:left="{ html: 'delete' }"><v-icon :class="{ 'red--text text--accent-4': selected.length > 0 }">remove_circle</v-icon></v-btn>
                <v-card>
                    <v-card-row>
                        <v-card-title>Are you sure you want to delete {{ selected.length > 1 ? 'these' : 'this' }} item{{ selected.length > 1 ? 's' : '' }}?</v-card-title>
                    </v-card-row>
                    <v-card-row actions>
                        <v-btn class="green--text darken-1" flat="flat" @click.native="deleteDialogShown = false">Cancel</v-btn>
                        <v-btn class="red--text accent-4" flat="flat" @click.native="onDelete">Delete</v-btn>
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
                <td><v-checkbox hide-details primary v-model="props.selected" v-if="deletePendingItems.indexOf(props.item.id) === -1"></v-checkbox></td>
                <td v-for="field in headers" :class="{ 'text-xs-right': field.text !== 'Name' }">{{ props.item[field.value] }}</td>
                <td v-if="deletePendingItems.indexOf(props.item.id) !== -1" colspan="3">Deleting...</td>
                <td v-if="deletePendingItems.indexOf(props.item.id) === -1">
                    <v-btn v-if="deleteAskingItems.indexOf(props.item.id) === -1" icon v-tooltip:left="{ html: 'details' }" @click.native="onDetail(props.item.id)"><v-icon class="info--text">details</v-icon></v-btn>
                    <span v-else>Are you sure?</span>
                </td>
                <td v-if="deletePendingItems.indexOf(props.item.id) === -1">
                    <v-btn v-if="deleteAskingItems.indexOf(props.item.id) === -1" icon v-tooltip:left="{ html: 'edit' }" @click.native="onEdit(props.item.id)"><v-icon class="orange--text text--darken-2">edit</v-icon></v-btn>
                    <v-btn v-else icon v-tooltip:left="{ html: 'cancel delete' }" @click.native="cancelDelete(props.item.id)"><v-icon class="success--text">undo</v-icon></v-btn>
                </td>
                <td v-if="deletePendingItems.indexOf(props.item.id) === -1">
                    <v-btn v-if="deleteAskingItems.indexOf(props.item.id) === -1" icon v-tooltip:left="{ html: 'delete' }" @click.native="deleteAskingItems.push(props.item.id)"><v-icon class="red--text text--accent-4">remove_circle</v-icon></v-btn>
                    <v-btn v-else icon v-tooltip:left="{ html: 'confirm delete' }" @click.native="onDeleteItem(props.item.id)"><v-icon class="red--text text--accent-4">delete</v-icon></v-btn>
                </td>
            </template>
        </v-data-table>
        <v-card-row v-if="activity" class="activity-row">
            <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
        </v-card-row>
    </v-card>
</template>

<script src="./dynamic-table.ts"></script>