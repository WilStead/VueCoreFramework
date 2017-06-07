import Vue from 'vue';
import { Component, Prop, Watch } from 'vue-property-decorator';
import * as ErrorMsg from '../../error-msg';
import { FieldDefinition } from '../../store/field-definition';
import VueFormGenerator from 'vue-form-generator';
import { DataItem, Repository, OperationReply } from '../../store/repository';
import { router } from '../../router';

@Component
export default class DynamicFormComponent extends Vue {
    @Prop()
    id: string;

    @Prop()
    operation: string;

    @Prop()
    parentId: string;

    @Prop()
    parentProp: string;

    @Prop()
    parentType: string;

    @Prop()
    repositoryType: string;

    @Prop()
    routeName: string;

    @Watch('id')
    onIdChanged(val: string, oldVal: string) {
        if (this.updateTimeout === 0) {
            this.updateTimeout = setTimeout(this.updateForm, 125);
        }
    }

    @Watch('operation')
    onOperationChanged(val: string, oldVal: string) {
        if (this.updateTimeout === 0) {
            this.updateTimeout = setTimeout(this.updateForm, 125);
        }
    }

    @Watch('parentType')
    onParentTypeChanged(val: string, oldVal: string) {
        this.parentRepository = new Repository(val);
        if (this.updateTimeout === 0) {
            this.updateTimeout = setTimeout(this.updateForm, 125);
        }
    }

