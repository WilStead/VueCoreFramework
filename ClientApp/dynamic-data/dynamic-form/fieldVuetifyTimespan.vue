<template>
    <div class="input-group input-group--dark">
        <v-subheader>{{ schema.label }}</v-subheader>
        <div class="field-vuetify-time-wrapper">
            <v-text-field v-if="showYear"
                          type="number"
                          :value="yearValue"
                          @input="onYearChange"
                          :disabled="disabled"
                          :max="yearMax"
                          :min="yearMin"
                          suffix="years"></v-text-field>
            <v-text-field v-if="showMonth"
                          type="number"
                          :value="monthValue"
                          @input="onMonthChange"
                          :disabled="disabled"
                          :max="monthMax"
                          :min="monthMin"
                          suffix="months"></v-text-field>
            <v-text-field v-if="showDay"
                          type="number"
                          :value="dayValue"
                          @input="onDayChange"
                          :disabled="disabled"
                          :max="dayMax"
                          :min="dayMin"
                          suffix="days"></v-text-field>
            <v-text-field v-if="showHour"
                          type="number"
                          :value="hourValue"
                          @input="onHourChange"
                          :disabled="disabled"
                          max="hourMax"
                          min="hourMin"
                          suffix="hours"></v-text-field>
            <v-text-field v-if="showMinute"
                          type="number"
                          :value="minuteValue"
                          @input="onMinuteChange"
                          :disabled="disabled"
                          max="minuteMax"
                          min="minuteMin"
                          suffix="minutes"></v-text-field>
            <v-text-field v-if="showSecond"
                          type="number"
                          :value="secondValue"
                          @input="onSecondChange"
                          :disabled="disabled"
                          max="secondMax"
                          min="secondMin"
                          step="schema.step"
                          suffix="seconds"></v-text-field>
        </div>
        <div class="field-vuetify-time-details input-group__details">
            <div class="input-group__messages">
                <div class="input-group__hint" v-if="schema.hint.length">{{ schema.hint }}</div>
                <div class="input-group__error" v-for="error in errors">{{ error }}</div>
            </div>
        </div>
    </div>
</template>

<script src="./fieldVuetifyTimespan.ts"></script>