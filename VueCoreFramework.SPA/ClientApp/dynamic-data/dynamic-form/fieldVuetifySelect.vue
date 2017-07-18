<template>
    <v-select v-model="selectedValue"
              :chips="schema.inputType === 'multiple'"
              :disabled="disabled"
              :errors="errors"
              :hint="schema.hint"
              :items="schema.inputType === 'multiple' ? schema.values.filter(v => v.value !== 0) : schema.values"
              :label="schema.placeholder"
              :multiple="schema.inputType === 'multiple'"
              :name="schema.inputName"
              :persistent-hint="schema.hint !== undefined"
              :prepend-icon="schema.icon"
              :rules="[() => (!schema.required || schema.inputType !== 'multiple' || selectedValue !== 0) || 'Please select an option']"
              :single-line="schema.inputType === 'single'">
    </v-select>
</template>

<script src="./fieldVuetifySelect.ts"></script>