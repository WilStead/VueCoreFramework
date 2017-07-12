import { abstractField } from 'vue-form-generator';
import { DataItem, Repository } from '../../store/repository';
import * as ErrorMsg from '../../error-msg';

import DynamicDataTable from '../dynamic-table/dynamic-data-table';

export default {
    mixins: [abstractField],
    data() {
        return {
            activity: false,
            childRepository: this.$store.getters.getRepository(this.schema.inputType) as Repository,
            deleteDialogShown: false,
            replaceDialogShown: false,
            repository: this.$store.getters.getRepository(this.schema.parentType) as Repository,
            selectActivity: true,
            selectDialogShown: false,
            selected: [] as DataItem[],
            selectErrorMessage: '',
            selectWarningMessage: '',
        };
    },
    methods: {
        onDelete() {
            this.deleteDialogShown = false;
            this.errors.splice(0);
            this.activity = true;
            this.repository.removeFromParent(this.$route.fullPath, this.model[this.model.primaryKeyProperty], this.schema.model)
                .then(response => {
                    this.errors.push("navigation success");
                    this.activity = false;
                    this.$emit("validated", this.errors.length === 0, this.errors, this);
                })
                .catch(error => {
                    this.activity = false;
                    let msg = 'A problem occurred. The item could not be removed. ';
                    if (error && error.message && error.message.startsWith("CODE:")) {
                        msg += error.message.replace('CODE:', '');
                    }
                    this.errors.push(msg);
                    this.$emit("validated", this.errors.length === 0, this.errors, this);
                    ErrorMsg.logError("fieldNavigation.onDelete", new Error(error));
                });
        },

        onNew() {
            if (this.value === "[None]") {
                this.activity = true;
                this.errors.splice(0);
                this.childRepository.add(this.$route.fullPath, this.schema.inverseType, this.model[this.model.primaryKeyProperty])
                    .then(data => {
                        this.activity = false;
                        this.$router.push({ name: this.schema.inputType, params: { operation: 'add', id: data[data.primaryKeyProperty] } });
                    })
                    .catch(error => {
                        this.activity = false;
                        let msg = 'A problem occurred. The new item could not be added. ';
                        if (error && error.message && error.message.startsWith("CODE:")) {
                            msg += error.message.replace('CODE:', '');
                        }
                        this.errors.push(msg);
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
            this.repository.replaceChildWithNew(this.$route.fullPath, this.model[this.model.primaryKeyProperty], this.schema.model)
                .then(data => {
                    this.$router.push({ name: this.schema.inputType, params: { operation: 'add', id: data[data.primaryKeyProperty] } });
                })
                .catch(error => {
                    this.activity = false;
                    let msg = 'A problem occurred. The item could not be added. ';
                    if (error && error.message && error.message.startsWith("CODE:")) {
                        msg += error.message.replace('CODE:', '');
                    }
                    this.errors.push(msg);
                    this.$emit("validated", this.errors.length === 0, this.errors, this);
                    ErrorMsg.logError("fieldNavigation.onReplace", new Error(error));
                });
        },

        onSelect() {
            if (!this.selected.length) {
                this.selectErrorMessage = '';
                this.selectWarningMessage = "You have not selected an item.";
            } else if (this.selected.length > 1) {
                this.selectErrorMessage = '';
                this.selectWarningMessage = "You can only select a single item.";
            } else if (this.childProp) {
                this.selectWarningMessage = '';
                this.selectActivity = true;
                this.repository.replaceChild(this.$route.fullPath,
                    this.model[this.model.primaryKeyProperty],
                    this.selected[0][this.selected[0].primaryKeyProperty],
                    this.schema.inverseType)
                    .then(response => {
                        this.selectErrorMessage = '';
                        this.selectDialogShown = false;
                        this.selectActivity = false;
                    })
                    .catch(error => {
                        this.selectActivity = false;
                        this.selectErrorMessage = "A problem occurred. The item could not be updated. ";
                        if (error && error.message && error.message.startsWith("CODE:")) {
                            this.selectErrorMessage += error.message.replace('CODE:', '');
                        }
                        ErrorMsg.logError("fieldNavigation.onSelect", new Error(error));
                    });
            } else {
                this.selectWarningMessage = '';
                this.selectErrorMessage = "There was a problem saving your selection. Please try going back to the previous page before trying again.";
            }
        },

        onSelectError(error: string) {
            this.selectErrorMessage = error;
        },

        onView() {
            this.activity = true;
            this.errors.splice(0);
            this.repository.getChildId(this.$route.fullPath, this.model[this.model.primaryKeyProperty], this.schema.model)
                .then(data => {
                    this.activity = false;
                    this.$router.push({ name: this.schema.inputType, params: { operation: 'view', id: data } });
                })
                .catch(error => {
                    this.activity = false;
                    let msg = 'A problem occurred. The item could not be accessed. ';
                    if (error && error.message && error.message.startsWith("CODE:")) {
                        msg += error.message.replace('CODE:', '');
                    }
                    this.errors.push(msg);
                    this.$emit("validated", this.errors.length === 0, this.errors, this);
                    ErrorMsg.logError("fieldNavigation.onView", new Error(error));
                });
        }
    }
};