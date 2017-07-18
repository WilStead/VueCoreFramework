import { abstractField } from 'vue-form-generator';

export default {
    mixins: [abstractField],
    data() {
        return {
            nullCheck: this.schema.inputType !== 'number' || this.schema.required || (this.value !== undefined && this.value !== null && this.value !== '[None]'),
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
            } else if (this.schema.inputType === "cultural") {
                let cValue = JSON.parse(value);
                let v = cValue[this.$store.state.userState.culture];
                if (!v) {
                    let def = cValue.default;
                    if (def) {
                        v = cValue[def];
                    }
                }
                return v;
            } else {
                return value;
            }
        },
        formatValueToModel(value) {
            if (value != null && this.schema.inputType === "number") {
                if (!this.nullCheck) {
                    return null;
                } else {
                    return Number(value);
                }
            } else if (this.schema.inputType === "cultural") {
                let cValue = JSON.parse(this.model[this.schema.model]);
                if (Object.keys(cValue).length === 0) {
                    cValue.default = this.$store.state.userState.culture;
                }
                cValue[this.$store.state.userState.culture] = value;
                return JSON.stringify(cValue);
            } else {
                return value;
            }
        }
    }
};