    @Watch('repositoryType')
    onRepositoryTypeChanged(val: string, oldVal: string) {
        this.repository = new Repository(val);
        if (this.updateTimeout === 0) {
            this.updateTimeout = setTimeout(this.updateForm, 125);
        }
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
    model: any = { dataType: this.repositoryType };
    parentRepository: Repository = null;
    repository: Repository = null;
    schema: any = {};
    updateTimeout = 0;
    vm: any;
    vmDefinition: Array<FieldDefinition>;

    mounted() {
        this.repository = new Repository(this.repositoryType);
        if (this.parentType && this.parentId) {
            this.parentRepository = new Repository(this.parentType);
        }
        this.updateForm();
    }

    onValidated(isValid: boolean, errors: Array<any>) {
        this.isValid = isValid;
    }

    addFieldToSchema(field: FieldDefinition) {
        let newField: FieldDefinition = Object.assign({}, field);
        if (newField.type.startsWith("object")) {
            if (newField.type === "object" || newField.type === "objectSelect") {
                let idField = this.vmDefinition.find(v => v.model === newField.model + "Id");
                if (idField) {
                    newField.buttons = [];
                    if (newField.type === "objectSelect") {
                        newField.buttons.push({
                            classes: 'btn btn--dark btn--flat primary--text',
                            label: 'Select',
                            onclick: function (model, field) {
                                router.push({
                                    name: newField.inputType + "Table",
                                    params: {
                                        operation: 'select',
                                        parentType: model.dataType,
                                        parentId: model.id,
                                        parentProp: newField.model
                                    }
                                });
                            }
                        });
                    }
                    this.addObjectButtons(newField, idField);
                }
            } else {
                newField.buttons = [{
                    classes: 'btn btn--dark btn--flat info--text',
                    label: 'View/Edit',
                    onclick: function (model, field) {
                        router.push({
                            name: newField.inputType + "Table",
                            params: {
                                operation: 'multiselect',
                                parentType: model.dataType,
                                parentId: model.id,
                                parentProp: newField.model
                            }
                        });
                    }
                }];
            }
            newField.type = "label";
        }
        if (field.groupName) {
            let group = this.schema.groups.find(g => g.legend == field.groupName);
            group.fields.push(newField);
        } else {
            this.schema.fields.push(newField);
        }
    }

    addObjectButtons(newField: FieldDefinition, idField: FieldDefinition) {
        if (this.operation === "edit"
            && (newField.type === "objectSelect" || !this.model[idField.model])) {
            newField.buttons.push({
                classes: 'btn btn--dark btn--flat success--text',
                label: 'Add',
                onclick: function (model, field) {
                    router.push({
                        name: newField.inputType,
                        params: {
                            operation: 'create',
                            id: Date.now().toString(),
                            parentType: model.dataType,
                            parentId: model.id,
                            parentProp: newField.model
                        }
                    });
                }
            });
        }
        newField.buttons.push({
            classes: 'btn btn--dark btn--flat info--text',
            label: 'View/Edit',
            onclick: function (model, field) {
                router.push({ name: newField.inputType, params: { operation: 'details', id: model[field.model + "Id"] } });
            }
        });
        if (this.operation === "edit" && !newField.required) {
            newField.buttons.push({
                classes: 'btn btn--dark btn--flat error--text',
                label: 'Delete',
                onclick: function (model, field, event) {
                    event.stopPropagation();
                    Vue.set(model, 'deleteDialogShown', true);
                    model.deleteProp = field.model;
                    model.deleteId = model[field.model + "Id"];
                }
            });
        }
    }

    onCancel() {
        this.activity = false;
        this.errorMessage = '';
        this.$router.go(-1);
    }

    onCreate() {
        this.activity = true;
        this.errorMessage = '';
        let timestamp = Date.now();
        let d = Object.assign({},
            this.model,
            {
                id: this.id,
                creationTimestamp: timestamp,
                updateTimestamp: timestamp
            }
        );
        delete d.dataType;
        this.repository.add(this.$route.fullPath, d)
            .then(data => {
                if (data.error) {
                    this.errorMessage = data.error;
                } else if (this.parentRepository && this.parentProp) {
                    this.parentRepository.find(this.$route.fullPath, this.parentId)
                        .then(data => {
                            if (data.error) {
                                this.errorMessage = data.error;
                                this.activity = false;
                            } else {
                                let vm = data.data;
                                vm[this.parentProp + "Id"] = data.data.id;
                                this.parentRepository.update(this.$route.fullPath, vm)
                                    .then(data => {
                                        if (data.error) {
                                            this.errorMessage = data.error;
                                        } else {
                                            this.$router.go(-1);
                                        }
                                        this.errorMessage = '';
                                        this.activity = false;
                                    })
                                    .catch(error => {
                                        this.errorMessage = "A problem occurred. The item could not be updated.";
                                        this.activity = false;
                                        ErrorMsg.logError("dynamic-form.onCreate", new Error(error));
                                    });
                            }
                        })
                        .catch(error => {
                            this.errorMessage = "A problem occurred while updating the data.";
                            this.activity = false;
                            ErrorMsg.logError("dynamic-form.onCreate", new Error(error));
                        });
                } else {
                    this.$router.go(-1);
                }
                this.activity = false;
            })
            .catch(error => {
                this.activity = false;
                this.errorMessage = "A problem occurred. The new item could not be added.";
                ErrorMsg.logError("dynamic-form.onCreate", new Error(error));
            });
    }

    onDelete() {
        this.activity = true;
        this.model.deleteDialogShown = false;
        this.vm[this.model.deleteProp] = null;
        this.vm[this.model.deleteProp + "Id"] = null;
        this.repository.update(this.$route.fullPath, this.vm)
            .then(data => {
                if (data.error) {
                    this.errorMessage = data.error;
                } else {
                    this.updateForm();
                }
                this.errorMessage = '';
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = "A problem occurred. The item could not be removed.";
                this.activity = false;
                ErrorMsg.logError("dynamic-form.onDelete", new Error(error));
            });
    }

    onEdit() {
        this.$router.push({ name: this.routeName, params: { operation: 'edit', id: this.id } });
    }

    onSave() {
        this.activity = true;
        this.errorMessage = '';
        let d = Object.assign({
            id: this.id,
            updateTimestamp: Date.now()
        },
            this.model);
        delete d.dataType;
        this.repository.update(this.$route.fullPath, d)
            .then(data => {
                if (data.error) {
                    this.errorMessage = data.error;
                } else {
                    this.$router.go(-1);
                }
                this.errorMessage = '';
                this.activity = false;
            })
            .catch(error => {
                this.errorMessage = "A problem occurred. The item could not be updated.";
                this.activity = false;
                ErrorMsg.logError("dynamic-form.onSave", new Error(error));
            });
    }

    updateForm() {
        this.activity = true;
        this.updateTimeout = 0;
        this.errorMessage = '';
        this.repository.find(this.$route.fullPath, this.id)
            .then(data => {
                this.model = { dataType: this.repositoryType };
                this.schema = { fields: [] };
                if (data.error) {
                    this.errorMessage = data.error;
                    this.activity = false;
                } else {
                    this.repository.getFieldDefinitions(this.$route.fullPath)
                        .then(defData => {
                            this.vmDefinition = defData;
                            this.vm = data.data;
                            let groups = this.vmDefinition.filter(v => v.groupName !== undefined && v.groupName !== null).map(v => v.groupName);
                            if (groups.length) {
                                this.schema.groups = [];
                                for (var i = 0; i < groups.length; i++) {
                                    this.schema.groups[i] = { fields: [] };
                                    this.schema.groups[i].legend = groups[i];
                                }
                            }
                            this.vmDefinition.forEach(field => {
                                this.model[field.model] = field.default || null;
                            });
                            for (var prop in this.vm) {
                                this.model[prop] = this.vm[prop];
                            }
                            this.vmDefinition.forEach(field => {
                                this.addFieldToSchema(field);
                            });
                            if (this.operation === 'details') {
                                this.schema.fields.forEach(f => f.disabled = true);
                            }
                            this.errorMessage = '';
                            this.activity = false;
                        })
                        .catch(error => {
                            this.errorMessage = "A problem occurred while updating the data.";
                            this.activity = false;
                            ErrorMsg.logError("dynamic-form.updateForm", new Error(error));
                        });
                }
            })
            .catch(error => {
                this.errorMessage = "A problem occurred while updating the data.";
                this.activity = false;
                ErrorMsg.logError("dynamic-form.updateForm", new Error(error));
            });
    }
}