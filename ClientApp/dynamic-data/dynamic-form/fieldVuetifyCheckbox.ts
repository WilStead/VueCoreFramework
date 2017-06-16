import { abstractField } from 'vue-form-generator';

export default {
    mixins: [abstractField],
    data() {
        return {
            checkState: this.value,
            indeterminate: !this.schema.required && (this.value === undefined || this.value === null)
        };
    },
    watch: {
        checkState(newVal) {
            this.indeterminate = false;
            this.value = newVal;
        }
    },
    methods: {
        formatValueToModel(value) {
            if (!this.schema.required && this.indeterminate) {
                return null;
            } else if (!this.value) {
                return false;
            } else {
                return true;
            }
        },
        onClear() {
            this.value = null;
            this.indeterminate = true;
        }
    }
};