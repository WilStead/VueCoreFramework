import { abstractField } from 'vue-form-generator';

export default {
    mixins: [abstractField],
    data() {
        return {
            pwVis: false
        };
    },
    methods: {
        formatValueToModel(value) {
            if (value != null && this.schema.inputType === "number") {
                return Number(value);
            }
            return value;
        }
    }
};