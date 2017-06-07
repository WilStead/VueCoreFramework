import { abstractField } from 'vue-form-generator';

export default {
    mixins: [abstractField],
    data() {
        return {
            formattedDate: new Date(this.value).toDateString(),
            menuDate: false,
            menuTime: false,
            valueTime: this.getShortTime(this.value)
        }
    },
    methods: {
        formatValueToModel(value) {
            return value;
        },
        getShortTime(value) {
            let d = new Date(value);
            let h = d.getHours();
            let am = h < 12;
            if (!am && h !== 12) {
                h -= 12;
            } else if (am && h === 0) {
                h += 12;
            }
            let m = d.getMinutes();
            return h + ":" + (m < 10 ? "0" : "") + m + (am ? "am" : "pm");
        }
    },
    watch: {
        valueTime(newValue) {
            let [h, m] = newValue.split(':');
            let a = m.substring(2);
            m = m.substring(0, 2);
            let val = new Date(this.value);
            val.setUTCHours(a === 'am' ? parseInt(h) : parseInt(h) + 12, parseInt(m));
            this.value = val.toISOString();
        }
    }
};