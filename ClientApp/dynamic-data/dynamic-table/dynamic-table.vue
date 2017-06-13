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
            <v-btn icon v-tooltip:left="{ html: 'new' }" @click.native="onNew"><v-icon class="success--text">add_circle</v-icon></v-btn>
            <v-dialog v-model="deleteDialogShown">
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
                <td><v-checkbox hide-details primary v-model="props.selected" v-if="deletePendingItems.indexOf(props.item.id) === -1"></v-checkbox></td>
                <td v-for="field in headers" :class="{ 'text-xs-right': field.text !== 'Name' }">{{ props.item[field.value] }}</td>
                <td v-if="deletePendingItems.indexOf(props.item.id) !== -1" colspan="3">Deleting...</td>
                <td v-if="deletePendingItems.indexOf(props.item.id) === -1">
                    <span v-if="deleteAskingItems.indexOf(props.item.id) !== -1">Are you sure?</span>
                </td>
                <td v-if="deletePendingItems.indexOf(props.item.id) === -1">
                    <v-btn v-if="deleteAskingItems.indexOf(props.item.id) === -1" icon v-tooltip:left="{ html: 'view/edit' }" @click.native="onViewItem(props.item.id)"><v-icon class="info--text">edit</v-icon></v-btn>
                    <v-btn v-else icon v-tooltip:left="{ html: 'cancel delete' }" @click.native="cancelDelete(props.item.id)"><v-icon class="success--text">undo</v-icon></v-btn>
                </td>
                <td v-if="deletePendingItems.indexOf(props.item.id) === -1">
                    <v-btn v-if="deleteAskingItems.indexOf(props.item.id) === -1" icon v-tooltip:left="{ html: 'delete' }" @click.native="deleteAskingItems.push(props.item.id)"><v-icon class="error--text">remove_circle</v-icon></v-btn>
                    <v-btn v-else icon v-tooltip:left="{ html: 'confirm delete' }" @click.native="onDeleteItem(props.item.id)"><v-icon class="error--text">delete</v-icon></v-btn>
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
                <td><v-checkbox hide-details primary v-model="props.selected" v-if="deletePendingChildItems.indexOf(props.item.id) === -1"></v-checkbox></td>
                <td v-for="field in headers" :class="{ 'text-xs-right': field.text !== 'Name' }">{{ props.item[field.value] }}</td>
                <td v-if="deletePendingChildItems.indexOf(props.item.id) !== -1" colspan="3">Deleting...</td>
                <td v-if="deletePendingChildItems.indexOf(props.item.id) === -1">
                    <span v-if="deleteAskingChildItems.indexOf(props.item.id) !== -1">Are you sure?</span>
                </td>
                <td v-if="deletePendingChildItems.indexOf(props.item.id) === -1">
                    <v-btn v-if="deleteAskingChildItems.indexOf(props.item.id) === -1" icon v-tooltip:left="{ html: 'view/edit' }" @click.native="onViewChildItem(props.item.id)"><v-icon class="info--text">edit</v-icon></v-btn>
                    <v-btn v-else icon v-tooltip:left="{ html: 'cancel delete' }" @click.native="cancelDeleteChild(props.item.id)"><v-icon class="success--text">undo</v-icon></v-btn>
                </td>
                <td v-if="deletePendingChildItems.indexOf(props.item.id) === -1">
                    <v-btn v-if="deleteAskingChildItems.indexOf(props.item.id) === -1" icon v-tooltip:left="{ html: 'delete' }" @click.native="deleteAskingChildItems.push(props.item.id)"><v-icon class="error--text">remove_circle</v-icon></v-btn>
                    <v-btn v-else icon v-tooltip:left="{ html: 'confirm delete' }" @click.native="onDeleteChildItem(props.item.id)"><v-icon class="error--text">delete</v-icon></v-btn>
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