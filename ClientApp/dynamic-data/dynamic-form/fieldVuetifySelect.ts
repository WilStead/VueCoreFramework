import { abstractField } from 'vue-form-generator';

export default {
    mixins: [abstractField],
    computed: {
        selectedValue: {
            get() {
                if (this.schema.inputType === 'multiple') {
                    let val = [];
                    if (this.value) {
                        for (var i = 131072; i > 0; i = Math.floor(i / 2)) {
                            let x = i & this.value;
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
                    return this.value;
                }
            },
            set(newValue) {
                if(this.schema.inputType === 'multiple') {
                    let val = 0;
                    if (newValue && newValue.length) {
                        for (var i = 0; i < newValue.length; i++) {
                            val += newValue[i];
                        }
                    }
                    this.value = val;
                } else {
                    this.value = newValue;
                }
            }
        }
    },
    methods: {
        formatValueToModel(value) {
            return value;
        }
    }
};