<template>
    <v-card>
        <v-alert error :value="errorMessage">{{ error }}</v-alert>
        <v-card-row>
            <vue-form-generator class="vfg-container" :schema="schema" :model="model" :options="formOptions" @validated="onValidated"></vue-form-generator>
        </v-card-row>
        <v-card-row v-if="success" transition="v-fade-transition" class="text-md-center success--text">Success</v-card-row>
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