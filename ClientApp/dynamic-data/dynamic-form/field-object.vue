<template>
    <div class="field-object-container" :id="getFieldId(schema)">
        <span>{{ value }}</span>
        <div v-if="hasValue">
            <v-alert v-if="errorMessage" error :value="errorMessage">{{ errorMessage }}</v-alert>
            <div v-else-if="deletePending" class="field-object-controls"><span>Deleting...</span></div>
            <div v-else>
                <div v-if="deleteAsking" class="field-object-controls">
                    <span>Are you sure?</span>
                    <v-btn icon v-tooltip:left="{ html: 'cancel delete' }" @click.native="deleteAsking = false"><v-icon class="success--text">undo</v-icon></v-btn>
                    <v-btn icon v-tooltip:left="{ html: 'confirm delete' }" @click.native="onDelete"><v-icon class="red--text text--accent-4">delete</v-icon></v-btn>
                </div>
                <div v-else class="field-object-controls">
                    <v-btn icon v-tooltip:left="{ html: 'details' }" @click.native="onDetail"><v-icon class="info--text">details</v-icon></v-btn>
                    <v-btn icon v-tooltip:left="{ html: 'edit' }" @click.native="onEdit"><v-icon class="orange--text text--darken-2">edit</v-icon></v-btn>
                    <v-btn icon v-tooltip:left="{ html: 'delete' }" @click.native="deleteAsking = true"><v-icon class="red--text text--accent-4">remove_circle</v-icon></v-btn>
                </div>
            </div>
        </div>
    </div>
</template>

<script src="./field-object.ts"></script>
<style src="./field-object.scss" lang="scss"></style>