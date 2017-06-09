<template>
    <v-dialog v-model="dialog">
        <v-btn slot="activator"
               :style="{ 'background-color': value === '[None]' ? '#000000' : value, color: lightText ? 'white' : 'black' }"
               :disabled="disabled"
               dark>
            {{ schema.label }}
            <v-icon right :style="{ color: lightText ? 'white' : 'black' }">format_color_fill</v-icon>
        </v-btn>
        <v-card class="color-picker-card">
            <v-card-row>
                <div class="color-picker-wrapper">
                    <div class="color-row">
                        <div class="color-spacer"></div>
                        <div class="color-picker">
                            <div class="color-field" :style="{ 'background-color': fullBright }">
                                <div class="saturation-field">
                                    <div class="brightness-field" @mousedown="startColorDrag($event)">
                                        <div class="color-dot" :style="{ top: colorY, left: colorX }"></div>
                                    </div>
                                </div>
                            </div>
                            <div class="hue-field"
                                 @mousedown="startHueDrag($event)">
                                <div class="hue-slider" :style="{ top: hueSliderY }"></div>
                            </div>
                        </div>
                    </div>
                </div>
            </v-card-row>
            <v-card-row>
                <v-container fluid class="pt-0 pb-0">
                    <v-layout row wrap>
                        <v-flex xs6>
                            <v-radio dark label="RGB" value="RGB" v-model="inputType"></v-radio>
                        </v-flex>
                        <v-flex xs6>
                            <v-radio dark label="HSL" value="HSL" v-model="inputType"></v-radio>
                        </v-flex>
                    </v-layout>
                </v-container>
            </v-card-row>
            <v-card-row>
                <v-container fluid class="pt-0 pb-0">
                    <v-layout row wrap>
                        <v-flex xs8>
                            <v-slider dark :label="inputType === 'RGB' ? 'R' : 'H'" :max="inputType === 'RGB' ? 255 : 1" :value="inputValue1" @input="onInput1"></v-slider>
                        </v-flex>
                        <v-flex xs4>
                            <v-text-field dark type="number" :value="inputValue1" @input="onInput1" :step="inputType === 'RGB' ? 1 : 0.001"></v-text-field>
                        </v-flex>
                        <v-flex xs8>
                            <v-slider dark :label="inputType === 'RGB' ? 'G' : 'S'" :max="inputType === 'RGB' ? 255 : 1" :value="inputValue2" @input="onInput2"></v-slider>
                        </v-flex>
                        <v-flex xs4>
                            <v-text-field dark type="number" :value="inputValue2" @input="onInput2" :step="inputType === 'RGB' ? 1 : 0.001"></v-text-field>
                        </v-flex>
                        <v-flex xs8>
                            <v-slider dark :label="inputType === 'RGB' ? 'B' : 'L'" :max="inputType === 'RGB' ? 255 : 1" :value="inputValue3" @input="onInput3"></v-slider>
                        </v-flex>
                        <v-flex xs4>
                            <v-text-field dark type="number" :value="inputValue3" @input="onInput3" :step="inputType === 'RGB' ? 1 : 0.001"></v-text-field>
                        </v-flex>
                    </v-layout>
                </v-container>
            </v-card-row>
            <v-card-row actions>
                <v-btn default flat @click.native="dialog = false">Cancel</v-btn>
                <v-btn primary dark @click.native="onSelect">Select</v-btn>
            </v-card-row>
        </v-card>
    </v-dialog>
</template>

<script src="./fieldVuetifyColor.ts"></script>