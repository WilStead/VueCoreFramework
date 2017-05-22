import Vue from 'vue';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as ErrorMsg from '../../components/error/error-msg';
import { FieldDefinition } from '../field-definition';
import VueFormGenerator from 'vue-form-generator';
import { Repository, OperationReply } from '../../store/repository';

@Component
export default class DynamicFormComponent extends Vue {
    @Prop()
    operation: string;

    @Prop()
    repository: Repository<any>;

    @Prop()
    vm: any;

    @Prop()
    vmDefinition: Array<FieldDefinition>;

    components = {
        'vue-form-generator': VueFormGenerator.component
    };

    activity = false;
    errorMessages: Array<string> = [];
    formOptions = {
        validateAfterChanged: true
    };
    isValid = false;
    model = {};
    schema = {
        fields: []
    };
    success = false;
    vmCopy: any;

    mounted() {
        this.clearForm();
        this.setFormData();
    }

    beforeRouteUpdate(to, from, next) {
        this.clearForm();
        this.setFormData();
        next();
    }

    clearForm() {
        this.success = false;
        this.vmCopy = Object.assign({}, this.vm);
        this.model = {};
        this.schema = { fields: [] };
        this.vmDefinition.forEach(field => {
            this.model[field.model] = field.default || null;
        });
        this.vmDefinition.forEach(field => {
            this.schema.fields.push(field);
        });
        if (this.operation === 'details') {
            this.schema.fields.forEach(f => f.readonly = true);
        }
    }

    onValidated(isValid: boolean, errors: Array<any>) {
        this.isValid = isValid;
    }

    setFormData() {
        for (var prop in this.vm) {
            this.model[prop] = this.vm[prop];
        }
    }

    onCancel() {
        this.success = false;
        this.errorMessages = [];
        this.$router.go(-1);
    }

    onCreate() {
        this.success = false;
        this.activity = true;
        let timestamp = Date.now();
        let d = Object.assign({}, this.model);
        d['id'] = timestamp.toString();
        d['creationTimestamp'] = timestamp;
        this.repository.add(d)
            .then(data => {
                this.errorMessages = data.errors;
                this.activity = false;
                if (data.errors.length === 0) {
                    this.$router.go(-1);
                }
            })
            .catch(error => {
                this.activity = false;
                ErrorMsg.showErrorMsgAndLog("A problem occurred. The new item could not be added.", error);
            });
    }

    onEdit() {
        this.success = false;
        this.operation = 'edit';
        this.clearForm();
        this.setFormData();
    }

    onSave() {
        this.success = false;
        this.activity = true;
        let d = Object.assign({}, this.model);
        this.repository.update(d)
            .then(data => {
                this.errorMessages = data.errors;
                this.activity = false;
                if (data.errors.length === 0) {
                    this.success = true;
                }
            })
            .catch(error => {
                this.activity = false;
                ErrorMsg.showErrorMsgAndLog("A problem occurred. The item could not be updated.", error);
            });
    }
}