<template>
    <div>
        <div class="field-vuetify-time-wrapper">
            <v-subheader>{{ schema.label }}</v-subheader>
            <v-text-field :type="number"
                          :value="dayValue"
                          @input="onDayChange(newValue)"
                          :disabled="disabled"
                          :errors="errors"
                          :max="dayMax"
                          min="0"
                          suffix="days"></v-text-field>
            <v-text-field :type="number"
                          :value="hourValue"
                          @input="onHourChange(newValue)"
                          :disabled="disabled"
                          :errors="errors"
                          max="24"
                          min="-1"
                          suffix="hours"></v-text-field>
            <v-text-field :type="number"
                          :value="minuteValue"
                          @input="onMinuteChange(newValue)"
                          :disabled="disabled"
                          :errors="errors"
                          max="60"
                          min="-1"
                          suffix="minutes"></v-text-field>
            <v-text-field :type="number"
                          :value="secondValue"
                          @input="onSecondChange(newValue)"
                          :disabled="disabled"
                          :errors="errors"
                          max="60"
                          min="0"
                          step="0.001"
                          suffix="seconds"></v-text-field>
        </div>
        <div class="input-group__details">
            <div class="input-group__hint">{{ schema.hint }}</div>
        </div>
    </div>
</template>