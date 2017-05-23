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
            <v-btn icon v-tooltip:left="{ html: 'new' }" @click.native.stop.prevent="onNew"><v-icon class="green--text">add_circle</v-icon></v-btn>
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
                <td><v-checkbox hide-details primary v-model="props.selected"></v-checkbox></td>
                <td v-for="field in headers" :class="{ 'text-xs-right': field.text !== 'Name' }">{{ props.item[field.value] }}</td>
                <td><v-btn icon v-tooltip:left="{ html: 'details' }" @click.native.stop.prevent="onDetail(props.item.id)"><v-icon class="info--text">details</v-icon></v-btn></td>
                <td><v-btn icon v-tooltip:left="{ html: 'edit' }" @click.native.stop.prevent="onEdit(props.item.id)"><v-icon class="orange--text text--darken-2">edit</v-icon></v-btn></td>
                <td>
                    <v-dialog v-model="deleteItemDialogShown[props.item.id]">
                        <v-btn icon slot="activator" v-tooltip:left="{ html: 'delete' }"><v-icon class="red--text text--accent-4">remove_circle</v-icon></v-btn>
                        <v-card>
                            <v-card-row>
                                <v-card-title>Are you sure you want to delete this item?</v-card-title>
                            </v-card-row>
                            <v-card-row actions>
                                <v-btn class="green--text darken-1" flat="flat" @click.native="deleteItemDialogShown[props.item.id] = false">Cancel</v-btn>
                                <v-btn class="red--text accent-4" flat="flat" @click.native="onDeleteItem(props.item.id)">Delete</v-btn>
                            </v-card-row>
                        </v-card>
                    </v-dialog>
                </td>
            </template>
        </v-data-table>
        <v-card-row v-if="activity" class="activity-row">
            <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
        </v-card-row>
    </v-card>
</template>

<script src="./dynamic-table.ts"></script>