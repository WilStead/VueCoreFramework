<template>
    <div class="field-vuetify-time-wrapper">
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
                          :errors="errors"
                          :hint="schema.hint"
                          :label="schema.placeholder"
                          :name="schema.inputName"
                          :persistent-hint="schema.hint !== undefined"
                          prepend-icon="event"
                          readonly
                          :rules="[rules]"></v-text-field>
            <v-date-picker v-model="value"
                           actions
                           :allowed-dates="{ min: schema.min, max: schema.max }"
                           :date-format="date => new Date(date).toDateString()"
                           :formatted-value.sync="formattedDate"
                           no-title
                           scrollable>
                <template scope="{ save, cancel }">
                    <v-card-row actions>
                        <v-btn flat primary @click.native="cancel()">Cancel</v-btn>
                        <v-btn flat primary @click.native="save()">Save</v-btn>
                    </v-card-row>
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
                transition="v-scale-transition">
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
                    <v-card-row actions>
                        <v-btn flat primary @click.native="cancel()">Cancel</v-btn>
                        <v-btn flat primary @click.native="save()">Save</v-btn>
                    </v-card-row>
                </template>
            </v-time-picker>
        </v-menu>
    </div>
</template>

<script src="./fieldVuetifyDateTime.ts"></script>