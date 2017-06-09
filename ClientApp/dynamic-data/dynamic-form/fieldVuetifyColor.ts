import { abstractField } from 'vue-form-generator';

export default {
    mixins: [abstractField],
    data() {
        let color = this.value === "[None]" ? "#000000" : this.value;
        let r = parseInt(color.substring(1, 2), 16);
        let g = parseInt(color.substring(3, 2), 16);
        let b = parseInt(color.substring(5, 2), 16);
        let [h, s, l] = this.rgbToHsl(r, g, b);
        return {
            dialog: false,
            inputType: "RGB",
            inputValue1: r,
            inputValue2: g,
            inputValue3: b,
            temp: color,
            hue: h,
            saturation: s,
            saturationWidth: 135.6,
            lightness: l,
            lightnessHeight: 135.6,
            hueHeight: 135.6
        }
    },
    watch: {
        inputType(newValue) {
            if (newValue === "RGB") {
                this.inputValue1 = parseInt(this.value.substring(1, 2), 16);
                this.inputValue2 = parseInt(this.value.substring(3, 2), 16);
                this.inputValue3 = parseInt(this.value.substring(5, 2), 16);
            } else {
                this.inputValue1 = this.hue;
                this.inputValue2 = this.saturation;
                this.inputValue3 = this.lightness;
            }
        }
    },
    computed: {
        hueSliderY() {
            let cf = this.getColorFieldDimension();
            return this.hue * cf - 4 + "px";
        },
        colorX() {
            let cf = this.getColorFieldDimension();
            return this.saturation * cf - 5 + "px";
        },
        colorY() {
            let cf = this.getColorFieldDimension();
            return this.lightness * cf - 5 + "px";
        }
    },
    methods: {
        formatValueToModel(value) {
            return value;
        },
        getColorFieldDimension() {
            let d = document.getElementsByClassName("dialog--active");
            if (!d.length) {
                return 0;
            }
            let cf = d[0].getElementsByClassName("color-field");
            if (!cf.length) {
                return 0;
            }
            return cf[0].clientHeight;
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
            let cf = this.getColorFieldDimension();
            this.saturation = event.offsetX / cf;
            this.lightness = event.offsetY / cf;
            this.updateColor();
            this.updateInputs();
        },
        onHuePick(event) {
            let cf = this.getColorFieldDimension();
            this.hue = event.offsetY / cf;
            this.updateColor();
            this.updateInputs();
        },
        onInput1(newValue) {
            if (this.inputType === "RGB") {
                this.temp = "#" + newValue.toString(16) + this.value.substring(3);
            } else {
                this.hue = newValue;
                this.updateColor();
            }
        },
        onInput2(newValue) {
            if (this.inputType === "RGB") {
                this.temp = this.value.substring(0, 3) + newValue.toString(16) + this.value.substring(5);
            } else {
                this.saturation = newValue;
                this.updateColor();
            }
        },
        onInput3(newValue) {
            if (this.inputType === "RGB") {
                this.temp = this.value.substring(0, 5) + newValue.toString(16);
            } else {
                this.lightness = newValue;
                this.updateColor();
            }
        },
        onSelect() {
            this.value = this.temp;
            this.dialog = false;
        },
        rgbToHsl(r, g, b) {
            r /= 255;
            g /= 255;
            b /= 255;
            let max = Math.max(r, g, b);
            let min = Math.min(r, g, b);
            let h: number;
            let s: number;
            let l = (max + min) / 2;

            if (max == min) {
                h = s = 0;
            } else {
                let d = max - min;
                s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
                switch (max) {
                    case r: h = (g - b) / d + (g < b ? 6 : 0); break;
                    case g: h = (b - r) / d + 2; break;
                    case b: h = (r - g) / d + 4; break;
                }
                h /= 6;
            }

            return [h, s, l];
        },
        updateColor() {
            let [r, g, b] = this.hslToRgb(this.hue, this.saturation, this.lightness);
            let rh = r.toString(16);
            if (rh.length < 2) {
                rh = "0" + rh;
            }
            let gh = g.toString(16);
            if (gh.length < 2) {
                gh = "0" + gh;
            }
            let bh = b.toString(16);
            if (bh.length < 2) {
                bh = "0" + bh;
            }
            this.temp = "#" + rh + gh + bh;
        },
        updateInputs() {
            if (this.inputType === "RGB") {
                this.inputValue1 = parseInt(this.temp.substring(1, 2), 16);
                this.inputValue2 = parseInt(this.temp.substring(3, 2), 16);
                this.inputValue3 = parseInt(this.temp.substring(5, 2), 16);
            } else {
                this.inputValue1 = this.hue;
                this.inputValue2 = this.saturation;
                this.inputValue3 = this.lightness;
            }
        }
    }
};