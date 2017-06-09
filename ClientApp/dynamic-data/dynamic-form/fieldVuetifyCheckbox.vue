<template>
    <v-checkbox v-model="value"
                :disabled="disabled"
                :hint="schema.hint"
                :label="schema.placeholder"
                :name="schema.inputName"
                :persistent-hint="schema.hint !== undefined"
                :prepend-icon="schema.icon"></v-checkbox>
</template>

<script src="./fieldVuetifyCheckbox.ts"></script>