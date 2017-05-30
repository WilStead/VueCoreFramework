<template>
    <v-card>
        <v-dialog v-model="model.deleteDialogShown">
            <v-card>
                <v-card-row>
                    <v-card-title>Are you sure you want to delete this item?</v-card-title>
                </v-card-row>
                <v-card-row actions>
                    <v-btn class="green--text darken-1" flat="flat" @click.native="model.deleteDialogShown = false">Cancel</v-btn>
                    <v-btn class="red--text accent-4" flat="flat" @click.native="onDelete">Delete</v-btn>
                </v-card-row>
            </v-card>
        </v-dialog>
        <v-alert error :value="errorMessage">{{ errorMessage }}</v-alert>
        <v-card-row>
            <vue-form-generator class="vfg-container" :schema="schema" :model="model" :options="formOptions" @validated="onValidated"></vue-form-generator>
        </v-card-row>
        <v-card-row v-if="activity" class="activity-row">
            <v-progress-circular indeterminate class="primary--text"></v-progress-circular>
        </v-card-row>
        <v-card-row v-else>
            <div v-if="operation === 'details'" class="submit-row condensed">
                <v-btn dark default ripple @click.native.stop.prevent="onCancel">Back</v-btn>
                <v-btn light primary ripple @click.native.stop.prevent="onEdit">Edit</v-btn>
            </div>
            <div v-else-if="operation === 'edit'" class="submit-row condensed">
                <v-btn dark default ripple @click.native.stop.prevent="onCancel">Cancel</v-btn>
                <v-btn light primary ripple :class="{ 'btn--disabled': !isValid }" @click.native.stop.prevent="onSave">Save</v-btn>
            </div>
            <div v-else-if="operation === 'create'" class="submit-row condensed">
                <v-btn dark default ripple @click.native.stop.prevent="onCancel">Cancel</v-btn>
                <v-btn light primary ripple :class="{ 'btn--disabled': !isValid }" @click.native.stop.prevent="onCreate">Create</v-btn>
            </div>
        </v-card-row>
    </v-card>
</template>

<script src="./dynamic-form.ts"></script>