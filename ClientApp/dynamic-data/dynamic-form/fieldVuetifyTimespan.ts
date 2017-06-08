import { abstractField } from 'vue-form-generator';
import * as moment from 'moment';

export default {
    mixins: [abstractField],
    data() {
        return {
            duration: moment.duration(this.value),
            dayValue: 0,
            hourValue: 0,
            minuteValue: 0,
            secondValue: 0
        };
    },
    methods: {
        formatValueToModel(value) {
            if (value != null && this.schema.inputType === "number") {
                return Number(value);
            }
            return value;
        },
        onDayChange(newValue) {
            this.duration.subtract(this.duration.days(), 'd');
            this.duration.add(newValue, 'd');
            this.updateValue();
        },
        onHourChange(newValue) {
            this.duration.subtract(this.duration.hours(), 'h');
            this.duration.add(newValue, 'h');
            this.updateValue();
        },
        onMinuteChange(newValue) {
            this.duration.subtract(this.duration.minutes(), 'm');
            this.duration.add(newValue, 'm');
            this.updateValue();
        },
        onSecondChange(newValue) {
            this.duration.subtract(this.duration.milliseconds(), 'ms');
            this.duration.subtract(this.duration.seconds(), 's');
            this.duration.add(newValue, 's');
            this.updateValue();
        },
        updateValue() {
            this.value = moment.utc(this.duration.asMilliseconds()).format("D.HH:mm:ss.SSS");
        }
    }
};