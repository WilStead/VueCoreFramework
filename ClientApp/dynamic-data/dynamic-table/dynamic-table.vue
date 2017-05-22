<template>
    <v-card>
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
            <v-btn icon class="green--text" @click.native.stop.prevent="onNew"><span class="sr-only">new</span><v-icon v-tooltip:left="{ html: 'new' }">add_circle</v-icon></v-btn>
            <v-dialog v-model="deleteDialogShown">
                <v-btn icon slot="activator" class="red--text"><span class="sr-only">delete</span><v-icon v-tooltip:left="{ html: 'delete' }">remove_circle</v-icon></v-btn>
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
                      :totalItems="totalItems"
                      :loading="loading"
                      :search="search"
                      select-all>
            <template slot="items" scope="props">
                <td><v-checkbox hide-details primary v-model="props.selected"></v-checkbox></td>
                <td v-for="field in headers" :class="{ 'text-xs-right': field.text !== 'Name' }">{{ props.item[field.value] }}</td>
                <td><v-btn icon v-tooltip:left="{ html: 'details' }" @click.native.stop.prevent="onDetail(props.item.id)"><v-icon>details</v-icon></v-btn></td>
                <td><v-btn icon v-tooltip:left="{ html: 'edit' }" @click.native.stop.prevent="onEdit(props.item.id)"><v-icon>edit</v-icon></v-btn></td>
            </template>
        </v-data-table>
        <v-card-row v-if="activity" class="activity-row">
            <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
        </v-card-row>
    </v-card>
</template>

<script src="./dynamic-table.ts"></script>