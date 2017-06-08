import { abstractField } from 'vue-form-generator';

export default {
    mixins: [abstractField],
    data() {
        return {
            dialog: false,
            inputType: "RGB",
            inputValue1: parseInt(this.value.substring(1, 2)),
            inputValue2: parseInt(this.value.substring(3, 2)),
            inputValue3: parseInt(this.value.substring(5, 2)),
            temp: "#000000",
            hue: 0,
            saturation: 0,
            saturationWidth: 135.6,
            lightness: 0,
            lightnessHeight: 135.6,
            hueHeight: 135.6
        }
    },
    computed: {
        hueSliderY() {
            return this.hue * this.hueHeight - 3;
        },
        colorX() {
            return this.saturation * this.saturationWidth - 5;
        },
        colorY() {
            return this.lightness * this.lightnessHeight - 5;
        }
    },
    methods: {
        formatValueToModel(value) {
            return value;
        },
        hslToRgb(h, s, l) {
            let r: number;
            let g: number;
            let b: number;

            if (s == 0) {
                r = g = b = l;
            } else {
                let hue2rgb = function hue2rgb(p, q, t) {
                    if (t < 0) t++;
                    if (t > 1) t--;
                    if (t < 1 / 6) return p + (q - p) * 6 * t;
                    if (t < 1 / 2) return q;
                    if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
                    return p;
                }

                let q = l < 0.5 ? l * (1 + s) : l + s - l * s;
                let p = 2 * l - q;
                r = hue2rgb(p, q, h + 1 / 3);
                g = hue2rgb(p, q, h);
                b = hue2rgb(p, q, h - 1 / 3);
            }
            return [Math.round(r * 255), Math.round(g * 255), Math.round(b * 255)];
        },
        onColorPick(event) {
            this.saturation = event.offsetX / this.saturationWidth;
            this.lightness = event.offsetY / this.lightnessHeight;
            this.updateColor();
        },
        onHuePick(event) {
            this.hue = event.offsetY / this.hueHeight;
            this.updateColor();
        },
        onInput1(newValue) {
            if (this.inputType === "RGB") {
                this.value = "#" + newValue.toString(16) + this.value.substring(3);
            } else {
                this.hue = newValue;
                this.updateColor();
            }
        },
        onInput2(newValue) {
            if (this.inputType === "RGB") {
                this.value = this.value.substring(0, 3) + newValue.toString(16) + this.value.substring(5);
            } else {
                this.saturation = newValue;
                this.updateColor();
            }
        },
        onInput3(newValue) {
            if (this.inputType === "RGB") {
                this.value = this.value.substring(0, 5) + newValue.toString(16);
            } else {
                this.lightness = newValue;
                this.updateColor();
            }
        },
        onSelect() {
            this.value = this.temp;
            this.dialog = false;
        },
        updateColor() {
            let [r, g, b] = this.hslToRgb(this.hue, this.saturation, this.lightness);
            this.value = "#" + r.toString(16) + g.toString(16) + b.toString(16);
        }
    }
};