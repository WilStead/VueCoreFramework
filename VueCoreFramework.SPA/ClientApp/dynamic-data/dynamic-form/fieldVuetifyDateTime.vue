<template>
    <div class="field-vuetify-time-wrapper" style="display: flex;">
        <v-menu v-if="schema.inputType === 'date' || schema.inputType === 'dateTime'"
                v-model="menuDate"
                :close-on-content-click="false"
                :disabled="disabled"
                lazy
                :nudge-left="40"
                offset-y
                transition="v-scale-transition">
            <v-text-field slot="activator"
                          v-model="formattedDate"
                          :disabled="disabled"
                          :hint="schema.hint"
                          :label="schema.placeholder"
                          :name="schema.inputName"
                          :persistent-hint="schema.hint !== undefined"
                          prepend-icon="event"
                          readonly></v-text-field>
            <v-date-picker v-model="value"
                           actions
                           :allowed-dates="{ min: schema.min, max: schema.max }"
                           :date-format="date => new Date(date).toDateString()"
                           :formatted-value.sync="formattedDate"
                           no-title
                           scrollable>
                <template scope="{ save, cancel }">
                    <v-card-actions>
                        <v-btn flat primary @click="cancel()">Cancel</v-btn>
                        <v-btn flat primary @click="save()">Save</v-btn>
                    </v-card-actions>
                </template>
            </v-date-picker>
        </v-menu>
        <v-menu v-if="schema.inputType === 'time' || schema.inputType === 'dateTime'"
                v-model="menuTime"
                :close-on-content-click="false"
                :disabled="disabled"
                lazy
                :nudge-left="40"
                offset-y
                transition="v-scale-transition"
                :style="{ 'margin-left': (schema.inputType === 'time' || schema.inputType === 'dateTime') ? '15px' : 0 }">
            <v-text-field slot="activator"
                          v-model="valueTime"
                          :disabled="disabled"
                          :hint="schema.hint"
                          :label="schema.placeholder"
                          :name="schema.inputName"
                          :persistent-hint="schema.hint !== undefined"
                          prepend-icon="access_time"
                          readonly></v-text-field>
            <v-time-picker v-model="valueTime"
                           actions>
                <template scope="{ save, cancel }">
                    <v-card-actions>
                        <v-btn flat primary @click="cancel()">Cancel</v-btn>
                        <v-btn flat primary @click="save()">Save</v-btn>
                    </v-card-actions>
                </template>
            </v-time-picker>
        </v-menu>
        <div v-if="!schema.required" class="null-clear-container">
            <v-btn class="btn-null-clear" floating primary :disabled="disabled" @click="onClear"><v-icon>remove</v-icon></v-btn>
        </div>
    </div>
</template>

<script src="./fieldVuetifyDateTime.ts"></script>