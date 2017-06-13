<template>
    <v-text-field :type="schema.inputType === 'password' ? (pwVis ? 'text' : schema.inputType) : schema.inputType"
                  v-model="value"
                  :append-icon="schema.inputType === 'password' ? (pwVis ? 'visibility_off' : 'visibility') : ''"
                  :append-icon-cb="() => (pwVis = !pwVis)"
                  :counter="value.length > schema.max"
                  :disabled="disabled"
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
                  :required="schema.required"
                  :rows="schema.rows"
                  :step="schema.step"
                  :suffix="schema.suffix"></v-text-field>
</template>

<script src="./fieldVuetifyText.ts"></script>