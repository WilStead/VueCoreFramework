<template>
    <v-select v-model="value"
              :chips="schema.inputType === 'multiple'"
              :disabled="disabled"
              :errors="errors"
              :hint="schema.hint"
              :items="schema.values"
              :label="schema.placeholder"
              :multiple="schema.inputType === 'multiple'"
              :name="schema.inputName"
              :persistent-hint="schema.hint !== undefined"
              :prepend-icon="schema.icon"
              :rules="[() => value !== undefined || 'Please select an option', rules]"
              :single-line="schema.inputType === 'single'">
    </v-select>
</template>

<script src="./fieldVuetifySelect.ts"></script>