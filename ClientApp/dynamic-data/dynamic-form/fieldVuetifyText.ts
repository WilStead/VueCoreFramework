import { abstractField } from 'vue-form-generator';

export default {
    mixins: [abstractField],
    data() {
        return {
            pwVis: false
        };
    },
    methods: {
        onChange(event) {
            if (this.schema.inputType === "file") {
                this.value = event.target.files;
            }
        },
        formatValueToModel(value) {
            if (value != null && this.schema.inputType === "number") {
                return Number(value);
            }
            return value;
        },
        rules() {
            if (!this.errors.length) {
                return true;
            } else {
                return this.errors.join('; ');
            }
        }
    }
};