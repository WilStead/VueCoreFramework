import { abstractField } from 'vue-form-generator';

export default {
    mixins: [abstractField],
    methods: {
        formatValueToModel(value) {
            return value;
        }
    }
};