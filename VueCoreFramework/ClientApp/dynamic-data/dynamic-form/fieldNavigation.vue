<template>
    <div class="field-container">
        <span class="field-content">{{ value }}</span>
        <v-progress-circular v-if="activity" indeterminate class="primary--text"></v-progress-circular>
        <div v-else class="progress-circular-placeholder"></div>
        <div class="field-button-container">
            <v-btn v-if="schema.allowedButtons.indexOf('new') !== -1" icon v-tooltip:top="{ html: 'new' }" @click.native="onNew"><v-icon class="success--text">add_circle</v-icon></v-btn>
            <v-btn v-if="schema.allowedButtons.indexOf('select') !== -1" icon v-tooltip:top="{ html: 'select' }" @click.native="onSelect"><v-icon class="primary--text">view_list</v-icon></v-btn>
            <v-btn v-if="schema.allowedButtons.indexOf('details') !== -1" icon v-tooltip:top="{ html: 'view/edit' }" @click.native="onView"><v-icon class="info--text">edit</v-icon></v-btn>
            <v-dialog v-if="schema.allowedButtons.indexOf('delete') !== -1" v-model="deleteDialogShown">
                <v-btn icon slot="activator" v-tooltip:top="{ html: 'delete' }"><v-icon class="error--text">remove_circle</v-icon></v-btn>
                <v-card>
                    <v-card-row>
                        <v-card-title>Are you sure you want to delete this item?</v-card-title>
                    </v-card-row>
                    <v-card-row actions>
                        <v-btn class="success--text" flat @click.native="deleteDialogShown = false">Cancel</v-btn>
                        <v-btn class="error--text" flat @click.native="onDelete">Delete</v-btn>
                    </v-card-row>
                </v-card>
            </v-dialog>
        </div>
        <v-dialog v-model="replaceDialogShown">
            <v-card>
                <v-card-row>
                    <v-card-title>Are you sure you want to replace the current item? This action cannot be undone.</v-card-title>
                </v-card-row>
                <v-card-row actions>
                    <v-btn class="success--text" flat @click.native="replaceDialogShown = false">Cancel</v-btn>
                    <v-btn class="error--text" flat @click.native="onReplace">Replace</v-btn>
                </v-card-row>
            </v-card>
        </v-dialog>
    </div>
</template>

<script src="./fieldNavigation.ts"></script>