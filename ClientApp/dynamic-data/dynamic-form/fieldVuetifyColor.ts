import { abstractField } from 'vue-form-generator';

export default {
    mixins: [abstractField],
    data() {
        let color = this.value === "[None]" ? "#000000" : this.value;
        let r = parseInt(color.substr(1, 2), 16);
        let g = parseInt(color.substr(3, 2), 16);
        let b = parseInt(color.substr(5, 2), 16);
        let [h, s, v] = this.rgbToHsv(r, g, b);
        let [h2, s2, l] = this.hsvToHsl(h, s, v);
        return {
            dialog: false,
            inputType: "RGB",
            temp: color,
            r, g, b,
            hue: h,
            saturation: s,
            hslSaturation: s2,
            brightness: v,
            lightness: l
        }
    },
    computed: {
        colorX() {
            let cf = this.getColorFieldDimension();
            return this.saturation * cf - 5 + "px";
        },
        colorY() {
            let cf = this.getColorFieldDimension();
            return (1 - this.brightness) * cf - 5 + "px";
        },
        fullBright() {
            let [r, g, b] = this.hsvToRgb(this.hue, 1, 1);
            return this.rgbToHex(r, g, b);
        },
        hueSliderY() {
            let cf = this.getColorFieldDimension();
            return this.hue * cf - 4 + "px";
        },
        inputValue1() {
            if (this.inputType === "RGB") {
                return this.r;
            } else {
                return this.hue;
            }
        },
        inputValue2() {
            if (this.inputType === "RGB") {
                return this.g;
            } else {
                return this.hslSaturation;
            }
        },
        inputValue3() {
            if (this.inputType === "RGB") {
                return this.b;
            } else {
                return this.lightness;
            }
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
        getColorFieldPosition() {
            let d = document.getElementsByClassName("dialog--active");
            if (!d.length) {
                return 0;
            }
            let cf = d[0].getElementsByClassName("color-field");
            if (!cf.length) {
                return 0;
            }
            let pos = cf[0].getBoundingClientRect();
            return [pos.left, pos.top];
        },
        getHueFieldPosition() {
            let d = document.getElementsByClassName("dialog--active");
            if (!d.length) {
                return 0;
            }
            let hf = d[0].getElementsByClassName("hue-field");
            if (!hf.length) {
                return 0;
            }
            let pos = hf[0].getBoundingClientRect();
            return [pos.left, pos.top];
        },
        hslToHsv(h, s, l) {
            l *= 2;
            s *= (l <= 1) ? l : 2 - l;
            let v = (l + s) / 2;
            let _s = (2 * s) / (l + s);

            return [h, _s, v];
        },
        hsvToHsl(h, s, v) {
            let _s = s * v;
            let l = (2 - s) * v;
            _s /= (l <= 1) ? l : 2 - l;
            _s = Math.round(_s * 1000) / 1000;
            l /= 2;
            l = Math.round(l * 1000) / 1000;
            return [h, _s, l];
        },
        hsvToRgb(h, s, v) {
            let i = Math.floor(h * 6);
            let f = h * 6 - i;
            let p = v * (1 - s);
            let q = v * (1 - f * s);
            let t = v * (1 - (1 - f) * s);
            let r: number, g: number, b: number;
            switch (i % 6) {
                case 0: r = v, g = t, b = p; break;
                case 1: r = q, g = v, b = p; break;
                case 2: r = p, g = v, b = t; break;
                case 3: r = p, g = q, b = v; break;
                case 4: r = t, g = p, b = v; break;
                case 5: r = v, g = p, b = q; break;
            }
            return [Math.round(r * 255), Math.round(g * 255), Math.round(b * 255)];
        },
        lightText() {
            let color = this.value === "[None]" ? "#000000" : this.value;
            let r = parseInt(color.substr(1, 2), 16);
            let g = parseInt(color.substr(3, 2), 16);
            let b = parseInt(color.substr(5, 2), 16);
            let brightness = ((r * 299) + (g * 587) + (b * 114)) / 255000;
            return brightness < 0.5;
        },
        onColorMove(event) {
            let cfD = this.getColorFieldDimension();
            let [cfX, cfY] = this.getColorFieldPosition();
            let x = Math.min(cfD, Math.max(0, event.clientX - cfX));
            let y = Math.min(cfD, Math.max(0, event.clientY - cfY));
            this.saturation = Math.round(x / cfD * 1000) / 1000;
            this.brightness = Math.round((1 - y / cfD) * 1000) / 1000;
            this.updateHsv();
        },
        onColorStop(event) {
            this.onColorMove(event);
            window.removeEventListener('mousemove', this.onColorMove, false);
            window.removeEventListener('mouseup', this.onColorStop, true);
        },
        onHueMove(event) {
            let cfD = this.getColorFieldDimension();
            let [hfX, hfY] = this.getHueFieldPosition();
            let y = Math.min(cfD, Math.max(0, event.clientY - hfY));
            this.hue = Math.round(y / cfD * 1000) / 1000;
            this.updateHsv();
        },
        onHueStop(event) {
            this.onHueMove(event);
            window.removeEventListener('mousemove', this.onHueMove, false);
            window.removeEventListener('mouseup', this.onHueStop, true);
        },
        onInput1(newValue) {
            if (newValue === undefined || isNaN(newValue)) {
                return;
            }
            if (this.inputType === "RGB") {
                this.r = Math.max(0, Math.min(255, newValue));
                this.updateRGB();
            } else {
                this.hue = Math.max(0, Math.min(1, newValue));
                this.updateHsl();
            }
        },
        onInput2(newValue) {
            if (newValue === undefined || isNaN(newValue)) {
                return;
            }
            if (this.inputType === "RGB") {
                this.g = Math.max(0, Math.min(255, newValue));
                this.updateRGB();
            } else {
                this.hslSaturation = Math.max(0, Math.min(1, newValue));
                this.updateHsl();
            }
        },
        onInput3(newValue) {
            if (newValue === undefined || isNaN(newValue)) {
                return;
            }
            if (this.inputType === "RGB") {
                this.b = Math.max(0, Math.min(255, newValue));
                this.updateRGB();
            } else {
                this.lightness = Math.max(0, Math.min(1, newValue));
                this.updateHsl();
            }
        },
        onSelect() {
            this.value = this.temp;
            this.dialog = false;
        },
        rgbToHex(r, g, b) {
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
            return "#" + rh + gh + bh;
        },
        rgbToHsv(r, g, b) {
            let max = Math.max(r, g, b), min = Math.min(r, g, b);
            let d = max - min;
            let h: number;
            let s = max === 0 ? 0 : d / max;
            let v = max / 255;
            switch (max) {
                case min: h = 0; break;
                case r: h = (g - b) + d * (g < b ? 6 : 0); h /= 6 * d; break;
                case g: h = (b - r) + d * 2; h /= 6 * d; break;
                case b: h = (r - g) + d * 4; h /= 6 * d; break;
            }

            return [h, s, v];
        },
        startColorDrag(event) {
            window.addEventListener('mousemove', this.onColorMove, false);
            window.addEventListener('mouseup', this.onColorStop, true);
        },
        startHueDrag(event) {
            window.addEventListener('mousemove', this.onHueMove, false);
            window.addEventListener('mouseup', this.onHueStop, true);
        },
        updateHsv() {
            let [r, g, b] = this.hsvToRgb(this.hue, this.saturation, this.brightness);
            this.r = r;
            this.g = g;
            this.b = b;
            let [h, s, l] = this.hsvToHsl(this.hue, this.saturation, this.brightness);
            this.hslSaturation = s;
            this.lightness = l;
            this.temp = this.rgbToHex(r, g, b);
        },
        updateHsl() {
            let [h, s, v] = this.hslToHsv(this.hue, this.hslSaturation, this.lightness);
            this.saturation = s;
            this.brightness = v;
            let [r, g, b] = this.hsvToRgb(this.hue, this.saturation, this.brightness);
            this.r = r;
            this.g = g;
            this.b = b;
            this.temp = this.rgbToHex(r, g, b);
        },
        updateRGB() {
            let [h, s, v] = this.rgbToHsv(this.r, this.g, this.b);
            this.hue = h;
            this.saturation = s;
            this.brightness = v;
            let [h2, s2, l] = this.hsvToHsl(h, s, v);
            this.hslSaturation = s2;
            this.lightness = l;
            this.temp = this.rgbToHex(this.r, this.g, this.b);
        }
    }
};