<template>
    <v-dialog v-model="dialog">
        <v-btn slot="activator"
               :style="{ 'background-color': value }"
               :disabled="disabled"
               light>{{ schema.label }}<v-icon right light>format_color_fill</v-icon></v-btn>
        <v-card>
            <v-card-row width="172px">
                <div class="color-row">
                    <div class="color-spacer"></div>
                    <div class="color-picker">
                        <div class="color-field" :style="{ 'background-color': value }">
                            <div class="saturation-field">
                                <div class="lightness-field" @click.native="onColorPick($event)">
                                    <div class="color-dot" :style="{ top: colorY, left: colorX }"></div>
                                </div>
                            </div>
                        </div>
                        <div class="hue-field"
                             @click.native="onHuePick($event)">
                            <div class="hue-slider" :style="{ top: hueSliderY }"></div>
                        </div>
                    </div>
                </div>
            </v-card-row>
            <v-card-row>
                <v-container fluid>
                    <v-layout row>
                        <v-flex xs3>
                            <v-container fluid>
                                <v-layout column justify-space-between>
                                    <v-radio dark label="RGB" value="RGB" v-model="inputType"></v-radio>
                                    <v-radio dark label="HSL" value="HSL" v-model="inputType"></v-radio>
                                </v-layout>
                            </v-container>
                        </v-flex>
                        <v-flex xs9>
                            <v-container fluid>
                                <v-layout row>
                                    <v-flex xs9>
                                        <v-slider :label="inputType === '0' ? 'R' : 'H'"></v-slider>
                                    </v-flex>
                                    <v-flex xs3>
                                        <v-text-field dark type="number" :value="inputValue1" @input="onInput1"></v-text-field>
                                    </v-flex>
                                    <v-flex xs9>
                                        <v-slider :label="inputType === '0' ? 'G' : 'S'"></v-slider>
                                    </v-flex>
                                    <v-flex xs3>
                                        <v-text-field dark type="number" :value="inputValue2" @input="onInput2"></v-text-field>
                                    </v-flex>
                                    <v-flex xs9>
                                        <v-slider :label="inputType === '0' ? 'B' : 'L'"></v-slider>
                                    </v-flex>
                                    <v-flex xs3>
                                        <v-text-field dark type="number" :value="inputValue3" @input="onInput3"></v-text-field>
                                    </v-flex>
                                </v-layout>
                            </v-container>
                        </v-flex>
                    </v-layout>
                </v-container>
            </v-card-row>
            <v-card-row actions>
                <v-btn default flat @click.native="dialog = false">Cancel</v-btn>
                <v-btn primary flat @click.native="onSelect">Select</v-btn>
            </v-card-row>
        </v-card>
    </v-dialog>
</template>

<script src="./fieldVuetifyColor.ts"></script>