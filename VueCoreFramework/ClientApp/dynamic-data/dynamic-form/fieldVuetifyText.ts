import { abstractField } from 'vue-form-generator';

export default {
    mixins: [abstractField],
    data() {
        return {
            nullCheck: this.schema.inputType !== 'number' || (this.value !== undefined && this.value !== null && this.value !== '[None]'),
            pwVis: false
        };
    },
    watch: {
        nullCheck(newVal) {
            if (!newVal) {
                this.value = null;
            }
        }
    },
    methods: {
        formatValueToField(value) {
            if (value != null && this.schema.inputType === "number" && value === '[None]') {
                return null;
            }
            return value;
        },
        formatValueToModel(value) {
            if (value != null && this.schema.inputType === "number") {
                if (!this.nullCheck) {
                    return null;
                }
                return Number(value);
            }
            return value;
        }
    }
};