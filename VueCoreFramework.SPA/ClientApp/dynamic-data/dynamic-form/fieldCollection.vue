<template>
    <v-card flat style="border: solid 1px #eee;">
        <v-alert error :value="errorMessage">{{ errorMessage }}</v-alert>
        <v-card-title primary-title v-if="!disabled && schema.navigationType === 'objectMultiSelect'" class="headline pt-0 pb-0">
            <v-spacer></v-spacer>
            <v-dialog fullscreen v-model="editDialogShown" :overlay="false">
                <v-btn icon slot="activator" v-tooltip:top="{ html: 'edit collection' }"><v-icon class="info--text">edit</v-icon></v-btn>
                <v-card>
                    <v-alert error :value="editErrorMessage">{{ editErrorMessage }}</v-alert>
                    <dynamic-data-table :childProp="schema.inverseType"
                                        :dataType="schema.inputType"
                                        :parentId="model[model.primaryKeyProperty]"
                                        :parentProp="schema.model"
                                        :parentType="model.dataType"
                                        :selected.sync="selected"
                                        tableType="multiselect"
                                        @onError="onEditError"></dynamic-data-table>
                    <v-container fluid>
                        <v-layout row justify-space-around>
                            <v-btn success icon v-tooltip:top="{ html: 'add' }" :disabled="!selected.length" @click="onAddSelect"><v-icon>arrow_downward</v-icon></v-btn>
                            <v-btn error icon v-tooltip:top="{ html: 'remove' }" :disabled="!selectedChildren.length" @click="onRemoveSelect"><v-icon>arrow_upward</v-icon></v-btn>
                        </v-layout>
                    </v-container>
                    <dynamic-data-table :childProp="schema.inverseType"
                                        :dataType="schema.inputType"
                                        :parentId="model[model.primaryKeyProperty]"
                                        :parentProp="schema.model"
                                        :parentType="model.dataType"
                                        :selected.sync="selectedChildren"
                                        tableType="child"
                                        @onError="onEditError"></dynamic-data-table>
                    <v-card-text v-if="activity" class="activity-row">
                        <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
                    </v-card-text>
                    <v-card-actions v-else>
                        <v-btn primary @click="editDialogShown = false">Finish</v-btn>
                    </v-card-actions>
                </v-card>
            </v-dialog>
        </v-card-title>
        <dynamic-data-table :allowEdit="!disabled && schema.navigationType === 'objectCollection'"
                            :childProp="schema.inverseType"
                            :dataType="schema.inputType"
                            :parentId="model[model.primaryKeyProperty]"
                            :parentProp="schema.model"
                            :parentType="model.dataType"
                            :tableType="schema.navigationType === 'objectCollection' ? 'collection' : 'child'"
                            @onError="onError"></dynamic-data-table>
    </v-card>
</template>

<script src="./fieldCollection.ts"></script>