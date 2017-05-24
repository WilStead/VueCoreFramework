import Vue from 'vue';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as ErrorMsg from '../../components/error/error-msg';
import { FieldDefinition } from '../field-definition';
import VueFormGenerator from 'vue-form-generator';
import { Repository, OperationReply } from '../../store/repository';

@Component
export default class DynamicFormComponent extends Vue {
    @Prop()
    id: string;

    @Prop()
    operation: string;

    @Prop()
    repository: Repository<any>;

    @Prop()
    routeName: string;

    @Prop()
    vmDefinition: Array<FieldDefinition>;

    @Watch('id')
    onIdChanged(val: string, oldVal: string) {
        this.updateForm();
    }

    @Watch('operation')
    onOperationChanged(val: string, oldVal: string) {
        this.updateForm();
    }

    components = {
        'vue-form-generator': VueFormGenerator.component
    };

    activity = false;
    errorMessages: Array<string> = [];
    formOptions = {
        validateAfterLoad: true,
        validateAfterChanged: true
    };
    isValid = false;
    model = {};
    schema = {
        fields: []
    };
    success = false;
    vm: any;
    vmCopy: any;

    mounted() {
        this.updateForm();
    }

    onValidated(isValid: boolean, errors: Array<any>) {
        this.isValid = isValid;
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
        this.$router.push({ name: this.routeName, params: { operation: 'edit', id: this.id } });
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

    updateForm() {
        this.repository.find(this.id)
            .then(data => {
                this.success = false;
                this.vm = data;
                this.vmCopy = Object.assign({}, this.vm);
                this.model = {};
                this.schema = { fields: [] };
                this.vmDefinition.forEach(field => {
                    this.model[field.model] = field.default || null;
                });
                this.vmDefinition.forEach(field => {
                    this.schema.fields.push(Object.assign({}, field));
                });
                if (this.operation === 'details') {
                    this.schema.fields.forEach(f => f.readonly = true);
                }
                for (var prop in this.vm) {
                    this.model[prop] = this.vm[prop];
                }
            });
    }
}