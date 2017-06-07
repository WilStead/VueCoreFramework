import { abstractField } from 'vue-form-generator';

export default {
    mixins: [abstractField],
    data() {
        return {
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
            this.updateValue();
        },
        onHourChange(newValue) {
            this.updateValue();
        },
        onMinuteChange(newValue) {
            this.updateValue();
        },
        onSecondChange(newValue) {
            this.updateValue();
        },
        updateValue() {
        }
    }
};