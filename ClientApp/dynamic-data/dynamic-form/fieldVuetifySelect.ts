import { abstractField } from 'vue-form-generator';

export default {
    mixins: [abstractField],
    methods: {
        formatValueToField(value) {
            if (this.schema.inputType === 'multiple') {
                let val = [];
                if (value) {
                    for (var i = 131072; i > 0; i = Math.floor(i / 2)) {
                        let x = i & value;
                        if (x > 0) {
                            val.push(x);
                        }
                    }
                }
                if (val.length === 0) {
                    val.push(0);
                }
                return val;
            } else {
                return value;
            }
        },
        formatValueToModel(value) {
            if (this.schema.inputType === 'multiple') {
                let val = 0;
                if (value && value.length) {
                    for (var i = 0; i < value.length; i++) {
                        val += value[i];
                    }
                }
                return val;
            } else {
                return value;
            }
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