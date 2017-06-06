<template>
    <v-text-field :type="schema.inputType === 'password' ? (pwVis ? 'text' : schema.inputType) : schema.inputType"
                  :value="value"
                  @input="value = $event"
                  @change="onChange"
                  :append-icon="schema.inputType === 'password' ? (pwVis ? 'visibility' : 'visibility_off') : ''"
                  :append-icon-cb="() => (pwVis = !pwVis)"
                  :counter="value.length > schema.max"
                  :disabled="disabled || schema.readonly"
                  :errors="errors"
                  :hint="schema.hint"
                  :label="schema.placeholder"
                  :max="schema.max"
                  :maxlength="schema.maxlength"
                  :min="schema.min"
                  :multi-line="schema.inputType === 'textArea'"
                  :multiple="schema.multiple"
                  :name="schema.inputName"
                  :prepend-icon="schema.icon"
                  :persistent-hint="schema.hint !== undefined"
                  :prefix="schema.prefix"
                  :required="schema.required"
                  :rows="schema.rows"
                  :rules="[rules]"
                  :step="schema.step"
                  :suffix="schema.suffix"></v-text-field>
</template>

<script src="./fieldVuetifyText.ts"></script>