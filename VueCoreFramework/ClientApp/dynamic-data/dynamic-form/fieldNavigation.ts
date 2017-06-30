import { abstractField } from 'vue-form-generator';
import { Repository } from '../../store/repository';
import * as ErrorMsg from '../../error-msg';

export default {
    mixins: [abstractField],
    data() {
        return {
            activity: false,
            deleteDialogShown: false,
            replaceDialogShown: false,
            repository: this.$store.getters.getRepository(this.schema.parentType)
        };
    },
    methods: {
        onDelete() {
            this.deleteDialogShown = false;
            this.errors.splice(0);
            this.activity = true;
            this.repository.removeFromParent(this.$route.fullPath, this.schema.parentId, this.schema.model)
                .then(data => {
                    if (data.error) {
                        this.errors.push(data.error);
                    } else {
                        this.errors.push("navigation success");
                    }
                    this.activity = false;
                    this.$emit("validated", this.errors.length === 0, this.errors, this);
                })
                .catch(error => {
                    this.activity = false;
                    this.errors.push("A problem occurred. The item could not be removed.");
                    this.$emit("validated", this.errors.length === 0, this.errors, this);
                    ErrorMsg.logError("fieldNavigation.onDelete", new Error(error));
                });
        },

        onNew() {
            if (this.value === "[None]") {
                this.activity = true;
                this.repository.add(this.$route.fullPath, this.schema.inverseType, this.model[this.model.primaryKeyProperty])
                    .then(data => {
                        this.activity = false;
                        if (data.error) {
                            this.activity = false;
                            this.errors.push(data.error);
                            this.$emit("validated", this.errors.length === 0, this.errors, this);
                        } else {
                            this.$router.push({ name: this.schema.inputType, params: { operation: 'add', id: data.data[data.data.primaryKeyProperty] } });
                        }
                    })
                    .catch(error => {
                        this.activity = false;
                        this.errors.push("A problem occurred. The new item could not be added.");
                        this.$emit("validated", this.errors.length === 0, this.errors, this);
                        ErrorMsg.logError("fieldNavigation.onNew", new Error(error));
                    });
            } else {
                this.replaceDialogShown = true;
            }
        },

        onReplace() {
            this.replaceDialogShown = false;
            this.errors.splice(0);
            this.activity = true;
            this.repository.replaceChildWithNew(this.$route.fullPath, this.schema.parentId, this.schema.model)
                .then(data => {
                    if (data.error) {
                        this.activity = false;
                        this.errors.push(data.error);
                        this.$emit("validated", this.errors.length === 0, this.errors, this);
                    } else {
                        this.$router.push({ name: this.schema.inputType, params: { operation: 'add', id: data.data[data.data.primaryKeyProperty] } });
                    }
                })
                .catch(error => {
                    this.activity = false;
                    this.errors.push("A problem occurred. The item could not be added.");
                    this.$emit("validated", this.errors.length === 0, this.errors, this);
                    ErrorMsg.logError("fieldNavigation.onReplace", new Error(error));
                });
        },

        onSelect() {
            this.$router.push({
                name: this.schema.inputType + "DataTable",
                params: {
                    childProp: this.schema.inverseType,
                    operation: 'select',
                    parentType: this.model.dataType,
                    parentId: this.model[this.model.primaryKeyProperty],
                    parentProp: this.schema.model
                }
            });
        },

        onView() {
            if (this.schema.navigationType === "objectMultiSelect") {
                this.$router.push({
                    name: this.schema.inputType + "DataTable",
                    params: {
                        operation: 'multiselect',
                        parentType: this.model.dataType,
                        parentId: this.model[this.model.primaryKeyProperty],
                        parentProp: this.schema.model
                    }
                });
            } else if (this.schema.navigationType === "objectCollection") {
                this.$router.push({
                    name: this.schema.inputType + "DataTable",
                    params: {
                        childProp: this.schema.inverseType,
                        operation: 'collection',
                        parentType: this.model.dataType,
                        parentId: this.model[this.model.primaryKeyProperty],
                        parentProp: this.schema.model
                    }
                });
            } else {
                this.activity = true;
                this.repository.getChildId(this.$route.fullPath, this.schema.parentId, this.schema.model)
                    .then(data => {
                        this.activity = false;
                        if (data.error) {
                            this.errors.push(data.error);
                            this.$emit("validated", this.errors.length === 0, this.errors, this);
                        } else {
                            this.$router.push({ name: this.schema.inputType, params: { operation: 'view', id: data.response } });
                        }
                    })
                    .catch(error => {
                        this.activity = false;
                        this.errors.push("A problem occurred. The item could not be accessed.");
                        this.$emit("validated", this.errors.length === 0, this.errors, this);
                        ErrorMsg.logError("fieldNavigation.onView", new Error(error));
                    });
            }
        }
    }
};