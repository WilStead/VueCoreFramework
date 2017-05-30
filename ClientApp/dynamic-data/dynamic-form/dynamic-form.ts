import Vue from 'vue';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as ErrorMsg from '../../error-msg';
import { FieldDefinition } from '../../store/field-definition';
import VueFormGenerator from 'vue-form-generator';
import { Repository, OperationReply } from '../../store/repository';

@Component
export default class DynamicFormComponent extends Vue {
    @Prop()
    id: string;

    @Prop()
    operation: string;

    @Prop()
    repository: Repository;

    @Prop()
    routeName: string;

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
    errorMessage = '';
    formOptions = {
        validateAfterLoad: true,
        validateAfterChanged: true
    };
    isValid = false;
    model = {};
    schema: any = {};
    success = false;
    vm: any;
    vmDefinition: Array<FieldDefinition>;

    mounted() {
        this.updateForm();
    }

    onValidated(isValid: boolean, errors: Array<any>) {
        this.isValid = isValid;
    }

    onCancel() {
        this.success = false;
        this.activity = false;
        this.errorMessage = '';
        this.$router.go(-1);
    }

    onCreate() {
        this.success = false;
        this.activity = true;
        this.errorMessage = '';
        let timestamp = Date.now();
        let d = Object.assign({}, this.model);
        d['id'] = timestamp.toString();
        d['creationTimestamp'] = timestamp;
        d['updateTimestamp'] = timestamp;
        this.repository.add(this.$route.fullPath, d)
            .then(data => {
                if (data.error) {
                    this.errorMessage = data.error;
                } else {
                    this.$router.go(-1);
                }
                this.activity = false;
            })
            .catch(error => {
                this.activity = false;
                this.errorMessage = "A problem occurred. The new item could not be added.";
                ErrorMsg.logError("dynamic-form.onCreate", error);
            });
    }

    onEdit() {
        this.$router.push({ name: this.routeName, params: { operation: 'edit', id: this.id } });
    }

    onSave() {
        this.success = false;
        this.activity = true;
        this.errorMessage = '';
        let d = Object.assign({}, this.model);
        this.repository.update(this.$route.fullPath, d)
            .then(data => {
                if (data.error) {
                    this.errorMessage = data.error;
                } else {
                    this.success = true;
                }
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = "A problem occurred. The item could not be updated.";
                this.activity = false;
                ErrorMsg.logError("dynamic-form.onSave", error);
            });
    }

    updateForm() {
        this.success = false;
        this.activity = true;
        this.errorMessage = '';
        this.repository.find(this.$route.fullPath, this.id)
            .then(data => {
                if (data.error) {
                    this.errorMessage = data.error;
                    this.activity = false;
                } else {
                    this.repository.getFieldDefinitions(this.$route.fullPath)
                        .then(defData => {
                            this.vmDefinition = defData;
                            this.vm = data.data;
                            this.model = {};
                            let groups = this.vmDefinition.filter(v => v.groupName !== undefined && v.groupName !== null).map(v => v.groupName);
                            if (groups.length) {
                                this.schema = { groups: [] };
                                for (var i = 0; i < groups.length; i++) {
                                    this.schema.groups[i] = { fields: [] };
                                    this.schema.groups[i].legend = groups[i];
                                }
                            }
                            this.schema = { fields: [] };
                            this.vmDefinition.forEach(field => {
                                this.model[field.model] = field.default || null;
                            });
                            this.vmDefinition.forEach(field => {
                                if (this.schema.groups) {
                                    let group: any;
                                    if (field.groupName) {
                                        group = this.schema.groups.find(g => g.legend == field.groupName);
                                    } else {
                                        group = this.schema.groups.find(g => g.legend == "Other");
                                        if (!group) {
                                            group = { legend: "Other", fields: [] };
                                            this.schema.groups.push(group);
                                        }
                                    }
                                    group.fields.push(Object.assign({}, field));
                                }
                                else {
                                    this.schema.fields.push(Object.assign({}, field));
                                }
                            });
                            if (this.operation === 'details') {
                                this.schema.fields.forEach(f => f.readonly = true);
                            }
                            for (var prop in this.vm) {
                                this.model[prop] = this.vm[prop];
                            }
                            this.activity = false;
                        })
                        .catch(error => {
                            this.errorMessage = "A problem occurred while updating the data.";
                            this.activity = false;
                            ErrorMsg.logError("dynamic-form.updateForm", error);
                        });
                }
            })
            .catch(error => {
                this.errorMessage = "A problem occurred while updating the data.";
                this.activity = false;
                ErrorMsg.logError("dynamic-form.updateForm", error);
            });
    }
}