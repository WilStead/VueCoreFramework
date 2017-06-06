<template>
    <v-checkbox v-model="value"
                :disabled="disabled"
                :errors="errors"
                :hint="schema.hint"
                :label="schema.placeholder"
                :name="schema.inputName"
                :persistent-hint="schema.hint !== undefined"
                :prepend-icon="schema.icon"
                :rules="[rules]"></v-checkbox>
</template>

<script src="./fieldVuetifyCheckbox.ts"></script>