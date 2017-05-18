import Vue from 'vue';
import { Component, Prop } from 'vue-property-decorator';
import * as ErrorMsg from '../../components/error/error-msg';
import { FieldDefinition } from '../../dynamic-forms/field-definition';
import VueFormGenerator from 'vue-form-generator';

@Component
export default class DynamicFormComponent extends Vue {
    @Prop()
    vm: any;

    @Prop()
    vmDefinition: Array<FieldDefinition>;

    @Prop()
    operation: string;

    @Prop()
    errorMessage: string;

    components = {
        'vue-form-generator': VueFormGenerator.component
    };

    status: string;
    submitted = false;
    vmCopy: any;

    model = {};

    schema = {
        fields: []
    };

    mounted() {
        this.vmDefinition.forEach(field => {
            this.schema.fields.push(field);
        });
        this.clearForm();
    }

    clearForm() {
        this.vmCopy = Object.assign({}, this.vm);
        this.vmDefinition.forEach(field => {
            this.model[field.model] = null;
            if (field.default) {
                this.model[field.model] = field.default;
            }
        });
    }
}