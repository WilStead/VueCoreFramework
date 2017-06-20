import { abstractField } from 'vue-form-generator';

export default {
    mixins: [abstractField],
    data() {
        return {
            checkState: this.value,
            indeterminate: !this.schema.required && (this.value === undefined || this.value === null || this.value === "[None]")
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
            } else if (!value) {
                return false;
            } else {
                return true;
            }
        },
        onClear() {
            this.indeterminate = true;
            this.value = null;
        }
    }
};