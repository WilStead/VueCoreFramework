<template>
    <v-layout row wrap style="margin: 0;">
        <div v-if="schema.inputType === 'number' && !schema.required" style="margin-right: 12px;">
            <v-checkbox dark v-model="nullCheck" :disabled="disabled || schema.readonly"></v-checkbox>
        </div>
        <div style="flex-grow: 1;">
            <v-text-field :type="schema.inputType === 'password' ? (pwVis ? 'text' : schema.inputType) : schema.inputType"
                          v-model="value"
                          :append-icon="schema.inputType === 'password' ? (pwVis ? 'visibility_off' : 'visibility') : ''"
                          :append-icon-cb="() => (pwVis = !pwVis)"
                          :counter="value && value.length && value.length > schema.max"
                          :disabled="disabled || (schema.inputType === 'number' && !schema.required && !nullCheck)"
                          :errors="errors"
                          :hint="schema.hint"
                          :label="schema.placeholder"
                          :max="schema.max"
                          :maxlength="schema.maxlength"
                          :min="schema.min"
                          :multi-line="schema.inputType === 'textArea'"
                          :multiple="schema.multiple"
                          :name="schema.inputName"
                          :persistent-hint="schema.hint !== undefined"
                          :prepend-icon="schema.icon"
                          :prefix="schema.prefix"
                          :readonly="schema.readonly"
                          :required="schema.required"
                          :rows="schema.rows"
                          :step="schema.step"
                          :suffix="schema.suffix"></v-text-field>
        </div>
    </v-layout>
</template>

<script src="./fieldVuetifyText.ts"></script>