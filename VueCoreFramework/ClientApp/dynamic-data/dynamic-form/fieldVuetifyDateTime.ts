import { abstractField } from 'vue-form-generator';
import * as moment from 'moment';

export default {
    mixins: [abstractField],
    data() {
        return {
            formattedDate: this.value === undefined || this.value === null || this.value === "[None]" ? "" : new Date(this.value).toDateString(),
            menuDate: false,
            menuTime: false,
            valueTime: this.value === undefined || this.value === null || this.value === "[None]" ? "12:00am" : moment(this.value).format("h:mma")
        }
    },
    methods: {
        formatValueToModel(value) {
            return value;
        },
        onClear() {
            this.value = null;
            this.formattedDate = "";
        }
    },
    watch: {
        valueTime(newValue) {
            let t = moment(newValue, "h:mma");
            let base = this.value === undefined || this.value === null || this.value === "[None]" ? moment().utc() : moment(this.value).utc();
            this.value = base.set({ 'hour': t.hours(), 'minute': t.minutes() }).toISOString();
        }
    }
};