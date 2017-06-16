<template>
    <v-layout row wrap style="margin: 0;">
        <div style="flex-grow: 1;">
            <v-checkbox v-model="checkState"
                        :disabled="disabled"
                        :indeterminate="indeterminate"
                        :hint="schema.hint"
                        :label="schema.placeholder"
                        :name="schema.inputName"
                        :persistent-hint="schema.hint !== undefined"
                        :prepend-icon="schema.icon"></v-checkbox>
        </div>
        <div v-if="!schema.required" style="margin-left: 12px;">
            <v-btn dark floating small primary :disabled="disabled" @click.native="onClear"><v-icon light>remove</v-icon></v-btn>
        </div>
    </v-layout>
</template>

<script src="./fieldVuetifyCheckbox.ts"></script>