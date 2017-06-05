<template>
    <div class="wrapper">
        <v-text-field :id="getFieldId(schema)"
                      :type="schema.inputType"
                      :value="value"
                      @input="value = $event.target.value"
                      @change="onChange"
                      :class="{ 'input-group--disabled': disabled || schema.readonly, 'input-group--required': schema.required }"
                      :accept="schema.accept"
                      :alt="schema.alt"
                      :autocomplete="schema.autocomplete"
                      :checked="schema.checked"
                      :dirname="schema.dirname"
                      :formaction="schema.formaction"
                      :formenctype="schema.formenctype"
                      :formmethod="schema.formmethod"
                      :formnovalidate="schema.formnovalidate"
                      :formtarget="schema.formtarget"
                      :height="schema.height"
                      :list="schema.list"
                      :max="schema.max"
                      :maxlength="schema.maxlength"
                      :min="schema.min"
                      :multiple="schema.multiple"
                      :name="schema.inputName"
                      :pattern="schema.pattern"
                      :label="schema.placeholder"
                      :size="schema.size"
                      :src="schema.src"
                      :step="schema.step"
                      :width="schema.width"
                      :files="schema.files"
                      :hint="schema.hint"></v-text-field>
        <span class="helper" v-if="schema.inputType === 'color' || schema.inputType === 'range'">{{ value }}</span>
    </div>
</template>

<script src="./auto-vuetify-text-field.ts"></script>