<template>
    <div class="field-container">
        <span class="field-content">{{ value }}</span>
        <v-progress-circular v-if="activity" indeterminate class="primary--text"></v-progress-circular>
        <div v-else class="progress-circular-placeholder"></div>
        <div class="field-button-container">
            <v-btn v-if="!schema.disabled && schema.navigationType !== 'objectReference' && (schema.navigationType === 'objectSelect' || model[schema.model] === '[None]')" icon v-tooltip:top="{ html: 'new' }" @click="onNew"><v-icon class="success--text">add_circle</v-icon></v-btn>
            <v-dialog fullscreen v-if="!schema.disabled && schema.navigationType === 'objectSelect'" v-model="selectDialogShown" :overlay="false">
                <v-btn icon slot="activator" v-tooltip:top="{ html: 'select' }"><v-icon class="primary--text">view_list</v-icon></v-btn>
                <v-card>
                    <v-alert error :value="selectErrorMessage">{{ selectErrorMessage }}</v-alert>
                    <v-alert warning :value="selectWarningMessage">{{ selectWarningMessage }}</v-alert>
                    <dynamic-data-table :childProp="schema.inverseType"
                                        :dataType="schema.inputType"
                                        :parentId="model[model.primaryKeyProperty]"
                                        :parentProp="schema.model"
                                        :parentType="model.dataType"
                                        :selected.sync="selected"
                                        @onError="onSelectError"></dynamic-data-table>
                    <v-card-text v-if="selectActivity" class="activity-row">
                        <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
                    </v-card-text>
                    <v-card-actions v-else>
                        <v-btn default @click="selectDialogShown = false">Cancel</v-btn>
                        <v-btn primary @click.stop="onSelect">Submit</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
            <v-btn v-if="model[schema.model] !== '[None]'" icon v-tooltip:top="{ html: 'view/edit' }" @click="onView"><v-icon class="info--text">edit</v-icon></v-btn>
            <v-dialog v-if="!schema.disabled && schema.navigationType !== 'objectReference' && (!schema.required && model[schema.model] !== '[None]')" v-model="deleteDialogShown">
                <v-btn icon slot="activator" v-tooltip:top="{ html: 'delete' }"><v-icon class="error--text">remove_circle</v-icon></v-btn>
                <v-card>
                    <v-card-title primary-title class="headline">Are you sure you want to delete this item?</v-card-title>
                    <v-card-actions>
                        <v-btn class="success--text" flat @click="deleteDialogShown = false">Cancel</v-btn>
                        <v-btn class="error--text" flat @click="onDelete">Delete</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
        </div>
        <v-dialog v-model="replaceDialogShown">
            <v-card>
                <v-card-title primary-title class="headline">Are you sure you want to replace the current item? This action cannot be undone.</v-card-title>
                <v-card-actions>
                    <v-btn class="success--text" flat @click="replaceDialogShown = false">Cancel</v-btn>
                    <v-btn class="error--text" flat @click="onReplace">Replace</v-btn>
                </v-card-actions>
            </v-card>
        </v-dialog>
    </div>
</template>

<script src="./fieldNavigation.ts"></script>